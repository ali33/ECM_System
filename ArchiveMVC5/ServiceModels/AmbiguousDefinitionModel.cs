using System.Collections.Generic;
using System.IO;
using System.Text;

using System;

namespace ArchiveMVC5.Models
{
    public class AmbiguousDefinitionModel
    {
        private Guid _languageId;
        private bool _isUnicode = true;
        private string _text;
        private LanguageModel _language;
        private Dictionary<string, string> _dictionary;

        public Guid Id { get; set; }

        public Guid LanguageId
        {
            get { return _languageId; }
            set
            {
                _languageId = value;
            }
        }

        public bool IsUnicode
        {
            get { return _isUnicode; }
            set
            {
                _isUnicode = value;
            }
        }

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
            }
        }

        public LanguageModel Language
        {
            get { return _language; }
            set
            {
                _language = value;
            }
        }

        public Dictionary<string, string> Dictionary
        {
            get
            {
                if (_dictionary == null)
                {
                    _dictionary = new Dictionary<string, string>();
                    byte[] byteArray = IsUnicode ? Encoding.UTF8.GetBytes(Text) : Encoding.ASCII.GetBytes(Text);
                    using (var me = new MemoryStream(byteArray))
                    {
                        var sr = new StreamReader(me);
                        while (true)
                        {
                            string line = sr.ReadLine();
                            if (line != null)
                            {
                                string text = line;
                                string key = text.Substring(0, text.IndexOf("=")).TrimEnd();
                                string value = text.Substring(text.IndexOf("=") + 1).TrimEnd();
                                _dictionary.Add(key, value);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }

                return _dictionary;
            }
        }
    }
}
