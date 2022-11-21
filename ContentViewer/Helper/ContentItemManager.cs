using System;
using System.Collections.Generic;
using Ecm.ContentViewer.Model;
using System.Collections.ObjectModel;
using System.Net;
using Ecm.ContentViewer.ViewModel;

namespace Ecm.ContentViewer.Helper
{
    internal class ContentItemManager
    {
        public ContentItemManager(MainViewerViewModel viewerContainer)
        {
            ViewerContainer = viewerContainer;
        }

        public MainViewerViewModel ViewerContainer { get; private set; }

        public void CreateBatch(BatchTypeModel batchType)
        {
            if (ViewerContainer.Items == null)
            {
                ViewerContainer.Items = new ObservableCollection<ContentItem>();
            }

            BatchModel batch = new BatchModel(Guid.NewGuid(), DateTime.Now, ViewerContainer.UserName, batchType) { Permission = new BatchPermissionModel() };

            batch.Permission.CanAnnotate = batchType.BatchTypePermission.CanCapture;
            batch.Permission.CanChangeDocumentType = batchType.BatchTypePermission.CanCapture;
            batch.Permission.CanDelegateItems = false;
            batch.Permission.CanDelete = batchType.BatchTypePermission.CanCapture;
            batch.Permission.CanEmail = batchType.BatchTypePermission.CanCapture;
            batch.Permission.CanInsertDocument = batchType.BatchTypePermission.CanCapture;
            batch.Permission.CanModifyDocument = batchType.BatchTypePermission.CanCapture;
            batch.Permission.CanModifyIndexes = batchType.BatchTypePermission.CanCapture;
            batch.Permission.CanPrint = batchType.BatchTypePermission.CanCapture;
            batch.Permission.CanReject = false;
            batch.Permission.CanReleaseLoosePage = true;
            batch.Permission.CanSendLink = false;
            batch.Permission.CanSplitDocument = batchType.BatchTypePermission.CanCapture;

            ViewerContainer.Items.Add(new ContentItem(batch));
        }

        public void InsertPages(List<string> pageFiles, ContentItem targetContainer, int insertIndex)
        {
            foreach (var pageFile in pageFiles)
            {
                InsertPage(pageFile, targetContainer, insertIndex);
                insertIndex++;
            }
        }

        public void InsertPage(string pageFile, ContentItem targetContainer, int insertIndex)
        {
            var item = new ContentItem(pageFile);
            item.SetParent(targetContainer);
            item.Load(ViewerContainer);
            targetContainer.Children.Insert(insertIndex, item);
            item.ChangeType |= ChangeType.NewPage;
        }

        public ContentItem CreateDocument(List<string> pageFiles, ContentItem batch, ContentTypeModel documentType)
        {
            var docItem = new ContentItem(new ContentModel(DateTime.Now, ViewerContainer.UserName, documentType));
            batch.Children.Add(docItem);
            InsertPages(pageFiles, docItem, 0);
            docItem.ChangeType |= ChangeType.NewDocument;
            return docItem;
        }

        public ContentItem CreateDocument(string pageFile, ContentItem batch, ContentTypeModel documentType)
        {
            var docItem = new ContentItem(new ContentModel(DateTime.Now, ViewerContainer.UserName, documentType));
            batch.Children.Add(docItem);
            InsertPage(pageFile, docItem, 0);
            docItem.ChangeType |= ChangeType.NewDocument;
            return docItem;
        }

        public void ReplacePage(string pageFile, ContentItem targetPage)
        {
            var replaceIndex = targetPage.Parent.Children.IndexOf(targetPage);

            if (replaceIndex == -1)
            {
                return;
            }

            var newPage = new ContentItem(pageFile);
            newPage.SetParent(targetPage.Parent);
            newPage.Load(ViewerContainer);
            targetPage.Parent.Children[replaceIndex] = newPage;
            newPage.PageData.Id = targetPage.PageData.Id;
            ViewerContainer.ThumbnailSelector.RemoveContentItem(targetPage);
            newPage.ChangeType |= ChangeType.ReplacePage;
        }

        public void ReplacePages(List<string> pageFiles, ContentItem targetPage)
        {
            if (pageFiles.Count == 1)
            {
                ReplacePage(pageFiles[0], targetPage);
                return;
            }

            var replaceIndex = targetPage.Parent.Children.IndexOf(targetPage);

            if (replaceIndex == -1)
            {
                return;
            }

            var targetContainer = targetPage.Parent;
            targetPage.Parent.Children.RemoveAt(replaceIndex);
            targetPage.Parent.DeletedPages.Add(targetPage);
            targetPage.Parent.ChangeType |= ChangeType.DeletePage;
            ViewerContainer.ThumbnailSelector.RemoveContentItem(targetPage);
            InsertPages(pageFiles, targetContainer, replaceIndex);
        }

        public void InsertPageBefore(string pageFile, ContentItem targetPage)
        {
            var beforeIndex = targetPage.Parent.Children.IndexOf(targetPage);
            var targetContainer = targetPage.Parent;
            InsertPage(pageFile, targetContainer, beforeIndex);
        }

        public void InsertPagesBefore(List<string> pageFiles, ContentItem targetPage)
        {
            var beforeIndex = targetPage.Parent.Children.IndexOf(targetPage);
            var targetContainer = targetPage.Parent;
            InsertPages(pageFiles, targetContainer, beforeIndex);
        }

        public void InsertPageAfter(string pageFile, ContentItem targetPage)
        {
            var afterIndex = targetPage.Parent.Children.IndexOf(targetPage) + 1;
            var targetContainer = targetPage.Parent;
            InsertPage(pageFile, targetContainer, afterIndex);
        }

        public void InsertPagesAfter(List<string> pageFiles, ContentItem targetPage)
        {
            var afterIndex = targetPage.Parent.Children.IndexOf(targetPage) + 1;
            var targetContainer = targetPage.Parent;
            InsertPages(pageFiles, targetContainer, afterIndex);
        }

        public IEnumerable<ContentItem> SortPages(List<ContentItem> pages)
        {
            for (var i = 0; i < pages.Count - 1; i++)
            {
                for (var j = i + 1; j < pages.Count; j++)
                {
                    if (pages[j].Parent == pages[i].Parent)
                    {
                        if (pages[j].Parent.Children.IndexOf(pages[j]) < pages[i].Parent.Children.IndexOf(pages[i]))
                        {
                            var temp = pages[i];
                            pages[i] = pages[j];
                            pages[j] = temp;
                        }
                    }
                    else if (pages[j].Parent.ItemType == ContentItemType.ContentModel && pages[i].Parent.ItemType == ContentItemType.Batch)
                    {
                        if (pages[j].BatchItem.Children.IndexOf(pages[j].Parent) < pages[i].BatchItem.Children.IndexOf(pages[i]))
                        {
                            var temp = pages[i];
                            pages[i] = pages[j];
                            pages[j] = temp;
                        }
                    }
                    else if (pages[j].Parent.ItemType == ContentItemType.Batch && pages[i].Parent.ItemType == ContentItemType.ContentModel)
                    {
                        if (pages[j].BatchItem.Children.IndexOf(pages[j]) < pages[i].BatchItem.Children.IndexOf(pages[i].Parent))
                        {
                            var temp = pages[i];
                            pages[i] = pages[j];
                            pages[j] = temp;
                        }
                    }
                }
            }

            return pages;
        }
    }
}
