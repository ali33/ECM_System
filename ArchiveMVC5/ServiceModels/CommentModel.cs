using System;

namespace ArchiveMVC5.Models
{
    public class CommentModel
    {
        public DateTime CreateDate { get; set; }

        public string Message { get; set; }

        public string UserName { get; set; }
    }
}