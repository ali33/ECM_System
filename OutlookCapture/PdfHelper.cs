using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using iTextSharp.text.pdf;
using iTextSharp.text;
using iTextSharp.text.html;
using iTextSharp.text.xml;
using iTextSharp.text.html.simpleparser;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using iTextSharp.tool.xml;

namespace OutlookCapture
{
    public class PdfHelper
    {
        public static string ConvertToPdf(string html)
        {
            //Create a byte array that will eventually hold our final PDF
            Byte[] bytes;

            //Boilerplate iTextSharp setup here
            //Create a stream that we can write to, in this case a MemoryStream
            using (var ms = new MemoryStream())
            {

                //Create an iTextSharp Document which is an abstraction of a PDF but **NOT** a PDF
                using (var doc = new Document())
                {

                    //Create a writer that's bound to our PDF abstraction and our stream
                    using (var writer = PdfWriter.GetInstance(doc, ms))
                    {

                        //Open the document for writing
                        doc.Open();

                        //Our sample HTML and CSS
                        //var example_html = @"<p>This <em>is </em><span class=""headline"" style=""text-decoration: underline;"">some</span> <strong>sample <em> text</em></strong><span style=""color: red;"">!!!</span></p>";
                        //var example_css = @".headline{font-size:200%}";

                        /**************************************************
                         * Example #1                                     *
                         *                                                *
                         * Use the built-in HTMLWorker to parse the HTML. *
                         * Only inline CSS is supported.                  *
                         * ************************************************/

                        //Create a new HTMLWorker bound to our document
                        using (var htmlWorker = new iTextSharp.text.html.simpleparser.HTMLWorker(doc))
                        {

                            //HTMLWorker doesn't read a string directly but instead needs a TextReader (which StringReader subclasses)
                            using (var sr = new StringReader(html))
                            {

                                //Parse the HTML
                                htmlWorker.Parse(sr);
                            }
                        }

                        /**************************************************
                         * Example #2                                     *
                         *                                                *
                         * Use the XMLWorker to parse the HTML.           *
                         * Only inline CSS and absolutely linked          *
                         * CSS is supported                               *
                         * ************************************************/

                        //XMLWorker also reads from a TextReader and not directly from a string
                        using (var srHtml = new StringReader(html))
                        {

                            //Parse the HTML
                            iTextSharp.tool.xml.XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, srHtml);
                        }

                        /**************************************************
                         * Example #3                                     *
                         *                                                *
                         * Use the XMLWorker to parse HTML and CSS        *
                         * ************************************************/

                        //In order to read CSS as a string we need to switch to a different constructor
                        //that takes Streams instead of TextReaders.
                        //Below we convert the strings into UTF8 byte array and wrap those in MemoryStreams
                        //using (var msCss = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(example_css)))
                        //{
                        using (var msHtml = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(html)))
                        {

                            //Parse the HTML
                            iTextSharp.tool.xml.XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, msHtml, System.Text.Encoding.UTF8);
                        }
                        //}


                        doc.Close();
                    }
                }

                //After all of the PDF "stuff" above is done and closed but **before** we
                //close the MemoryStream, grab all of the active bytes from the stream
                bytes = ms.ToArray();
            }

            //Now we just need to do something with those bytes.
            //Here I'm writing them to disk but if you were in ASP.Net you might Response.BinaryWrite() them.
            //You could also write the bytes to a database in a varbinary() column (but please don't) or you
            //could pass them to another function for further PDF processing.
            var testFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "test.pdf");
            System.IO.File.WriteAllBytes(testFile, bytes);

            return testFile;
        }

        //public void ConvertHTMLToPDF(string HTMLCode)
        //{
        //    string fileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".pdf";

        //    //Render PlaceHolder to temporary stream 
        //    System.IO.StringWriter stringWrite = new StringWriter();

        //    /********************************************************************************/
        //    //Try adding source strings for each image in content
        //    string tempPostContent = string.Empty;// getImage(HTMLCode);
        //    /*********************************************************************************/

        //    StringReader reader = new StringReader(tempPostContent);

        //    //Create PDF document 
        //    Document doc = new Document(PageSize.A4);
        //    HTMLWorker parser = new HTMLWorker(doc);
        //    PdfWriter.GetInstance(doc, new FileStream(fileName, FileMode.Create));
        //    doc.Open();

        //    try
        //    {
        //        //Parse Html and dump the result in PDF file
        //        parser.Parse(reader);
        //    }
        //    catch (Exception ex)
        //    {
        //        //Display parser errors in PDF. 
        //        Paragraph paragraph = new Paragraph("Error!" + ex.Message);
        //        Chunk text = paragraph.Chunks[0] as Chunk;
        //        if (text != null)
        //        {
        //            text.Font.Color = BaseColor.RED;
        //        }
        //        doc.Add(paragraph);
        //    }
        //    finally
        //    {
        //        doc.Close();
        //    }
        //}

        //public string getImage(string input)
        //{
        //    if (input == null)
        //        return string.Empty;
        //    string tempInput = input;
        //    string pattern = @"<img(.|\n)+?>";
        //    string src = string.Empty;

        //    //Change the relative URL's to absolute URL's for an image, if any in the HTML code.
        //    foreach (Match m in Regex.Matches(input, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.RightToLeft))
        //    {
        //        if (m.Success)
        //        {
        //            string tempM = m.Value;
        //            string pattern1 = "src=[\'|\"](.+?)[\'|\"]";
        //            Regex reImg = new Regex(pattern1, RegexOptions.IgnoreCase | RegexOptions.Multiline);
        //            Match mImg = reImg.Match(m.Value);

        //            if (mImg.Success)
        //            {
        //                src = mImg.Value.ToLower().Replace("src=", "").Replace("\"", "");

        //                if (src.ToLower().Contains("http://") == false)
        //                {
        //                    //Insert new URL in img tag
        //                    src = "src=\"" + context.Request.Url.Scheme + "://" +
        //                        context.Request.Url.Authority + src + "\"";
        //                    try
        //                    {
        //                        tempM = tempM.Remove(mImg.Index, mImg.Length);
        //                        tempM = tempM.Insert(mImg.Index, src);

        //                        //insert new url img tag in whole html code
        //                        tempInput = tempInput.Remove(m.Index, m.Length);
        //                        tempInput = tempInput.Insert(m.Index, tempM);
        //                    }
        //                    catch (Exception e)
        //                    {

        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return tempInput;
        //}

        //string getSrc(string input)
        //{
        //    string pattern = "src=[\'|\"](.+?)[\'|\"]";

        //    System.Text.RegularExpressions.Regex reImg = new System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Multiline);

        //    System.Text.RegularExpressions.Match mImg = reImg.Match(input);
        //    if (mImg.Success)
        //    {
        //        return mImg.Value.Replace("src=", "").Replace("\"", ""); ;
        //    }

        //    return string.Empty;
        //}
    }
}
