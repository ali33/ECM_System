using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Ecm.Mvvm;

namespace Ecm.Workflow.Activities.CustomActivityModel
{
    public class DocumentLookupInfoModel : BaseDependencyProperty
    {
        private ObservableCollection<LookupInfoModel> _lookupInfos = new ObservableCollection<LookupInfoModel>();

        public Guid DocumentTypeId { get; set; }

        public ObservableCollection<LookupInfoModel> LookupInfos
        {
            get { return _lookupInfos; }
            set
            {
                _lookupInfos = value;
                OnPropertyChanged("LookupInfos");
            }
        }
    }
}
