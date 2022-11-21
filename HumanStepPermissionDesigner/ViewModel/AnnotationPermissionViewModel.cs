using Ecm.CaptureDomain;
using Ecm.Mvvm;
using Ecm.Workflow.Activities.CustomActivityDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Ecm.Workflow.Activities.HumanStepPermissionDesigner.ViewModel
{
    public class AnnotationPermissionViewModel : ComponentViewModel
    {
        private const string ADD = "Add";
        private const string DELETE = "Delete";
        private const string SEE = "See";
        private const string HIDE = "Hide";

        //private HumanStepAnnotationPermission _annotationPermission;
        private AnnotationPermission _annotationPermission;
        private DocumentType _docType;
        private RelayCommand _checkCommand;
        private RelayCommand _checkAllCommand;

        private bool _allText;
        private bool _allHighlight;
        private bool _allRedaction;
        private bool _isDataModified;

        public AnnotationPermissionViewModel(AnnotationPermission annotationPermission, DocumentType docType)
        {
            _annotationPermission = annotationPermission;
            DocType = docType;
        }
        //public AnnotationPermissionViewModel(HumanStepAnnotationPermission annotationPermission, DocumentType docType)
        //{
        //    _annotationPermission = annotationPermission;
        //    DocType = docType;
        //}

        public DocumentType DocType
        {
            get { return _docType; }
            set
            {
                _docType = value;
                OnPropertyChanged("DocType");
            }
        }

        public bool AllowedSeeText
        {
            get { return _annotationPermission.CanSeeText; }
            set
            {
                _annotationPermission.CanSeeText = value;
                OnPropertyChanged("AllowedSeeText");
                CheckAllText(SEE, value);
            }
        }

        public bool AllowedAddText
        {
            get { return _annotationPermission.CanAddText; }
            set
            {
                _annotationPermission.CanAddText = value;
                OnPropertyChanged("AllowedAddText");

                CheckAllText(ADD, value);
            }
        }

        public bool AllowedDeleteText
        {
            get { return _annotationPermission.CanDeleteText; }
            set
            {
                _annotationPermission.CanDeleteText = value;
                OnPropertyChanged("AllowedDeleteText");
                CheckAllText(DELETE, value);
            }
        }

        public bool AllowedSeeHighlight
        {
            get { return _annotationPermission.CanSeeHighlight; }
            set
            {
                _annotationPermission.CanSeeHighlight = value;
                OnPropertyChanged("AllowedSeeHighlight");
                CheckAllHighlight(SEE, value);
            }
        }

        public bool AllowedAddHighlight
        {
            get { return _annotationPermission.CanAddHighlight; }
            set
            {
                _annotationPermission.CanAddHighlight = value;
                OnPropertyChanged("AllowedAddHighlight");
                CheckAllHighlight(ADD, value);
            }

        }

        public bool AllowedDeleteHighlight
        {
            get { return _annotationPermission.CanDeleteHighlight; }
            set
            {
                _annotationPermission.CanDeleteHighlight = value;
                OnPropertyChanged("AllowedDeleteHighlight");
                CheckAllHighlight(DELETE, value);
            }
        }

        public bool AllowedHideRedaction
        {
            get { return _annotationPermission.CanHideRedaction; }
            set
            {
                _annotationPermission.CanHideRedaction = value;
                OnPropertyChanged("AllowedHideRedaction");
                CheckAllRedaction(HIDE, value);
            }

        }

        public bool AllowedAddRedaction
        {
            get { return _annotationPermission.CanAddRedaction; }
            set
            {
                _annotationPermission.CanAddRedaction = value;
                OnPropertyChanged("AllowedAddRedaction");
                CheckAllRedaction(ADD, value);
            }
        }

        public bool AllowedDeleteRedaction
        {
            get { return _annotationPermission.CanDeleteRedaction; }
            set
            {
                _annotationPermission.CanDeleteRedaction = value;
                OnPropertyChanged("AllowedDeleteRedaction");
                CheckAllRedaction(DELETE, value);
            }
        }

        public bool AllText
        {
            get { return _allText; }
            set
            {
                _allText = value;
                OnPropertyChanged("AllText");
            }
        }

        public bool AllHighlight
        {
            get { return _allHighlight; }
            set
            {
                _allHighlight = value;
                OnPropertyChanged("AllHighlight");
            }
        }

        public bool AllRedaction
        {
            get { return _allRedaction; }
            set
            {
                _allRedaction = value;
                OnPropertyChanged("AllRedaction");
            }
        }

        public bool IsDataModified
        {
            get { return _isDataModified; }
            set
            {
                _isDataModified = value;
                OnPropertyChanged("IsDataModified");
            }
        }

        public ICommand CheckCommand
        {
            get
            {
                if (_checkCommand == null)
                {
                    _checkCommand = new RelayCommand(p => CheckItem());
                }

                return _checkCommand;
            }
        }

        public ICommand CheckAllCommand
        {
            get
            {
                if (_checkAllCommand == null)
                {
                    _checkAllCommand = new RelayCommand(p => CheckAllItem(p));
                }

                return _checkAllCommand;
            }
        }   

        private void CheckAllItem(object p)
        {
            string action = p.ToString();
            _isDataModified = true;

            switch (action)
            {
                case "Text":
                    AllowedAddText = AllowedDeleteText = AllowedSeeText = AllText ;
                    break;
                case "Highlight":
                    AllowedAddHighlight = AllowedDeleteHighlight = AllowedSeeHighlight = AllHighlight;
                    break;
                case "Redaction":
                    AllowedAddRedaction = AllowedDeleteRedaction = AllowedHideRedaction = AllRedaction;
                    break;
            }
        }

        private void CheckItem()
        {
            IsDataModified = true;
        }

        private void CheckAllText(string action, bool isChecked)
        {
            switch (action)
            {
                case ADD:
                    if (isChecked && AllowedSeeText && AllowedDeleteText)
                    {
                        AllText = true;
                    }
                    else
                    {
                        AllText = false;
                    }
                    break;
                case DELETE:
                    if (isChecked && AllowedSeeText && AllowedAddText)
                    {
                        AllText = true;
                    }
                    else
                    {
                        AllText = false;
                    }
                    break;
                case SEE:
                    if (isChecked && AllowedAddText && AllowedDeleteText)
                    {
                        AllText = true;
                    }
                    else
                    {
                        AllText = false;
                    }
                    break;
            }
        }

        private void CheckAllHighlight(string action, bool isChecked)
        {
            switch (action)
            {
                case ADD:
                    if (isChecked && AllowedSeeHighlight && AllowedDeleteHighlight)
                    {
                        AllHighlight = true;
                    }
                    else
                    {
                        AllHighlight = false;
                    }
                    break;
                case DELETE:
                    if (isChecked && AllowedSeeHighlight && AllowedAddHighlight)
                    {
                        AllHighlight = true;
                    }
                    else
                    {
                        AllHighlight = false;
                    }
                    break;
                case SEE:
                    if (isChecked && AllowedAddHighlight && AllowedDeleteHighlight)
                    {
                        AllHighlight = true;
                    }
                    else
                    {
                        AllHighlight = false;
                    }
                    break;
            }
        }

        private void CheckAllRedaction(string action, bool isChecked)
        {
            switch (action)
            {
                case ADD:
                    if (isChecked && AllowedHideRedaction && AllowedDeleteRedaction)
                    {
                        AllRedaction = true;
                    }
                    else
                    {
                        AllRedaction = false;
                    }
                    break;
                case DELETE:
                    if (isChecked && AllowedHideRedaction && AllowedAddRedaction)
                    {
                        AllRedaction = true;
                    }
                    else
                    {
                        AllRedaction = false;
                    }
                    break;
                case HIDE:
                    if (isChecked && AllowedAddRedaction && AllowedDeleteRedaction)
                    {
                        AllRedaction = true;
                    }
                    else
                    {
                        AllRedaction = false;
                    }
                    break;
            }
        }
    }
}
