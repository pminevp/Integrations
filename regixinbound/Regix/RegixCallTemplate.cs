using regixinbound.RegixServiceReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace regixinbound.Regix
{
   public class RegixCallTemplate
    {
        public void Call()
        {
            RegiXEntryPointClient client = new RegiXEntryPointClient();

            ServiceRequestData request = new ServiceRequestData();



            request.Operation = "TechnoLogica.RegiX.GraoNBDAdapter.APIService.INBDAPI.ValidPersonSearch";

            XmlDocument doc = new XmlDocument();

            doc.LoadXml(@"<ValidPersonRequest

                            xmlns:xsd=""http://www.w3.org/2001/XMLSchema""

                            xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""

                           xmlns=""http://egov.bg/RegiX/GRAO/NBD/ValidPersonRequest"">

                            <EGN>8506258485</EGN>

                          </ValidPersonRequest>");

            request.Argument = doc.DocumentElement;



            request.CallContext = new CallContext();

            request.CallContext.AdministrationName = "Администрация";

            request.CallContext.AdministrationOId = "1.2.3.4.5.6.7.8.9";

            request.CallContext.EmployeeIdentifier = "myusername@administration.domain";

            request.CallContext.EmployeeNames = "Първо Второ Трето";

            request.CallContext.EmployeePosition = "Експерт в отдел";

            request.CallContext.LawReason = "На основание чл. X от Наредба/Закон/Нормативен акт";

            request.CallContext.Remark = "За тестване на системата";

            request.CallContext.ServiceType = "За административна услуга";

            request.CallContext.ServiceURI = "123-12345-01.01.2017";



            request.CitizenEGN = "XXXXXXXXXX";

            request.EmployeeEGN = "XXXXXXXXXX";



            request.ReturnAccessMatrix = false;

            request.SignResult = true;



            ServiceResultData response = client.ExecuteSynchronous(request);

            if (!response.HasError)
            {

                XmlElement result = response.Data.Response.Any;

                Console.WriteLine("Hello RegiX!");

            }
            else
            {

                Console.WriteLine(response.Error);

            }
        }
    }
}
