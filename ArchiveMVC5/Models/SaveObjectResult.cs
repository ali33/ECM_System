using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArchiveMVC5.Models
{
    public class SaveObjectResult
    {
        public SaveObjectResult()
        {
            ErrorMessages = new List<ErrorMessageModel>();
        }

        public Guid Id { get; set; }
        public List<ErrorMessageModel> ErrorMessages { get; set; }
    }

    public class ErrorMessageModel
    {
        public string FieldName { get; set; }
        public string Error { get; set; }
    }
}