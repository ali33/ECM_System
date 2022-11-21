using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Documents.Serialization;
using System.Windows.Markup;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using System.Windows.Xps.Serialization;
using Ecm.AppHelper;
using Ecm.CaptureViewer.Controls;

namespace Ecm.CaptureViewer.Helper
{
    public class DownloadFileHelper
    {
        public DownloadFileHelper(WorkingFolder helper)
        {
            _workingFolderHelper = helper;
        }

        public string FileName { get; set; }

        public string FolderName { get; set; }

        public void Add(List<CanvasElement> canvasList, string fileName)
        {
            _attachments.Add(GetUniqueFileName(fileName, _extension), canvasList);
        }

        public void Add(byte[] binary, string fileName, string extension)
        {
            _fileAttachments.Add(GetUniqueFileName(fileName, extension), binary);
        }

        public void Save()
        {
            try
            {
                CollectGarbageHelper.CollectGarbage();

                if (_fileAttachments.Count + _attachments.Count == 1)
                {
                    if (_fileAttachments.Count == 1)
                    {
                        SaveBinaryDocument(FileName, _fileAttachments[_fileAttachments.Keys.First()]);
                    }
                    else
                    {
                        SaveFileDocument(_attachments.Keys.First(), FileName);
                    }
                }
                else
                {
                    foreach (string fileName in _fileAttachments.Keys)
                    {
                        SaveBinaryDocument(fileName, _fileAttachments[fileName]);
                    }

                    foreach (string fileName in _attachments.Keys)
                    {
                        SaveFileDocument(fileName, fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        // Private methods
        private string GetUniqueFileName(string fileName, string extension)
        {
            // Rule: Limit the maximum characters of filename to 50
            if (fileName.Length > 50)
            {
                fileName = fileName.Substring(0, 50);
            }

            string tmpFileName = fileName;
            int index = 1;
            while (_distinctFileNames.Contains(FolderName + tmpFileName + extension))
            {
                tmpFileName = fileName + string.Format(" ({0})", index);
                index++;
            }

            tmpFileName = FolderName + tmpFileName + extension;
            _distinctFileNames.Add(tmpFileName);

            return tmpFileName;
        }

        private void SaveBinaryDocument(string fileName, byte[] buffer)
        {
            File.WriteAllBytes(fileName, buffer);
        }

        private void SaveFileDocument(string fileName, string outFileName)
        {
            try
            {
                List<CanvasElement> canvases = _attachments[fileName];
                _pageOrientations.Clear();
                if (canvases.Count > 0)
                {
                    FixedDocument document = new FixedDocument();
                    int sequence = 1;
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
                    if (File.Exists(outFileName))
                    {
                        File.Delete(outFileName);
                    }

                    XpsDocument xpsDocument = new XpsDocument(outFileName, FileAccess.ReadWrite);
                    XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
                    writer.WritingPrintTicketRequired += WriterWritingPrintTicketRequired;
                    writer.Write(document);

                    xpsDocument.Close();
                }
            }
            finally
            {
                CollectGarbageHelper.CollectGarbage();
            }
        }

        private void WriterWritingPrintTicketRequired(object sender, WritingPrintTicketRequiredEventArgs e)
        {
            if (_pageOrientations.ContainsKey(e.Sequence) && e.CurrentPrintTicketLevel == PrintTicketLevel.FixedPagePrintTicket)
            {
                e.CurrentPrintTicket = new PrintTicket { PageOrientation = _pageOrientations[e.Sequence] };
            }
        }

        public const string _extension = ".xps";
        public const string _tmpFileName = "__tmp_image_page___{0}.tif";
        public Action<Exception> HandleException;

        private readonly WorkingFolder _workingFolderHelper;
        private readonly Dictionary<string, List<CanvasElement>> _attachments = new Dictionary<string, List<CanvasElement>>();
        private readonly List<string> _distinctFileNames = new List<string>();
        private readonly Dictionary<string, byte[]> _fileAttachments = new Dictionary<string, byte[]>();
        private readonly List<string> _tmpFileNames = new List<string>();
        private Dictionary<int, PageOrientation> _pageOrientations = new Dictionary<int, PageOrientation>();
    }
}
