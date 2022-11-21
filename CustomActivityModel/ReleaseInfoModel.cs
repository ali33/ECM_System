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
    public class ReleaseInfoModel : BaseDependencyProperty
    {
        [NonSerialized()]
        private LoginInfoModel _loginInfo;
        [NonSerialized()]
        private ObservableCollection<MappingModel> _mappingInfos = new ObservableCollection<MappingModel>();

        public LoginInfoModel LoginInfoModel
        {
            get { return _loginInfo; }
            set
            {
                _loginInfo = value;
                OnPropertyChanged("LoginInfoModel");
            }
        }

        [XmlArray("MappingInfos"), XmlArrayItem(typeof(MappingModel), ElementName = "MappingModel")]
        public ObservableCollection<MappingModel> MappingInfos
        {
            get { return _mappingInfos; }
            set
            {
                _mappingInfos = value;
                OnPropertyChanged("MappingInfos");
            }
        }

    }
}
