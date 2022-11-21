using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using iTextSharp.text.pdf;
using iTextSharp.text.factories;
using iTextSharp.text.pdf.parser;

namespace Ecm.ContentExtractor
{
    public class PdfExtractor
    {
        public PdfExtractor()
        {

        }
        public string ExtractTextFromPdf(string filePath)
        {
            using (Stream newpdfStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
            {
                PdfReader pdfReader = new PdfReader(newpdfStream);
                return GetText(pdfReader);
            }
        }

        public string ExtractTextFromPdf(byte[] fileByte)
        {
            PdfReader pdfReader = new PdfReader(fileByte);
            return GetText(pdfReader);
        }

        public string ExtractToText(string pdfFilePath)
        {
            PdfReader reader = new PdfReader(pdfFilePath);
            int numOfPage = reader.NumberOfPages;
            PRTokeniser tokeniser = null;
            PRTokeniser.TokType tokenType = PRTokeniser.TokType.STRING;
            StringBuilder values = new StringBuilder();

            for (int i = 1; i <= numOfPage; i++)
            {
                byte[] pageByte = reader.GetPageContent(i);
                tokeniser = null;// new PRTokeniserpageByte();

                while (tokeniser.NextToken())
                {
                    tokenType = tokeniser.TokenType;
                    var stringValue = tokeniser.StringValue;

                    if (tokenType == PRTokeniser.TokType.STRING || tokenType == PRTokeniser.TokType.NUMBER)
                    {
                        values.Append(stringValue);
                    }
                    else if (tokenType == PRTokeniser.TokType.END_ARRAY)
                    {
                        values.Append(" ");
                    }
                    else if (tokenType == PRTokeniser.TokType.OTHER)
                    {
                        switch (stringValue)
                        {
                            case "TJ":
                                values.Append(" ");
                                break;
                            case "ET":
                            case "TD":
                            case "Td":
                            case "Tm":
                            case "T*":
                                values.Append(Environment.NewLine);
                                break;
                        }
                    }
                }
            }

            return values.ToString();
        }

        public string PdfCommentExtractToText()
        {
            PdfReader pdfreader = new PdfReader(@"D:\National Frozen Foods.pdf");
            for (int p = 1; p <= pdfreader.NumberOfPages; p++)
            {
                PdfDictionary PageDictionary = pdfreader.GetPageN(p);
                PdfArray Annots = PageDictionary.GetAsArray(PdfName.ANNOTS);
                if ((Annots == null) || (Annots.Length == 0))
                    continue;

                foreach (PdfObject oAnnot in Annots.ArrayList)
                {
                    PdfDictionary AnnotationDictionary = (PdfDictionary)PdfReader.GetPdfObject(oAnnot);

                    //See if the annotation is a rich media annotation
                    if (AnnotationDictionary.Get(PdfName.SUBTYPE).Equals(PdfName.RICHMEDIA))
                    {
                        //See if it has content
                        if (AnnotationDictionary.Contains(PdfName.RICHMEDIACONTENT))
                        {
                            //Get the content dictionary
                            PdfDictionary RMC = AnnotationDictionary.GetAsDict(PdfName.RICHMEDIACONTENT);
                            if (RMC.Contains(PdfName.ASSETS))
                            {
                                //Get the assset sub dictionary if it exists
                                PdfDictionary Assets = RMC.GetAsDict(PdfName.ASSETS);
                                //Get the names sub array.
                                PdfArray names = Assets.GetAsArray(PdfName.NAMES);
                                //Make sure it has values
                                if (names.ArrayList.Count > 0)
                                {
                                    //A single piece of content can have multiple assets. The array returned is in the form {name, IR, name, IR, name, IR...}
                                    for (int i = 0; i < names.ArrayList.Count; i++)
                                    {
                                        //Get the IndirectReference for the current asset
                                        PdfIndirectReference ir = (PdfIndirectReference)names.ArrayList[++i];
                                        //Get the true object from the main PDF
                                        PdfDictionary obj = (PdfDictionary)PdfReader.GetPdfObject(ir);
                                        //Get the sub Embedded File object
                                        PdfDictionary ef = obj.GetAsDict(PdfName.EF);
                                        //Get the filespec sub object
                                        PdfIndirectReference fir = (PdfIndirectReference)ef.Get(PdfName.F);
                                        //Get the true file stream of the filespec
                                        PRStream objStream = (PRStream)PdfReader.GetPdfObject(fir);
                                        //Get the raw bytes for the given object
                                        byte[] bytes = PdfReader.GetStreamBytes(objStream);
                                        //Do something with the bytes here
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        public static void ExtractImagesFromPDF(string sourcePdf, string outputPath)
        {
            // NOTE:  This will only get the first image it finds per page.
            PdfReader pdf = new PdfReader(sourcePdf);
            RandomAccessFileOrArray raf = new iTextSharp.text.pdf.RandomAccessFileOrArray(sourcePdf);
            try
            {
                for (int pageNumber = 1; pageNumber <= pdf.NumberOfPages; pageNumber++)
                {
                    PdfDictionary pg = pdf.GetPageN(pageNumber);
                    PdfDictionary res =
                      (PdfDictionary)PdfReader.GetPdfObject(pg.Get(PdfName.RESOURCES));
                    PdfDictionary xobj =
                      (PdfDictionary)PdfReader.GetPdfObject(res.Get(PdfName.XOBJECT));
                    if (xobj != null)
                    {
                        foreach (PdfName name in xobj.Keys)
                        {
                            PdfObject obj = xobj.Get(name);
                            if (obj.IsIndirect())
                            {
                                PdfDictionary tg = (PdfDictionary)PdfReader.GetPdfObject(obj);
                                PdfName type =
                                  (PdfName)PdfReader.GetPdfObject(tg.Get(PdfName.SUBTYPE));
                                if (PdfName.IMAGE.Equals(type))
                                {
                                    int XrefIndex = Convert.ToInt32(((PRIndirectReference)obj).Number.ToString(System.Globalization.CultureInfo.InvariantCulture));
                                    PdfObject pdfObj = pdf.GetPdfObject(XrefIndex);
                                    PdfStream pdfStrem = (PdfStream)pdfObj;
                                    byte[] bytes = PdfReader.GetStreamBytesRaw((PRStream)pdfStrem);
                                    if ((bytes != null))
                                    {
                                        using (System.IO.MemoryStream memStream = new System.IO.MemoryStream(bytes))
                                        {
                                            memStream.Position = 0;
                                            System.Drawing.Image img = System.Drawing.Image.FromStream(memStream);
                                            // must save the file while stream is open.
                                            if (!Directory.Exists(outputPath))
                                                Directory.CreateDirectory(outputPath);

                                            string path = Path.Combine(outputPath, String.Format(@"{0}.jpg", pageNumber));
                                            System.Drawing.Imaging.EncoderParameters parms = new System.Drawing.Imaging.EncoderParameters(1);
                                            parms.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Compression, 0);
                                            // GetImageEncoder is found below this method
                                            System.Drawing.Imaging.ImageCodecInfo jpegEncoder = GetImageEncoder("JPEG");
                                            img.Save(path, jpegEncoder, parms);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                pdf.Close();
            }
        }

        public static System.Drawing.Imaging.ImageCodecInfo GetImageEncoder(string imageType)
        {
            imageType = imageType.ToUpperInvariant();
            foreach (ImageCodecInfo info in ImageCodecInfo.GetImageEncoders())
            {
                if (info.FormatDescription == imageType)
                {
                    return info;
                }
            }
            return null;
        }

        private string GetText(PdfReader reader)
        {
            string text = string.Empty;
            for (int i = 1; i <= reader.NumberOfPages; i++)
            {
                text += PdfTextExtractor.GetTextFromPage(reader, i, new iTextSharp.text.pdf.parser.SimpleTextExtractionStrategy());
                PdfReaderContentParser d = new PdfReaderContentParser(reader);
            }
            return text;
        }

    }
}

