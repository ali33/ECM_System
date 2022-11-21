using CaptureMVC.Enums;
using CaptureMVC.Models;
using Ecm.CaptureDomain;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace CaptureMVC.Utility
{
    public class ProcessImages
    {
        public static Byte[] TiffToJpeg(Byte[] tiffBytes)
        {
            Byte[] jpegBytes;

            using (System.IO.MemoryStream inStream = new System.IO.MemoryStream(tiffBytes))
            {
                using (System.IO.MemoryStream outStream = new System.IO.MemoryStream())
                {
                    System.Drawing.Bitmap.FromStream(inStream).Save(outStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    jpegBytes = outStream.ToArray();
                }
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

        //[9/20/2013]Update: change method's name to standardpublic static byte[] MergeTiff(List<Bitmap> bmps)
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

        //public static Image AddNote(Image image,AnnotationTypeModel type, Point point, Size size, String str)
        //{
        //    Bitmap bmp = new Bitmap(image);

        //    Graphics gBmp = Graphics.FromImage(bmp);
        //    gBmp.CompositingMode = CompositingMode.SourceOver;
        //    // draw a green rectangle to the bitmap in memory
        //    int alpha = 0;
        //    Color green = Color.FromArgb(alpha, 0, 0xff, 0);
        //    Brush greenBrush = new SolidBrush(green);
        //    switch (type)
        //    {
        //        case AnnotationTypeModel.Highlight:
        //            alpha = 128;
        //            break;
        //        case AnnotationTypeModel.Line:
        //            break;
        //        case AnnotationTypeModel.OCRZone:
        //            alpha = 0;
        //            green = Color.FromArgb(alpha, 0, 0xff, 0);
        //            greenBrush = new SolidBrush(green);
        //            gBmp.DrawRectangle(Pens.Red, point.X, point.Y, size.Width, size.Height);        
        //            break;
        //        case AnnotationTypeModel.Redaction:
        //            alpha = 0;
        //            break;
        //        case AnnotationTypeModel.Text:
        //            break;
        //    }
        //    //bmp.Dispose();
        //    gBmp.Dispose();
        //    //redBrush.Dispose();
        //    greenBrush.Dispose();

        //    return bmp;
        //}

        //public static Bitmap AddAnnotation(Bitmap bmpIn, AnnotationModel annotation)
        //{
        //    var bmp = new Bitmap(bmpIn);
        //    Graphics g = Graphics.FromImage(bmp);
        //    g.CompositingMode = CompositingMode.SourceOver;
        //    Color c;
        //    Brush b;
        //    int alpha;
        //    RectangleF rect;
        //    switch (annotation.Type)
        //    {
        //        case AnnotationTypeModel.Highlight:
        //            alpha = 128;
        //            c = Color.FromArgb(alpha, 0, 0xff, 0);
        //            b = new SolidBrush(c);
        //            rect = new RectangleF((float)annotation.Left,
        //                                             (float)annotation.Top,
        //                                             (float)annotation.Width,
        //                                             (float)annotation.Height);
        //            g.FillRectangle(b, rect);
        //            b.Dispose();
        //            break;
        //        case AnnotationTypeModel.Line:
        //            break;
        //        case AnnotationTypeModel.Redaction:
        //            //alpha = 0;
        //            c = Color.Black;
        //            b = new SolidBrush(c);
        //            rect = new RectangleF((float)annotation.Left,
        //                                             (float)annotation.Top,
        //                                             (float)annotation.Width,
        //                                             (float)annotation.Height);
        //            g.FillRectangle(b, rect);
        //            b.Dispose();

        //            break;
        //        case AnnotationTypeModel.Text:
        //            c = Color.FromArgb(204, 255, 255, 205);
        //            b = new SolidBrush(c);
        //            rect = new RectangleF((float)annotation.Left,
        //                                             (float)annotation.Top,
        //                                             (float)annotation.Width,
        //                                             (float)annotation.Height);
        //            g.FillRectangle(b, rect);
        //            c = Color.Black;
        //            b.Dispose();
        //            b = new SolidBrush(c);
        //            g.DrawString(annotation.Content, new Font(new FontFamily("Comic Sans MS"), 12), b, rect);
        //            b.Dispose();
        //            break;
        //    }
        //    //bmp.Dispose();
        //    g.Dispose();

        //    return bmp;
        //}

        //public static Image AddAnnotation(Image img, AnnotationModel annotation)
        //{
        //    Bitmap bmp = new Bitmap(img);
        //    bmp.SetResolution(img.HorizontalResolution, img.VerticalResolution);
        //    Graphics g = Graphics.FromImage(bmp);
        //    g.CompositingMode = CompositingMode.SourceOver;
        //    Color c;
        //    Brush b;
        //    int alpha;
        //    RectangleF rect;
        //    switch (annotation.Type)
        //    {
        //        case AnnotationTypeModel.Highlight:
        //            alpha = 128;
        //            c = Color.FromArgb(alpha, 0, 0xff, 0);
        //            b = new SolidBrush(c);
        //            rect = new RectangleF((float)annotation.Left,
        //                                             (float)annotation.Top,
        //                                             (float)annotation.Width,
        //                                             (float)annotation.Height);
        //            g.FillRectangle(b, rect);
        //            b.Dispose();
        //            break;
        //        case AnnotationTypeModel.Line:
        //            break;
        //        case AnnotationTypeModel.Redaction:
        //            //alpha = 0;
        //            c = Color.Black;
        //            b = new SolidBrush(c);
        //            rect = new RectangleF((float)annotation.Left,
        //                                             (float)annotation.Top,
        //                                             (float)annotation.Width,
        //                                             (float)annotation.Height);
        //            g.FillRectangle(b, rect);
        //            b.Dispose();

        //            break;
        //        case AnnotationTypeModel.Text:
        //            c = Color.FromArgb(204, 255, 255, 205);
        //            b = new SolidBrush(c);
        //            rect = new RectangleF((float)annotation.Left,
        //                                             (float)annotation.Top,
        //                                             (float)annotation.Width,
        //                                             (float)annotation.Height);
        //            g.FillRectangle(b, rect);
        //            c = Color.Black;
        //            b.Dispose();
        //            b = new SolidBrush(c);
        //            g.DrawString(annotation.Content, new Font(new FontFamily("Comic Sans MS"), 12), b, rect);
        //            b.Dispose();
        //            break;
        //    }
        //    //bmp.Dispose();
        //    g.Dispose();
        //    Image i = (Image)bmp;
        //    return i;
        //}

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
            int newHeight = (int)(((double)image.Height * width) / image.Width);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="imgArray"></param>
        /// <returns></returns>
        public static string GetStringImage(byte[] imgArray)
        {
            var mimeType = "image/unknown";
            Guid id = Guid.Empty;
            float fDpi = 0;

            try
            {
                using (MemoryStream ms = new MemoryStream(imgArray))
                {
                    using (Image img = Image.FromStream(ms))
                    {
                        id = img.RawFormat.Guid;
                        fDpi = img.HorizontalResolution;
                    }
                }
            }
            catch { }// Do nothing

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

            var base64 = Convert.ToBase64String(imgArray);
            return string.Format("data:{0};base64,{1}", mimeType, base64);
        }

        public static string GetStringImage(byte[] imgArray, 
                                            double imgWidth, out int dpi,
                                            int thumbWidth, int thumbHeight, out string stringThumbnail,
                                            List<Annotation> redactions)
        {
            try
            {
                // Get image from byte array
                using (MemoryStream ms = new MemoryStream(imgArray))
                {
                    var mimeType = "image/unknown";
                    Guid id = Guid.Empty;

                    Image img;
                    Image thumbnailImage;
                    var imgStream = new MemoryStream();

                    img = Image.FromStream(ms);
                    id = img.RawFormat.Guid;
                    Bitmap bitmapImg = null;
                    double scale = 1;

                    // Draw redaction to image if any
                    if (redactions != null && redactions.Count > 0)
                    {
                        try
                        {
                            using (Graphics g = Graphics.FromImage(img))
                            {
                                if (img.Width != imgWidth)
                                {
                                    scale = img.Width / imgWidth;
                                }

                                // Create color background
                                Brush brush = new SolidBrush(Utilities.GetRedactionBackground());
                                // Draw redaction to image
                                for (int i = 0; i < redactions.Count; i++)
                                {
                                    g.FillRectangle(brush, (int)(redactions[i].Left * scale), (int)(redactions[i].Top * scale),
                                                           (int)(redactions[i].Width * scale), (int)(redactions[i].Height * scale));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // In case image is pixel index
                            bitmapImg = new Bitmap(img);
                            using (Graphics g = Graphics.FromImage(bitmapImg))
                            {
                                if (img.Width != imgWidth)
                                {
                                    scale = img.Width / imgWidth;
                                }

                                // Create color background
                                Brush brush = new SolidBrush(Utilities.GetRedactionBackground());
                                // Draw redaction to image
                                for (int i = 0; i < redactions.Count; i++)
                                {
                                    g.FillRectangle(brush, (int)(redactions[i].Left * scale), (int)(redactions[i].Top * scale),
                                                           (int)(redactions[i].Width * scale), (int)(redactions[i].Height * scale));
                                }
                            }
                        }
                    }

                    #region Set Mime type

                    if (id == ImageFormat.Tiff.Guid)
                    {
                        if (bitmapImg == null)
                        {
                            // Convert to JPEG if image type is TIFF
                            img.Save(imgStream, ImageFormat.Jpeg);
                        }
                        else
                        {
                            // Convert to JPEG if image type is TIFF
                            bitmapImg.Save(imgStream, ImageFormat.Jpeg);
                        }
                        // Set mime type
                        mimeType = "image/jpeg";
                    }
                    else
                    {
                        if (bitmapImg == null)
                        {
                            img.Save(imgStream, img.RawFormat);
                        }
                        else
                        {
                            bitmapImg.Save(imgStream, img.RawFormat);
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
                        else if (id == ImageFormat.Wmf.Guid)
                        {
                            mimeType = "image/wmf";
                        }
                    }

                    #endregion

                    // Set dpi
                    dpi = (int)img.HorizontalResolution;

                    // Get thumbnail string image
                    using (MemoryStream thumbStream = new MemoryStream())
                    {
                        if (bitmapImg == null)
                        {
                            using (img)
                            {
                                thumbnailImage = img.GetThumbnailImage(thumbWidth, thumbHeight, null, IntPtr.Zero);
                                using (thumbnailImage)
                                {
                                    thumbnailImage.Save(thumbStream, ImageFormat.Jpeg);
                                    var base64Thumb = Convert.ToBase64String(thumbStream.ToArray());
                                    stringThumbnail = string.Format("data:{0};base64,{1}", mimeType, base64Thumb);
                                }
                            }
                        }
                        else
                        {
                            img.Dispose();
                            using (bitmapImg)
                            {
                                thumbnailImage = bitmapImg.GetThumbnailImage(thumbWidth, thumbHeight, null, IntPtr.Zero);
                                using (thumbnailImage)
                                {
                                    thumbnailImage.Save(thumbStream, ImageFormat.Jpeg);
                                    var base64Thumb = Convert.ToBase64String(thumbStream.ToArray());
                                    stringThumbnail = string.Format("data:{0};base64,{1}", mimeType, base64Thumb);
                                }
                            }
                        }
                    }

                    // Get string image
                    string stringImage;
                    using (imgStream)
                    {
                        stringImage = Convert.ToBase64String(imgStream.ToArray());
                    }
                    return string.Format("data:{0};base64,{1}", mimeType, stringImage);
                }
            }
            catch
            {
                dpi = 0;
                stringThumbnail = string.Empty;
                return string.Empty;
            }
        }


        #region Private methods

        //private static void CalculatePosition(Annotation anno,
        //                                      out float left, out float top, out float width, out float height)
        //{ 

        //}

        #endregion


        #region New code

        private static Bitmap Resize(Bitmap image, int width, int height)
        {
            // A holder for the result 
            Bitmap result = new Bitmap(width, height);

            // Use a graphics object to draw the resized image into the bitmap 
            using (Graphics graphics = Graphics.FromImage(result))
            {
                // Set the resize quality modes to high quality 
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;

                // Draw the image into the target bitmap 
                graphics.DrawImage(image, 0, 0, result.Width, result.Height);
            }

            return result;
        }

        private static void DrawAnnotation(Bitmap image, List<Annotation> annotations)
        {
            using (Graphics graphics = Graphics.FromImage(image))
            {
                // Create color background
                Brush brush = new SolidBrush(Utilities.GetRedactionBackground());

                // Draw redaction to image
                for (int i = 0; i < annotations.Count; i++)
                {
                    graphics.FillRectangle(brush,
                                           (int)(Math.Round(annotations[i].Left)),
                                           (int)(Math.Round(annotations[i].Top)),
                                           (int)(Math.Round(annotations[i].Width)),
                                           (int)(Math.Round(annotations[i].Height)));
                }
            }
        }

        public static int ToPixel(double location, double dpi)
        {
            return (int)(location * dpi) / 96;
        }

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
        #endregion

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
        public static Bitmap ResizeImage(System.Drawing.Image image, int width, int height)
        {
            //a holder for the result 
            Bitmap result = new Bitmap(width, height);

            //use a graphics object to draw the resized image into the bitmap 
            using (Graphics graphics = Graphics.FromImage(result))
            {
                //set the resize quality modes to high quality 
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
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