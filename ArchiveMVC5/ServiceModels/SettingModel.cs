

using System.Collections.Generic;
namespace ArchiveMVC5.Models
{
    public class SettingModel
    {
        public SettingModel()
        {
            Languages = new List<LanguageModel>();
            OCRCorrections = new List<AmbiguousDefinitionModel>();
        }

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

        public List<AmbiguousDefinitionModel> OCRCorrections { get; set; }
 
        public List<LanguageModel> Languages { get; set; }
    }
}
