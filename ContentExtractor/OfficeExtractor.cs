using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Ecm.ContentExtractor.OpenOffice;

namespace Ecm.ContentExtractor
{
    public class OfficeExtractor
    {
        public string ExtractText(string file, string tempPath)
        {
            try
            {
                string pdfFile = Path.Combine(tempPath, Guid.NewGuid().ToString() + ".pdf");
                string text = string.Empty;
                PdfConverter.ConvertToPdf(file, pdfFile);
                PdfExtractor pdf = new PdfExtractor();
                text = pdf.ExtractTextFromPdf(pdfFile);

                File.Delete(pdfFile);

                return text;
            }
            catch
            {
                throw;
            }
        }

        //public string ExtractText(string file, string filterDllName, string filterPersClass)
        //{
        //    using (TextReader reader = new FilterReader(file, filterDllName, filterPersClass))
        //    {
        //        return reader.ReadToEnd();
        //    }
        //}

    }
}
