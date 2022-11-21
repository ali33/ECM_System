using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Ecm.Mvvm;

namespace Ecm.Workflow.Activities.CustomActivityModel
{
    public class LookupConfigurationModel : BaseDependencyProperty
    {
        private ObservableCollection<LookupInfoModel> _batchLookups = new ObservableCollection<LookupInfoModel>();
        private ObservableCollection<DocumentLookupInfoModel> _documentLookups = new ObservableCollection<DocumentLookupInfoModel>();

        public ObservableCollection<LookupInfoModel> BatchLookups
        {
            get { return _batchLookups; }
            set
            {
                _batchLookups = value;
                OnPropertyChanged("BatchLookups");
            }
        }

        public ObservableCollection<DocumentLookupInfoModel> DocumentLookups
        {
            get { return _documentLookups; }
            set
            {
                _documentLookups = value;
                OnPropertyChanged("DocumentLookups");
            }
        }

    }
}
