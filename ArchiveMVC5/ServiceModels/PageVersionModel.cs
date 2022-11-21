using System.Collections.Generic;

using System;

namespace ArchiveMVC5.Models
{
    public class PageVersionModel
    {
        private Guid _docId;
        private int _pageNumber;
        private string _fileExtension;
        private byte[] _fileBinaries;
        private string _fileHash;

        public PageVersionModel()
        {
            Annotations = new List<AnnotationVersionModel>();
        }

        public Guid Id { get; set; }

        public Guid PageId { get; set; }

        public Guid DocVersionId { get; set; }

        public Guid DocId
        {
            get { return _docId; }
            set
            {
                _docId = value;
            }
        }

        public int PageNumber
        {
            get { return _pageNumber; }
            set
            {
                _pageNumber = value;
            }
        }

        public string FileExtension
        {
            get { return _fileExtension; }
            set
            {
                _fileExtension = value;
            }
        }

        public string FileHash
        {
            get { return _fileHash; }
            set
            {
                _fileHash = value;
            }
        }

        public byte[] FileBinaries
        {
            get { return _fileBinaries; }
            set
            {
                _fileBinaries = value;
            }
        }

        public double Width { get; set; }

        public double Height { get; set; }

        public FileFormatModel FileFormat { get; set; }

        public FileTypeModel FileType { get; set; }

        public double RotateAngle { get; set; }

        public List<AnnotationVersionModel> Annotations { get; set; }
    }
}
