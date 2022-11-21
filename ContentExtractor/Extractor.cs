using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Reflection;
using System.Threading;
using log4net;
using System.Diagnostics;
using System.Windows.Forms;
using System.Xml;

namespace Ecm.ContentExtractor
{
    public class Extractor
    {
        private const string DOC = ".doc";
        private const string DOCX = ".docx";
        public const string XLS = ".xls";
        private const string XLSX = ".xlsx";
        private const string PPT = ".ppt";
        private const string PPTX = ".pptx";
        private const string PDF = ".pdf";
        private const string TIFF = ".tiff";
        private const string TIF = ".tif";
        private const string BITMAP = ".bmp";
        private const string JPEG = ".jpg";
        private const string PNG = ".png";
        private const string GIF = ".gif";
        private const string TXT = ".txt";
        private const string RTF = ".rtf";
        private const string LOG = ".log";
        private const string XML = ".xml";
        private const string HTML = ".html";
        private const string HTM = ".htm";

        private const string TEMP_WORKING_FOLDER = "OpenDocExtractorTemp";
        private readonly ILog _log = LogManager.GetLogger(typeof(Extractor));

        public string ExtractToText(string fileName, string language)
        {
            FileInfo info = new FileInfo(fileName);
            string ext = info.Extension;

            if(CheckValidExtension(ext))
            {
                switch (ext)
                {
                    case DOC:
                    case DOCX:
                    case XLS:
                    case XLSX:
                    case PPT:
                    case PPTX:
                        OfficeExtractor officeExtractor = new OfficeExtractor();
                        return officeExtractor.ExtractText(fileName, Path.GetDirectoryName(fileName));
                    case HTM:
                    case HTML:
                        return new HtmlExtractor().ExtractText(fileName);
                    case PDF:
                        PdfExtractor pdfExtractor = new PdfExtractor();
                        return pdfExtractor.ExtractTextFromPdf(fileName);
                    case RTF:
                    case TXT:
                    case LOG:
                        return File.ReadAllText(fileName);
                    default:
                        ImageExtractor imageExtractor = new ImageExtractor();
                        Bitmap image = (Bitmap)Image.FromFile(fileName);
                        imageExtractor.ExtractText(new List<Bitmap> { image }, language);
                        //image.Dispose();
                        return imageExtractor.OcrText;
                }
            }

            return null;
        }

        public string ExtractToText(byte[] fileByte, string fileExtension, string language, string workingFolder)
        {
            string ext = "." + fileExtension;
            string text = string.Empty;

            if (CheckValidExtension(ext))
            {
                try
                {
                    string tempPath = Path.Combine(workingFolder, TEMP_WORKING_FOLDER);
                    string fileName = Path.Combine(tempPath, Guid.NewGuid().ToString() + ext);

                    if (!Directory.Exists(tempPath))
                    {
                        Directory.CreateDirectory(tempPath);
                    }

                    using (MemoryStream me = new MemoryStream(fileByte))
                    {
                        using (FileStream fileStream = new FileStream(fileName, FileMode.CreateNew))
                        {
                            me.WriteTo(fileStream);
                        }
                    }

                    FileInfo info = new FileInfo(fileName);

                    switch (ext)
                    {
                        case DOC:
                        case DOCX:
                        case XLS:
                        case XLSX:
                        case PPT:
                        case PPTX:
                            OfficeExtractor officeExtractor = new OfficeExtractor();
                            text = officeExtractor.ExtractText(fileName, tempPath);
                            break;
                        case HTM:
                        case HTML:
                            text = new HtmlExtractor().ExtractText(fileName);
                            break;
                        case XML:
                            XmlDocument xDoc = new XmlDocument();
                            xDoc.Load(fileName);
                            text = xDoc.InnerText;
                            break;
                        case PDF:
                            PdfExtractor pdfExtractor = new PdfExtractor();
                            text = pdfExtractor.ExtractTextFromPdf(fileName);
                            break;
                        case TIF:
                        case TIFF:
                        case JPEG:
                        case PNG:
                        case GIF:
                        case BITMAP:
                            ImageExtractor imageExtractor = new ImageExtractor();
                            Bitmap bitmap = (Bitmap)Image.FromFile(fileName);
                            imageExtractor.ExtractText(new List<Bitmap> { bitmap }, language);
                            Thread.Sleep(20000);

                            if (string.IsNullOrEmpty(imageExtractor.OcrText))
                            {
                                Thread.Sleep(20000);
                            }

                            text = imageExtractor.OcrText == null ? string.Empty : imageExtractor.OcrText;
                            bitmap.Dispose();
                            break;
                        default:
                            text = File.ReadAllText(fileName);
                            break;
                    }

                    File.Delete(fileName);
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }

            byte[] encodeString = Encoding.Unicode.GetBytes(text);

            return Encoding.Unicode.GetString(encodeString);
        }

        public bool CheckValidExtension(string ext)
        {
            switch (ext)
            {
                case DOC:
                case DOCX:
                case XLS:
                case XLSX:
                case PPT:
                case PPTX:
                case PDF:
                case TIF:
                case TIFF:
                case JPEG:
                case PNG:
                case GIF:
                case BITMAP:
                case TXT:
                case HTM:
                case HTML:
                case XML:
                case RTF:
                case LOG:
                    return true;
                default:
                    return false;
            }
        }

    }
}
