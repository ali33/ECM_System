using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using System.ComponentModel;

namespace ArchiveMVC.Models
{
    public class PicklistModel
    {
        private Guid _fieldId;
        private string _value;

        public PicklistModel()
        {
        }

        public Guid Id { get; set; }

        public Guid FieldId
        {
            get { return _fieldId; }
            set
            {
                _fieldId = value;
            }
        }

        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
            }
        }

    }
}
