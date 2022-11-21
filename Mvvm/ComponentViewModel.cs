using System;
namespace Ecm.Mvvm
{
    public abstract class ComponentViewModel : BaseDependencyProperty
    {
        private bool _editPanelVisibled;
        private bool _isProcessing;
        public virtual bool IsChanged { get; set; }
        private bool _isEditMode;

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

        public bool EditPanelVisibled
        {
            get { return _editPanelVisibled; }
            set
            {
                _editPanelVisibled = value;
                OnPropertyChanged("EditPanelVisibled");
            }
        }

        public bool IsEditMode
        {
            get { return _isEditMode; }
            set
            {
                _isEditMode = value;
                OnPropertyChanged("IsEditMode");
            }
        }

        public Action ResetListView { get; set; }

        public virtual void Initialize()
        {
        }
    }
}