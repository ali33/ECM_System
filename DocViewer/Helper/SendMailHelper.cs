using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Xps;
using System.Windows.Xps.Serialization;
using System.Printing;
using System.Windows;
using System.Windows.Xps.Packaging;
using System.IO;
using Ecm.AppHelper;
using Ecm.DocViewer.Controls;
using Ecm.Utility;

namespace Ecm.DocViewer.Helper
{
    /// <summary>
    ///   The purpose of this class is to ....
    /// </summary>
    public class SendMailHelper
    {
        public const string _extension = ".xps";
        public const string _tmpFileName = "__tmp_image_page___{0}.tif";
        public Action<Exception> HandleException;
        private readonly Dictionary<string, List<CanvasElement>> _attachments = new Dictionary<string, List<CanvasElement>>();
        private readonly List<string> _distinctFileNames = new List<string>();
        private readonly Dictionary<string, byte[]> _fileAttachments = new Dictionary<string, byte[]>();
        private readonly List<string> _tmpFileNames = new List<string>();
        private readonly WorkingFolder _workingFolderHelper;
        private readonly UtilsMapi _mapi = new UtilsMapi();
        private Dictionary<int, PageOrientation> _pageOrientations = new Dictionary<int, PageOrientation>();
        
        public SendMailHelper(WorkingFolder helper)
        {
            _workingFolderHelper = helper;
        }

        public event EventHandler EndProcess;

        public event EventHandler StartProcess;

        public void AddAttachment(List<CanvasElement> canvasList, string fileName)
        {
            _attachments.Add(GenerateUniqueFileName(fileName, _extension), canvasList);
        }

        public void AddAttachment(List<CanvasElement> canvasList, string fileName, string extension)
        {
            _attachments.Add(GenerateUniqueFileName(fileName, extension), canvasList);
        }

        public void AddAttachment(byte[] binary, string fileName, string extension)
        {
            _fileAttachments.Add(GenerateUniqueFileName(fileName, extension), binary);
        }

        public void SendMail()
        {
            if (StartProcess != null)
            {
                StartProcess(this, EventArgs.Empty);
            }

            var worker = new BackgroundWorker();
            worker.DoWork += DoSendMail;
            worker.RunWorkerCompleted += DoSendMailCompleted;
            worker.RunWorkerAsync();
        }

        private void Clean()
        {
            if (_attachments.Count > 0)
            {
                foreach (string fileName in _attachments.Keys)
                {
                    _workingFolderHelper.Delete(fileName);
                }

                foreach (string fileName in _tmpFileNames)
                {
                    _workingFolderHelper.Delete(fileName);
                }
            }

            if (_fileAttachments.Count > 0)
            {
                foreach (string fileName in _fileAttachments.Keys)
                {
                    _workingFolderHelper.Delete(fileName);
                }
            }
        }

        private string GenerateUniqueFileName(string fileName, string extension)
        {
            // Rule: Limit the maximum characters of filename to 50
            if (fileName.Length > 50)
            {
                fileName = fileName.Substring(0, 50);
            }

            string tmpFileName = fileName;
            int index = 1;
            while (_distinctFileNames.Contains(tmpFileName + extension))
            {
                tmpFileName = fileName + string.Format(" ({0})", index);
                index++;
            }

            tmpFileName = tmpFileName + extension;
            _distinctFileNames.Add(tmpFileName);

            return tmpFileName;
        }

        private void DoSendMail(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (_fileAttachments.Count > 0)
                {
                    foreach (string fileName in _fileAttachments.Keys)
                    {
                        string filePath = _workingFolderHelper.Save(_fileAttachments[fileName], fileName);
                        _mapi.AddAttachment(filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void DoSendMailCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (e.Result is Exception)
                {
                    throw e.Result as Exception;
                }

                if (_attachments.Count > 0)
                {
                    foreach (string fileName in _attachments.Keys)
                    {
                        List<CanvasElement> canvases = _attachments[fileName];

                        if (canvases.Count > 0)
                        {
                            FixedDocument document = new FixedDocument();
                            int sequence = 1;
                            _pageOrientations.Clear();
                            foreach (CanvasElement item in canvases)
                            {
                                FixedPage page = new FixedPage();
                                if (item.Width > item.Height)
                                {
                                    _pageOrientations.Add(sequence, PageOrientation.Landscape);
                                }
                                else
                                {
                                    _pageOrientations.Add(sequence, PageOrientation.Portrait);
                                }
                                sequence++;
                                page.Children.Add(item.ItemContainer);
                                page.Width = item.Width;
                                page.Height = item.Height;
                                Size size = new Size(item.Width, item.Height);
                                page.Measure(size);
                                page.Arrange(new Rect(size));
                                page.UpdateLayout();

                                PageContent pageContent = new PageContent();
                                ((IAddChild)pageContent).AddChild(page);
                                document.Pages.Add(pageContent);
                            }
                            // we use the writer to write the fixed document to the xps document
                            string filePath = _workingFolderHelper.GetFullFileName(fileName);
                            if (File.Exists(filePath))
                            {
                                File.Delete(filePath);
                            }
                            XpsDocument xpsDocument = new XpsDocument(filePath, FileAccess.ReadWrite);
                            XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
                            writer.WritingPrintTicketRequired += PrintTicketRequired;
                            writer.Write(document);
                            xpsDocument.Close();
                            _mapi.AddAttachment(filePath);
                        }
                    }
                }

                _mapi.SendMailPopup(string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            finally
            {
                CollectGarbageHelper.CollectGarbage();
                Clean();

                if (EndProcess != null)
                {
                    EndProcess(this, EventArgs.Empty);
                }
            }
        }

        private void PrintTicketRequired(object sender, System.Windows.Documents.Serialization.WritingPrintTicketRequiredEventArgs e)
        {
            if (_pageOrientations.ContainsKey(e.Sequence) && e.CurrentPrintTicketLevel == PrintTicketLevel.FixedPagePrintTicket)
            {
                e.CurrentPrintTicket = new PrintTicket { PageOrientation = _pageOrientations[e.Sequence] };
            }
        }

    }
}