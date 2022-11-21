using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Serialization;

using Ecm.Mvvm;
using System.Resources;
using System.Reflection;

namespace Ecm.Workflow.Activities.CustomActivityModel
{
    public class BatchTypeModel : BaseDependencyProperty
    {
        private Guid _id;
        private string _name;
        private bool _isSelected;
        private ObservableCollection<FieldModel> _fields = new ObservableCollection<FieldModel>();
        private ObservableCollection<DocumentTypeModel> _docTypes = new ObservableCollection<DocumentTypeModel>();

        public BatchTypeModel()
        {
        }

        public Guid Id { get; set; }

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
        public string UniqueId { get; set; }

        [XmlIgnore]
        public Guid WorkflowDefinitionId { get; set; }

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

        [XmlIgnore]
        public ObservableCollection<DocumentTypeModel> DocTypes
        {
            get { return _docTypes; }
            set
            {
                _docTypes = value;
                OnPropertyChanged("DocTypes");
            }
        }

    }
}
