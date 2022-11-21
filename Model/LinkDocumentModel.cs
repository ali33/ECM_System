using Ecm.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ecm.Model
{
    public class LinkDocumentModel : BaseDependencyProperty
    {
        public Guid Id { get; set; }

        public Guid DocumentId { get; set; }

        public Guid LinkedDocumentId { get; set; }

        public string Notes { get; set; }

        public DocumentModel RootDocument { get; set; }

        public DocumentModel LinkedDocument { get; set; }
    }
}
