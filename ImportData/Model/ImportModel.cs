using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecm.ImportData.ViewModel
{
    public class ImportModel
    {
        public ImportModel()
        {
            list = new ObservableCollection<ItemDocType>();
        }

        private ObservableCollection<ItemDocType> list;
        


    }
}
