using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using Ecm.CaptureModel;

namespace Ecm.Capture
{
    public class TaskMenuItem : BaseDependencyProperty
    {
        private BatchTypeModel _batchType;
        private TaskType _type;
        private bool _isSelected;
        private long _counts;

        public BatchTypeModel BatchType
        {
            get { return _batchType; }
            set
            {
                _batchType = value;
                OnPropertyChanged("BatchType");
            }
        }

        public TaskType Type
        {
            get { return _type; }
            set
            {
                _type = value;
                OnPropertyChanged("Type");
            }
        }

        public string Name { get; set; }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        public long Counts
        {
            get { return _counts; }
            set
            {
                _counts = value;
                OnPropertyChanged("Counts");
            }
        }

    }

    public enum TaskType
    {
        None,
        InProcessing,
        Error,
        Locked,
        Available,
        Rejected
    }
}
