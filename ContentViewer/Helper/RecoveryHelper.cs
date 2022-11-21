using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Ecm.ContentViewer.Model;
using Ecm.ContentViewer.Model;
using Ecm.Utility;

namespace Ecm.ContentViewer.Helper
{
    public class RecoveryHelper
    {

        private const string _fileName = "BackupData.xml";
        private const string _rootNode = "Batch";

        public RecoveryHelper(ViewerContainer workspace)
        {
            ViewerContainer = workspace;
        }

        public ViewerContainer ViewerContainer { get; set; }

        public void Backup()
        {
            bool hasData = false;
            var recoveryInfo = new List<RecoveryBatch>();
            foreach (ContentItem item in ViewerContainer.Items)
            {
                var batchInfo = new RecoveryBatch { Version = 1, BatchData = item.BatchData };
                recoveryInfo.Add(batchInfo);

                if (item.Children != null && item.Children.Count > 0)
                {
                    List<ContentItem> loosePages = item.Children.Where(p => p.ItemType == ContentItemType.Page).ToList();
                    if (loosePages.Count > 0)
                    {
                        hasData = true;
                        batchInfo.Pages = new List<RecoveryPage>();
                        BackupPages(loosePages, batchInfo.Pages);
                    }

                    List<ContentItem> docs = item.Children.Where(p => p.ItemType == ContentItemType.ContentModel).ToList();
                    if (docs.Count > 0)
                    {
                        hasData = true;
                        batchInfo.Documents = new List<RecoveryDoc>();
                        BackupDoc(docs, batchInfo.Documents);
                    }
                }
            }

            if (hasData)
            {
                string xml = UtilsSerializer.Serialize(recoveryInfo);
                ViewerContainer.WorkingFolder.Save(xml, _fileName);
            }
        }

        public void Restore()
        {
            if (ViewerContainer.WorkingFolder.Exists(_fileName))
            {
                // THis case occurs when user close browser window (tab)
                RestoreWithBackupInfo();
                ViewerContainer.WorkingFolder.Delete(_fileName);
            }
            else
            {
                // This case occurs when system is crashed
                RestoreWithNoBackupInfo();
            }
        }

        private void BackupDoc(IEnumerable<ContentItem> docs, List<RecoveryDoc> result)
        {
            foreach (ContentItem doc in docs)
            {
                var docInfo = new RecoveryDoc { DocumentData = doc.DocumentData, Pages = new List<RecoveryPage>() };
                BackupPages(doc.Children.ToList(), docInfo.Pages);
                result.Add(docInfo);
            }
        }

        private void BackupPages(IEnumerable<ContentItem> pages, List<RecoveryPage> result)
        {
            result.AddRange(pages.Select(page => new RecoveryPage { FileName = page.FilePath, PageData = page.PageData }));
        }

        private void RestoreWithBackupInfo()
        {
            var recoveryInfo = UtilsSerializer.Deserialize<List<RecoveryBatch>>(ViewerContainer.WorkingFolder.ReadText(_fileName), _rootNode);
            if (recoveryInfo.Count > 0)
            {
                if (ViewerContainer.Items == null)
                {
                    ViewerContainer.Items = new ObservableCollection<ContentItem>();
                }
                else
                {
                    ViewerContainer.Items.Clear();
                }

                foreach (RecoveryBatch batch in recoveryInfo)
                {
                    // Get the associated batch profile
                    var batchType = ViewerContainer.BatchTypes.FirstOrDefault(p => p.Id == batch.BatchData.BatchType.Id);
                    if (batchType != null && batch.BatchData.CreatedBy == ViewerContainer.UserName)
                    {
                        batch.BatchData.BatchType = batchType;
                        var batchItem = new ContentItem(batch.BatchData);
                        var backupBatchFields = new List<FieldValueModel>(batch.BatchData.FieldValues);
                        batch.BatchData.FieldValues.Clear();
                        foreach (var metaField in batchType.Fields)
                        {
                            var field = new FieldValueModel { Field = metaField, Value = metaField.DefaultValue };
                            var backupField = backupBatchFields.FirstOrDefault(p => p.Field.Id == metaField.Id);
                            if (backupField != null)
                            {
                                field.Value = backupField.Value;
                            }

                            batch.BatchData.FieldValues.Add(field);
                        }

                        batchItem.SetBatchData(batch.BatchData);
                        if (batch.Pages != null && batch.Pages.Count > 0)
                        {
                            foreach (RecoveryPage page in batch.Pages)
                            {
                                if (ViewerContainer.WorkingFolder.Exists(page.FileName))
                                {
                                    var pageItem = new ContentItem(page.PageData, page.FileName);
                                    batchItem.Children.Add(pageItem);
                                }
                            }
                        }

                        if (batch.Documents != null && batch.Documents.Count > 0)
                        {
                            foreach (RecoveryDoc doc in batch.Documents)
                            {
                                // Get the associated document profile
                                var docType = batchType.DocTypes.FirstOrDefault(p => p.Id == doc.DocumentData.DocumentType.Id);
                                if (docType != null)
                                {
                                    doc.DocumentData.DocumentType = docType;
                                    var docItem = new ContentItem(doc.DocumentData);
                                    var backupDocFields = new List<FieldValueModel>(doc.DocumentData.FieldValues);
                                    doc.DocumentData.FieldValues.Clear();
                                    foreach (var metaField in docType.Fields)
                                    {
                                        var field = new FieldValueModel { Field = metaField, Value = metaField.DefaultValue };
                                        var backupField = backupDocFields.FirstOrDefault(p => p.Field.Id == metaField.Id);
                                        if (backupField != null)
                                        {
                                            field.Value = backupField.Value;
                                        }

                                        doc.DocumentData.FieldValues.Add(field);
                                    }

                                    batchItem.Children.Add(docItem);
                                    docItem.SetDocumentData(doc.DocumentData);
                                    foreach (RecoveryPage page in doc.Pages)
                                    {
                                        if (ViewerContainer.WorkingFolder.Exists(page.FileName))
                                        {
                                            var pageItem = new ContentItem(page.PageData, page.FileName);
                                            docItem.Children.Add(pageItem);
                                        }
                                    }

                                    if (docItem.Children.Count == 0)
                                    {
                                        batchItem.Children.Remove(docItem);
                                    }
                                }
                            }
                        }

                        if (batchItem.Children.Count > 0)
                        {
                            ViewerContainer.Items.Add(batchItem);
                            batchItem.IsChanged = true;
                            ViewerContainer.IsChanged = true;
                        }
                    }
                }

                if (ViewerContainer.Items.Count > 0)
                {
                    ViewerContainer.InitializeDefaultSelection();
                }
            }

            try
            {
                File.Delete(Path.Combine(ViewerContainer.WorkingFolder.Dir, _fileName));
            }
            catch { }
        }

        private void RestoreWithNoBackupInfo()
        {
            string[] files = ViewerContainer.WorkingFolder.GetVisibleFiles();
            if (files.Length > 0)
            {
                if (ViewerContainer.Items == null)
                {
                    ViewerContainer.Items = new ObservableCollection<ContentItem>();
                }
                else
                {
                    ViewerContainer.Items.Clear();
                }

                ViewerContainer.IsChanged = true;

                // Try to recovery with the best order
                List<string> sortedFiles = files.OrderBy(File.GetCreationTime).ToList();

                var firstBatchType = ViewerContainer.BatchTypes[0];
                ViewerContainer.Items.Add(new ContentItem(new BatchModel(Guid.Empty, DateTime.Now, ViewerContainer.UserName, firstBatchType)));

                foreach (string file in sortedFiles)
                {
                    var pageItem = new ContentItem(file);
                    ViewerContainer.Items[0].Children.Add(pageItem);
                }
            }
        }
    }
}