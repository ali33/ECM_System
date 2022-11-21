using System;
using System.IO;
using Microsoft.Office.Interop.Word;
using Word = Microsoft.Office.Interop.Word;
using Microsoft.Office.Interop.Excel;
using Excel = Microsoft.Office.Interop.Excel;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;
using Microsoft.Win32;
using System.Collections;
namespace ArchiveMVC.Utility
{
    public class ConvertDocumentHelper
    {
        public ConvertDocumentHelper()
        {

        }
        object _MissingValue = System.Reflection.Missing.Value;
        public string Word2Html(string name, string temp)
        {
            try
            {
                if (!IsWordInstalled())
                    return null;
                object inName = (object)name;
                object outName = Path.ChangeExtension(inName.ToString(), "html");
                object fileFormat = WdSaveFormat.wdFormatHTML;

                //Word2Html convert = new Word2Html();
                //try
                //{
                //    convert.Convert(inName, out outName);
                //}
                //catch
                //{
                //    return null;
                //}
                Word.Application word = new Word.Application();
                word.Visible = false;
                word.ScreenUpdating = false;
                // Use the dummy value as a placeholder for optional arguments
                Document doc = word.Documents.Open(ref inName, ref _MissingValue,
                 ref _MissingValue, ref _MissingValue, ref _MissingValue, ref _MissingValue, ref _MissingValue,
                 ref _MissingValue, ref _MissingValue, ref _MissingValue, ref _MissingValue, ref _MissingValue,
                 ref _MissingValue, ref _MissingValue, ref _MissingValue, ref _MissingValue);
                doc.Activate();

                

                doc.SaveAs(ref outName,
                     ref fileFormat, ref _MissingValue, ref _MissingValue,
                     ref _MissingValue, ref _MissingValue, ref _MissingValue, ref _MissingValue,
                     ref _MissingValue, ref _MissingValue, ref _MissingValue, ref _MissingValue,
                     ref _MissingValue, ref _MissingValue, ref _MissingValue, ref _MissingValue);
                object saveChanges = WdSaveOptions.wdDoNotSaveChanges;
                ((_Document)doc).Close(ref saveChanges, ref _MissingValue, ref _MissingValue);
                doc = null;

                // word has to be cast to type _Application so that it will find
                // the correct Quit method.
                ((Word._Application)word).Quit(ref _MissingValue, ref _MissingValue, ref _MissingValue);
                word = null;

                return outName.ToString();
            }catch (Exception e){
                throw e;
            }
        }
        public string Word2Pdf(string name, string temp)
        {
            try
            {
                if (!IsWordInstalled())
                    return null;
                object inName = (object)name;
                object outName = Path.ChangeExtension(inName.ToString(), "pdf");
                object fileFormat = WdSaveFormat.wdFormatPDF;

                //Word2Html convert = new Word2Html();
                //try
                //{
                //    convert.Convert(inName, out outName);
                //}
                //catch
                //{
                //    return null;
                //}
                Word.Application word = new Word.Application();
                word.Visible = false;
                word.ScreenUpdating = false;
                // Use the dummy value as a placeholder for optional arguments
                Document doc = word.Documents.Open(ref inName, ref _MissingValue,
                 ref _MissingValue, ref _MissingValue, ref _MissingValue, ref _MissingValue, ref _MissingValue,
                 ref _MissingValue, ref _MissingValue, ref _MissingValue, ref _MissingValue, ref _MissingValue,
                 ref _MissingValue, ref _MissingValue, ref _MissingValue, ref _MissingValue);
                doc.Activate();
                
                doc.SaveAs(ref outName,
                     ref fileFormat, ref _MissingValue, ref _MissingValue,
                     ref _MissingValue, ref _MissingValue, ref _MissingValue, ref _MissingValue,
                     ref _MissingValue, ref _MissingValue, ref _MissingValue, ref _MissingValue,
                     ref _MissingValue, ref _MissingValue, ref _MissingValue, ref _MissingValue);
                object saveChanges = WdSaveOptions.wdDoNotSaveChanges;
                ((_Document)doc).Close(ref saveChanges, ref _MissingValue, ref _MissingValue);
                doc = null;

                // word has to be cast to type _Application so that it will find
                // the correct Quit method.
                ((Word._Application)word).Quit(ref _MissingValue, ref _MissingValue, ref _MissingValue);
                word = null;

                return outName.ToString();
            }
            catch (Exception e) { throw e; }
        }
        public string Excel2Html(string name, string temp)
        {
            try
            {
                if (!IsExcelInstalled())
                    return null;
                object inName = (object)name;
                object outName = Path.ChangeExtension(inName.ToString(), "html");
                object format = Microsoft.Office.Interop.Excel.XlFileFormat.xlHtml;

                Excel.Application excel = new Excel.Application();
                //excel.Visible = false;
                excel.ScreenUpdating = false;
                object missing = Type.Missing;

                object trueObject = true;

                excel.Visible = false;

                excel.DisplayAlerts = false;
                // Use the dummy value as a placeholder for optional arguments
                Excel.Workbook workbook = excel.Workbooks.Open(name, missing, trueObject, missing,
                                    missing, missing, missing, missing, missing, missing, missing, missing, 
                                    missing, missing, missing);
                IEnumerator wsEnumerator = excel.ActiveWorkbook.Worksheets.GetEnumerator();

                while (wsEnumerator.MoveNext())
                {
                    Workbook wsCurrent = workbook;//(Workbook)wsEnumerator.Current;
                    wsCurrent.SaveAs(outName, format, missing, missing, missing,
                    missing, XlSaveAsAccessMode.xlNoChange, missing, missing, missing, missing, missing);
                }

                excel.Quit();

                return outName.ToString();
            }
            catch (Exception e) { throw e; }

        }
        public string Excel2Pdf(string name, string temp)
        {
            try
            {
                if (!IsExcelInstalled())
                    return null;
                object inName = (object)name;
                object outName = Path.ChangeExtension(inName.ToString(), "pdf");
                object format = WdSaveFormat.wdFormatPDF;

                Excel.Application excel = new Excel.Application();
                //excel.Visible = false;
                excel.ScreenUpdating = false;
                object missing = Type.Missing;

                object trueObject = true;

                excel.Visible = false;

                excel.DisplayAlerts = false;
                // Use the dummy value as a placeholder for optional arguments
                Excel.Workbook workbook = excel.Workbooks.Open(name, missing, trueObject, missing,
                                    missing, missing, missing, missing, missing, missing, missing, missing,
                                    missing, missing, missing);
                IEnumerator wsEnumerator = excel.ActiveWorkbook.Worksheets.GetEnumerator();
                workbook.ExportAsFixedFormat(XlFixedFormatType.xlTypePDF, outName, missing, missing, missing, missing, missing, missing, missing);
                //while (wsEnumerator.MoveNext())
                //{
                //    Workbook wsCurrent = workbook;//(Workbook)wsEnumerator.Current;
                //    wsCurrent.SaveAs(outName, format, missing, missing, missing,
                //    missing, XlSaveAsAccessMode.xlNoChange, missing, missing, missing, missing, missing);
                //}

                excel.Quit();

                return outName.ToString();
            }
            catch (Exception e) { throw e; }
        }
        public string PowerPoint2Html(string name, string temp)
        {
            try
            {
                if (!IsPowerPointInstalled())
                    return null;
                string outName = Path.ChangeExtension(name, "html");
                object format = Microsoft.Office.Interop.Excel.XlFileFormat.xlHtml;

                //Create a PowerPoint Application Object
                PowerPoint.Application ppApp = new PowerPoint.Application();

                //Create a PowerPoint Presentation object
                PowerPoint.Presentation prsPres = ppApp.Presentations.Open(name, Microsoft.Office.Core.MsoTriState.msoTrue, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoFalse);
                //Call the SaveAs method of Presentaion object and specify the format as HTML
                prsPres.SaveAs(outName, PowerPoint.PpSaveAsFileType.ppSaveAsHTML, Microsoft.Office.Core.MsoTriState.msoTrue);

                //Close the Presentation object.
                prsPres.Close();
                //Close the Application object
                ppApp.Quit();

                return outName.ToString();
            }
            catch (Exception e) { throw e; }
        }
        public string PowerPoint2Pdf(string name, string temp)
        {
            try
            {
                if (!IsPowerPointInstalled())
                    return null;
                string outName = Path.ChangeExtension(name, "pdf");

                //Create a PowerPoint Application Object
                PowerPoint.Application ppApp = new PowerPoint.Application();

                //Create a PowerPoint Presentation object
                PowerPoint.Presentation prsPres = ppApp.Presentations.Open(name, Microsoft.Office.Core.MsoTriState.msoTrue, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoFalse);
                //Call the SaveAs method of Presentaion object and specify the format as HTML
                prsPres.SaveAs(outName, PowerPoint.PpSaveAsFileType.ppSaveAsPDF, Microsoft.Office.Core.MsoTriState.msoTrue);

                //Close the Presentation object
                prsPres.Close();
                //Close the Application object
                ppApp.Quit();

                return outName.ToString();
            }
            catch (Exception e) { throw e; }
        }

        private static bool IsWordInstalled()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\Winword.exe");
            if (key != null)
            {
                key.Close();
            }
            return key != null;
        }
        private static bool IsExcelInstalled()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\excel.exe");
            if (key != null)
            {
                key.Close();
            }
            return key != null;
        }
        private static bool IsPowerPointInstalled()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\powerpnt.exe");
            if (key != null)
            {
                key.Close();
            }
            return key != null;
        }

        public static string ConvertExtensionToFilterType(string extension)
        {
            switch (extension)
            {
                case ".doc":
                case ".docx":
                case ".txt":
                case ".rtf":
                case ".html":
                case ".htm":
                case ".xml":
                case ".odt":
                case ".wps":
                case ".wpd":
                    return "writer_pdf_Export";
                case ".xls":
                case ".xlsb":
                case ".xlsx":
                case ".ods":
                    return "calc_pdf_Export";
                case ".ppt":
                case ".pptx":
                case ".odp":
                    return "impress_pdf_Export";

                default:
                    return null;
            }
        }
        //public class Word2Html
        //{
        //    Microsoft.Office.Interop.Word.Application _Word = new Microsoft.Office.Interop.Word.Application();
        //    object _MissingValue = System.Reflection.Missing.Value;

        //    public void Convert(string wordFileName, out string htmlFileName)
        //    {
        //        _Word.Visible = false;
        //        _Word.ScreenUpdating = false;

        //        // Cast as Object for word Open method
        //        object filename = (object)wordFileName;

        //        // Use the dummy value as a placeholder for optional arguments
        //        Document doc = _Word.Documents.Open(ref filename, ref _MissingValue,
        //         ref _MissingValue, ref _MissingValue, ref _MissingValue, ref _MissingValue, ref _MissingValue,
        //         ref _MissingValue, ref _MissingValue, ref _MissingValue, ref _MissingValue, ref _MissingValue,
        //         ref _MissingValue, ref _MissingValue, ref _MissingValue, ref _MissingValue);
        //        doc.Activate();

        //        object outputFileName = htmlFileName = Path.ChangeExtension(wordFileName, "html");
        //        object fileFormat = WdSaveFormat.wdFormatHTML;

        //        // Save document into HTML Format
        //        doc.SaveAs(ref outputFileName,
        //         ref fileFormat, ref _MissingValue, ref _MissingValue,
        //         ref _MissingValue, ref _MissingValue, ref _MissingValue, ref _MissingValue,
        //         ref _MissingValue, ref _MissingValue, ref _MissingValue, ref _MissingValue,
        //         ref _MissingValue, ref _MissingValue, ref _MissingValue, ref _MissingValue);

        //        // Close the Word document, but leave the Word application open.
        //        // doc has to be cast to type _Document so that it will find the
        //        // correct Close method.    
        //        object saveChanges = WdSaveOptions.wdDoNotSaveChanges;
        //        ((_Document)doc).Close(ref saveChanges, ref _MissingValue, ref _MissingValue);
        //        doc = null;

        //        // word has to be cast to type _Application so that it will find
        //        // the correct Quit method.
        //        ((_Application)_Word).Quit(ref _MissingValue, ref _MissingValue, ref _MissingValue);
        //        _Word = null;
        //    }
        //}
    }
}