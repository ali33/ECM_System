using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CaptureMVC.Enums
{
    /// <summary>
    /// Emun chứa các trạng thái xoay của ảnh
    /// </summary>
    public enum RotateTypeEnum
    {
        /// <summary>
        /// Không xoay
        /// </summary>
        Rolate_0 = 0,

        /// <summary>
        /// Xoay 90 độ
        /// </summary>
        Rolate_90 = 1,
        
        /// <summary>
        /// Xoay 180 độ
        /// </summary>
        Rolate_180 = 2,

        /// <summary>
        /// Xoay 270 độ
        /// </summary>
        Rolate_270 = 3,

    }
}