using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ArchiveMVC.Models
{
    public class DocTypeResult
    {
        public DocumentTypeModel DocType;
        public string IconKey;
    }
    [Serializable]
    public class PageSerializable
    {
        [Required]
        public string ImgKey { set; get; }
        public List<AnnotationModel> Annotations { set; get; }
        public string LanguageCode { set; get; }
        public double PageWidth { set; get; }
        public double PageHeight { set; get; }
        public double RotateAngle { get; set; }
        public Guid PageId { get; set; }
    }

    [Serializable]
    public class FieldValueSerializable
    {
        [Required]
        public Guid Id { set; get; }
        [Required]
        public string Value { set; get; }

        public List<TableFieldValueSerializable> TableFieldValues { get; set; }
    }

    [Serializable]
    public class TableFieldValueSerializable
    {
        public Guid Id { get; set; }

        [Required]
        public Guid FieldId { get; set; }

        [Required]
        public int RowIndex { get; set; }

        public string Value { get; set; }

    }

    [Serializable]
    public class DocumentSerializable
    {
        [Required]
        public Guid DocumentTypeId { set; get; }
        [Required]
        public List<PageSerializable> Pages { set; get; }

        public List<FieldValueSerializable> FieldValues { set; get; }

        public List<string> GetImageKeys()
        {
            var rs = new List<string>();
            foreach (var p in Pages)
            {
                rs.Add(p.ImgKey);
            }
            return rs;
        }

        public Guid DocumentId { set; get; }

        public Guid TempId { get; set; }
    }

    [Serializable]
    public class DocumentCollection
    {
        [Required]
        public List<DocumentSerializable> Documents { set; get; }
        public List<string> GetImageKeys()
        {
            var rs = new List<string>();
            foreach (var d in Documents)
            {
                rs.AddRange(d.GetImageKeys());
            }
            return rs;
        }
    }

    [Serializable]
    public class SaveOption
    {
        [Required]
        public DocumentSerializable Document { set; get; }
        public string Format { set; get; }
        public string Range { set; get; }
        public List<int> Pages { set; get; }
        public string SendBy { set; get; }
        public string MailTo { set; get; }
        public List<string> CC { set; get; }
        public List<string> BCC { set; get; }
    }

    [Serializable]
    public class FieldSerializable
    {
        public Guid DocumentTypeId { get; set; }
        public Guid FieldId { get; set; }
    }

    [Serializable]
    public class DocumentFieldValueSerializable
    {
        public Guid DocumentId { get; set; }
        public Guid FieldId { get; set; }
    }

    public class JsonMessage
    {
        public int Code { set; get; }
        public String Message { set; get; }
    }

    public class MsgCode
    {
        public static int Error = 0;
        public static int Success = 1;
    }
}