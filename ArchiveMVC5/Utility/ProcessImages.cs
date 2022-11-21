using ArchiveMVC5.Enums;
using ArchiveMVC5.Models;
using ArchiveMVC5.Models.DataProvider;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace ArchiveMVC5.Utility
{
    public class ProcessImages
    {
        public static byte[] ConvertTo(DocumentSerializable doc, string option = "pdf", string range = "all", List<int> pages = null)
        {
            ///Draw annotation on each page
            List<Bitmap> bmps = new List<Bitmap>();
            DocumentProvider provider = new DocumentProvider(Utilities.UserName, Utilities.Password);
            ////////Truong hop gui mail trong search, k co cache page
            //Lay document tu DB dua vao id cua document user gui len - doc.DocumentId
            var isCached = doc.Pages != null && doc.Pages.Count() > 0 &&
                Guid.Parse(doc.Pages.First().ImgKey) != Guid.Empty;
            if (!isCached)
            {
                var document = provider.GetDocument(doc.DocumentId);
                foreach (var page in document.Pages)
                {
                    //moi page trong document kiem tra neu la Image
                    if (page.FileType == FileTypeModel.Image)
                    {
                        var b = page.FileBinaries;
                        Bitmap img = ProcessImages.ResizeImage(
                            ProcessImages.ByteArrayToImage(b), (int)page.Width);
                        Bitmap bmp = new Bitmap(img);
                        if (page.Annotations != null)
                            foreach (var a in page.Annotations)
                            {
                                bmp = ProcessImages.AddAnnotation(bmp, a);
                            }
                        bmps.Add(bmp);
                    }
                }
            }
            /////Truong hop user dang view, capture
            else
            {
                foreach (var p in doc.Pages)
                {
                    var pageNumber = doc.Pages.IndexOf(p) + 1;
                    if (range != "all" && !pages.Contains(pageNumber))
                        continue;
                    //Trong View, Capture, Page da dc cache
                    var file = (CacheObject)System.Web.HttpContext.Current.Cache[p.ImgKey];
                    if (file is CacheImage)
                    {
                        var b = ((CacheImage)file).FileBinaries;
                        Bitmap img = ProcessImages.ResizeImage(
                            ProcessImages.ByteArrayToImage(b), (int)p.PageWidth);
                        Bitmap bmp = new Bitmap(img);
                        if (p.Annotations != null)
                            foreach (var a in p.Annotations)
                            {
                                bmp = ProcessImages.AddAnnotation(bmp, a);
                            }
                        bmps.Add(bmp);
                    }
                }
            }




            if (option.Equals("pdf"))
            {
                ///
                ///Create PDF document return to client
                ///
                PdfDocument pdfDoc = new PdfDocument();
                foreach (var bmp in bmps)
                {
                    var p = pdfDoc.AddPage(new PdfPage());
                    XGraphics xgr = XGraphics.FromPdfPage(p);
                    var imgrz = ProcessImages.ResizeImage(bmp, Constant.WIDTH_A4);
                    XImage img = XImage.FromGdiPlusImage(bmp);
                    xgr.DrawImage(img, 0, 0);
                }
                //string outFile = Server.MapPath("~/Temp/") + key + ".pdf";
                byte[] buffer;
                using (MemoryStream ms = new MemoryStream())
                {
                    pdfDoc.Save(ms);
                    pdfDoc.Close();
                    buffer = ms.ToArray();
                }
                return buffer;
            }
            else
            {
                var bytes = ProcessImages.MergeTiff(bmps);
                var key = Guid.NewGuid().ToString();
                //return File(bytes, ContentTypeEnumeration.Image.TIFF);
                //CacheImage cache = new CacheImage() { FileBinaries = bytes, ContentType = ContentTypeEnumeration.Image.TIFF };
                CacheFilesBinary cache = new CacheFilesBinary()
                {
                    FileBinaries = bytes,
                    ContentType = ContentTypeEnumeration.Image.TIFF,
                    OrginalFileName = key + ".tiff"
                };
                return bytes;
            }
        }

        public static Byte[] TiffToJpeg(Byte[] tiffBytes)
        {
            //Byte[] tiffBytes;
            Byte[] jpegBytes;

            using (System.IO.MemoryStream inStream = new System.IO.MemoryStream(tiffBytes))
            using (System.IO.MemoryStream outStream = new System.IO.MemoryStream())
            {
                System.Drawing.Bitmap.FromStream(inStream).Save(outStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                jpegBytes = outStream.ToArray();
            }
            return jpegBytes;
        }

        public static Bitmap BitmapToTiff(Bitmap bitmap)
        {
            //Byte[] tiffBytes;
            var b = ImageToByteArray(bitmap);
            //Byte[] tiffBytes;

            using (System.IO.MemoryStream inStream = new System.IO.MemoryStream(b))
            using (System.IO.MemoryStream outStream = new System.IO.MemoryStream())
            {
                System.Drawing.Bitmap.FromStream(inStream).Save(outStream, System.Drawing.Imaging.ImageFormat.Tiff);
                //tiffBytes = outStream.ToArray();
                return (Bitmap)Image.FromStream(outStream);
            }
            //return null;// tiffBytes;
        }

        //[9/20/2013] Update: change method's name to standard
        public static byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            if (imageIn == null)
                return null;
            using (MemoryStream ms = new MemoryStream())
            {
                imageIn.Save(ms, ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }

        public static byte[] ImageToByteArray(System.Drawing.Image imageIn, ImageFormat imageFormat)
        {
            try
            {
                imageFormat = imageFormat ?? ImageFormat.Jpeg;
                MemoryStream ms = new MemoryStream();
                imageIn.Save(ms, imageFormat);
                return ms.ToArray();
            }
            catch { }
            return null;
        }

        //[9/20/2013]Update: change method's name to standard
        public static Image ByteArrayToImage(byte[] byteArrayIn)
        {
            try
            {
                MemoryStream ms = new MemoryStream(byteArrayIn);
                Image returnImage = Image.FromStream(ms);
                return returnImage;
            }
            catch (Exception e) { throw e; }
        }
        /// <summary>
        /// Xoay ảnh với góc chỉ định
        /// </summary>
        /// <param name="image"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Image Rotate(Image image, RotateType type)
        {
            Bitmap bmp = new Bitmap(image);
            using (Graphics gfx = Graphics.FromImage(bmp))
            {
                gfx.Clear(Color.White);
                gfx.DrawImage(image, 0, 0, image.Width, image.Height);
            }
            switch (type)
            {
                case RotateType.Rolate_0:
                    //bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    break;
                case RotateType.Rolate_90:
                    bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    break;
                case RotateType.Rolate_180:
                    bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    break;
                case RotateType.Rolate_270:
                    bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    break;
            }
            return bmp;
        }

        public static Image Rotate(Image image, int angle)
        {
            Bitmap bmp = new Bitmap(image);
            using (Graphics gfx = Graphics.FromImage(bmp))
            {
                gfx.Clear(Color.White);
                gfx.DrawImage(image, 0, 0, image.Width, image.Height);
            }
            switch (angle)
            {
                case 90:
                    bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    break;
                case 180:
                    bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    break;
                case 270:
                    bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    break;
            }
            return bmp;
        }

        public static Image AddNote(Image image,AnnotationTypeModel type, Point point, Size size, String str)
        {
            Bitmap bmp = new Bitmap(image);
            
            Graphics gBmp = Graphics.FromImage(bmp);
            gBmp.CompositingMode = CompositingMode.SourceOver;
            // draw a green rectangle to the bitmap in memory
            int alpha = 0;
            Color green = Color.FromArgb(alpha, 0, 0xff, 0);
            Brush greenBrush = new SolidBrush(green);
            switch (type)
            {
                case AnnotationTypeModel.Highlight:
                    alpha = 128;
                    break;
                case AnnotationTypeModel.Line:
                    break;
                case AnnotationTypeModel.OCRZone:
                    alpha = 0;
                    green = Color.FromArgb(alpha, 0, 0xff, 0);
                    greenBrush = new SolidBrush(green);
                    gBmp.DrawRectangle(Pens.Red, point.X, point.Y, size.Width, size.Height);        
                    break;
                case AnnotationTypeModel.Redaction:
                    alpha = 0;
                    break;
                case AnnotationTypeModel.Text:
                    break;
            }
            //bmp.Dispose();
            gBmp.Dispose();
            //redBrush.Dispose();
            greenBrush.Dispose();

            return bmp;
        }

        public static Bitmap AddAnnotation(Bitmap bmpIn, AnnotationModel annotation)
        {
            var bmp = new Bitmap(bmpIn);
            Graphics g = Graphics.FromImage(bmp);
            g.CompositingMode = CompositingMode.SourceOver;
            Color c;
            Brush b;
            int alpha;
            RectangleF rect;
            switch (annotation.Type)
            {
                case AnnotationTypeModel.Highlight:
                    alpha = 128;
                    c = Color.FromArgb(alpha, 0, 0xff, 0);
                    b = new SolidBrush(c);
                    rect = new RectangleF((float)annotation.Left,
                                                     (float)annotation.Top,
                                                     (float)annotation.Width,
                                                     (float)annotation.Height);
                    g.FillRectangle(b, rect);
                    b.Dispose();
                    break;
                case AnnotationTypeModel.Line:
                    break;
                case AnnotationTypeModel.Redaction:
                    //alpha = 0;
                    c = Color.Black;
                    b = new SolidBrush(c);
                    rect = new RectangleF((float)annotation.Left,
                                                     (float)annotation.Top,
                                                     (float)annotation.Width,
                                                     (float)annotation.Height);
                    g.FillRectangle(b, rect);
                    b.Dispose();

                    break;
                case AnnotationTypeModel.Text:
                    c = Color.FromArgb(204, 255, 255, 205);
                    b = new SolidBrush(c);
                    rect = new RectangleF((float)annotation.Left,
                                                     (float)annotation.Top,
                                                     (float)annotation.Width,
                                                     (float)annotation.Height);
                    g.FillRectangle(b, rect);
                    c = Color.Black;
                    b.Dispose();
                    b = new SolidBrush(c);
                    g.DrawString(annotation.Content, new Font(new FontFamily("Comic Sans MS"), 12), b, rect);
                    b.Dispose();
                    break;
            }
            //bmp.Dispose();
            g.Dispose();

            return bmp;
        }

        public static Image AddAnnotation(Image img, AnnotationModel annotation)
        {
            Bitmap bmp = new Bitmap(img);
            bmp.SetResolution(img.HorizontalResolution, img.VerticalResolution);
            Graphics g = Graphics.FromImage(bmp);
            g.CompositingMode = CompositingMode.SourceOver;
            Color c;
            Brush b;
            int alpha;
            RectangleF rect;
            switch (annotation.Type)
            {
                case AnnotationTypeModel.Highlight:
                    alpha = 128;
                    c = Color.FromArgb(alpha, 0, 0xff, 0);
                    b = new SolidBrush(c);
                    rect = new RectangleF((float)annotation.Left,
                                                     (float)annotation.Top,
                                                     (float)annotation.Width,
                                                     (float)annotation.Height);
                    g.FillRectangle(b, rect);
                    b.Dispose();
                    break;
                case AnnotationTypeModel.Line:
                    break;
                case AnnotationTypeModel.Redaction:
                    //alpha = 0;
                    c = Color.Black;
                    b = new SolidBrush(c);
                    rect = new RectangleF((float)annotation.Left,
                                                     (float)annotation.Top,
                                                     (float)annotation.Width,
                                                     (float)annotation.Height);
                    g.FillRectangle(b, rect);
                    b.Dispose();

                    break;
                case AnnotationTypeModel.Text:
                    c = Color.FromArgb(204, 255, 255, 205);
                    b = new SolidBrush(c);
                    rect = new RectangleF((float)annotation.Left,
                                                     (float)annotation.Top,
                                                     (float)annotation.Width,
                                                     (float)annotation.Height);
                    g.FillRectangle(b, rect);
                    c = Color.Black;
                    b.Dispose();
                    b = new SolidBrush(c);
                    g.DrawString(annotation.Content, new Font(new FontFamily("Comic Sans MS"), 12), b, rect);
                    b.Dispose();
                    break;
            }
            //bmp.Dispose();
            g.Dispose();
            Image i = (Image)bmp;
            return i;
        }

        public static byte[] MergeTiff(List<Bitmap> bmps)
        {
            //get the codec for tiff files
            ImageCodecInfo info = null;
            foreach (ImageCodecInfo ice in ImageCodecInfo.GetImageEncoders())
                if (ice.MimeType == "image/tiff")
                    info = ice; 
            //var outFile = tempFolder + "\\merge_tiff_" + Guid.NewGuid().ToString() +  ".tiff";
            //use the save encoder
            Encoder enc = Encoder.SaveFlag;
            EncoderParameters ep = new EncoderParameters(1);
            ep.Param[0] = new EncoderParameter(enc, (long)EncoderValue.MultiFrame);
            int frame = 0;
            Bitmap pages = null;
            MemoryStream outStream = new MemoryStream();
            foreach (var bmp in bmps)
            {
                if (frame == 0)
                {
                    pages = bmp;
                    //save the first frame
                    pages.Save(outStream, info, ep);
                }
                else
                {
                    //save the intermediate frames
                    ep.Param[0] = new EncoderParameter(enc, (long)EncoderValue.FrameDimensionPage);
                    pages.SaveAdd(bmp, ep);
                }
                if (frame == bmps.Count - 1)
                {
                    //flush and close.
                    ep.Param[0] = new EncoderParameter(enc, (long)EncoderValue.Flush);
                    pages.SaveAdd(ep);
                }
                frame++;
            }
            var bytes = outStream.ToArray();
            return bytes;
        }

        /// <summary>
        /// Get content type of Image from byte array
        /// </summary>'
        /// <author>ThoDinh</author>
        /// <date>9/19/2013</date>
        /// <param name="imageData"> Byte array of image</param>
        /// <returns>Content type of image "image/*"</returns>
        public static string GetImageMimeType(byte[] imageData)
        {
            String mimeType = "image/unknown";
            try
            {
                Guid id;

                using (MemoryStream ms = new MemoryStream(imageData))
                {
                    using (Image img = Image.FromStream(ms))
                    {
                        id = img.RawFormat.Guid;
                    }
                }

                if (id == ImageFormat.Png.Guid)
                {
                    mimeType = "image/png";
                }
                else if (id == ImageFormat.Bmp.Guid)
                {
                    mimeType = "image/bmp";
                }
                else if (id == ImageFormat.Emf.Guid)
                {
                    mimeType = "image/x-emf";
                }
                else if (id == ImageFormat.Exif.Guid)
                {
                    mimeType = "image/jpeg";
                }
                else if (id == ImageFormat.Gif.Guid)
                {
                    mimeType = "image/gif";
                }
                else if (id == ImageFormat.Icon.Guid)
                {
                    mimeType = "image/ico";
                }
                else if (id == ImageFormat.Jpeg.Guid)
                {
                    mimeType = "image/jpeg";
                }
                else if (id == ImageFormat.MemoryBmp.Guid)
                {
                    mimeType = "image/bmp";
                }
                else if (id == ImageFormat.Tiff.Guid)
                {
                    mimeType = "image/tiff";
                }
                else if (id == ImageFormat.Wmf.Guid)
                {
                    mimeType = "image/wmf";
                }
            }
            catch
            {
                
            }

            return mimeType;
        }

        //public static Image ResizeImage(Image image, int width)
        //{
        //    try
        //    {
        //        int newHeight = (int)(image.Height / ((double)image.Width / width));
        //        using (Bitmap resizedImg = new Bitmap(image, width, newHeight))
        //            {
        //                using (MemoryStream stream = new MemoryStream())
        //                {
        //                    resizedImg.Save(stream, image.RawFormat);
        //                    return Image.FromStream(stream);
        //                }
        //            }
        //    }
        //    catch (Exception exp)
        //    {
        //        // log error
        //    }
        //    return null;
        //}
        /// <summary> 
        /// Resize the image to the specified width and height. 
        /// </summary> 
        /// <param name="image">The image to resize.</param> 
        /// <param name="width">The width to resize to.</param> 
        /// <param name="height">The height to resize to.</param> 
        /// <returns>The resized image.</returns> 
        public static System.Drawing.Bitmap ResizeImage(System.Drawing.Image image, int width)
        {
            int newHeight = (int)(((double)image.Height * width)/image.Width);
            //a holder for the result 
            Bitmap result = new Bitmap(width, newHeight);

            //use a graphics object to draw the resized image into the bitmap 
            using (Graphics graphics = Graphics.FromImage(result))
            {
                //set the resize quality modes to high quality 
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                //draw the image into the target bitmap 
                graphics.DrawImage(image, 0, 0, result.Width, result.Height);
            }

            //return the resulting bitmap 
            return result;
        }

        public static Size GetImageSize(byte[] imgArray)
        {
            var image = ProcessImages.ByteArrayToImage(imgArray);
            return image.Size;
        }

        public static float GetHorizontalResolution(byte[] imgArray)
        {
            var image = ProcessImages.ByteArrayToImage(imgArray);
            return image.HorizontalResolution;
        }


        public static Bitmap GetCroppedBitmap(byte[] file, int top, int left, int width, int height)
        {
            Bitmap croppedBitmap = null;
            Image image = ProcessImages.ByteArrayToImage(file);
            if (image != null)
            {
                var croppedArea = new Rectangle(left,
                                                top,
                                                width,
                                                height);
                //var croppedArea = new Rectangle(ToPixel(left, image.HorizontalResolution),
                //                                ToPixel(top, image.HorizontalResolution),
                //                                ToPixel(width, image.HorizontalResolution),
                //                                ToPixel(height, image.HorizontalResolution));
                croppedBitmap = GenerateCroppedBitmap(image, croppedArea);
            }

            return croppedBitmap;
        }

        public static Bitmap GetBitmap(byte[] file)
        {
            Image image = ProcessImages.ByteArrayToImage(file);

            if (image != null)
            {
                return new Bitmap(image);
            }

            return null;
        }

        /// <summary>
        /// Convert ?? to pixel
        /// </summary>
        /// <param name="location"></param>
        /// <param name="dpi"></param>
        /// <returns></returns>

        public static int ToPixel(double location, double dpi)
        {
            return (int)(location * dpi) / 96;
        }
        /// <summary>
        /// Create a bitmap from cropped area
        /// </summary>
        /// <param name="filePath">Path of image file</param>
        /// <param name="croppedArea">Postion area to crop</param>
        /// <returns></returns>
        public static Bitmap GenerateCroppedBitmap(Image image, Rectangle croppedArea)
        {
            try
            {
                Bitmap bmpImage = new Bitmap(image);
                Bitmap bmpCrop = bmpImage.Clone(croppedArea, bmpImage.PixelFormat);
                return (bmpCrop);
            }
            catch (Exception e) { return null; }
        }

    }
    /// <summary> 
    /// Provides various image untilities, such as high quality resizing and the ability to save a JPEG. 
    /// </summary> 
    public static class ImageUtilities
    {
        /// <summary> 
        /// A quick lookup for getting image encoders 
        /// </summary> 
        private static Dictionary<string, ImageCodecInfo> encoders = null;

        /// <summary> 
        /// A quick lookup for getting image encoders 
        /// </summary> 
        public static Dictionary<string, ImageCodecInfo> Encoders
        {
            //get accessor that creates the dictionary on demand 
            get
            {
                //if the quick lookup isn't initialised, initialise it 
                if (encoders == null)
                {
                    encoders = new Dictionary<string, ImageCodecInfo>();
                }

                //if there are no codecs, try loading them 
                if (encoders.Count == 0)
                {
                    //get all the codecs 
                    foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageEncoders())
                    {
                        //add each codec to the quick lookup 
                        encoders.Add(codec.MimeType.ToLower(), codec);
                    }
                }

                //return the lookup 
                return encoders;
            }
        }

        /// <summary> 
        /// Resize the image to the specified width and height. 
        /// </summary> 
        /// <param name="image">The image to resize.</param> 
        /// <param name="width">The width to resize to.</param> 
        /// <param name="height">The height to resize to.</param> 
        /// <returns>The resized image.</returns> 
        public static System.Drawing.Bitmap ResizeImage(System.Drawing.Image image, int width, int height)
        {
            //a holder for the result 
            Bitmap result = new Bitmap(width, height);

            //use a graphics object to draw the resized image into the bitmap 
            using (Graphics graphics = Graphics.FromImage(result))
            {
                //set the resize quality modes to high quality 
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                //draw the image into the target bitmap 
                graphics.DrawImage(image, 0, 0, result.Width, result.Height);
            }

            //return the resulting bitmap 
            return result;
        }

        /// <summary>  
        /// Saves an image as a jpeg image, with the given quality  
        /// </summary>  
        /// <param name="path">Path to which the image would be saved.</param>  
        /// <param name="quality">An integer from 0 to 100, with 100 being the  
        /// highest quality</param>  
        /// <exception cref="ArgumentOutOfRangeException"> 
        /// An invalid value was entered for image quality. 
        /// </exception> 
        public static void SaveJpeg(string path, Image image, int quality)
        {
            //ensure the quality is within the correct range 
            if ((quality < 0) || (quality > 100))
            {
                //create the error message 
                string error = string.Format("Jpeg image quality must be between 0 and 100, with 100 being the highest quality.  A value of {0} was specified.", quality);
                //throw a helpful exception 
                throw new ArgumentOutOfRangeException(error);
            }

            //create an encoder parameter for the image quality 
            EncoderParameter qualityParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            //get the jpeg codec 
            ImageCodecInfo jpegCodec = GetEncoderInfo("image/jpeg");

            //create a collection of all parameters that we will pass to the encoder 
            EncoderParameters encoderParams = new EncoderParameters(1);
            //set the quality parameter for the codec 
            encoderParams.Param[0] = qualityParam;
            //save the image using the codec and the parameters 
            image.Save(path, jpegCodec, encoderParams);
        }

        /// <summary>  
        /// Returns the image codec with the given mime type  
        /// </summary>  
        public static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            //do a case insensitive search for the mime type 
            string lookupKey = mimeType.ToLower();

            //the codec to return, default to null 
            ImageCodecInfo foundCodec = null;

            //if we have the encoder, get it to return 
            if (Encoders.ContainsKey(lookupKey))
            {
                //pull the codec from the lookup 
                foundCodec = Encoders[lookupKey];
            }

            return foundCodec;
        }
    } 
}