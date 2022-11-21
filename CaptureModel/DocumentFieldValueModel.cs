using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ecm.Mvvm;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using System.Data;

namespace Ecm.CaptureModel
{
    public class DocumentFieldValueModel : BaseDependencyProperty
    {
        private string _value;
        private DocFieldMetaDataModel _docFieldMetaDataModel;
        private bool _showMultipleUpdate;
        private bool _isValid;

        public Guid Id { get; set; }

        public Guid DocId { get; set; }

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
                OnPropertyChanged("Value");
            }
        }

        /// <summary>
        /// The <see cref="DocumentFieldMetaData"/> object
        /// </summary>
        public DocFieldMetaDataModel Field
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
        public bool IsValid
        {
            get { return _isValid; }
            set
            {
                _isValid = value;
                OnPropertyChanged("IsValid");
            }
        }

    }
}
