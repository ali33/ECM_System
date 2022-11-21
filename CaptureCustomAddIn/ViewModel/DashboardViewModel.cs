using Ecm.CaptureModel;
using Ecm.CaptureModel.DataProvider;
using Ecm.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;

namespace Ecm.CaptureCustomAddIn.ViewModel
{
    public class DashboardViewModel : BaseDependencyProperty
    {
        private bool _isShowTableDetail;
        private bool _canUpdateIndexValue;
        private BatchModel _workitem;
        private DocumentModel _activeDocument;
        private PageModel _activePage;
        private LookupProvider _lookupProvider = new LookupProvider();
        private ActionLogProvider _actionLogProvider = new ActionLogProvider();

        public DashboardViewModel(BatchModel workitem, DocumentModel activeDocument, PageModel activePage)
        {
            _workitem = workitem;
            _activeDocument = activeDocument;
            _activePage = activePage;
            InitializeData();
        }

        public bool IsShowTableDetail
        {
            get { return _isShowTableDetail; }
            set
            {
                _isShowTableDetail = value;
                OnPropertyChanged("IsShowTableDetail");
            }
        }

        public ObservableCollection<CommentModel> Comments { get; set; }

        public ObservableCollection<FieldValueModel> FieldValues { get; set; }

        public ObservableCollection<FieldValueModel> BatchFieldValues { get; set; }

        public Func<FieldModel, string, System.Data.DataTable> LookupData;

        //Private method
        private DataTable GetLookupData(FieldModel fieldMetaData, string fieldValue)
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

        private void InitializeData()
        {
            FieldValues = new ObservableCollection<FieldValueModel>(_activeDocument.FieldValues.Where(p => !p.Field.IsSystemField));
            List<DocumentFieldPermissionModel> fieldPermissions = _activeDocument.DocumentPermission.FieldPermissions.ToList();
            foreach (FieldValueModel fieldValue in FieldValues)
            {
                DocumentFieldPermissionModel fieldPermission = fieldPermissions.FirstOrDefault(p => p.FieldId == fieldValue.FieldId);
                fieldValue.IsHidden = fieldPermission.Hidden && fieldValue.Field.IsRestricted;
                fieldValue.IsReadOnly = fieldPermission.CanRead && !fieldPermission.CanWrite && !fieldValue.Field.IsRequired;
                fieldValue.IsWrite = fieldPermission.CanWrite;
            }

            BatchFieldValues = new ObservableCollection<FieldValueModel>(_workitem.FieldValues.Where(p => !p.Field.IsSystemField));

            foreach (FieldValueModel fieldValue in BatchFieldValues)
            {
                fieldValue.IsWrite = true;
                fieldValue.IsReadOnly = false;
                fieldValue.IsHidden = false;
            }

            LookupData = GetLookupData;
            Comments = _workitem.Comments;

            try
            {
                foreach (CommentModel comment in Comments)
                {
                    comment.User = new UserProvider().GetUser(comment.CreatedBy);
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }
    }
}
