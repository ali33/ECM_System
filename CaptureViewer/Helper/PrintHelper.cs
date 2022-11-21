using System;
using System.Collections.Generic;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Windows.Xps;
using System.Windows.Xps.Serialization;
using Ecm.AppHelper;
using Ecm.CaptureViewer.Controls;
using PageOrientation = System.Printing.PageOrientation;

namespace Ecm.CaptureViewer.Helper
{
    /// <summary>
    ///   The purpose of this class is to ....
    /// </summary>
    public class PrintHelper
    {
        private readonly int _maxPage;
        private readonly string _printJobName;
        private readonly List<string> _tmpFileNames = new List<string>();
        private readonly WorkingFolder _workingFolderHelper;
        private PrintDialog _printDialog;
        private Dictionary<int, PageOrientation> _pageOrientations = new Dictionary<int, PageOrientation>();

        public const string _tmpFileName = "__tmp_image_page___{0}.tif";
        public PrintHelper(int maxPage, string printJobName, WorkingFolder helper)
        {
            _maxPage = maxPage;
            _printJobName = printJobName;
            _workingFolderHelper = helper;
        }

        public PrintHelper(string printJobName, WorkingFolder helper)
            : this(0, printJobName, helper)
        {
        }

        public event EventHandler EndPrint;

        public event EventHandler StartPrint;

        public Action<Exception> HandleException;

        public void Print(List<CanvasElement> canvasList)
        {
            try
            {
                _pageOrientations = new Dictionary<int, PageOrientation>();
                if (StartPrint != null)
                {
                    StartPrint(this, EventArgs.Empty);
                }

                _printDialog = new PrintDialog();
                if (_maxPage > 0)
                {
                    _printDialog.UserPageRangeEnabled = true;
                    _printDialog.PageRangeSelection = PageRangeSelection.UserPages;
                    _printDialog.MinPage = 1;
                    _printDialog.MaxPage = Convert.ToUInt32(_maxPage);
                    _printDialog.PageRange = new PageRange(1, _maxPage);
                }

                if (_printDialog.ShowDialog().Value)
                {
                    int from = 0;
                    int to = canvasList.Count - 1;

                    if (_printDialog.PageRangeSelection == PageRangeSelection.UserPages)
                    {
                        from = _printDialog.PageRange.PageFrom - 1;
                        to = _printDialog.PageRange.PageTo - 1;
                    }

                    var document = new FixedDocument();
                    int sequence = 1;
                    for (int i = from; i <= to; i++)
                    {
                        CanvasElement item = canvasList[i];
                        var page = new FixedPage();
                        _pageOrientations.Add(sequence, item.Width > item.Height ? PageOrientation.Landscape : PageOrientation.Portrait);

                        sequence++;
                        page.Children.Add(CreateImage(item));
                        page.Width = item.Width;
                        page.Height = item.Height;
                        var size = new Size(item.Width, item.Height);
                        page.Measure(size);
                        page.Arrange(new Rect(size));
                        page.UpdateLayout();

                        var pageContent = new PageContent();
                        ((IAddChild)pageContent).AddChild(page);
                        document.Pages.Add(pageContent);
                    }

                    _printDialog.PrintQueue.CurrentJobSettings.Description = _printJobName;
                    XpsDocumentWriter writer = PrintQueue.CreateXpsDocumentWriter(_printDialog.PrintQueue);
                    writer.WritingPrintTicketRequired += PrintTicketRequired;
                    writer.Write(document.DocumentPaginator);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            finally
            {
                // Delete all temporary files
                foreach (string fileName in _tmpFileNames)
                {
                    _workingFolderHelper.Delete(fileName);
                }

                if (EndPrint != null)
                {
                    EndPrint(this, EventArgs.Empty);
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

        private Image CreateImage(CanvasElement item)
        {
            string tempFileName = FileHelper.CreateOnePageTiff(item, _workingFolderHelper);
            _tmpFileNames.Add(tempFileName);

            var tmpPath = new Uri("file:///" + tempFileName, UriKind.Absolute);
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.UriSource = tmpPath;
            bitmapImage.EndInit();
            var image = new Image {Source = bitmapImage, Width = item.Width, Height = item.Height};

            return image;
        }

    }
}