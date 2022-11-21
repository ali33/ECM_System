using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Ocr;
using System.Drawing;
using System.ComponentModel;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace Ecm.ContentExtractor
{
    public class ImageExtractor
    {
        public string OcrText { get; set; }
        private const string _tessDataDirName = "TessData";
        private BackgroundWorker ocrWorker;
        public string TessDataDir { get; set; }
        public Dictionary<string, string> AmbiguousDefinition { get; set; }

        public ImageExtractor()
        {
            TessDataDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Remove(0,6), _tessDataDirName);
        }

        public void ExtractText(List<Bitmap> images, string language)
        {
            OCRImages ocr = new OCRImages();

            ocrWorker = new BackgroundWorker();
            ocrWorker.RunWorkerCompleted += OCRWorkerRunWorkerCompleted;
            ocrWorker.DoWork += OCRWorkerDoWork;
            var parames = new List<object> { images, language};
            ocrWorker.RunWorkerAsync(parames);
        }

        public void ExtractText(List<string> files, string language)
        {
            OCRFiles ocr = new OCRFiles();

            var ocrWorker = new BackgroundWorker();
            ocrWorker.RunWorkerCompleted += OCRToFileWorkerRunWorkerCompleted;
            ocrWorker.DoWork += OCRToFileWorkerDoWork;
            var parames = new List<object> { files, language };
            ocrWorker.RunWorkerAsync(parames);
        }

        private void OCRWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            var para = e.Argument as List<object>;
            List<Bitmap> images = para[0] as List<Bitmap>;
            var language = para[1] as string;
            OCR<Image> ocrEngine = new OCRImages(TessDataDir, language);
            string ocredData = string.Empty;

            foreach (Bitmap image in images)
            {
                var deskewUtil = new Deskew(image);
                var skewedImage = Deskew.RotateImage(image, deskewUtil.GetSkewAngle());
                string result = ocrEngine.RecognizeText(new List<Image> { skewedImage }, language, Rectangle.Empty, sender as BackgroundWorker, e);
                if (string.IsNullOrEmpty(result))
                {
                    ocrEngine.PSM = OCR<Image>.PSM_SINGLE_LINE;
                    result = ocrEngine.RecognizeText(new List<Image> { image }, language, Rectangle.Empty, sender as BackgroundWorker, e);
                }

                if (AmbiguousDefinition != null && !string.IsNullOrEmpty(result))
                {
                    result = Processor.PostProcess(result, language, AmbiguousDefinition);
                }

                ocredData += result;
                image.Dispose();
            }

            e.Result = ocredData;
        }

        private void OCRWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                OcrText = e.Result as string;
            }
        }

        private void OCRToFileWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            var para = e.Argument as List<object>;
            List<string> files = para[0] as List<string>;
            var language = para[1] as string;
            OCR<string> ocrEngine = new OCRFiles();
            var ocredData = new Dictionary<long, string>();

            foreach (string file in files)
            {
                //var deskewUtil = new Deskew(image);
                //var skewedImage = Deskew.RotateImage(image, deskewUtil.GetSkewAngle());
                string result = ocrEngine.RecognizeText(new List<string>(){ file }, language, Rectangle.Empty, sender as BackgroundWorker, e);

                //if (string.IsNullOrEmpty(result))
                //{
                //    ocrEngine.PSM = OCR<Image>.PSM_SINGLE_LINE;
                //    result = ocrEngine.RecognizeText(new List<Image> {image}, language, Rectangle.Empty, sender as BackgroundWorker, e);
                //}

                if (AmbiguousDefinition != null && !string.IsNullOrEmpty(result))
                {
                    result = Processor.PostProcess(result, language, AmbiguousDefinition);
                }

            }
        }

        private void OCRToFileWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                OcrText = e.Result as string;
            }
        }
    }
}
