using Ecm.Mvvm;

namespace Ecm.Model
{
    public class SettingModel : BaseDependencyProperty
    {
        private int _maxSearchRows;
        private int _searchResultItemPerPage;
        private string _serverWorkingFolder;
        private string _luceneFolder;

        public int MaxSearchRows
        {
            get { return _maxSearchRows; }
            set
            {
                _maxSearchRows = value;
                OnPropertyChanged("MaxSearchRows");
            }
        }

        public int SearchResultItemPerPage
        {
            get { return _searchResultItemPerPage; }
            set
            {
                _searchResultItemPerPage = value;
                OnPropertyChanged("SearchResultItemPerPage");
            }
        }

        public string ServerWorkingFolder
        {
            get { return _serverWorkingFolder; }
            set
            {
                _serverWorkingFolder = value;
                OnPropertyChanged("ServerWorkingFolder");
            }
        }

        public string LuceneFolder
        {
            get { return _luceneFolder; }
            set
            {
                _luceneFolder = value;
                OnPropertyChanged("LuceneFolder");
            }
        }
    }
}
