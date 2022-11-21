using Ecm.CaptureCustomAddIn.View;
using Ecm.CaptureModel;
using Ecm.CaptureModel.DataProvider;
using Ecm.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Ecm.CaptureCustomAddIn.ViewModel
{
    public class BatchTypeSelectionViewModel : BaseDependencyProperty
    {
        private RelayCommand _sendtoCaptureCommand;
        private RelayCommand _cancelCommand;
        private BatchTypeModel _selectedBatchType;
        private DocTypeModel _selectedContentType;
        private string _filePath;
        private string _extension;
        private FileFormatModel _fileFormat;
        private List<MailItemInfo> _mailInfos;
        private ObservableCollection<DocTypeModel> _contentTypes = new ObservableCollection<DocTypeModel>();

        public BatchTypeSelectionViewModel(Action<bool> onCLose, string filePath, string extension, FileFormatModel fileFormat)
        {
            _filePath = filePath;
            _extension = extension;
            _fileFormat = fileFormat;
            Initialize();
            OnClose = onCLose;
        }

        public BatchTypeSelectionViewModel(Action<bool> onCLose, List<MailItemInfo> mailInfos)
        {
            _mailInfos = mailInfos;
            InitializeOutlook();
            OnClose = onCLose;
        }

        private Action<bool> OnClose { get; set; }

        public BatchTypeModel SelectedBatchType
        {
            get { return _selectedBatchType; }
            set
            {
                _selectedBatchType = value;
                OnPropertyChanged("SelectedBatchType");

                if (value != null)
                {
                    try
                    {

                        ContentTypes = value.DocTypes;
                    }
                    catch (Exception ex)
                    {
                        ProcessHelper.ProcessException(ex);
                    }
                }
            }
        }

        public DocTypeModel SelectedContentType
        {
            get { return _selectedContentType; }
            set
            {
                _selectedContentType = value;
                OnPropertyChanged("SelectedContentType");
            }
        }

        public ObservableCollection<BatchTypeModel> BatchTypes { get; set; }

        public ObservableCollection<DocTypeModel> ContentTypes
        {
            get { return _contentTypes; }
            set
            {
                _contentTypes = value;
                OnPropertyChanged("ContentTypes");
            }
        }

        public ICommand SendToCaptureCommand
        {
            get
            {
                if (_sendtoCaptureCommand == null)
                {
                    _sendtoCaptureCommand = new RelayCommand(p => SendToCapture(), p => CanSend());
                }
                return _sendtoCaptureCommand;
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand(p => Cancel());
                }
                return _cancelCommand;
            }
        }

        //Private Method
        private void InitializeOutlook()
        {
            try
            {
                BatchTypes = new ObservableCollection<BatchTypeModel>(new BatchTypeProvider().GetCaptureBatchTypes().Where(p => p.IsApplyForOutlook));
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void Initialize()
        {
            try
            {
                BatchTypes = new BatchTypeProvider().GetCaptureBatchTypes();
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void Cancel()
        {
            if (OnClose != null)
            {
                OnClose(false);
            }
        }

        private bool CanSend()
        {
            return SelectedContentType != null;
        }

        private void SendToCapture()
        {
            try
            {
                MainView mainView = null;
                if (OnClose != null)
                {
                    OnClose(true);
                }

                if (_mailInfos != null)
                {
                    mainView = new MainView(SelectedBatchType, SelectedContentType, _mailInfos);
                }
                else
                {
                    mainView = new MainView(SelectedBatchType, SelectedContentType, _filePath, _extension, _fileFormat);
                }

                switch (_fileFormat)
                {
                    case FileFormatModel.Doc:
                        mainView.Title = "MS Word Capture";
                        break;
                    case FileFormatModel.Xls:
                        mainView.Title = "MS Excel Capture";
                        break;
                    case FileFormatModel.Ppt:
                        mainView.Title = "MS Power Point Capture";
                        break;
                    default:
                        mainView.Title = "MS Outlook Capture";
                        break;
                }

                mainView.ShowDialog();
            }
            catch (Exception ex)
            {
            }

        }
    }
}
