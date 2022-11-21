using Ecm.Mvvm;

namespace Ecm.ContentViewer.Model
{
    public class ContentViewerPermission : BaseDependencyProperty
    {
        public bool CanAddHighlight
        {
            get { return _canAddHighlight; }
            set
            {
                _canAddHighlight = value;
                OnPropertyChanged("CanAddHighlight");
            }
        }

        public bool CanAddLine
        {
            get { return _canAddLine; }
            set
            {
                _canAddLine = value;
                OnPropertyChanged("CanAddLine");
            }
        }

        public bool CanAddRedaction
        {
            get { return _canAddRedaction; }
            set
            {
                _canAddRedaction = value;
                OnPropertyChanged("CanAddRedaction");
            }
        }

        public bool CanAddText
        {
            get { return _canAddText; }
            set
            {
                _canAddText = value;
                OnPropertyChanged("CanAddText");
            }
        }

        public bool CanDeleteHighlight
        {
            get { return _canDeleteHighlight; }
            set
            {
                _canDeleteHighlight = value;
                OnPropertyChanged("CanDeleteHighlight");
            }
        }

        public bool CanDeleteLine
        {
            get { return _canDeleteLine; }
            set
            {
                _canDeleteLine = value;
                OnPropertyChanged("CanDeleteLine");
            }
        }

        public bool CanDeleteRedaction
        {
            get { return _canDeleteRedaction; }
            set
            {
                _canDeleteRedaction = value;
                OnPropertyChanged("CanDeleteRedaction");
            }
        }

        public bool CanDeleteText
        {
            get { return _canDeleteText; }
            set
            {
                _canDeleteText = value;
                OnPropertyChanged("CanDeleteText");
            }
        }

        public bool CanEmail
        {
            get { return _canEmail; }
            set
            {
                _canEmail = value;
                OnPropertyChanged("CanEmail");
            }
        }

        public bool CanHideAnnotation
        {
            get { return _canHideAnnotation; }
            set
            {
                _canHideAnnotation = value;
                OnPropertyChanged("CanHideAnnotation");
            }
        }

        public bool CanPrint
        {
            get { return _canPrint; }
            set
            {
                _canPrint = value;
                OnPropertyChanged("CanPrint");
            }
        }

        public bool CanSeeHighlight
        {
            get { return _canSeeHighlight; }
            set
            {
                _canSeeHighlight = value;
                OnPropertyChanged("CanSeeHighlight");
            }
        }

        public bool CanSeeLine
        {
            get { return _canSeeLine; }
            set
            {
                _canSeeLine = value;
                OnPropertyChanged("CanSeeLine");
            }
        }

        public bool CanSeeText
        {
            get { return _canSeeText; }
            set
            {
                _canSeeText = value;
                OnPropertyChanged("CanSeeText");
            }
        }

        public static ContentViewerPermission GetAllowAll()
        {
            return new ContentViewerPermission
                {
                    CanAddHighlight = true,
                    CanAddLine = true,
                    CanAddRedaction = true,
                    CanAddText = true,
                    CanDeleteHighlight = true,
                    CanDeleteLine = true,
                    CanDeleteRedaction = true,
                    CanDeleteText = true,
                    CanEmail = true,
                    CanHideAnnotation = true,
                    CanPrint = true,
                    CanSeeHighlight = true,
                    CanSeeLine = true,
                    CanSeeText = true
                };
        }

        public bool CanApplyOCRTemplate
        {
            get { return _canApplyOCRTemplate; }
            set
            {
                _canApplyOCRTemplate = value;
                OnPropertyChanged("CanApplyOCRTemplate");
            }
        }

        private bool _canAddHighlight;
        private bool _canAddLine;
        private bool _canAddRedaction;
        private bool _canAddText;
        private bool _canDeleteHighlight;
        private bool _canDeleteLine;
        private bool _canDeleteRedaction;
        private bool _canDeleteText;
        private bool _canEmail;
        private bool _canHideAnnotation;
        private bool _canPrint;
        private bool _canSeeHighlight;
        private bool _canSeeLine;
        private bool _canSeeText;
        private bool _canApplyOCRTemplate;
    }
}