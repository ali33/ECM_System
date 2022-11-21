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

namespace Ecm.OutlookClient
{
    public class PdfHelper
    {
        public static string ConvertToPdf(string html)
        {
            //create document
            Document document = new Document();
            try
            {
                //writer - have our own path!!! and see you have write permissions...
                string fileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".pdf";
                PdfWriter.GetInstance(document, new FileStream(fileName, FileMode.Create));
                document.Open();

                List<iTextSharp.text.IElement> htmlarraylist = iTextSharp.text.html.simpleparser.HTMLWorker.ParseToList(new StringReader(html.Replace("px","")), null);
                //add the collection to the document
                for (int k = 0; k < htmlarraylist.Count; k++)
                {
                    document.Add((IElement)htmlarraylist[k]);
                }

                //document.Add(new Paragraph("And the same with indentation...."));
                // or add the collection to an paragraph 
                // if you add it to an existing non emtpy paragraph it will insert it from
                //the point youwrite -
                Paragraph mypara = new Paragraph();//make an emtphy paragraph as "holder"
                mypara.IndentationLeft = 36;
                //mypara.InsertRange(0, htmlarraylist);
                document.Add(mypara);
                document.Close();

                return fileName;
            }
            catch (Exception exx)
            {
                Console.Error.WriteLine(exx.StackTrace);
                Console.Error.WriteLine(exx.Message);
            }

            return null;
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
