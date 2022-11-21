using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;

namespace ArchiveMVC.Models
{
    public class LanguageModel
    {
        private Guid _id;
        private string _name;
        private string _format;

        public Guid Id
        {
            get { return _id; } 
            set
            {
                _id = value;
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
            }
        }

        public string Format
        {
            get { return _format; }
            set
            {
                _format = value;
            }
        }
    }
}
