using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArchiveMVC5.Models
{
    public class PagingModel
    {
        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public int TotalPage { get; set; }

        public string SortColumnName { get; set; }

        public string SortDirection { get; set; }

        public int TotalRows { get; set; }

    }
}