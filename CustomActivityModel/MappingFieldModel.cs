using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using System.Xml.Serialization;
using System.Collections.ObjectModel;

namespace Ecm.Workflow.Activities.CustomActivityModel
{
    [Serializable()]
    [XmlRoot("MappingModel")]
    public class MappingFieldModel : BaseDependencyProperty
    {
        [NonSerialized()]
        private string _CaptureField;
        [NonSerialized()]
        private string _archiveField;
        [NonSerialized()]
        private Guid _CaptureFieldId;
        [NonSerialized()]
        private Guid _archiveFieldId;
        [NonSerialized()]
        private ObservableCollection<MappingFieldModel> _columnMappings = new ObservableCollection<MappingFieldModel>();

        public string CaptureField
        {
            get { return _CaptureField; }
            set
            {
                _CaptureField = value;
                OnPropertyChanged("CaptureField");
            }
        }

        public string ArchiveField
        {
            get { return _archiveField; }
            set
            {
                _archiveField = value;
                OnPropertyChanged("ArchiveField");
            }
        }

        public Guid CaptureFieldId
        {
            get { return _CaptureFieldId; }
            set
            {
                _CaptureFieldId = value;
                OnPropertyChanged("CaptureFieldId");
            }
        }

        public Guid ArchiveFieldId
        {
            get { return _archiveFieldId; }
            set
            {
                _archiveFieldId = value;
                OnPropertyChanged("ArchiveFieldId");
            }
        }

        [XmlArray("ColumnMappingModels"), XmlArrayItem(typeof(MappingFieldModel), ElementName = "MappingColumnModels")]
        public ObservableCollection<MappingFieldModel> ColumnMappings
        {
            get { return _columnMappings; }
            set
            {
                _columnMappings = value;
                OnPropertyChanged("ColumnMappings");
            }
        }
    }
}
