using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Xml.Serialization;

using Ecm.Mvvm;
using System.Resources;
using System.Reflection;

namespace Ecm.Workflow.Activities.CustomActivityModel
{
    public class DocumentTypeModel : BaseDependencyProperty
    {
        private string _name;
        private bool _isSelected;
        private ObservableCollection<FieldModel> _fields = new ObservableCollection<FieldModel>();

        public Guid Id { get; set; }

        public Guid BatchTypeId { get; set; }

        [XmlIgnore]
        public string UniqueId { get; set; }

        [XmlIgnore]
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        [XmlIgnore]
        public ObservableCollection<FieldModel> Fields
        {
            get { return _fields; }
            set
            {
                _fields = value;
                OnPropertyChanged("Fields");
            }
        }

        [XmlIgnore]
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }
    }
}
