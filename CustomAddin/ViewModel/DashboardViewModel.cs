using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Model;
using Ecm.Model.DataProvider;
using System.Collections.ObjectModel;
using Microsoft.Office.Interop.Excel;
using Ecm.Mvvm;

namespace Ecm.CustomAddin.ViewModel
{
    public class DashboardViewModel : ComponentViewModel
    {
        private PageModel _pageModel;
        private DocumentModel _docModel;
        private bool _canUpdateIndexValue;
        private DocumentProvider _documentProvider = new DocumentProvider();
        private LookupProvider _lookupProvider = new LookupProvider();
        private DocumentVersionProvider _docVersionProvider = new DocumentVersionProvider();
 
        public DashboardViewModel(DocumentModel docModel, PageModel pageModel)
        {
            _pageModel = pageModel;
            _docModel = docModel;
            InitData();
        }

        private System.Data.DataTable LookupData(FieldMetaDataModel fieldMetaData, string fieldValue)
        {
            try
            {
                return _lookupProvider.GetLookupData(fieldMetaData.LookupInfo, fieldValue);
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }

            return null;
        }

        public ObservableCollection<FieldValueModel> FieldValues { get; set; }

        public ObservableCollection<ActionLogModel> ActionLogs { get; set; }

        public ObservableCollection<PageModel> OtherFiles { get; set; }

        public ObservableCollection<DocumentVersionModel> Versions{ get; set; }

        public bool CanUpdateIndexValue
        {
            get { return _docModel != null && _docModel.DocumentType.DocumentTypePermission.AllowedUpdateFieldValue; }
            set
            {
                _canUpdateIndexValue = value;
                OnPropertyChanged("CanUpdateIndexValue");
            }
        }

        public Func<FieldMetaDataModel, string, System.Data.DataTable> GetLookupData;

        private void InitData()
        {
            if (_docModel != null && _pageModel != null)
            {

                foreach (var field in _docModel.DocumentType.Fields)
                {
                    if (_docModel.FieldValues.Any(f => f.Field.Id == field.Id))
                    {
                        continue;
                    }

                    if (field.IsSystemField)
                    {
                        continue;
                    }

                    FieldValueModel fieldValue = new FieldValueModel();

                    fieldValue.Field = field;

                    if (_docModel.FieldValues == null)
                    {
                        _docModel.FieldValues = new List<FieldValueModel>();
                    }

                    _docModel.FieldValues.Add(fieldValue);
                }

                FieldValues = new ObservableCollection<FieldValueModel>(_docModel.FieldValues);
                OtherFiles = new ObservableCollection<PageModel>(_docModel.Pages.Where(p => p.Id != _pageModel.Id));
            }

            GetLookupData = LookupData;

            try
            {
                ActionLogs = new ObservableCollection<ActionLogModel>(new ActionLogProvider().GetActionLogByDocument(_docModel.Id));
                Versions = new ObservableCollection<DocumentVersionModel>(_docVersionProvider.GetDocumentVersions(_docModel.Id));
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }
    }
}
