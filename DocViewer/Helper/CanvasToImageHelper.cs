using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;
using System.Windows.Controls;

namespace Ecm.DocViewer.Helper
{
    public enum ImageFormats
    {
        JPG, BMP, PNG, GIF, TIF
    }

    public class CanvasToImageHelper
    {
        private ObservableCollection<BitmapSource> imagecollection;
        public ObservableCollection<BitmapSource> ImageCollection
        {
            get
            {
                this.imagecollection = this.imagecollection ?? new ObservableCollection<BitmapSource>();
                return this.imagecollection;
            }
        }

        public RenderTargetBitmap RenderVisaulToBitmap(Visual vsual, int width, int height)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Default);
            rtb.Render(vsual);

            BitmapSource bsource = rtb;
            this.ImageCollection.Add(bsource);
            return rtb;
        }

        public MemoryStream GenerateImage(Visual vsual, int widhth, int height, ImageFormats format)
        {
            BitmapEncoder encoder = null;

            switch (format)
            {
                case ImageFormats.JPG:
                    encoder = new JpegBitmapEncoder();
                    break;
                case ImageFormats.PNG:
                    encoder = new PngBitmapEncoder();
                    break;
                case ImageFormats.BMP:
                    encoder = new BmpBitmapEncoder();
                    break;
                case ImageFormats.GIF:
                    encoder = new GifBitmapEncoder();
                    break;
                case ImageFormats.TIF:
                    encoder = new TiffBitmapEncoder();
                    break;

            }

            if (encoder == null) return null;

            RenderTargetBitmap rtb = this.RenderVisaulToBitmap(vsual, widhth, height);
            MemoryStream file = new MemoryStream();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            encoder.Save(file);

            return file;
        }

        public void SaveCanvasToImageFile(InkCanvas inkCanvas, string fileName)
        {
            using (MemoryStream memstream = GenerateImage(inkCanvas, (int)inkCanvas.ActualWidth, (int)inkCanvas.ActualHeight, ImageFormats.JPG))
            {
                if (memstream != null)
                {
                    //SaveFileDialog fdlg = new SaveFileDialog
                    //{
                    //    DefaultExt = "jpg",
                    //    Title = "Choose  filename and location",
                    //    Filter = "*Jpeg files|.jpg|Bmp Files|*.bmp|PNG Files|*.png|Tiff Files|*.tif|Gif Files|*.gif"
                    //};

                    //bool? result = fdlg.ShowDialog();

                    //if (result.HasValue && result.Value)
                    //{
                    using (FileStream fstream = File.OpenWrite(fileName))
                    {
                        memstream.WriteTo(fstream);
                        fstream.Flush();
                        fstream.Close();
                    }
                    //}
                }
            }
        }

    }
}
