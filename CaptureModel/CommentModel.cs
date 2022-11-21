using System;
using Ecm.Mvvm;

namespace Ecm.CaptureModel
{
    public class CommentModel : BaseDependencyProperty
    {
        private DateTime _createdDate;
        private string _createdBy;
        private string _note;
        
        public Guid Id { get; set; }

        public Guid InstanceId { get; set; }

        public bool IsBatchId { get; set; }

        public string Note
        {
            get { return _note; }
            set
            {
                _note = value;
                OnPropertyChanged("Note");
            }
        }

        public DateTime CreatedDate
        {
            get { return _createdDate; }
            set
            {
                _createdDate = value;
                OnPropertyChanged("CreatedDate");
            }
        }

        public string CreatedBy
        {
            get { return _createdBy; }
            set
            {
                _createdBy = value;
                OnPropertyChanged("CreatedBy");
            }
        }

        public UserModel User { get; set; }
    }
}