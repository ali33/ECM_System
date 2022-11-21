using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OutlookCapture
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
