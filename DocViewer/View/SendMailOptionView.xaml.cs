using Ecm.DocViewer;
using Ecm.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace Ecm.DocViewer
{
    /// <summary>
    /// Interaction logic for RangeSelector.xaml
    /// </summary>
    public partial class SendMailOptionView
    {
        ResourceManager _resource = new ResourceManager("Ecm.DocViewer.ViewerContainer", Assembly.GetExecutingAssembly());
        private RelayCommand _optionCommand;

        public static readonly DependencyProperty TiffLableProperty =
           DependencyProperty.Register("TiffLable", typeof(string), typeof(SendMailOptionView));

        public static readonly DependencyProperty XPSLableProperty =
           DependencyProperty.Register("XPSLable", typeof(string), typeof(SendMailOptionView));

        public string TiffLable
        {
            get { return GetValue(TiffLableProperty) as string; }
            set { SetValue(TiffLableProperty, value); }
        }

        public string XPSLable
        {
            get { return GetValue(XPSLableProperty) as string; }
            set { SetValue(XPSLableProperty, value); }
        }

        public SendMailOptionView(bool forMail)
        {
            InitializeComponent();
            InitializeCommands();
            IsXps = true;

            if (forMail)
            {
                XPSLable = _resource.GetString("XPSAttachment");
                TiffLable = _resource.GetString("TifAttachment");
            }
            else
            {
                XPSLable = _resource.GetString("XPSSaveFile");
                TiffLable = _resource.GetString("TifSaveFile");
            }
        }

        public DialogViewer Dialog { get; set; }

        public bool IsXps { get; set; }

        public List<int> SelectedPageIndexes { get; private set; }

        public RoutedCommand OKCommand;

        public RoutedCommand CancelCommand;

        public ICommand OptionCommand
        {
            get
            {
                if (_optionCommand == null)
                {
                    _optionCommand = new RelayCommand(p => SelectFileType(p));
                }

                return _optionCommand;
            }
        }

        private void SelectFileType(object p)
        {
            if (p.ToString().ToLower() == "xps")
            {
                IsXps = true;
            }
            else
            {
                IsXps = false;
            }
        }

        private void InitializeCommands()
        {
            OKCommand = new RoutedCommand("OKCommand", typeof(SendMailOptionView));
            CommandBindings.Add(new CommandBinding(OKCommand, Ok, CanOk));

            CancelCommand = new RoutedCommand("CancelCommand", typeof(SendMailOptionView),
                                              new InputGestureCollection {new KeyGesture(Key.Escape)});
            CommandBindings.Add(new CommandBinding(CancelCommand, Cancel));

            btnRangeOk.Command = OKCommand;
            btnRangeCancel.Command = CancelCommand;
        }

        private void CanOk(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = rbAll.IsChecked == true || ParseCustomRange();
        }

        private void Ok(object sender, ExecutedRoutedEventArgs e)
        {
            if (rbAll.IsChecked == true)
            {
                SelectedPageIndexes = null;
            }

            Dialog.DialogResult = DialogResult.OK;
            Dialog.Close();
        }

        private void Cancel(object sender, ExecutedRoutedEventArgs e)
        {
            Dialog.DialogResult = DialogResult.Cancel;
            Dialog.Close();
        }

        private void TxtCustomRangePreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Only accept characters: numeric (0 - 9), comma, space and subtract
            // The format of token that user can type is liek "1, 3, 5-12"
            var keys = new[]
                           {
                               Key.D0, Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9,
                               Key.Subtract, Key.Space, Key.OemMinus, Key.OemComma, Key.Left, Key.Right, Key.Back,
                               Key.Delete, Key.Home, Key.End,
                               Key.NumPad0, Key.NumPad1, Key.NumPad2, Key.NumPad3, Key.NumPad4, Key.NumPad5, Key.NumPad6, 
                               Key.NumPad7, Key.NumPad8, Key.NumPad9
                           };

            e.Handled = !keys.Contains(e.Key);
        }

        private bool ParseCustomRange()
        {
            string[] temps = txtCustomRange.Text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> tokens = (from p in temps
                                   where p.Trim() != string.Empty
                                   select p.Trim()).ToList();
            if (tokens.Count == 0)
            {
                return false;
            }

            SelectedPageIndexes = new List<int>();
            foreach (string token in tokens)
            {
                if (!token.Contains('-'))
                {
                    int pageIndex = Convert.ToInt32(token);
                    if (pageIndex == 0)
                    {
                        return false;
                    }

                    if (!SelectedPageIndexes.Any(p => p == pageIndex))
                    {
                        SelectedPageIndexes.Add(pageIndex);
                    }
                }
                else
                {
                    string[] pageIndexes = token.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);

                    if (pageIndexes.Length != 2)
                    {
                        return false;
                    }

                    int startPageIndex = Convert.ToInt32(pageIndexes[0]);
                    int endPageIndex = Convert.ToInt32(pageIndexes[1]);
                    if (startPageIndex == 0 || startPageIndex > endPageIndex)
                    {
                        return false;
                    }

                    for (int pageIndex = startPageIndex; pageIndex <= endPageIndex; pageIndex++)
                    {
                        if (!SelectedPageIndexes.Any(p => p == pageIndex))
                        {
                            SelectedPageIndexes.Add(pageIndex);
                        }
                    }
                }
            }

            SelectedPageIndexes = SelectedPageIndexes.OrderBy(p => p).ToList();
            return (SelectedPageIndexes != null && SelectedPageIndexes.Count != 0);
        }
    }
}
