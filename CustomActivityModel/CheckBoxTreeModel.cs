using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Ecm.Mvvm;

namespace Ecm.Workflow.Activities.CustomActivityModel
{
    public class CheckBoxTreeModel : BaseDependencyProperty
    {
        private bool? _isChecked = false;
        private List<CheckBoxTreeModel> _children = new List<CheckBoxTreeModel>();

        public string DisplayText
        {
            get;
            set;
        }

        public string Value { get; set; }

        public Guid Id { get; set; }

        public List<CheckBoxTreeModel> Children
        {
            get { return _children; }
            set
            {
                _children = value;
                OnPropertyChanged("Children");
            }
        }

        public CheckBoxTreeModel Parent { get; set; }

        public bool IsInitiallySelected { get; set; }

        public bool? IsChecked
        {
            get { return _isChecked; }
            set
            {
                SetIsChecked(value, true, true);
            }
        }

        void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == _isChecked)
                return;

            _isChecked = value;

            if (updateChildren && _isChecked.HasValue)
                this.Children.ForEach(c => c.SetIsChecked(_isChecked, true, false));

            if (updateParent && Parent != null)
                Parent.VerifyCheckState();

            this.OnPropertyChanged("IsChecked");
        }

        void VerifyCheckState()
        {
            bool? state = null;
            for (int i = 0; i < this.Children.Count; ++i)
            {
                bool? current = this.Children[i].IsChecked;
                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }
            this.SetIsChecked(state, false, true);
        }
    }
}
