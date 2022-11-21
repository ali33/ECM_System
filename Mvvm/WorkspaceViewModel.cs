using System;
using System.Windows.Input;
using Ecm.Mvvm;

namespace Ecm.Mvvm
{
    public abstract class WorkspaceViewModel : BaseDependencyProperty
    {
        public event EventHandler RequestActivate;

        /// <summary>
        ///   Raised when this workspace should be removed from the UI.
        /// </summary>
        public event EventHandler RequestClose;

        public event EventHandler RequestCloseAll;

        public event EventHandler RequestCloseOthers;

        /// <summary>
        ///   Returns the command that, when invoked, attempts to remove all workspaces from the user interface.
        /// </summary>
        public ICommand CloseAllCommand
        {
            get
            {
                if (_closeAllCommand == null)
                {
                    _closeAllCommand = new RelayCommand(param => OnRequestCloseAll(), param => CanCloseAll());
                }

                return _closeAllCommand;
            }
        }

        /// <summary>
        ///   Returns the command that, when invoked, attempts to remove this workspace from the user interface.
        /// </summary>
        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                {
                    _closeCommand = new RelayCommand(param => OnRequestClose());
                }
                return _closeCommand;
            }
        }

        /// <summary>
        ///   Returns the command that, when invoked, attempts to remove other workspaces from the user interface.
        /// </summary>
        public ICommand CloseOthersCommand
        {
            get
            {
                if (_closeOthersCommand == null)
                {
                    _closeOthersCommand = new RelayCommand(param => OnRequestCloseOthers(), param => CanCloseOthers());
                }
                return _closeOthersCommand;
            }
        }

        public bool IsProcessing
        {
            get
            {
                return _isProcessing;
            }
            set
            {
                _isProcessing = value;
                OnPropertyChanged("IsProcessing");
            }
        }

        protected bool CanCloseAll()
        {
            if (AllowToCloseAll != null)
            {
                return AllowToCloseAll();
            }
            return false;
        }

        protected bool CanCloseOthers()
        {
            if (AllowToCloseOthers != null)
            {
                return AllowToCloseOthers();
            }
            return false;
        }

        protected void OnRequestActivate()
        {
            if (RequestActivate != null)
            {
                RequestActivate(this, EventArgs.Empty);
            }
        }

        protected void OnRequestClose()
        {
            if (RequestClose != null)
            {
                RequestClose(this, EventArgs.Empty);
            }
        }

        protected void OnRequestCloseAll()
        {
            if (RequestCloseAll != null)
            {
                RequestCloseAll(this, EventArgs.Empty);
            }
        }

        protected void OnRequestCloseOthers()
        {
            if (RequestCloseOthers != null)
            {
                RequestCloseOthers(this, EventArgs.Empty);
            }
        }

        public Func<bool> AllowToCloseAll;

        public Func<bool> AllowToCloseOthers;

        private RelayCommand _closeAllCommand;

        private RelayCommand _closeCommand;

        private RelayCommand _closeOthersCommand;

        private bool _isProcessing;
    }
}