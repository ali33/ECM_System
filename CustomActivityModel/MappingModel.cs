using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Ecm.Workflow.Activities.CustomActivityModel
{
    [Serializable()]
    [XmlRoot("ReleaseInfoModel")]
    public class MappingModel : BaseDependencyProperty
    {
        [NonSerialized()]
        private ObservableCollection<MappingFieldModel> _fieldMaps = new ObservableCollection<MappingFieldModel>();

        public Guid ReleaseDocumentTypeId { get; set; }

        public Guid CaptureDocumentTypeId { get; set; }

        [XmlArray("FieldMaps"), XmlArrayItem(typeof(MappingFieldModel), ElementName = "MappingFieldModel")]
        public ObservableCollection<MappingFieldModel> FieldMaps
        {
            get { return _fieldMaps; }
            set
            {
                _fieldMaps = value;
                OnPropertyChanged("FieldMaps");
            }
        }
    }
}
