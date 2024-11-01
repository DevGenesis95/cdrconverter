using CorelDRAW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ApiConvertorv3.Helpers
{
    public static class PdfConvertor
    {
        public static string ConvertToPdf(string filePath)
        {
            CorelDRAW.Application cdr =
                 new Application();
            try
            {

                string extension = System.IO.Path.GetExtension(filePath);
                string fileName = filePath.Substring(0, filePath.Length - extension.Length);
                
                string cdrPath = fileName + ".cdr";  //@"C:\cdrSamples\placecards\Placards.cdr";


                //pdf


                string pdfPath = fileName + ".pdf"; //@"C:\cdrSamples\placecards\Placards.pdf";

                cdr.OpenDocument(cdrPath, 1);
                cdr.ActiveDocument.PublishToPDF(pdfPath);//*


                return pdfPath;
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {

                cdr.ActiveDocument.Close();
                cdr.Quit();
            }

          

        }

    }
}