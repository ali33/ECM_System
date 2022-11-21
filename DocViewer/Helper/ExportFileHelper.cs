using Ecm.Model;
using Ecm.DocViewer.Controls;
using Ecm.DocViewer.Model;
using Ecm.Mvvm;
using Ecm.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Resources;
using System.Reflection;

namespace Ecm.DocViewer.Helper
{
    public class ExportFileHelper
    {
        ResourceManager _resource = new ResourceManager("Ecm.DocViewer.ViewerContainer", Assembly.GetExecutingAssembly());
        private ViewerContainer ViewerContainer;
        public ExportFileHelper(ViewerContainer viewerContainer)
        {
            ViewerContainer = viewerContainer;
        }

        public void SendMail()
        {
            try
            {
                var emailHelper = new SendMailHelper(ViewerContainer.WorkingFolder);
                emailHelper.StartProcess += EmailHelperStartProcess;
                emailHelper.EndProcess += EmailHelperEndProcess;

                if (ViewerContainer.OpeningItem.PageData.FileType == FileTypeModel.Image)
                {
                    var dialogViewer = new DialogViewer { Width = 500, Height = 280, Text = _resource.GetString("AttachmentDialogTtitle") };
                    var rangeSelector = new SendMailOptionView(true)
                    {
                        Dialog = dialogViewer
                    };

                    dialogViewer.WpfContent = rangeSelector;
                    if (dialogViewer.ShowDialog() == DialogResult.OK)
                    {
                        string filename = Guid.NewGuid().ToString();

                        if (rangeSelector.IsXps)
                        {
                            var items = new List<CanvasElement>();
                            var canvasItems = ViewerContainer.OpeningItems.Where(p => !p.Image.IsNonImagePreview).Select(p => p.Image).ToList();
                            if (rangeSelector.SelectedPageIndexes == null || rangeSelector.SelectedPageIndexes.Count == 0)
                            {
                                foreach (CanvasElement item in canvasItems)
                                {
                                    CanvasElement clonedItem = item.Clone();
                                    clonedItem.EnableHideAnnotation = ViewerContainer.ImageViewer.EnableHideAnnotation;
                                    items.Add(clonedItem);
                                }
                            }
                            else
                            {
                                int itemCount = canvasItems.Count;
                                foreach (int pageIndex in rangeSelector.SelectedPageIndexes)
                                {
                                    if (pageIndex <= itemCount)
                                    {
                                        CanvasElement clonedItem = canvasItems[pageIndex - 1].Clone();
                                        clonedItem.EnableHideAnnotation = ViewerContainer.ImageViewer.EnableHideAnnotation;
                                        items.Add(clonedItem);
                                    }
                                }
                            }

                            emailHelper.AddAttachment(items, filename);
                        }
                        else
                        {
                            List<byte[]> files = new List<byte[]>();
                            List<ContentItem> items = null;
                            items = ViewerContainer.OpeningItems.Where(p => !p.Image.IsNonImagePreview).ToList();

                            if (rangeSelector.SelectedPageIndexes != null)
                            {
                                foreach (int index in rangeSelector.SelectedPageIndexes)
                                {
                                    ContentItem item = items[index - 1];
                                    if (item.PageData.FileExtension == "tiff")
                                    {
                                        files.Add(item.PageData.FileBinaries);
                                    }
                                    else
                                    {
                                        byte[] tiff = ImageProcessing.ConvertToTiff(item.PageData.FileBinaries, item.PageData.FileExtension);
                                        files.Add(tiff);
                                    }
                                }
                            }
                            else
                            {
                                foreach (ContentItem item in items)
                                {
                                    if (item.PageData.FileExtension == "tiff")
                                    {
                                        files.Add(item.PageData.FileBinaries);
                                    }
                                    else
                                    {
                                        byte[] tiff = ImageProcessing.ConvertToTiff(item.PageData.FileBinaries, item.PageData.FileExtension);
                                        files.Add(tiff);
                                    }
                                }
                            }

                            if (files.Count > 1)
                            {
                                byte[] mergeTiff = ImageProcessing.MergeTiffBytes(files, AppHelper.WorkingFolder.CreateTempFolder());
                                emailHelper.AddAttachment(mergeTiff, filename, ".tiff");
                            }
                            else if (files.Count == 1)
                            {
                                emailHelper.AddAttachment(files[0], filename, ".tiff");
                            }
                        }

                        emailHelper.SendMail();
                    }
                }
                else
                {
                    using (var stream = new FileStream(ViewerContainer.OpeningItem.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        byte[] binary = stream.ReadAllBytes();
                        emailHelper.AddAttachment(binary, Guid.NewGuid().ToString(), Path.GetExtension(ViewerContainer.OpeningItem.FilePath));
                    }

                    emailHelper.SendMail();
                }

                ViewerContainer.AddActionLog(new ActionLogModel { Message = "Send mail document(s) by user: " + ViewerContainer.UserName });

            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        public void SaveFile()
        {
            try
            {
                if (ViewerContainer.OpeningItem.PageData.FileType == FileTypeModel.Image)
                {
                    var dialogViewer = new DialogViewer { Width = 500, Height = 280, Text = _resource.GetString("SaveFileDialogTitle") };
                    var rangeSelector = new SendMailOptionView(false)
                    {
                        Dialog = dialogViewer
                    };

                    dialogViewer.WpfContent = rangeSelector;
                    if (dialogViewer.ShowDialog() == DialogResult.OK)
                    {
                        string filename = Guid.NewGuid().ToString();
                        DownloadFileHelper downloadHelper = new DownloadFileHelper(ViewerContainer.WorkingFolder);

                        if (rangeSelector.IsXps)
                        {
                            var items = new List<CanvasElement>();
                            var canvasItems = ViewerContainer.OpeningItems.Where(p => !p.Image.IsNonImagePreview).Select(p => p.Image).ToList();
                            if (rangeSelector.SelectedPageIndexes == null || rangeSelector.SelectedPageIndexes.Count == 0)
                            {
                                foreach (CanvasElement item in canvasItems)
                                {
                                    CanvasElement clonedItem = item.Clone();
                                    clonedItem.EnableHideAnnotation = ViewerContainer.ImageViewer.EnableHideAnnotation;
                                    items.Add(clonedItem);
                                }
                            }
                            else
                            {
                                int itemCount = canvasItems.Count;
                                foreach (int pageIndex in rangeSelector.SelectedPageIndexes)
                                {
                                    if (pageIndex <= itemCount)
                                    {
                                        CanvasElement clonedItem = canvasItems[pageIndex - 1].Clone();
                                        clonedItem.EnableHideAnnotation = ViewerContainer.ImageViewer.EnableHideAnnotation;
                                        items.Add(clonedItem);
                                    }
                                }
                            }

                            string extension = ".xps";
                            downloadHelper.FileName = DialogService.ShowSaveFileDialog(string.Format("{0} documents ({1})|*{1}", extension.Replace(".", string.Empty).ToUpper(), extension),
                                                                                       filename + extension);

                            if (!string.IsNullOrWhiteSpace(downloadHelper.FileName))
                            {
                                downloadHelper.Add(items, filename);
                            }
                        }
                        else
                        {
                            List<byte[]> files = new List<byte[]>();
                            List<ContentItem> items = null;
                            items = ViewerContainer.OpeningItems.Where(p => !p.Image.IsNonImagePreview).ToList();

                            if (rangeSelector.SelectedPageIndexes != null)
                            {
                                foreach (int index in rangeSelector.SelectedPageIndexes)
                                {
                                    ContentItem item = items[index - 1];
                                    if (item.PageData.FileExtension == "tiff")
                                    {
                                        files.Add(item.PageData.FileBinaries);
                                    }
                                    else
                                    {
                                        byte[] tiff = ImageProcessing.ConvertToTiff(item.PageData.FileBinaries, item.PageData.FileExtension);
                                        files.Add(tiff);
                                    }
                                }
                            }
                            else
                            {
                                foreach (ContentItem item in items)
                                {
                                    if (item.PageData.FileExtension == "tiff")
                                    {
                                        files.Add(item.PageData.FileBinaries);
                                    }
                                    else
                                    {
                                        byte[] tiff = ImageProcessing.ConvertToTiff(item.PageData.FileBinaries, item.PageData.FileExtension);
                                        files.Add(tiff);
                                    }
                                }
                            }

                            downloadHelper.FileName = DialogService.ShowSaveFileDialog(string.Format("{0} documents ({1})|*{1}", "tiff", ".tiff"),
                                                           filename + ".tiff");

                            if (!string.IsNullOrWhiteSpace(downloadHelper.FileName))
                            {
                                if (files.Count > 1)
                                {
                                    byte[] mergeTiff = ImageProcessing.MergeTiffBytes(files, AppHelper.WorkingFolder.CreateTempFolder());
                                    downloadHelper.Add(mergeTiff, filename, ".tiff");
                                }
                                else if (files.Count == 1)
                                {
                                    downloadHelper.Add(files[0], filename, ".tiff");
                                }
                            }
                        }

                        downloadHelper.Save();
                    }
                }
                else
                {
                    using (var stream = new FileStream(ViewerContainer.OpeningItem.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        byte[] binary = stream.ReadAllBytes();
                        string extension = Path.GetExtension(ViewerContainer.OpeningItem.FilePath).Replace(".", "");
                        string defaultName = Guid.NewGuid().ToString() + "." + extension;
                        string filePath = DialogService.ShowSaveFileDialog(string.Format("{0}|*.{1}", extension.ToUpper(), extension.ToLower()), defaultName);
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            File.WriteAllBytes(filePath, binary);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
        }

        private void EmailHelperEndProcess(object sender, EventArgs e)
        {
            ViewerContainer.IsProcessing = false;
            ViewerContainer.CollectGarbage();
        }

        private void EmailHelperStartProcess(object sender, EventArgs e)
        {
            ViewerContainer.IsProcessing = true;
        }
    }
}
