using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ecm.AppHelper;
using Ecm.DocViewer.Controls;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace Ecm.DocViewer.Helper
{
    public class FileHelper
    {
        public static void ConvertToPdf(List<string> pageFiles, string destFile)
        {
            var document = new PdfDocument();
            foreach (string file in pageFiles)
            {
                XImage image = XImage.FromFile(file);
                PdfPage page = document.AddPage();
                page.Width = XUnit.FromInch(image.PixelWidth / image.HorizontalResolution);
                page.Height = XUnit.FromInch(image.PixelHeight / image.VerticalResolution);
                XGraphics graphic = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);
                graphic.DrawImage(image, 0, 0);
                graphic.Dispose();
                image.Dispose();
                page.Close();
            }

            using (FileStream stream = File.Open(destFile, FileMode.Create))
            {
                document.Save(stream);
            }
        }

        public static void ConvertToPdf(List<CanvasElement> items, string destFile, WorkingFolder helper)
        {
            // Keep temp tiff files to delete after finishing
            List<string> tmpFileNames = null;

            try
            {
                var document = new PdfDocument();
                tmpFileNames = new List<string>();

                foreach (CanvasElement item in items)
                {
                    CanvasElement itemCanvas;
                    string tempFileName = null;

                    CanvasElement item1 = item;
                    Application.Current.Dispatcher.Invoke(
                        new Action(
                            delegate
                            {
                                // Create an item canvas to populate annotations and direction of page
                                itemCanvas = item1.Clone();
                                // Create TIFF file
                                tempFileName = CreateOnePageTiff(itemCanvas, helper);
                                tmpFileNames.Add(tempFileName);
                            }));

                    var tmpPath = new Uri("file:///" + tempFileName, UriKind.Absolute);
                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.UriSource = tmpPath;
                    bitmapImage.EndInit();

                    XImage image = XImage.FromBitmapSource(bitmapImage);
                    PdfPage page = document.AddPage();
                    page.Width = XUnit.FromInch(image.PixelWidth / image.HorizontalResolution);
                    page.Height = XUnit.FromInch(image.PixelHeight / image.VerticalResolution);
                    XGraphics graphic = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);
                    graphic.DrawImage(image, 0, 0);
                    graphic.Dispose();
                    image.Dispose();
                    page.Close();
                }

                using (FileStream stream = File.Open(destFile, FileMode.Create))
                {
                    document.Save(stream);
                }

                document.Dispose();
            }
            finally
            {
                if (tmpFileNames != null)
                {
                    foreach (string fileName in tmpFileNames)
                    {
                        helper.Delete(fileName);
                    }
                }
            }
        }

        public static void CreateMultipageTiff(List<CanvasElement> items, string destFile, WorkingFolder helper)
        {
            // Keep temp tiff files to delete after finishing
            List<string> tmpFileNames = null;

            try
            {
                tmpFileNames = new List<string>();

                foreach (CanvasElement item in items)
                {
                    CanvasElement itemCanvas;
                    string tempFileName;

                    CanvasElement item1 = item;
                    Application.Current.Dispatcher.Invoke(
                        new Action(
                            delegate
                            {
                                // Create an item canvas to populate annotations and direction of page
                                itemCanvas = item1.Clone();

                                // Create TIFF file
                                tempFileName = CreateOnePageTiff(itemCanvas, helper);
                                tmpFileNames.Add(tempFileName);
                            }));
                }

                using (var fs = new FileStream(destFile, FileMode.Create))
                {
                    var tiffEncoder = new TiffBitmapEncoder { Compression = TiffCompressOption.Lzw };

                    foreach (string fileName in tmpFileNames)
                    {
                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.UriSource = new Uri("file:///" + fileName, UriKind.Absolute);
                        bitmapImage.EndInit();

                        var fcb = new FormatConvertedBitmap(bitmapImage, PixelFormats.Pbgra32, BitmapPalettes.WebPaletteTransparent, 1.0);
                        tiffEncoder.Frames.Add(BitmapFrame.Create(fcb));
                    }

                    tiffEncoder.Save(fs);
                }
            }
            finally
            {
                if (tmpFileNames != null)
                {
                    foreach (string fileName in tmpFileNames)
                    {
                        helper.Delete(fileName);
                    }
                }
            }
        }

        public static string CreateOnePageTiff(CanvasElement pageItem, WorkingFolder helper)
        {
            var encoder = CreateEncoder(pageItem);
            return helper.Save(encoder);
        }

        public static byte[] CreateOnePageTiff(CanvasElement pageItem)
        {
            var encoder = CreateEncoder(pageItem);
            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                return stream.ToArray();
            }
        }

        private static TiffBitmapEncoder CreateEncoder(CanvasElement pageItem)
        {
            pageItem.Measure(new Size(pageItem.Width, pageItem.Height));
            pageItem.Arrange(new Rect(new Size(pageItem.Width, pageItem.Height)));
            pageItem.Effect = null;

            var width = (int)pageItem.Width;
            var heigth = (int)pageItem.Height;
            double dpiX = 96;
            double dpiY = 96;

            if (pageItem.DpiX < 96)
            {
                width = (int)(pageItem.Width * pageItem.DpiX / 96);
                heigth = (int)(pageItem.Height * pageItem.DpiY / 96);
                dpiX = pageItem.DpiX;
                dpiY = pageItem.DpiY;
            }

            var rtb = new RenderTargetBitmap(width, heigth, dpiX, dpiY, PixelFormats.Pbgra32);
            var dv = new DrawingVisual();

            using (DrawingContext ctx = dv.RenderOpen())
            {
                var vb = new VisualBrush(pageItem);
                ctx.DrawRectangle(vb, null, new Rect(new Size(pageItem.Width, pageItem.Height)));
            }

            rtb.Render(dv);
            rtb.Freeze();

            var encoder = new TiffBitmapEncoder { Compression = TiffCompressOption.Lzw };
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            return encoder;
        }
    }
}