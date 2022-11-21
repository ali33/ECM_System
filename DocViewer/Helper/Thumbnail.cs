using System.Collections.Generic;
using System.Linq;
using Ecm.DocViewer.Model;
using Ecm.Domain;
using System;

namespace Ecm.DocViewer.Helper
{
    public class Thumbnail
    {
        public Thumbnail(DocThumbnail thumbnail)
        {
            DocThumbnail = thumbnail;
        }

        public DocThumbnail DocThumbnail { get; private set; }

        public bool IsDragDropInProcessing { get; set; }

        public void DropContentItems(SingleItemList<ContentItem> selectedItems, ContentItem target)
        {
            if (target == null || selectedItems == null || selectedItems.Count == 0)
            {
                return;
            }

            if (target.ItemType == ContentItemType.Batch && DocThumbnail.ViewerContainer.DocViewerMode == DocViewerMode.OCRTemplate)
            {
                return;
            }

            var targetContainer = target;
            var insertIndex = 0;

            if (target.ItemType == ContentItemType.Page)
            {
                targetContainer = target.Parent;
                insertIndex = targetContainer.Children.IndexOf(target) + 1;
            }

            // Step 1: Move documents in the selection
            var currentBatch = selectedItems[0].BatchItem;
            var sortedDocs = currentBatch.Children.Where(p => p.ItemType == ContentItemType.Document && selectedItems.Contains(p)).Reverse().ToList();
            sortedDocs.ForEach(p =>
            {
                if (p.Parent != targetContainer)
                {
                    if (targetContainer.ItemType == ContentItemType.Batch)
                    {
                        if (p.DocumentData.Id != Guid.Empty)
                        {
                            p.BatchItem.ChangeType |= ChangeType.DeleteDocument;
                            p.BatchItem.DeletedDocuments.Add(p);
                        }

                        p.Parent.Children.Remove(p);
                        targetContainer.Children.Insert(insertIndex, p);
                        p.ChangeType |= ChangeType.NewDocument; 
                    }
                    else
                    {
                        p.Children.Reverse().ToList().ForEach(r =>
                                                                  {
                                                                      var tempParent = r.Parent;
                                                                      tempParent.Children.Remove(r);
                                                                      targetContainer.Children.Insert(insertIndex, r);
                                                                      r.ChangeType |= ChangeType.NewPage;
                                                                  });
                        p.Parent.Children.Remove(p);
                        if (p.DocumentData.Id != Guid.Empty)
                        {
                            p.BatchItem.ChangeType |= ChangeType.DeleteDocument;
                            p.BatchItem.DeletedDocuments.Add(p);
                        }
                        selectedItems.Remove(p);
                        DocThumbnail.ViewerContainer.ThumbnailSelector.RemoveItem(p);
                    }
                }
                else
                {
                    var removeIndex = targetContainer.Children.IndexOf(p);
                    targetContainer.Children.RemoveAt(removeIndex);
                    insertIndex = insertIndex > removeIndex ? insertIndex - 1 : insertIndex;
                    targetContainer.Children.Insert(insertIndex, p);
                }
            });

            // Step 2: Move pages which doesn't belong to items in step 1
            var notMovedPages = selectedItems.Where(p => p.ItemType == ContentItemType.Page && 
                                                         !sortedDocs.Any(r => r.Children.Any(q => q == p))).ToList();
            var sortedPages = DocThumbnail.ViewerContainer.ContentItemManager.SortPages(notMovedPages).Reverse();

            foreach (var page in sortedPages)
            {
                if (page.Parent == targetContainer)
                {
                    targetContainer.ChangeType |= ChangeType.ReOrderPage;
                    var removeIndex = targetContainer.Children.IndexOf(page);

                    //if (targetContainer.DocumentData != null && targetContainer.DocumentData.Pages != null && targetContainer.DocumentData.Pages.Count > 0)
                    //{
                    //    targetContainer.DocumentData.Pages.RemoveAt(removeIndex);
                    //}

                    targetContainer.Children.RemoveAt(removeIndex);
                    insertIndex = insertIndex > removeIndex ? insertIndex - 1 : insertIndex;
                    targetContainer.Children.Insert(insertIndex, page);
                }
                else
                {
                    page.ChangeType |= ChangeType.NewPage;
                    var tempParent = page.Parent;
                    targetContainer.Children.Insert(insertIndex, page);
                    tempParent.Children.Remove(page);

                    // Delete document if this document has no item
                    if (tempParent.Children.Count == 0 && tempParent.ItemType == ContentItemType.Document)
                    {
                        if (tempParent.Parent != null)
                        {
                            DocThumbnail.ViewerContainer.ThumbnailSelector.RemoveItem(tempParent);
                            tempParent.Parent.Children.Remove(tempParent);
                        }

                        if (tempParent.DocumentData.Id != Guid.Empty)
                        {
                            tempParent.BatchItem.ChangeType |= ChangeType.DeleteDocument;
                            tempParent.BatchItem.DeletedDocuments.Add(tempParent);
                        }
                    }
                    else if (page.PageData.Id != Guid.Empty)
                    {
                        tempParent.ChangeType |= ChangeType.DeletePage;
                        tempParent.DeletedPages.Add(page);
                    }
                }
            }
            DocThumbnail.ViewerContainer.DisplayItem();
        }

        public void DropFilesFromMachine(string[] files, ContentItem target, int insertIndex)
        {
            DocThumbnail.ViewerContainer.ImportManager.DropFilesFromLocalMachine(files, target, insertIndex);
        }
    }
}
