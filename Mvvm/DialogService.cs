using System;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace Ecm.Mvvm
{
    public static class DialogService
    {
        public static void Initialize(Action<string> messageDialog, Action<string> errorDialog, Func<string, DialogServiceResult> twoStateConfirmDialog,
                                      Func<string, DialogServiceResult> threeStateConfirmDialog, Func<string, string> fileBrowserDialog,
                                      Func<string, string> folderBrowserDialog, Func<string, string, string> saveFileDilog)
        {
            _messageDialog = messageDialog;
            _errorDialog = errorDialog;
            _twoStateConfirmDialog = twoStateConfirmDialog;
            _threeStateConfirmDialog = threeStateConfirmDialog;
            _fileBrowseDialog = fileBrowserDialog;
            _folderBrowseDialog = folderBrowserDialog;
            _saveFileDialog = saveFileDilog;
        }

        public static void InitializeDefault()
        {
            _messageDialog = (msg => MessageBox.Show(msg, "Cloud ECM", MessageBoxButton.OK, MessageBoxImage.Information));
            _errorDialog = (msg => MessageBox.Show(msg, "Cloud ECM", MessageBoxButton.OK, MessageBoxImage.Error));
            _twoStateConfirmDialog = OpenTwoStateConfirmDialog;
            _threeStateConfirmDialog = OpenThreeStateConfirmDialog;
            _saveFileDialog = OpenSaveFileDialog;
            _fileBrowseDialog = OpenFileBrowserDialog;
            _folderBrowseDialog = OpenFolderBrowserDialog;
        }

        public static DialogServiceResult ShowConfirmDialog(string message, string compareValue)
        {
            var cd = new ConfirmDialog(message, compareValue);

            DialogResult dr = cd.ShowDialog();
            cd.Dispose();

            if (dr == DialogResult.OK)
            {
                return DialogServiceResult.Yes;
            }

            return DialogServiceResult.Cancel;
        }

        public static void ShowErrorDialog(string message)
        {
            if (_errorDialog != null)
            {
                _errorDialog(message);
            }
        }

        public static string ShowFileBrowseDialog(string filter)
        {
            if (_fileBrowseDialog != null)
            {
                return _fileBrowseDialog(filter);
            }
            return "";
        }

        public static string ShowFolderBrowseDialog(string focusPath)
        {
            if (_folderBrowseDialog != null)
            {
                return _folderBrowseDialog(focusPath);
            }

            return "";
        }

        public static void ShowMessageDialog(string message)
        {
            if (_messageDialog != null)
            {
                _messageDialog(message);
            }
        }

        public static string ShowSaveFileDialog(string filter, string defaultName)
        {
            if (_saveFileDialog != null)
            {
                return _saveFileDialog(filter, defaultName);
            }

            return "";
        }

        public static DialogServiceResult ShowThreeStateConfirmDialog(string message)
        {
            if (_threeStateConfirmDialog != null)
            {
                return _threeStateConfirmDialog(message);
            }
            return DialogServiceResult.Yes;
        }

        public static DialogServiceResult ShowTwoStateConfirmDialog(string message)
        {
            if (_twoStateConfirmDialog != null)
            {
                return _twoStateConfirmDialog(message);
            }
            return DialogServiceResult.Yes;
        }

        private static string OpenFileBrowserDialog(string filter)
        {
            var fileDialog = new OpenFileDialog
                                 {
                                     Filter = filter,
                                     CheckFileExists = true,
                                     ValidateNames = true,
                                     Multiselect = false,
                                     Title = "Cloud ECM"
                                 };

            if (fileDialog.ShowDialog().Value)
            {
                return fileDialog.FileName;
            }
            return null;
        }

        private static string OpenFolderBrowserDialog(string startPath)
        {
            var folderDialog = new FolderBrowserDialog
                                   {
                                       SelectedPath = startPath,
                                       ShowNewFolderButton = true,
                                       Description = "Please select a folder to save your document(s)."
                                   };
            DialogResult result = folderDialog.ShowDialog();

            if (result == DialogResult.Yes || result == DialogResult.OK)
            {
                return folderDialog.SelectedPath;
            }

            return "";
        }

        private static string OpenSaveFileDialog(string filter, string defaultName)
        {
            var saveFileDialog = new SaveFileDialog { Title = "Cloud ECM", Filter = filter, FileName = defaultName };

            if (saveFileDialog.ShowDialog().Value)
            {
                return saveFileDialog.FileName;
            }

            return "";
        }

        private static DialogServiceResult OpenThreeStateConfirmDialog(string message)
        {
            MessageBoxResult result = MessageBox.Show(
                message, "Cloud ECM", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            switch (result)
            {
                case MessageBoxResult.Cancel:
                    return DialogServiceResult.Cancel;
                case MessageBoxResult.No:
                case MessageBoxResult.None:
                    return DialogServiceResult.No;
                case MessageBoxResult.OK:
                case MessageBoxResult.Yes:
                    return DialogServiceResult.Yes;
                default:
                    return DialogServiceResult.Yes;
            }
        }

        private static DialogServiceResult OpenTwoStateConfirmDialog(string message)
        {
            MessageBoxResult result = MessageBox.Show(
                message, "Cloud ECM", MessageBoxButton.YesNo, MessageBoxImage.Question);
            switch (result)
            {
                case MessageBoxResult.Cancel:
                    return DialogServiceResult.Cancel;
                case MessageBoxResult.No:
                case MessageBoxResult.None:
                    return DialogServiceResult.No;
                case MessageBoxResult.OK:
                case MessageBoxResult.Yes:
                    return DialogServiceResult.Yes;
                default:
                    return DialogServiceResult.Yes;
            }
        }

        // ------------------------------------------------------------------------------------------------------------------------
        // Member variables, including instance and static members. All member vars must have an underscore prefix in front of them.

        private static Action<string> _errorDialog;

        private static Func<string, string> _fileBrowseDialog;

        private static Func<string, string> _folderBrowseDialog;

        private static Action<string> _messageDialog;

        private static Func<string, string, string> _saveFileDialog;

        private static Func<string, DialogServiceResult> _threeStateConfirmDialog;

        private static Func<string, DialogServiceResult> _twoStateConfirmDialog;
    }

    public enum DialogServiceResult
    {
        Yes,

        No,

        Cancel
    }
}