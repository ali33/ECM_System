using System;
using System.Collections.Generic;
using Ecm.Mvvm;

namespace ArchiveMVC.Models
{
    [Serializable]
    public class PageModel
    {
        private Guid _id;
        private Guid _docId;
        private int _pageNumber;
        private string _fileExtension;
        private byte[] _fileBinaries;
        private string _fileHash;

        public PageModel()
        {
            Annotations = new List<AnnotationModel>();
            ContentLanguageCode = "eng";
        }

        public Guid Id
        {
            get { return _id; }
            set
            {
                _id = value;
            }
        }

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

        public byte[] FileHeader { get; set; }

        public string FilePath { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

        public FileFormatModel FileFormat { get; set; }

        public FileTypeModel FileType { get; set; }

        public double RotateAngle { get; set; }

        public List<AnnotationModel> Annotations { get; set; }

        public string ContentLanguageCode { get; set; }

        public string Content { get; set; }

        public string OriginalFileName { get; set; }
    }
}
