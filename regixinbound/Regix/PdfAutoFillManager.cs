using Aspose.Pdf;
using Aspose.Pdf.Forms;
using devRegix = regixinbound.RegixServiceReference;
using prodRegix = regixinbound.RegiXEntryPointProduction;
using RegixInbound.DAL.B;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Diagnostics;

namespace regixinbound.Regix
{
    public class PdfAutoFillManager
    {
        dbFormFillEntities entities = new dbFormFillEntities();
        private string TempDirectory { get => $"{WorkingDirectory}{TemporaryFolder}"; }
        private string WorkingDirectory { get; }
        private string TemporaryFolder = @"\temp\";

        public PdfAutoFillManager(string workingDirectory)
        {
            WorkingDirectory = workingDirectory;

            if (!Directory.Exists(TempDirectory))
            {
                Directory.CreateDirectory(TempDirectory);
            }
        }

        public List<string> AutoFillForm(bool isProduction)
        {
            var logs = new List<string>();

            var sw = new Stopwatch();
            var logPath = "D:\\AutoFillForms\\";
            var logName = $"log_{DateTime.Now.ToString("ddMMyyyy")}.log";
            var logFullPath = logPath + logName;
            sw.Start();
            //var allFormsForPopulating = entities.tblAutoFillForms.Where(d => d.IdAutoFill==41 ).ToList();
            var allFormsForPopulating = entities.tblAutoFillForms.Where(d => d.FormData != null && d.DateFormFill == null).ToList();
            // var allFormsForPopulating = entities.tblAutoFillForms.Where(d => d.FormData == null && d.DateFormFill == null).OrderBy(a=>a.DateReg).ToList();

            sw.Stop();
            //  File.AppendAllText(logFullPath, "\r\n Vreme neohodimo za  vzimane na vsichki formi za populvane: " + sw.ElapsedMilliseconds);
            if (allFormsForPopulating.Count > 0)
            {
                logs.Add($"\r\n ### {DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")} Vreme neobhodimo za  vzimane na vsichki formi za populvane: " + sw.ElapsedMilliseconds);
            }
            sw = new Stopwatch();

            foreach (var selectedForm in allFormsForPopulating)
            {
                logs.Add($"\r\n ### {DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")} obrabotvana forma: " + selectedForm.FormId);
                sw.Start();
                var endpointData = GetDataFromEndpoint(selectedForm.IdAutoFill, selectedForm.PersonId, isProduction);
                sw.Stop();
                logs.Add($"\r\n ### {DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")} Vreme za vzimane na Dannite ot service-ite: " + sw.ElapsedMilliseconds);
                sw = new Stopwatch();
                sw.Start();

                /* prepeare the form and start filling */
                var path = SaveFileFromBlob(selectedForm);
                var formToPdfMapping = entities.tblFieldsDocToRegixFields.Where(d => d.FormId == selectedForm.FormId && d.IsActive == true).ToList();
                sw.Stop();
                logs.Add($"\r\n ### {DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")} Vreme neobhodimo za mapvane kum PDF " + sw.ElapsedMilliseconds);
                sw = new Stopwatch();
                sw.Start();
                using (FileStream outFile = new FileStream(path, FileMode.Open))
                {
                    Document doc = new Document(outFile);
                    PopulatePdfFields(endpointData, formToPdfMapping, doc);
                    doc.Save();
                }
                sw.Stop();
                logs.Add($"\r\n ### {DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")} Vreme neobhodimo za populvane na temp formata na diska " + sw.ElapsedMilliseconds);
                sw = new Stopwatch();
                sw.Start();
                SaveBlobFromFile(selectedForm.IdAutoFill, path);
                sw.Stop();
                logs.Add($"\r\n ### {DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")} Vreme neobhodimo za zapisvane na BLOB filea " + sw.ElapsedMilliseconds);

            }

            return logs;
        }

        private void PopulatePdfFields(Dictionary<string, XmlElement> endpointData, List<tblFieldsDocToRegixFields> formToPdfMapping, Document doc)
        {
            foreach (var pdfMap in formToPdfMapping)
            {
                var dataXml = endpointData.Where(s => s.Key.Trim() == pdfMap.RegixSpravkaId).ToList();

                if (dataXml.Count > 0)
                {
                    var data = dataXml.First().Value;
                    var regisSpravkaFields = pdfMap.RegixSpravkaFieldId.Split(',');
                    var prefix = "";

                    if (!string.IsNullOrEmpty(pdfMap.RegixSpravkaFieldPrefix))
                    {
                        prefix = pdfMap.RegixSpravkaFieldPrefix;
                    }

                    var prefixes = prefix.Split(',');

                    List<string> fieldData = new List<string>();

                    int i = 0;
                    foreach (var regixSpravkaField in regisSpravkaFields)
                    {
                        var node = data.GetElementsByTagName(regixSpravkaField);
                        if (node.Count > 0)
                        {
                            var result = node[0].InnerText;

                            if (prefixes.Count() > 0 && prefix.Count() > i)
                            {
                                result = prefixes[i] + " " + result;
                            }

                            fieldData.Add(result);
                        }

                        i++;

                    }

                    if (fieldData.Count > 0)
                    {
                        var value = string.Join(" ", fieldData);
                        doc.Form.XFA[pdfMap.FieldId] = value;
                    }

                }
            }
        }

        private Dictionary<string, XmlElement> GetDataFromEndpoint(int autofillFormId, string egn, bool isProduction)
        {
            var results = new Dictionary<string, XmlElement>();
            var endpointsPerFormId = entities.pdfGenerationInfoView.Where(d => d.IdAutoFill == autofillFormId).ToList();

            foreach (var selectedEndpoint in endpointsPerFormId)
            {
                var requestBuilder = new RequestBuilder(selectedEndpoint.RequestTemplate, selectedEndpoint.Operation);
                requestBuilder.PopulateField(selectedEndpoint.t);

                XmlElement data;
                if (isProduction)
                {
                    var request = requestBuilder.BuildRequestProd();
                    prodRegix.RegiXEntryPointClient Prodclient = new prodRegix.RegiXEntryPointClient();
                    var response = Prodclient.ExecuteSynchronous(request);
                    data = response.Data.Response.Any;
                }
                else
                {
                    var request = requestBuilder.BuildRequest();
                    devRegix.RegiXEntryPointClient client = new devRegix.RegiXEntryPointClient();
                    var response = client.ExecuteSynchronous(request);
                    data = response.Data.Response.Any;
                }

                results.Add(selectedEndpoint.RegixSpravkaId, data);
            }

            return results;
        }


        private string SaveFileFromBlob(tblAutoFillForms selectedForm)
        {
            var fileData = selectedForm.FormData;
            var fileName = selectedForm.FormId;
            var filePath = TempDirectory + fileName;

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            System.IO.File.WriteAllBytes(filePath, fileData);

            return filePath;
        }

        private void SaveBlobFromFile(int idAutoFillForm, string filePath)
        {
            var form = entities.tblAutoFillForms.First(s => s.IdAutoFill == idAutoFillForm);
            var fileBytes = File.ReadAllBytes(filePath);

            form.FormData = fileBytes;
            form.DateFormFill = DateTime.Now;

            entities.tblAutoFillForms.AddOrUpdate(form);
            entities.SaveChanges();

            File.Delete(filePath);
        }
    }

    public class RequestBuilder
    {
        devRegix.ServiceRequestData request = new devRegix.ServiceRequestData();


        private string requestTemplate;
        private readonly string operation;
        public RequestBuilder(string requestTemplate, string operation)
        {
            this.requestTemplate = requestTemplate;
            this.operation = operation;
        }

        public void PopulateField(string value)
        {
            requestTemplate = requestTemplate.Replace($"$payload$", $"{value}");
        }

        public prodRegix.ServiceRequestData BuildRequestProd()
        {
            var prodRequest = new prodRegix.ServiceRequestData();

            prodRequest.Operation = operation;
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(requestTemplate);
            prodRequest.Argument = doc.DocumentElement;
            prodRequest.ReturnAccessMatrix = false;
            prodRequest.SignResult = true;

            return prodRequest;
        }

        public devRegix.ServiceRequestData BuildRequest()
        {
            request.Operation = operation;
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(requestTemplate);
            request.Argument = doc.DocumentElement;
            request.ReturnAccessMatrix = false;
            request.SignResult = true;

            return request;
        }
    }
}
