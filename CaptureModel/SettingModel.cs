using Ecm.Mvvm;

namespace Ecm.CaptureModel
{
    public class SettingModel : BaseDependencyProperty
    {
        private string _serverWorkingFolder;
        private bool _enabledBarcode;
        private bool _enabledOCR;
        private int _searchResultPageSize;
        private bool _isSaveFileInFolder;

        public int SearchResultPageSize
        {
            get { return _searchResultPageSize; }
            set
            {
                _searchResultPageSize = value;
                OnPropertyChanged("SearchResultPageSize");
            }
        }

        public string ServerWorkingFolder
        {
            get
            {
                return _serverWorkingFolder;
            }
            set
            {
                _serverWorkingFolder = value;
                OnPropertyChanged("ServerWorkingFolder");
            }
        }

        public bool EnabledBarcodeClient
        {
            get { return _enabledBarcode; }
            set
            {
                _enabledBarcode = value;
                OnPropertyChanged("EnabledBarcodeClient");
            }
        }

        public bool EnabledOCRClient
        {
            get { return _enabledOCR; }
            set
            {
                _enabledOCR = value;
                OnPropertyChanged("EnabledOCRClient");
            }
        }

        public bool IsSaveFileInFolder
        {
            get { return _isSaveFileInFolder; }
            set
            {
                _isSaveFileInFolder = value;
                OnPropertyChanged("IsSaveFileInFolder");
            }
        }
    }
}
