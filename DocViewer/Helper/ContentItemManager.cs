using System;
using System.Collections.Generic;
using System.Linq;
using Ecm.DocViewer.Model;
using Ecm.Domain;
using Ecm.Model;

namespace Ecm.DocViewer.Helper
{
    internal class ContentItemManager
    {
        public ContentItemManager(ViewerContainer viewerContainer)
        {
            ViewerContainer = viewerContainer;
        }

        public ViewerContainer ViewerContainer { get; private set; }

        public void InsertPages(Dictionary<string, string> pageFiles, ContentItem targetContainer, int insertIndex)
        {
            foreach (var pageFile in pageFiles)
            {
                InsertPage(pageFile.Key, pageFile.Value, targetContainer, insertIndex);
                insertIndex++;
            }
        }

        public void InsertPage(string pageFile, string originalFileName, ContentItem targetContainer, int insertIndex)
        {
            var item = new ContentItem(pageFile, originalFileName);
            item.SetParent(targetContainer);
            item.Load(ViewerContainer);
            targetContainer.Children.Insert(insertIndex, item);
            item.ChangeType |= ChangeType.NewPage;
        }

        public ContentItem CreateDocument(Dictionary<string, string> pageFiles, ContentItem batch, DocumentTypeModel documentType)
        {
            var docItem = new ContentItem(new DocumentModel(DateTime.Now, ViewerContainer.UserName, documentType));
            batch.Children.Add(docItem);
            InsertPages(pageFiles, docItem, 0);
            docItem.ChangeType |= ChangeType.NewDocument;
            return docItem;
        }

        public ContentItem CreateDocument(string pageFile, string originalFileName, ContentItem batch, DocumentTypeModel documentType)
        {
            var docItem = new ContentItem(new DocumentModel(DateTime.Now, ViewerContainer.UserName, documentType));
            batch.Children.Add(docItem);
            InsertPage(pageFile, originalFileName, docItem, 0);
            docItem.ChangeType |= ChangeType.NewDocument;
            return docItem;
        }

        public void ReplacePage(string pageFile, string originalFileName, ContentItem targetPage)
        {
            var replaceIndex = targetPage.Parent.Children.IndexOf(targetPage);

            if (replaceIndex == -1)
            {
                return;
            }

            var newPage = new ContentItem(pageFile, originalFileName);
            newPage.SetParent(targetPage.Parent);
            newPage.Load(ViewerContainer);
            targetPage.Parent.Children[replaceIndex] = newPage;
            newPage.PageData.Id = targetPage.PageData.Id;
            ViewerContainer.ThumbnailSelector.RemoveItem(targetPage);
            newPage.ChangeType |= ChangeType.ReplacePage;
        }

        public void ReplacePages(Dictionary<string, string> pageFiles, ContentItem targetPage)
        {
            if (pageFiles.Count == 1)
            {
                var pair = pageFiles.First();
                ReplacePage(pair.Key, pair.Value, targetPage);
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
            ViewerContainer.ThumbnailSelector.RemoveItem(targetPage);
            InsertPages(pageFiles, targetContainer, replaceIndex);
        }

        public void InsertPageBefore(string pageFile, string originalFileName, ContentItem targetPage)
        {
            var beforeIndex = targetPage.Parent.Children.IndexOf(targetPage);
            var targetContainer = targetPage.Parent;
            InsertPage(pageFile, originalFileName, targetContainer, beforeIndex);
        }

        public void InsertPagesBefore(Dictionary<string, string> pageFiles, ContentItem targetPage)
        {
            var beforeIndex = targetPage.Parent.Children.IndexOf(targetPage);
            var targetContainer = targetPage.Parent;
            InsertPages(pageFiles, targetContainer, beforeIndex);
        }

        public void InsertPageAfter(string pageFile, string originalFileName, ContentItem targetPage)
        {
            var afterIndex = targetPage.Parent.Children.IndexOf(targetPage) + 1;
            var targetContainer = targetPage.Parent;
            InsertPage(pageFile, originalFileName, targetContainer, afterIndex);
        }

        public void InsertPagesAfter(Dictionary<string, string> pageFiles, ContentItem targetPage)
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
                    else if (pages[j].Parent.ItemType == ContentItemType.Document && pages[i].Parent.ItemType == ContentItemType.Batch)
                    {
                        if (pages[j].BatchItem.Children.IndexOf(pages[j].Parent) < pages[i].BatchItem.Children.IndexOf(pages[i]))
                        {
                            var temp = pages[i];
                            pages[i] = pages[j];
                            pages[j] = temp;
                        }
                    }
                    else if (pages[j].Parent.ItemType == ContentItemType.Batch && pages[i].Parent.ItemType == ContentItemType.Document)
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
