using System;

namespace Ecm.ExcelImport.Converter
{
    public class ImageConverter : System.Windows.Forms.AxHost
    {
        public ImageConverter()
            : base(Guid.Empty.ToString())
        {
        }

        public static stdole.IPictureDisp Convert(System.Drawing.Image image)
        {
            return (stdole.IPictureDisp)System.Windows.Forms.AxHost.GetIPictureDispFromPicture(image);
        }
    }
}
