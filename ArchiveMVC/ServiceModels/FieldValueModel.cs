using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Xml.Serialization;
using Ecm.Domain;
using Ecm.Mvvm;
using System.Collections.Generic;

using ArchiveMVC;

namespace ArchiveMVC.Models
{
    public class FieldValueModel
    {
        private FieldMetaDataModel _field;
        private string _value;
        private bool _multipleUpdate;
        private bool _showMultipleUpdate;
        private bool _isValid;
        private IList<string> _pickListValues;

        public FieldValueModel()
        {
            TableValues = new List<TableFieldValueModel>();
        }

        public FieldMetaDataModel Field
        {
            get { return _field; }
            set
            {
                _field = value;
            }
        }

        public int MaxLength
        {
            get { return _field.MaxLength; }
        }

        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                IsValid = Field.IsSystemField || Field.DataType == FieldDataType.Table || this["Value"] == string.Empty;
            }
        }

        public IList<string> PickListValues
        {
            get
            {
                if (_field == null || _field.DataType != FieldDataType.Picklist || _field.Picklists == null || _field.Picklists.Count == 0)
                {
                    return null;
                }

                if (_pickListValues == null)
                {
                    _pickListValues = _field.Picklists.Select(p => p.Value).OrderBy(p => p).ToList();
                    _pickListValues.Insert(0, string.Empty);
                }

                return _pickListValues;
            }
        }

        [XmlIgnore]
        public bool IsValid
        {
            get { return _isValid; } 
            set
            {
                _isValid = value;
            }
        }

        [XmlIgnore]
        public List<TableFieldValueModel> TableValues { get; set; }

        [XmlIgnore]
        public bool MultipleUpdate
        {
            get { return _multipleUpdate; }
            set
            {
                _multipleUpdate = value;
            }
        }

        [XmlIgnore]
        public bool ShowMultipleUpdate
        {
            get { return _showMultipleUpdate; }
            set
            {
                _showMultipleUpdate = value;
            }
        }

        [XmlIgnore]
        public bool ShowRequiredNotification
        {
            get { return AllowToEditIndex && Field.IsRequired; }
        }

        [XmlIgnore]
        public bool AllowToEditIndex
        {
            get
            {
                return (ShowMultipleUpdate && MultipleUpdate) || !ShowMultipleUpdate;
            }
        }

        [XmlIgnore]
        public DataTable LookupData { get; set; }

        //[XmlIgnore]
        //public BitmapImage SnippetImage { get; set; }
        
        //[20-9-2013] Update SnippetImage to byte array
        public byte[] SnippetImage { get; set; }

        [XmlIgnore]
        public string SnippetImageKey { get; set; }

        [XmlIgnore]
        public string Error
        {
            get
            {
                return this["Value"];
            }
        }

        [XmlIgnore]
        public string this[string columnName]
        {
            get
            {
                if (columnName == "Value" && AllowToEditIndex)
                {
                    if (Field.IsRequired && string.IsNullOrEmpty(Value) && Field.DataType != FieldDataType.Table)
                    {
                        return string.Format(Resources.Resource.uiRequiredValueMessage, Field.Name);
                    }


                    if (Field.DataType == FieldDataType.Date && !string.IsNullOrEmpty(Value))
                    {
                        DateTime dateTime;
                        DateTime.TryParse(Value, out dateTime);

                        if (dateTime < Convert.ToDateTime(Resources.Resource.MinDate) ||
                            dateTime > Convert.ToDateTime(Resources.Resource.MaxDate))
                        {
                            return Resources.Resource.uiInvalidDateMessage;
                        }
                    }

                }

                return string.Empty;
            }
        }
    }
}
