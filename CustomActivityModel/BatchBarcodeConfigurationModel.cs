using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;

namespace Ecm.Workflow.Activities.CustomActivityModel
{
    public class BatchBarcodeConfigurationModel: BaseDependencyProperty
    {
        private List<ReadActionModel> _readActions = new List<ReadActionModel>();
        private List<SeparationActionModel> _separationActions = new List<SeparationActionModel>();

        public List<ReadActionModel> ReadActions
        {
            get { return _readActions; }
            set
            {
                _readActions = value;
                OnPropertyChanged("ReadActions");
            }
        }

        public List<SeparationActionModel> SeparationActions
        {
            get { return _separationActions; }
            set
            {
                _separationActions = value;
                OnPropertyChanged("SeparationActions");
            }
        }

    }
}
