using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using System.Xml.Serialization;
using System.Windows.Media.Imaging;
using System.Data;
using System.ComponentModel;
using System.Reflection;
using Ecm.ScriptEngine;
using System.Resources;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;

namespace Ecm.ContentViewer.Model
{
    public class FieldValueModel : BaseDependencyProperty, IDataErrorInfo
    {
        private ResourceManager _resource = new ResourceManager("Ecm.ContentViewer.Model.Resources", Assembly.GetExecutingAssembly());
        private string _value;
        private FieldModel _docFieldMetaDataModel;
        private bool _isValid;
        private bool _multipleUpdate;
        private bool _showMultipleUpdate;
        private bool _isReadOnly;
        private bool _isHidden;
        private bool _isWrite;
        private IList<string> _pickListValues;

        public FieldValueModel()
        {
            TableValues = new ObservableCollection<TableFieldValueModel>();
        }

        public Guid Id { get; set; }

        public Guid DocId { get; set; }

        public Guid BatchId { get; set; }
        /// <summary>
        /// Identifier of the <see cref="DocumentFieldMetaData"/> object contains the value.
        /// </summary>
        public Guid FieldId { get; set; }

        public string Value
        {
            get { return _value; }
            set
           {
                _value = value;

                if (Field == null)
                {
                    IsValid = false;
                }
                else
                {
                    IsValid = Field.IsSystemField || this["Value"] == string.Empty || Field.Name == Common.SYS_BATCH_NAME_INDEX || (Field.IsRequired && !string.IsNullOrEmpty(value));
                }

                OnPropertyChanged("Value");
            }
        }

        /// <summary>
        /// The <see cref="DocumentFieldMetaData"/> object
        /// </summary>
        public FieldModel Field
        {
            get { return _docFieldMetaDataModel; }
            set
            {
                _docFieldMetaDataModel = value;
                OnPropertyChanged("Field");
            }
        }

        [XmlIgnore]
        public DataTable LookupData { get; set; }

        [XmlIgnore]
        public BitmapImage SnippetImage { get; set; }

        [XmlIgnore]
        public bool IsValid
        {
            get { return _isValid; }
            set
            {
                _isValid = value;
                OnPropertyChanged("IsValid");
            }
        }

        [XmlIgnore]
        public ObservableCollection<TableFieldValueModel> TableValues { get; set; }

        [XmlIgnore]
        public IList<string> PickListValues
        {
            get
            {
                if (_docFieldMetaDataModel == null || _docFieldMetaDataModel.DataType != FieldDataType.Picklist || _docFieldMetaDataModel.Picklists == null || _docFieldMetaDataModel.Picklists.Count == 0)
                {
                    return null;
                }

                if (_pickListValues == null)
                {
                    _pickListValues = _docFieldMetaDataModel.Picklists.Select(p => p.Value).OrderBy(p => p).ToList();
                    _pickListValues.Insert(0, string.Empty);
                }

                return _pickListValues;
            }
        }

        [XmlIgnore]
        public bool MultipleUpdate
        {
            get { return _multipleUpdate; }
            set
            {
                _multipleUpdate = value;
                OnPropertyChanged("MultipleUpdate");
            }
        }

        [XmlIgnore]
        public bool ShowMultipleUpdate
        {
            get { return _showMultipleUpdate; }
            set
            {
                _showMultipleUpdate = value;
                OnPropertyChanged("ShowMultipleUpdate");
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
        public bool IsReadOnly 
        {
            get{return _isReadOnly;}
            set{
                _isReadOnly=value;
                OnPropertyChanged("IsReadOnly");
            }
        }

        [XmlIgnore]
        public bool IsHidden
        {
            get { return _isHidden; }
            set
            {
                _isHidden = value;
                OnPropertyChanged("IsHidden");
            }
        }

        [XmlIgnore]
        public bool IsWrite
        {
            get { return _isWrite; }
            set
            {
                _isWrite = value;
                OnPropertyChanged("IsWrite");
            }
        }

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
                    if (Field.IsRequired && string.IsNullOrEmpty(Value))
                    {
                        return string.Format(_resource.GetString("uiRequiredValueMessage"), Field.Name);
                    }


                    if (Field.DataType == FieldDataType.Date && !string.IsNullOrEmpty(Value))
                    {
                        DateTime dateTime;
                        DateTime.TryParse(Value, out dateTime);

                        if (dateTime < Convert.ToDateTime(_resource.GetString("MinDate")) ||
                            dateTime > Convert.ToDateTime(_resource.GetString("MaxDate")))
                        {
                            return _resource.GetString("uiInvalidDateMessage");
                        }
                    }

                    if (!string.IsNullOrEmpty(Field.ValidationScript) && !string.IsNullOrEmpty(Value))
                    {
                        string scriptValue = Field.ValidationScript.Replace("<<Value>>", Value);
                        string script = CSharpScriptEngine.script.Replace("<<ScriptHere>>", scriptValue);
                        Assembly ass = CSharpScriptEngine.CompileCode(script);

                        if (!CSharpScriptEngine.RunScript(ass))
                        {
                            return _resource.GetString("uiEnterDataInvalid");
                        }
                    }

                    if(!string.IsNullOrEmpty(Field.ValidationPattern) && !string.IsNullOrEmpty(Value))
                    {
                        Regex regex = new Regex(Field.ValidationPattern);

                        if (!regex.IsMatch(Value))
                        {
                            return _resource.GetString("uiEnterDataInvalid");
                        }
                    }
                }

                return string.Empty;
            }
        }
    }
}
