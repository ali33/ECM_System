using System;
using System.Linq;
using Ecm.Mvvm;
using Ecm.Model;
using System.Collections.ObjectModel;
using Ecm.DocViewer.Model;

namespace Ecm.Audit.ViewModel
{
    public class DocumentViewModel : ComponentViewModel
    {

        public DocumentViewModel(DocumentModel document)
        {
            Document = document;
            Initialize();
            OpenDocumentVersion();
        }

        public DocumentModel Document { get; private set; }

        public ObservableCollection<BatchTypeModel> BatchTypes { get; private set; }

        public ObservableCollection<DocumentTypeModel> DocumentTypes { get; private set; }

        public ObservableCollection<ContentItem> Items { get; private set; }

        public string UserName
        {
            get { return LoginViewModel.LoginUser.Username; }
        }


        private new void Initialize()
        {
            if (Document.DocumentType.AnnotationPermission == null)
            {
                Document.DocumentType.AnnotationPermission = new AnnotationPermissionModel
                {
                    AllowedAddHighlight = false,
                    AllowedAddRedaction = false,
                    AllowedAddText = false,
                    AllowedDeleteHighlight = false,
                    AllowedDeleteRedaction = false,
                    AllowedDeleteText = false,
                    AllowedHideRedaction = true,
                    AllowedSeeHighlight = true,
                    AllowedSeeText = true,
                };
            }
            else
            {
                Document.DocumentType.AnnotationPermission.AllowedAddHighlight = false;
                Document.DocumentType.AnnotationPermission.AllowedAddRedaction = false;
                Document.DocumentType.AnnotationPermission.AllowedAddText = false;
                Document.DocumentType.AnnotationPermission.AllowedDeleteHighlight = false;
                Document.DocumentType.AnnotationPermission.AllowedDeleteRedaction = false;
                Document.DocumentType.AnnotationPermission.AllowedDeleteText = false;
                Document.DocumentType.AnnotationPermission.AllowedHideRedaction = false;
                Document.DocumentType.AnnotationPermission.AllowedSeeHighlight = true;
                Document.DocumentType.AnnotationPermission.AllowedSeeText = true;
            }

            if (Document.DocumentType.DocumentTypePermission == null)
            {
                Document.DocumentType.DocumentTypePermission = new DocumentTypePermissionModel
                {
                    AllowedAppendPage = false,
                    AllowedCapture = false,
                    AllowedChangeDocumentType = false,
                    AllowedDeletePage = false,
                    AllowedDownloadOffline = false,
                    AllowedHideAllAnnotation = false,
                    AllowedReOrderPage = false,
                    AllowedReplacePage = false,
                    AllowedRotatePage = false,
                    AllowedSearch = true,
                    AllowedSeeRetrictedField = true,
                    AllowedSplitDocument = false,
                    AllowedUpdateFieldValue = false,
                    AlowedPrintDocument = true
                };
            }
            else
            {
                Document.DocumentType.DocumentTypePermission.AllowedAppendPage = false;
                Document.DocumentType.DocumentTypePermission.AllowedCapture = false;
                Document.DocumentType.DocumentTypePermission.AllowedChangeDocumentType = false;
                Document.DocumentType.DocumentTypePermission.AllowedDeletePage = false;
                Document.DocumentType.DocumentTypePermission.AllowedDownloadOffline = false;
                Document.DocumentType.DocumentTypePermission.AllowedHideAllAnnotation = false;
                Document.DocumentType.DocumentTypePermission.AllowedReOrderPage = false;
                Document.DocumentType.DocumentTypePermission.AllowedReplacePage = false;
                Document.DocumentType.DocumentTypePermission.AllowedRotatePage = false;
                Document.DocumentType.DocumentTypePermission.AllowedSearch = true;
                Document.DocumentType.DocumentTypePermission.AllowedSeeRetrictedField = true;
                Document.DocumentType.DocumentTypePermission.AllowedSplitDocument = false;
                Document.DocumentType.DocumentTypePermission.AllowedUpdateFieldValue = false;
                Document.DocumentType.DocumentTypePermission.AlowedPrintDocument = true;

            }

            BatchTypes = new ObservableCollection<BatchTypeModel>
                                {
                                    new BatchTypeModel
                                        {
                                            Id = Guid.Empty, 
                                            Name = "Default batch", 
                                            DocumentTypes = new ObservableCollection<DocumentTypeModel>{Document.DocumentType}
                                        }
                                };
            Items = new ObservableCollection<ContentItem>
                        {
                            new ContentItem(new BatchModel(Guid.Empty, DateTime.Now, UserName, BatchTypes[0]))
                        };
        }

        private void OpenDocumentVersion()
        {
            try
            {
                foreach (var field in Document.DocumentType.Fields)
                {
                    var fieldValue = Document.FieldValues.FirstOrDefault(p => p.Field.Id == field.Id);
                    if (fieldValue == null)
                    {
                        Document.FieldValues.Add(new FieldValueModel { Field = field, Value = string.Empty });
                    }
                }

                var docItem = new ContentItem(Document);
                Items[0].Children.Add(docItem);
                foreach (var page in Document.Pages)
                {
                    var pageItem = new ContentItem(page, page.FileBinaries);
                    docItem.Children.Add(pageItem);
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

    }
}
