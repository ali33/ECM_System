using Ecm.Mvvm;

namespace ArchiveMVC.Models
{
    public class SettingModel
    {
        private int _maxSearchRows;
        private int _searchResultItemPerPage;
        private string _serverWorkingFolder;

        public int MaxSearchRows
        {
            get { return _maxSearchRows; }
            set
            {
                _maxSearchRows = value;
            }
        }

        public int SearchResultItemPerPage
        {
            get { return _searchResultItemPerPage; }
            set
            {
                _searchResultItemPerPage = value;
            }
        }

        public string ServerWorkingFolder
        {
            get { return _serverWorkingFolder; }
            set
            {
                _serverWorkingFolder = value;
            }
        }
    }
}
