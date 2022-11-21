using Ecm.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DataEncryptor
{
    public class DataViewModel : ComponentViewModel, IDataErrorInfo
    {
        private const string DATABASE = "Database";
        private const string DATA = "Data";
        private const string PASSWORD = "Password hash";

        private string _password;
        private string _encryptedKey;
        private string _encryptedString;
        private string _passwordType;


        private RelayCommand _encryptCommand;
        private RelayCommand _decryptCommand;
        private RelayCommand _copyCommand;

        public ObservableCollection<string> PasswordTypies { get; set; }

        public DataViewModel()
        {
            PasswordTypies = new ObservableCollection<string>();
            PasswordTypies.Add(DATABASE);
            PasswordTypies.Add(DATA);
            PasswordTypies.Add(PASSWORD);
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged("Password");
            }
        }

        public string PasswordType
        {
            get { return _passwordType; }
            set
            {
                _passwordType = value;
                OnPropertyChanged("PasswordType");
            }
        }

        public string EncryptedKey
        {
            get { return _encryptedKey; }
            set
            {
                _encryptedKey = value;
                OnPropertyChanged("EncryptedKey");
            }
        }

        public string EncryptedString
        {
            get { return _encryptedString; }
            set
            {
                _encryptedString = value;
                OnPropertyChanged("EncryptedString");
            }
        }

        public ICommand EncryptCommand
        {
            get
            {
                if (_encryptCommand == null)
                {
                    _encryptCommand = new RelayCommand(p => Encrypt(), p => CanEncrypt());
                }

                return _encryptCommand;
            }
        }

        public ICommand DecryptCommand
        {
            get
            {
                if (_decryptCommand == null)
                {
                    _decryptCommand = new RelayCommand(p => Decrypt(), p => CanDecrypt());
                }

                return _decryptCommand;
            }
        }

        public ICommand CopyCommand
        {
            get
            {
                if (_copyCommand == null)
                {
                    _copyCommand = new RelayCommand(p => Copy(), p => CanCopy());
                }

                return _copyCommand;
            }
        }

        private void Copy()
        {
            Clipboard.SetText(EncryptedString);
        }

        private bool CanCopy()
        {
            return !string.IsNullOrEmpty(EncryptedString);
        }

        private void Encrypt()
        {
            switch (PasswordType.ToLower())
            {
                case "database":
                    EncryptedString = Ecm.Utility.CryptographyHelper.EncryptDatabasePasswordUsingSymmetricAlgorithm(Password, EncryptedKey);
                    break;
                case "data":
                    EncryptedString = Ecm.Utility.CryptographyHelper.EncryptUsingSymmetricAlgorithm(Password, EncryptedKey);
                    break;
                case "password hash":
                    EncryptedString = Ecm.Utility.CryptographyHelper.GenerateHash(Password, EncryptedKey);
                    break;
            }
        }

        private bool CanEncrypt()
        {
            return !string.IsNullOrEmpty(Password) && !string.IsNullOrEmpty(EncryptedKey);
        }

        private void Decrypt()
        {
            switch (PasswordType.ToLower())
            {
                case "database":
                    EncryptedString = Ecm.Utility.CryptographyHelper.DecryptDatabasePasswordUsingSymmetricAlgorithm(EncryptedString, EncryptedKey);
                    break;
                case "data":
                    EncryptedString = Ecm.Utility.CryptographyHelper.DecryptUsingSymmetricAlgorithm(EncryptedString, EncryptedKey);
                    break;
            }
        }

        private bool CanDecrypt()
        {
            return !string.IsNullOrEmpty(EncryptedString) && PasswordType.ToLower() != "password hash";
        }


        public string Error
        {
            get { return null; }
        }

        public string this[string columnName]
        {
            get
            {
                if (columnName == "Password")
                {
                    if (string.IsNullOrEmpty(Password))
                    {
                        return "Password is required";
                    }
                }

                if (columnName == "EncryptedKey")
                {
                    if (string.IsNullOrEmpty(EncryptedKey))
                    {
                        return "Encrypted key is required";
                    }
                }


                return string.Empty;
            }
        }

    }
}

