using Aspose.Pdf.Operators;
using regixinbound.Regix;
using regixinbound.RegixServiceReference;
using RegixInbound.DAL.B;
using System;
using System.Collections.Generic;
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
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var manager = new PdfAutoFillManager(path);
            var  timestart= System.DateTime.Now;
            var timeInSeconds = 5;
            var timeEnd = DateTime.Now.AddMinutes(timeInSeconds);
            Console.WriteLine ("Системата ще работи във фонов режим " + timeInSeconds+ "мин. - до:" + timeEnd);
          
            while (DateTime.Now.Ticks < timeEnd.Ticks)
            {
                manager.AutoFillForm();
            }

            
           
        }
    }
}
 