using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ecm.Mvvm;

namespace Ecm.ContentViewer.Model
{
    public class PageModel : BaseDependencyProperty
    {
        public PageModel()
        {
            Annotations = new List<AnnotationModel>();
            ContentLanguageCode = "eng";
        }

        public Guid Id { get; set; }

        public Guid DocId { get; set; }

        /// <summary>
        /// The order of this page in a document
        /// </summary>
        public int PageNumber { get; set; }

        public string FileExtension { get; set; }

        public byte[] FileBinary { get; set; }

        public string FilePath { get; set; }

        /// <summary>
        /// The hash code which is used to track whether the file is changed
        /// </summary>
        public string FileHash { get; set; }

        /// <summary>
        /// The angle which this page is rotated
        /// </summary>
        public double RotateAngle { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

        public bool IsRejected { get; set; }

        public FileTypeModel FileType { get; set; }

        public FileFormatModel FileFormat { get; set; }

        public string ContentLanguageCode { get; set; }
        /// <summary>
        /// Collection of annotations are added on this page.
        /// </summary>
        public List<AnnotationModel> Annotations { get; set; }

        public string Content { get; set; }

        public byte[] FileHeader { get; set; }

        public string OriginalFileName { get; set; }
    }
}
