using Aspose.Pdf.Operators;
using regixinbound.Regix;
using regixinbound.RegixServiceReference;
using RegixInbound.DAL.B;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace regixinbound
{
    class Program
    {
        static void Main(string[] args)
        {
            var isProduction = bool.Parse(ConfigurationSettings.AppSettings["isProduction"]);
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var manager = new PdfAutoFillManager(path);
            var  timestart= System.DateTime.Now;
            var timeInSeconds = 300;
            var timeEnd = DateTime.Now.AddMinutes(timeInSeconds);
            var logPath = "D:\\AutoFillForms\\";
            var logName = $"log_{DateTime.Now.ToString("ddMMyyyy")}.log";
            var logFullPath = logPath + logName;

            Console.WriteLine ("Системата ще работи във фонов режим " + timeInSeconds+ "мин. - до:" + timeEnd);
          
            while (DateTime.Now.Ticks < timeEnd.Ticks)
            {
               var logs=  manager.AutoFillForm(isProduction);
                var allLog = string.Join("", logs);

                File.AppendAllText(logFullPath, allLog);
            }

            
           
        }
    }
}
 