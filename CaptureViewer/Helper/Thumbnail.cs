using System.Collections.Generic;
using System.Linq;
using Ecm.CaptureViewer.Model;
using Ecm.CaptureModel;
using System;
using Ecm.CaptureDomain;

namespace Ecm.CaptureViewer.Helper
{
    public class Thumbnail
    {
        public Thumbnail(DocThumbnail thumbnail)
        {
            DocThumbnail = thumbnail;
        }

        public DocThumbnail DocThumbnail { get; private set; }

        public bool IsDragDropProcessing { get; set; }

        public void DropContentItems(SingleItemList<ContentItem> selectedItems, ContentItem target)
        {
            if (!DocThumbnail.ViewerContainer.PermissionManager.CanModifiedDocument())
            {
                return;
            }

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
                        DocThumbnail.ViewerContainer.ThumbnailSelector.RemoveContentItem(p);
                    }
                }
                else
                {
                    var removeIndex = targetContainer.Children.IndexOf(p);
                    targetContainer.Children.RemoveAt(removeIndex);
                    insertIndex = insertIndex > removeIndex ? insertIndex - 1 : insertIndex;
                    targetContainer.Children.Insert(insertIndex, p);
                    targetContainer.DocumentData.Pages.Insert(insertIndex, p.PageData);
                }
            });

            // Step 2: Move pages which doesn't belong to items in step 1
            var notMovedPages = selectedItems.Where(p => p.ItemType == ContentItemType.Page && 
                                                         !sortedDocs.Any(r => r.Children.Any(q => q == p))).ToList();
            var sortedPages = DocThumbnail.ViewerContainer.ContentItemManager.SortPages(notMovedPages).Reverse();

            foreach(ContentItem p in sortedPages)
            {
                if (p.Parent == targetContainer)
                {
                    targetContainer.ChangeType |= ChangeType.ReOrderPage;
                    var removeIndex = targetContainer.Children.IndexOf(p);


                    targetContainer.Children.RemoveAt(removeIndex);
                    insertIndex = insertIndex > removeIndex ? insertIndex - 1 : insertIndex;
                    targetContainer.Children.Insert(insertIndex, p);

                    if (targetContainer.DocumentData != null && targetContainer.DocumentData.Pages != null && targetContainer.DocumentData.Pages.Count > 0)
                    {
                        targetContainer.DocumentData.Pages.RemoveAt(removeIndex);
                        targetContainer.DocumentData.Pages.Insert(insertIndex, p.PageData);
                    }

                }
                else
                {
                    p.ChangeType |= ChangeType.NewPage;
                    var tempParent = p.Parent;
                    tempParent.Children.Remove(p);

                    if (tempParent.DocumentData != null && tempParent.DocumentData.Pages != null)
                    {
                        tempParent.DocumentData.Pages.Remove(p.PageData);

                        if (p.PageData.Id != Guid.Empty)
                        {
                            tempParent.DocumentData.DeletedPages.Add(p.PageData.Id);
                        }
                    }

                    targetContainer.Children.Insert(insertIndex, p);

                    if (targetContainer.DocumentData != null && targetContainer.DocumentData.Pages != null)
                    {
                        p.PageData.DocId = targetContainer.DocumentData.Id;
                        p.PageData.Id = Guid.Empty;
                        targetContainer.DocumentData.Pages.Insert(insertIndex, p.PageData);
                    }

                    // Delete document if this document has no item
                    if (tempParent.Children.Count == 0 && tempParent.ItemType == ContentItemType.Document)
                    {
                        if (tempParent.Parent != null)
                        {
                            DocThumbnail.ViewerContainer.ThumbnailSelector.RemoveContentItem(tempParent);
                            tempParent.Parent.Children.Remove(tempParent);

                            if (tempParent.Parent.BatchData != null && tempParent.DocumentData != null && tempParent.DocumentData.Id != Guid.Empty)
                            {
                                tempParent.Parent.BatchData.Documents.Remove(tempParent.DocumentData);
                                tempParent.Parent.BatchData.DeletedDocuments.Add(tempParent.DocumentData.Id);
                            }
                        }

                        if (tempParent.DocumentData.Id != Guid.Empty)
                        {
                            tempParent.BatchItem.ChangeType |= ChangeType.DeleteDocument;
                            tempParent.BatchItem.DeletedDocuments.Add(tempParent);
                            tempParent.Parent.BatchData.Documents.Remove(tempParent.DocumentData);
                            tempParent.Parent.BatchData.DeletedDocuments.Add(tempParent.DocumentData.Id);
                        }
                    }
                    else if (p.PageData.Id != Guid.Empty)
                    {
                        tempParent.ChangeType |= ChangeType.DeletePage;
                        tempParent.DeletedPages.Add(p);
                        tempParent.DocumentData.DeletedPages.Add(p.PageData.Id);
                    }
                }
            }

            DocThumbnail.ViewerContainer.DisplayItem();
        }

        public void DropFilesFromLocalMachine(string[] files, ContentItem target, int insertIndex)
        {
            DocThumbnail.ViewerContainer.ImportManager.DropFilesFromLocalMachine(files, target, insertIndex);
        }
    }
}
