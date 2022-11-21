using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;

namespace Ecm.ContentViewer.Model
{
    public class ContentFieldPermissionModel : BaseDependencyProperty
    {
        private bool _canRead;
        private bool _canWrite;
        private bool _hidden = true;

        public Guid Id { get; set; }

        public Guid FieldId { get; set; }

        public Guid UserGroupId { get; set; }

        public Guid DocTypeId { get; set; }

        public bool CanRead
        {
            get { return _canRead; }
            set
            {
                _canRead = value;
                OnPropertyChanged("CanRead");
            }
        }

        public bool CanWrite
        {
            get { return _canWrite; }
            set
            {
                _canWrite = value;
                OnPropertyChanged("CanWrite");
            }
        }

        public bool Hidden
        {
            get { return _hidden; }
            set
            {
                _hidden = value;
                OnPropertyChanged("Hidden");
            }
        }

        public ContentTypeModel DocType { get; set; }

        public FieldModel Field { get; set; }
    }
}
