using Ecm.ContentViewer.Model;
using Ecm.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Ecm.ContentViewer.ViewModel
{
    public class IndexViewerViewModel : BaseDependencyProperty
    {
        private ObservableCollection<FieldValueModel> _fieldValues;
        private bool _canUpdateIndexValue;
        private bool _isShowTableDetail;
        private RelayCommand _previousDocumentCommand;
        private RelayCommand _nextDocumentCommand;

        public IndexViewerViewModel(MainViewerViewModel mainViewModel)
        {
            MainViewModel = mainViewModel;
        }

        public MainViewerViewModel MainViewModel { get; set; }

        public ObservableCollection<FieldValueModel> FieldValues
        {
            get { return _fieldValues; }
            set
            {
                _fieldValues = value;
                OnPropertyChanged("FieldValues");
            }
        }

        public bool CanUpdateIndexValue
        {
            get { return _canUpdateIndexValue; }
            set
            {
                _canUpdateIndexValue = value;
                OnPropertyChanged("CanUpdateIndexValue");
            }
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

        public List<ContentItem> IndexedItems { get; private set; }

        public ICommand PreviousDocumentCommand
        {
            get
            {
                if (_previousDocumentCommand == null)
                {
                    _previousDocumentCommand = new RelayCommand(p => Previous(), p => CanPrevious());
                }

                return _previousDocumentCommand;
            }
        }

        public ICommand NextDocumentCommand
        {
            get
            {
                if (_nextDocumentCommand == null)
                {
                    _nextDocumentCommand = new RelayCommand(p => Next(), p => CanNext());
                }

                return _nextDocumentCommand;
            }
        }


        //Private methods
        private void Next()
        {
            try
            {
                var allDocs = GetAllDocuments();
                var maxIndex = IndexedItems.Select(item => allDocs.IndexOf(item)).Max();
                MainViewModel.ThumbnailSelector.LeftMouseClick(allDocs[maxIndex + 1]);
                PopulateIndexPanel(MainViewModel.ThumbnailSelector.Cursor);
            }
            catch (Exception ex)
            {
                MainViewModel.HandleException(ex);
            }
        }

        private bool CanNext()
        {
            var allDocs = GetAllDocuments();
            if (allDocs != null && IndexedItems.Count > 0)
            {
                var maxIndex = IndexedItems.Select(item => allDocs.IndexOf(item)).Max();
                return maxIndex < allDocs.Count - 1;
            }
            else
            {
                return false;
            }
        }

        private bool CanPrevious()
        {
            var allDocs = GetAllDocuments();
            if (allDocs != null && IndexedItems.Count > 0)
            {
                var minIndex = IndexedItems.Select(item => allDocs.IndexOf(item)).Min();
                return minIndex > 0;
            }
            else
            {
                return false;
            }
        }

        private void Previous()
        {
            try
            {
                var allDocs = GetAllDocuments();
                var minIndex = IndexedItems.Select(item => allDocs.IndexOf(item)).Min();
                MainViewModel.ThumbnailSelector.LeftMouseClick(allDocs[minIndex - 1]);
                PopulateIndexPanel(MainViewModel.ThumbnailSelector.Cursor);
            }
            catch (Exception ex)
            {
                MainViewModel.HandleException(ex);
            }
        }

        private List<ContentItem> GetAllDocuments()
        {
            if (MainViewModel.Items != null && MainViewModel.Items.Count > 0)
            {
                return (from item in MainViewModel.Items
                        from subItem in item.Children
                        where subItem.ItemType == ContentItemType.ContentModel
                        select subItem).ToList();
            }

            return null;
        }

        private void ShowIndexPanel(List<FieldValueModel> fieldValues, int indexedItemCount)
        {
            if (indexedItemCount > 0)
            {
                FieldValues = new ObservableCollection<FieldValueModel>();
                foreach (FieldValueModel fieldValue in fieldValues)
                {
                    if (fieldValue.Field.IsSystemField)
                    {
                        continue;
                    }

                    var clonedFieldValue = new FieldValueModel
                    {
                        Field = fieldValue.Field,
                        ShowMultipleUpdate = indexedItemCount > 1,
                        Value = fieldValue.Value,
                        SnippetImage = fieldValue.SnippetImage,
                        IsHidden = fieldValue.Field.IsRestricted && fieldValue.IsHidden,
                        IsReadOnly = fieldValue.IsReadOnly && !fieldValue.Field.IsRequired,
                        IsWrite = fieldValue.IsWrite,
                        TableValues = fieldValue.TableValues,
                        MultipleUpdate = fieldValue.MultipleUpdate
                    };

                    FieldValues.Add(clonedFieldValue);

                    // Register event to listen the value of cloned index and then update back the selected documents/batches
                    clonedFieldValue.PropertyChanged += FieldValuePropertyChanged;
                }

                CanUpdateIndexValue = MainViewModel.PermissionManager.CanModifyIndex();
            }
            else
            {
                MainViewModel.FieldValues.Clear();
                IndexedItems.Clear();
            }

            //ShowIndexViewer(true);
        }

        private void FieldValuePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                // Update the value back the index of selection documetns/batches
                var clonedFieldValue = sender as FieldValueModel;
                if (clonedFieldValue != null && e.PropertyName == "Value")
                {
                    IndexedItems.ForEach(p =>
                    {
                        FieldValueModel fieldValue = p.ItemType == ContentItemType.ContentModel ? p.DocumentData.FieldValues.FirstOrDefault(q => q.Field.Name == clonedFieldValue.Field.Name) :
                                                                                         p.BatchData.FieldValues.FirstOrDefault(q => q.Field.Name == clonedFieldValue.Field.Name);
                        if (fieldValue != null &&
                            fieldValue.Value + string.Empty != clonedFieldValue.Value + string.Empty)
                        {
                            fieldValue.Value = clonedFieldValue.Value;
                            fieldValue.MultipleUpdate = clonedFieldValue.MultipleUpdate;
                            fieldValue.ShowMultipleUpdate = clonedFieldValue.ShowMultipleUpdate;

                            if (fieldValue.Field.DataType == FieldDataType.Table && clonedFieldValue.TableValues.Count > 0)
                            {
                                fieldValue.TableValues = clonedFieldValue.TableValues;
                            }

                            MainViewModel.IsChanged = true;
                            p.ChangeType |= ChangeType.UpdateIndex;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                MainViewModel.HandleException(ex);
            }
        }

        //Public methods

        public void PopulateIndexPanel(ContentItem item)
        {
            var fieldValues = new List<FieldValueModel>();
            var indexedItemCount = 0;
            var selectedItems = MainViewModel.ThumbnailSelector.SelectedItems;
            IndexedItems = new List<ContentItem>();
            
            if (item != null)
            {

                // Get first document
                ContentItem firstDocItem = null;

                if (item.ItemType == ContentItemType.Batch)
                {
                    if (selectedItems[0].Children.Count > 0)
                    {
                        firstDocItem = selectedItems[0].Children[0];
                    }
                }
                else
                {
                    if (item.ItemType == ContentItemType.ContentModel)
                    {
                        firstDocItem = selectedItems[0];
                    }
                    else
                    {
                        firstDocItem = selectedItems[0].Parent;
                    }
                }

                var allSelectedDocItems = selectedItems.Where(p => p.ItemType == ContentItemType.ContentModel &&
                                                                    p != firstDocItem &&
                                                                    p.DocumentData.DocumentType.Id == firstDocItem.DocumentData.DocumentType.Id).ToList();
                var allSelectedDocOfPageItems = selectedItems.Where(p => p.ItemType == ContentItemType.Page &&
                                                                            p.Parent != firstDocItem &&
                                                                            !allSelectedDocItems.Contains(p.Parent) &&
                                                                            p.Parent.DocumentData.DocumentType.Id == firstDocItem.DocumentData.DocumentType.Id).ToList();
                IndexedItems.Add(firstDocItem);
                IndexedItems.AddRange(allSelectedDocItems);
                IndexedItems.AddRange(allSelectedDocOfPageItems);
                indexedItemCount = IndexedItems.Count;
                fieldValues = IndexedItems[0].DocumentData.FieldValues;
            }

            ShowIndexPanel(fieldValues, indexedItemCount);
        }

        public void PopulateBatchIndexPanel(ContentItem item)
        {
            var fieldValues = new List<FieldValueModel>();
            var indexedItemCount = 0;
            var selectedItems = MainViewModel.ThumbnailSelector.SelectedItems;
            IndexedItems = new List<ContentItem>();

            if (item != null)
            {
                if (item.ItemType == ContentItemType.Batch)
                {
                    fieldValues = item.BatchData.FieldValues;
                }
                else
                {
                    if (item.ItemType == ContentItemType.Page)
                    {
                        if (item.Parent.ItemType == ContentItemType.ContentModel)
                        {
                            fieldValues = item.Parent.Parent.BatchData.FieldValues;
                        }
                        else
                        {
                            fieldValues = item.Parent.BatchData.FieldValues;
                        }
                    }
                    else
                    {
                        fieldValues = item.Parent.BatchData.FieldValues;
                    }
                }
                indexedItemCount = 1;
                IndexedItems.Add(item.BatchItem);
            }

            ShowIndexPanel(fieldValues, indexedItemCount);
        }
    }
}
