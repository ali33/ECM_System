using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using System.Data.SqlClient;
using System.Configuration;
using System.Windows;
using System.Net;
using System.Net.Sockets;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;

using Ecm.Domain;
using Ecm.Utility;
using Ecm.Core;
using Ecm.Mvvm;
using log4net;

namespace Ecm.DataCreator.ViewModel
{
    public class MainViewModel : BaseDependencyProperty, IDataErrorInfo
    {
        #region Private members

        private readonly ILog _log = LogManager.GetLogger(typeof(MainViewModel));

        private const string _dataProvider = "System.Data.SqlClient";
        private const string _sysQuery = "SELECT name FROM sys.databases WHERE database_id > 4";
        private ObservableCollection<string> _databaseNames = new ObservableCollection<string>();
        private string _serverName;
        private string _username;
        private string _password;
        private string _adminUsername;
        private string _adminPassword;
        private string _confirmAdminPassword;
        private string _adUsername;
        private string _adPassword;
        private string _confirmationAdPassword;
        private string _archiveConnectionString;
        private string _captureConnectionString;
        private string _primaryConnectionString;
        private string _archiveConnectionStringNoneEncrypted;
        private string _captureConnectionStringNoneEncrypted;
        private string _primaryConnectionStringNoneEncrypted;
        private string _archiveDatabaseName;
        private string _captureDatabaseName;
        private string _primaryDatabaseName;
        private bool _isConnectionSuccess;
        private bool _isAdminPanelEnabled;
        private bool _isAdEnabled;
        private bool _hasError;
        private string _adminEmail;
        private string _adEmail;
        private bool _isCreateSample;
        private DbMode _databaseMode;
        private string _workingFolder;
        private bool _isSaveFileInFolder;
        private string _locationSaveFile;

        private bool _enablePrimaryDatabase;
        private bool _enableArchiveDatabase;
        private bool _enableCaptureDatabase;

        private RelayCommand _testConnectionCommand;
        private RelayCommand _okCommand;
        private RelayCommand _cancelCommand;
        private Action CloseView { get; set; }


        private const string _engLangId = "053F8C59-B6BB-45EA-902B-1D61FB98134F";
        private const string _wfSystemId = "40194B17-1418-42BE-AD8F-E1E52CB771D3";
        private const string _wfSystemUserName = "WorkflowSystem";
        private const string _wfSystemFullName = "Built-in work-flow system";
        private const string _wfSystemEmail = "";

        #endregion

        public MainViewModel(Action closeView)
        {
            WorkingFolder = @"C:\ECM Solutions\temp\eDocPro";
            LocationSaveFile = @"C:\ECM Solutions\Data";

            CloseView = closeView;
            DatabaseMode = DbMode.CreateNewArchive;

            PropertyChanged += ViewModelPropertyChanged;
        }

        #region Public properties

        public bool IsTestConnectionSuccess
        {
            get { return _isConnectionSuccess; }
            set
            {
                _isConnectionSuccess = value;
                OnPropertyChanged("IsTestConnectionSuccess");

                if (value)
                {
                    IsAdminPanelEnabled = true;
                }
                else
                {
                    IsAdminPanelEnabled = false;
                    IsAdEnabled = false;
                }

                DatabaseNames.Clear();
                ArchiveDatabaseName = string.Empty;
                CaptureDatabaseName = string.Empty;
                PrimaryDatabaseName = string.Empty;
            }
        }

        public ObservableCollection<string> DatabaseNames
        {
            get { return _databaseNames; }
            set
            {
                _databaseNames = value;
                OnPropertyChanged("DatabaseNames");
            }
        }

        public string ServerName
        {
            get { return _serverName; }
            set
            {
                _serverName = value;
                OnPropertyChanged("ServerName");

                IsTestConnectionSuccess = false;
            }
        }

        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged("Username");

                IsTestConnectionSuccess = false;
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged("Password");

                IsTestConnectionSuccess = false;
            }
        }

        public string AdminUsername
        {
            get { return _adminUsername; }
            set
            {
                _adminUsername = value;
                OnPropertyChanged("AdminUsername");
            }
        }

        public string AdminPassword
        {
            get { return _adminPassword; }
            set
            {
                _adminPassword = value;
                OnPropertyChanged("AdminPassword");
            }
        }

        public string ConfirmAdminPassword
        {
            get { return _confirmAdminPassword; }
            set
            {
                _confirmAdminPassword = value;
                OnPropertyChanged("ConfirmAdminPassword");
            }
        }

        public string AdUsername
        {
            get { return _adUsername; }
            set
            {
                _adUsername = value;
                OnPropertyChanged("AdUsername");
            }
        }

        public string AdPassword
        {
            get { return _adPassword; }
            set
            {
                _adPassword = value;
                OnPropertyChanged("AdPassword");
            }
        }

        public string ConfirmationAdPassword
        {
            get { return _confirmationAdPassword; }
            set
            {
                _confirmationAdPassword = value;
                OnPropertyChanged("ConfirmationAdPassword");
            }
        }

        public string ArchiveDatabaseName
        {
            get { return _archiveDatabaseName; }
            set
            {
                _archiveDatabaseName = value;
                OnPropertyChanged("ArchiveDatabaseName");

                _archiveConnectionString = string.IsNullOrEmpty(value) ? string.Empty : BuildEncryptedConnectionString(value);
                _archiveConnectionStringNoneEncrypted = string.IsNullOrEmpty(value) ? string.Empty : BuildConnectionString(value);
            }
        }

        public string CaptureDatabaseName
        {
            get { return _captureDatabaseName; }
            set
            {
                _captureDatabaseName = value;
                OnPropertyChanged("CaptureDatabaseName");

                _captureConnectionString = string.IsNullOrEmpty(value) ? string.Empty : BuildEncryptedConnectionString(value);
                _captureConnectionStringNoneEncrypted = string.IsNullOrEmpty(value) ? string.Empty : BuildConnectionString(value);
            }
        }

        public string PrimaryDatabaseName
        {
            get { return _primaryDatabaseName; }
            set
            {
                _primaryDatabaseName = value;
                OnPropertyChanged("PrimaryDatabaseName");

                _primaryConnectionString = string.IsNullOrEmpty(value) ? string.Empty : BuildEncryptedConnectionString(value);
                _primaryConnectionStringNoneEncrypted = string.IsNullOrEmpty(value) ? string.Empty : BuildConnectionString(value);
            }
        }

        public bool IsAdEnabled
        {
            get { return _isAdEnabled; }
            set
            {
                _isAdEnabled = value;
                OnPropertyChanged("IsAdEnabled");
            }
        }

        public bool IsAdminPanelEnabled
        {
            get { return _isAdminPanelEnabled; }
            set
            {
                _isAdminPanelEnabled = value;
                OnPropertyChanged("IsAdminPanelEnabled");
            }
        }

        public string AdminEmail
        {
            get { return _adminEmail; }
            set
            {
                _adminEmail = value;
                OnPropertyChanged("AdminEmail");
            }
        }

        public string AdEmail
        {
            get { return _adEmail; }
            set
            {
                _adEmail = value;
                OnPropertyChanged("AdEmail");
            }
        }

        public bool IsCreateSample
        {
            get { return _isCreateSample; }
            set
            {
                _isCreateSample = value;
                OnPropertyChanged("IsCreateSample");
            }
        }

        public string WorkingFolder
        {
            get { return _workingFolder; }
            set
            {
                _workingFolder = value;
                OnPropertyChanged("WorkingFolder");
            }
        }

        public bool IsSaveFileInFolder
        {
            get { return _isSaveFileInFolder; }
            set
            {
                _isSaveFileInFolder = value;
                OnPropertyChanged("IsSaveFileInFolder");
            }
        }
        public string LocationSaveFile
        {
            get { return _locationSaveFile; }
            set
            {
                _locationSaveFile = value;
                OnPropertyChanged("LocationSaveFile");
            }
        }


        public string Error
        {
            get { return null; }
        }

        public string this[string columnName]
        {
            get
            {
                if (columnName == "AdminEmail")
                {
                    if (AdminEmail == null)
                    {
                        return null;
                    }

                    var reg = new Regex(@"^[a-z0-9,!#\$%&'\*\+/=\?\^_`\{\|}~-]+(\.[a-z0-9,!#\$%&'\*\+/=\?\^_`\{\|}~-]+)*@[a-z0-9-]+(\.[a-z0-9-]+)*\.([a-z]{2,})$");
                    _hasError = false;
                    if (!reg.IsMatch(AdminEmail))
                    {
                        _hasError = true;
                        return Resources.uiEmailInvalid;
                    }
                }

                if (columnName == "AdEmail")
                {
                    if (AdEmail == null)
                    {
                        return null;
                    }

                    var reg = new Regex(@"^[a-z0-9,!#\$%&'\*\+/=\?\^_`\{\|}~-]+(\.[a-z0-9,!#\$%&'\*\+/=\?\^_`\{\|}~-]+)*@[a-z0-9-]+(\.[a-z0-9-]+)*\.([a-z]{2,})$");
                    _hasError = false;
                    if (!reg.IsMatch(AdEmail))
                    {
                        _hasError = true;
                        return Resources.uiEmailInvalid;
                    }
                }

                return string.Empty;
            }
        }

        public ICommand TestConnectionCommand
        {
            get { return _testConnectionCommand ?? (_testConnectionCommand = new RelayCommand(p => TestConnection(), p => CanTestConnection())); }
        }

        public ICommand OkCommand
        {
            get { return _okCommand ?? (_okCommand = new RelayCommand(p => Save(), p => CanSave())); }
        }

        public ICommand CancelCommand
        {
            get { return _cancelCommand ?? (_cancelCommand = new RelayCommand(p => Cancel())); }
        }

        private string GetExecutePath
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }

        public DbMode DatabaseMode
        {
            get { return _databaseMode; }
            set
            {
                _databaseMode = value;
                OnPropertyChanged("DatabaseMode");
            }
        }

        public bool EnablePrimaryDatabase
        {
            get { return _enablePrimaryDatabase; }
            set
            {
                _enablePrimaryDatabase = value;
                OnPropertyChanged("EnablePrimaryDatabase");
            }
        }

        public bool EnableArchiveDatabase
        {
            get { return _enableArchiveDatabase; }
            set
            {
                _enableArchiveDatabase = value;
                OnPropertyChanged("EnableArchiveDatabase");
            }
        }

        public bool EnableCaptureDatabase
        {
            get { return _enableCaptureDatabase; }
            set
            {
                _enableCaptureDatabase = value;
                OnPropertyChanged("EnableCaptureDatabase");
            }
        }

        #endregion

        #region Public methods

        public void LoadDatabaseName()
        {
            string connectionString = BuildConnectionString(string.Empty);
            try
            {
                if (DatabaseNames.Count == 0)
                {
                    DatabaseNames = new ObservableCollection<string>(GetDatabaseNames(connectionString).ToList());
                }
            }
            catch (Exception)
            {
                DialogService.ShowMessageDialog("Error while retrieving databases. Please make sure entering correct server and credential information.");
            }
        }

        #endregion

        #region Private methods

        private void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsTestConnectionSuccess" || e.PropertyName == "DatabaseMode")
            {
                EnablePrimaryDatabase = IsTestConnectionSuccess;
                EnableArchiveDatabase = IsTestConnectionSuccess &&
                                        (DatabaseMode == DbMode.CreateNewArchive || DatabaseMode == DbMode.CreateNewArchiveAndCapture || DatabaseMode == DbMode.IncludeNewArchive);
                EnableCaptureDatabase = IsTestConnectionSuccess &&
                                        (DatabaseMode == DbMode.CreateNewCapture || DatabaseMode == DbMode.CreateNewArchiveAndCapture || DatabaseMode == DbMode.IncludeNewCapture);
            }
        }

        private void Cancel()
        {
            if (CloseView != null)
            {
                CloseView();
            }
        }

        private bool CanSave()
        {
            bool isvalid = _isConnectionSuccess &&
                           !string.IsNullOrEmpty(AdminPassword) &&
                           !string.IsNullOrEmpty(AdminPassword) &&
                           !string.IsNullOrEmpty(ConfirmAdminPassword) &&
                           AdminPassword == ConfirmAdminPassword &&
                           !string.IsNullOrEmpty(PrimaryDatabaseName) &&
                           !string.IsNullOrEmpty(_primaryConnectionString);

            if (isvalid)
            {
                switch (DatabaseMode)
                {
                    case DbMode.CreateNewArchive:
                    case DbMode.IncludeNewArchive:
                        isvalid = !string.IsNullOrEmpty(ArchiveDatabaseName) && !string.IsNullOrEmpty(_archiveConnectionString);
                        break;
                    case DbMode.CreateNewArchiveAndCapture:
                        isvalid = !string.IsNullOrEmpty(ArchiveDatabaseName) && !string.IsNullOrEmpty(CaptureDatabaseName) &&
                                  !string.IsNullOrEmpty(_captureConnectionString) && !string.IsNullOrEmpty(_archiveConnectionString);
                        break;
                    case DbMode.CreateNewCapture:
                    case DbMode.IncludeNewCapture:
                        isvalid = !string.IsNullOrEmpty(CaptureDatabaseName) && !string.IsNullOrEmpty(_captureConnectionString);
                        break;
                }

                if (IsAdEnabled && isvalid)
                {
                    isvalid = !string.IsNullOrEmpty(AdUsername) &&
                              !string.IsNullOrEmpty(AdPassword) &&
                              !string.IsNullOrEmpty(ConfirmationAdPassword) &&
                              AdPassword == ConfirmationAdPassword;
                }
            }

            return isvalid && !_hasError;
        }

        private void Save()
        {
            try
            {
                string dataCreateConfig = Path.Combine(GetExecutePath, "DataCreator.exe.config");
                WriteConnectionStringToConfigFile(dataCreateConfig);
#if !DEBUG
                string webConfigFile = Path.Combine(GetExecutePath, "WCF\\web.config");
                if (File.Exists(webConfigFile))
                {
                    WriteConnectionStringToConfigFile(webConfigFile);
                }
#endif
                CreateDatabase();
                CreateAdminUser();

                if ((DatabaseMode == DbMode.CreateNewArchive || DatabaseMode == DbMode.CreateNewArchiveAndCapture || DatabaseMode == DbMode.IncludeNewArchive) && IsCreateSample)
                {
                    CreateSampleData();
                }

                CreateDefaultSetting();

                MessageBox.Show("Initialize data successfully.");

                if (CloseView != null)
                {
                    CloseView();
                }

            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
            }
        }

        private IEnumerable<string> GetDatabaseNames(string connectionString)
        {
            DataTable dt = Db.GetDataTable(_sysQuery, connectionString, _dataProvider);
            IList<string> dbNames = new List<string>();
            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (row["Name"] != null)
                    {
                        dbNames.Add(row["Name"].ToString());
                    }
                }
            }

            return dbNames;
        }

        private string BuildConnectionString(string dbName)
        {
            string result;
            if (string.IsNullOrEmpty(dbName))
            {
                result = "Data Source=" + ServerName + ";User Id=" + Username + ";Password=" + Password;
            }
            else
            {
                result = "Data Source=" + ServerName + ";Initial Catalog=" + dbName + ";User Id=" +
                          Username + ";Password=" + Password;
            }

            return result;
        }

        private string BuildEncryptedConnectionString(string dbName)
        {
            string result;
            string encryptedKey = "D4A88355-7148-4FF2-A626-151A40F57330";
            string encryptPassword = CryptographyHelper.EncryptDatabasePasswordUsingSymmetricAlgorithm(Password, encryptedKey);


            if (string.IsNullOrEmpty(dbName))
            {
                result = "Data Source=" + ServerName + ";User Id=" + Username + ";Password=" + encryptPassword;
            }
            else
            {
                result = "Data Source=" + ServerName + ";Initial Catalog=" + dbName + ";User Id=" +
                          Username + ";Password=" + encryptPassword;
            }

            return result;
        }

        private bool CanTestConnection()
        {
            return !string.IsNullOrEmpty(ServerName) &&
                   !string.IsNullOrEmpty(Username) &&
                   !string.IsNullOrEmpty(Password);
        }

        private void TestConnection()
        {
            //_ecmConnectionString = BuildConnectionString(ECMDatabaseName);
            //_primaryConnectionString = BuildConnectionString(PrimaryDatabaseName);
            //IsTestConnectionSuccess = Db.ConnectToDB(_ecmConnectionString, _dataProvider) && Db.ConnectToDB(_primaryConnectionString, _dataProvider);

            string connectionString = BuildConnectionString(string.Empty);
            IsTestConnectionSuccess = Db.ConnectToDB(connectionString, _dataProvider);

            if (IsTestConnectionSuccess)
            {
                IsAdminPanelEnabled = true;
                DialogService.ShowMessageDialog("Test connection sucessfully");
            }
            else
            {
                _archiveConnectionString = string.Empty;
                _primaryConnectionString = string.Empty;
                IsAdminPanelEnabled = false;
                DialogService.ShowMessageDialog("Test connection failed");
            }
        }

        #region generate data

        private void CreateDatabase()
        {
            try
            {
                if (DatabaseMode == DbMode.CreateNewArchive || DatabaseMode == DbMode.CreateNewCapture ||
                    DatabaseMode == DbMode.CreateNewArchiveAndCapture)
                {
                    var file = new FileInfo(Path.Combine(Environment.CurrentDirectory, "ECMPrimary.sql"));
                    string script = file.OpenText().ReadToEnd();
                    ExecuteScript(script, _primaryConnectionStringNoneEncrypted);

                    CreateDefaultEnglishLanguage();
                }

                if (DatabaseMode == DbMode.CreateNewArchive || DatabaseMode == DbMode.IncludeNewArchive ||
                    DatabaseMode == DbMode.CreateNewArchiveAndCapture)
                {
                    var file = new FileInfo(Path.Combine(Environment.CurrentDirectory, "ECMArchive.sql"));
                    var script = file.OpenText().ReadToEnd();
                    ExecuteScript(script, _archiveConnectionStringNoneEncrypted);
                }

                if (DatabaseMode == DbMode.CreateNewCapture || DatabaseMode == DbMode.IncludeNewCapture ||
                    DatabaseMode == DbMode.CreateNewArchiveAndCapture)
                {
                    CreateCapture();
                }
            }
            catch
            {
                MessageBox.Show("Initialize database fail. Please review contact administrator or review log file.");
                throw;
            }
        }

        private void CreateAdminUser()
        {
            SqlConnection primaryConnection = null;
            SqlConnection archiveConnection = null;
            SqlConnection captureConnection = null;

            try
            {
                if (!string.IsNullOrEmpty(_primaryConnectionString))
                {
                    primaryConnection = new SqlConnection(_primaryConnectionStringNoneEncrypted);
                    primaryConnection.Open();
                    string adPasswordHash = CryptographyHelper.GenerateHash(AdminUsername, AdminPassword);

                    if (DatabaseMode == DbMode.CreateNewArchive || DatabaseMode == DbMode.CreateNewArchiveAndCapture || DatabaseMode == DbMode.CreateNewCapture)
                    {
                        const string query = @"INSERT INTO [UserPrimary](UserName,Password,Email,FullName,ArchiveConnectionString,CaptureConnectionString,IsAdmin,[Type], [LanguageId]) 
                                               VALUES('{0}','{1}','{2}','{3}','{4}','{5}',{6},{7},'{8}')";

                        var comm = new SqlCommand(string.Format(query, AdminUsername, adPasswordHash, AdminEmail,
                                                                "Built-in Administrator", _archiveConnectionString, _captureConnectionString, 1, 1, _engLangId))
                        {
                            Connection = primaryConnection,
                            CommandType = CommandType.Text
                        };
                        comm.ExecuteNonQuery();

                        // Add work-flow system user
                        #region
                        var wfPasswordHash = CryptographyHelper.GenerateHash(_wfSystemUserName, _wfSystemId);

                        const string queryWf = @"
INSERT INTO [UserPrimary]
    (
        [ID], [UserName], [Password], [Email], [FullName], [ArchiveConnectionString], [CaptureConnectionString], [IsAdmin], 
        [Type], [LanguageId]
    ) 
VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', 1, 1, '{7}')";


                        var cmdWf = new SqlCommand(
                            string.Format(queryWf, _wfSystemId, _wfSystemUserName, wfPasswordHash, _wfSystemEmail, _wfSystemFullName,
                                            _archiveConnectionString, _captureConnectionString, _engLangId))
                        {
                            Connection = primaryConnection,
                            CommandType = CommandType.Text
                        };
                        cmdWf.ExecuteNonQuery();
                        #endregion
                    }
                    else if (DatabaseMode == DbMode.IncludeNewArchive)
                    {
                        const string query = @"UPDATE [UserPrimary]
                                               SET [ArchiveConnectionString] = '{0}'
                                               WHERE [IsAdmin] = 1 AND [UserName] = '{1}' AND [Password] = '{2}'";

                        var comm = new SqlCommand(string.Format(query, _archiveConnectionString, AdminUsername, adPasswordHash))
                        {
                            Connection = primaryConnection,
                            CommandType = CommandType.Text
                        };
                        comm.ExecuteNonQuery();
                    }
                    else if (DatabaseMode == DbMode.IncludeNewCapture)
                    {
                        const string query = @"UPDATE [UserPrimary]
                                               SET [CaptureConnectionString] = '{0}'
                                               WHERE [IsAdmin] = 1 AND [UserName] = '{1}' AND [Password] = '{2}'";

                        var comm = new SqlCommand(string.Format(query, _captureConnectionString, AdminUsername, adPasswordHash))
                        {
                            Connection = primaryConnection,
                            CommandType = CommandType.Text
                        };
                        comm.ExecuteNonQuery();
                    }
                }

                //if (DatabaseMode == DbMode.CreateNewArchive || DatabaseMode == DbMode.CreateNewArchiveAndCapture || DatabaseMode == DbMode.IncludeNewArchive)
                //{
                //    if (!string.IsNullOrEmpty(_archiveConnectionString))
                //    {
                //        archiveConnection = new SqlConnection(_archiveConnectionString);
                //        archiveConnection.Open();

                //        var comm = new SqlCommand("INSERT INTO [User](UserName,Type,LanguageId) VALUES('" + AdminUsername + "',1,NULL)")
                //                       {
                //                           Connection = archiveConnection,
                //                           CommandType = CommandType.Text
                //                       };

                //        comm.ExecuteNonQuery();
                //    }
                //}

                //if (DatabaseMode == DbMode.CreateNewArchiveAndCapture || DatabaseMode == DbMode.CreateNewCapture || DatabaseMode == DbMode.IncludeNewCapture)
                //{
                //    if (!string.IsNullOrEmpty(_captureConnectionString))
                //    {
                //        captureConnection = new SqlConnection(_captureConnectionString);
                //        captureConnection.Open();

                //        var comm = new SqlCommand("INSERT INTO [User](UserName,Type,LanguageId) VALUES('" + AdminUsername + "',1,NULL)")
                //                       {
                //                           Connection = captureConnection,
                //                           CommandType = CommandType.Text
                //                       };

                //        comm.ExecuteNonQuery();
                //    }
                //}
            }
            catch
            {
                MessageBox.Show("Create admin user fail. Please contact administrator or review log file.");
                throw;
            }
            finally
            {
                if (primaryConnection != null && primaryConnection.State == ConnectionState.Open)
                {
                    primaryConnection.Close();
                }

                if (archiveConnection != null && archiveConnection.State == ConnectionState.Open)
                {
                    archiveConnection.Close();
                }

                if (captureConnection != null && captureConnection.State == ConnectionState.Open)
                {
                    captureConnection.Close();
                }
            }
        }

        private void CreateSampleData()
        {
            try
            {
                _log.Info(@"Start create sample data");
                string clientHost = GetHost();
                var documentType = new DocumentType();
                var fields = new List<FieldMetaData>();
                var field = new FieldMetaData();
                // before this call, we must ensure that the correct connectionstring was saved to app.config
                _log.Info(@"Start login");

                var user = new SecurityManager().Login(AdminUsername, AdminPassword, clientHost);
                user.ClientHost = clientHost;
                _log.Info(@"Login successfully");

                SettingManager settingManager = new SettingManager(user);
                settingManager.InitSetting(new Setting { SearchResultPageSize = 100 });

                //LanguageManager languageManager = new LanguageManager(user);
                //languageManager.Save(new Domain.Language
                //{
                //    Format = "en-US",
                //    Name = "English",
                //    DateFormat = "M/d/yyyy",
                //    TimeFormat = "h:mm:ss tt",
                //    ThousandChar = ",",
                //    DecimalChar = "."
                //});
                //languageManager.Save(new Domain.Language
                //{
                //    Format = "vi-VN",
                //    Name = "Vietnamese",
                //    DateFormat = "d/M/yyyy",
                //    TimeFormat = "h:mm:ss tt",
                //    ThousandChar = ".",
                //    DecimalChar = ","
                //});

                string executionPath = Environment.CurrentDirectory;

                documentType.CreatedBy = AdminUsername;
                documentType.CreatedDate = DateTime.Now;
                documentType.Name = "Sample document type";
                documentType.ModifiedDate = DateTime.Now;

                field.DataType = "String";
                field.DisplayOrder = 1;
                field.Name = "String field";
                field.MaxLength = 80;

                fields.Add(field);
                field = new FieldMetaData { DataType = "Integer", DisplayOrder = 2, Name = "Integer field" };
                fields.Add(field);
                field = new FieldMetaData { DataType = "Decimal", DisplayOrder = 3, Name = "Decimal field" };
                fields.Add(field);
                field = new FieldMetaData { DataType = "Date", DisplayOrder = 4, Name = "Date field" };
                fields.Add(field);

                DocTypeManager docTypeManager = new DocTypeManager(user);

                documentType.FieldMetaDatas = fields;
                docTypeManager.Save(documentType);

                var fieldValues = new List<FieldValue>();
                var pages = new List<Page>();

                var document = new Document
                {
                    CreatedBy = AdminUsername,
                    CreatedDate = DateTime.Now,
                    BinaryType = "Image",
                    DocTypeId = documentType.Id,
                    DocumentType = documentType
                };

                /*foreach (var f in documentType.FieldMetaDatas)
                {
                    var fieldValue = new FieldValue { FieldId = f.Id, FieldMetaData = f };
                    switch (f.DataType)
                    {
                        case "String":
                            fieldValue.Value = "Color tiff 2 pages 240 DPI";
                            break;
                        case "Integer":
                            fieldValue.Value = "1111";
                            break;
                        case "Decimal":
                            fieldValue.Value = "1111.1";
                            break;
                        case "Date":
                            fieldValue.Value = DateTime.Now.ToShortDateString();
                            break;
                    }

                    fieldValues.Add(fieldValue);
                }

                document.FieldValues = fieldValues;
                byte[] fileTiff = File.ReadAllBytes(Path.Combine(executionPath, "SamplePictures/240 DPI Color.TIF"));
                List<byte[]> pageBinaries = ImageProcessing.SplitTiff(fileTiff);

                var page = new Page
                {
                    FileBinary = pageBinaries[0],
                    FileExtention = "tiff",
                    FileHash = CryptographyHelper.GenerateFileHash(pageBinaries[0]),
                    PageNumber = 1
                };

                pages.Add(page);
                page = new Page
                {
                    FileBinary = pageBinaries[1],
                    FileExtention = "tiff",
                    FileHash = CryptographyHelper.GenerateFileHash(pageBinaries[1]),
                    PageNumber = 2
                };

                pages.Add(page);
                document.Pages = pages;*/
                DocumentManager documentManager = new DocumentManager(user);

                // TODO Temporary comment out for passing error
                //documentManager.InsertDocument(document);

                fieldValues = new List<FieldValue>();
                document = new Document
                {
                    CreatedBy = AdminUsername,
                    CreatedDate = DateTime.Now.AddDays(1),
                    BinaryType = "Native",
                    DocTypeId = documentType.Id,
                    DocumentType = documentType
                };


                foreach (var f in documentType.FieldMetaDatas)
                {
                    var fieldValue = new FieldValue { Id = f.Id, FieldMetaData = f };
                    switch (f.DataType)
                    {
                        case "String":
                            fieldValue.Value = "Cloud ECM";
                            break;
                        case "Integer":
                            fieldValue.Value = "1112";
                            break;
                        case "Decimal":
                            fieldValue.Value = "1112.1";
                            break;
                        case "Date":
                            fieldValue.Value = DateTime.Now.AddDays(1).ToShortDateString();
                            break;
                    }

                    fieldValues.Add(fieldValue);
                }

                document.FieldValues = fieldValues;
                byte[] fileDocx = File.ReadAllBytes(Path.Combine(executionPath, "SamplePictures/CLOUD ECM.pptx"));
                var page = new Page
                {
                    FileBinary = fileDocx,
                    FileExtension = "pptx",
                    FileHash = CryptographyHelper.GenerateFileHash(fileDocx),
                    PageNumber = 1,
                    ContentLanguageCode = "eng"
                };

                pages = new List<Page> { page };
                document.Pages = pages;
                documentManager.InsertDocument(document);

                fieldValues = new List<FieldValue>();
                document = new Document
                {
                    CreatedBy = AdminUsername,
                    CreatedDate = DateTime.Now.AddDays(2),
                    BinaryType = "Image",
                    DocTypeId = documentType.Id,
                    DocumentType = documentType
                };

                foreach (var f in documentType.FieldMetaDatas)
                {
                    var fieldValue = new FieldValue { FieldId = f.Id, FieldMetaData = f };

                    switch (f.DataType)
                    {
                        case "String":
                            fieldValue.Value = "Black & White tiff";
                            break;
                        case "Integer":
                            fieldValue.Value = "1113";
                            break;
                        case "Decimal":
                            fieldValue.Value = "1113.1";
                            break;
                        case "Date":
                            fieldValue.Value = DateTime.Now.AddDays(6).ToShortDateString();
                            break;
                    }

                    fieldValues.Add(fieldValue);
                }

                document.FieldValues = fieldValues;

                byte[] fileBLTiff = File.ReadAllBytes(Path.Combine(executionPath, "SamplePictures/multiTifTest.tif"));
                List<byte[]> blTiffBinaries = ImageProcessing.SplitTiff(fileBLTiff);

                page = new Page
                {
                    FileBinary = blTiffBinaries[0],
                    FileExtension = "tiff",
                    FileHash = CryptographyHelper.GenerateFileHash(blTiffBinaries[0]),
                    PageNumber = 1,
                    DocTypeId = documentType.Id,
                    ContentLanguageCode = "eng"
                };

                pages = new List<Page> { page };
                page = new Page
                {
                    FileBinary = blTiffBinaries[1],
                    FileExtension = "tiff",
                    FileHash = CryptographyHelper.GenerateFileHash(blTiffBinaries[1]),
                    PageNumber = 2,
                    DocTypeId = documentType.Id,
                    ContentLanguageCode = "eng"
                };

                pages.Add(page);
                document.Pages = pages;
                documentManager.InsertDocument(document);

                fieldValues = new List<FieldValue>();
                document = new Document
                {
                    CreatedBy = AdminUsername,
                    CreatedDate = DateTime.Now.AddDays(2),
                    BinaryType = "Image",
                    DocTypeId = documentType.Id,
                    DocumentType = documentType
                };


                foreach (var f in documentType.FieldMetaDatas)
                {
                    var fieldValue = new FieldValue { FieldId = f.Id, FieldMetaData = f };
                    switch (f.DataType)
                    {
                        case "String":
                            fieldValue.Value = "Tulips";
                            break;
                        case "Integer":
                            fieldValue.Value = "1114";
                            break;
                        case "Decimal":
                            fieldValue.Value = "1114.1";
                            break;
                        case "Date":
                            fieldValue.Value = DateTime.Now.AddDays(2).ToShortDateString();
                            break;
                    }

                    fieldValues.Add(fieldValue);
                }

                document.FieldValues = fieldValues;
                byte[] fileJpg = File.ReadAllBytes(Path.Combine(executionPath, "SamplePictures/Tulips.jpg"));
                pages = new List<Page>();
                page = new Page
                {
                    FileBinary = fileJpg,
                    FileExtension = "jpg",
                    FileHash = CryptographyHelper.GenerateFileHash(fileJpg),
                    PageNumber = 1,
                    ContentLanguageCode = "eng"
                };

                pages.Add(page);
                document.Pages = pages;

                documentManager.InsertDocument(document);

                fieldValues = new List<FieldValue>();
                document = new Document
                {
                    CreatedBy = AdminUsername,
                    CreatedDate = DateTime.Now.AddDays(4),
                    BinaryType = "Native",
                    DocTypeId = documentType.Id,
                    DocumentType = documentType
                };

                foreach (var f in documentType.FieldMetaDatas)
                {
                    var fieldValue = new FieldValue { FieldId = f.Id, FieldMetaData = f };
                    switch (f.DataType)
                    {
                        case "String":
                            fieldValue.Value = "Enterprise Content Management";
                            break;
                        case "Integer":
                            fieldValue.Value = "1115";
                            break;
                        case "Decimal":
                            fieldValue.Value = "1115.1";
                            break;
                        case "Date":
                            fieldValue.Value = DateTime.Now.AddDays(4).ToShortDateString();
                            break;
                    }

                    fieldValues.Add(fieldValue);
                }

                document.FieldValues = fieldValues;

                pages = new List<Page>();
                byte[] filePdf = File.ReadAllBytes(Path.Combine(executionPath, "SamplePictures/ECM.pdf"));
                page = new Page
                {
                    FileBinary = filePdf,
                    FileExtension = "pdf",
                    FileHash = CryptographyHelper.GenerateFileHash(filePdf),
                    PageNumber = 1,
                    ContentLanguageCode = "eng"
                };

                pages.Add(page);
                document.Pages = pages;

                documentManager.InsertDocument(document);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Create sample data fail. Please contact administrator to supported");
                _log.Error("Create sample data fail. Please contact administrator to supported", ex);
                throw;
            }
        }

        private void CreateCapture()
        {
            var file = new FileInfo(Path.Combine(Environment.CurrentDirectory, "ECMCapture.sql"));
            string script = file.OpenText().ReadToEnd();
            ExecuteScript(script, _captureConnectionStringNoneEncrypted);

            file = new FileInfo(Path.Combine(Environment.CurrentDirectory, "SqlWorkflowInstanceStoreSchema.sql"));
            script = file.OpenText().ReadToEnd();
            ExecuteScript(script, _captureConnectionStringNoneEncrypted);

            file = new FileInfo(Path.Combine(Environment.CurrentDirectory, "SqlWorkflowInstanceStoreLogic.sql"));
            script = file.OpenText().ReadToEnd();
            ExecuteScript(script, _captureConnectionStringNoneEncrypted);

            file = new FileInfo(Path.Combine(Environment.CurrentDirectory, "WorkflowTracking.sql"));
            script = file.OpenText().ReadToEnd();
            ExecuteScript(script, _captureConnectionStringNoneEncrypted);
        }

        private void CreateDefaultEnglishLanguage()
        {
            const string query = @"INSERT INTO [Language] ([Id], [Name],[Format],[DateFormat],[TimeFormat],[ThousandChar],[DecimalChar])
                                   VALUES ('{0}','English','en-US','M/d/yyyy','h:mm:ss tt',',','.')";

            ExecuteScript(string.Format(query, _engLangId), _primaryConnectionStringNoneEncrypted);
        }

        private void CreateDefaultSetting()
        {
            try
            {
                if (DatabaseMode == DbMode.CreateNewArchive || DatabaseMode == DbMode.IncludeNewArchive || DatabaseMode == DbMode.CreateNewArchiveAndCapture)
                {
                    using (SqlConnection cnn = new SqlConnection(_archiveConnectionStringNoneEncrypted))
                    {
                        SqlCommand command = new SqlCommand();
                        string sql = @"INSERT INTO [Setting] VALUES('ServerWorkingFolder','{0}')";
                        sql = string.Format(sql, WorkingFolder);
                        command.CommandText = sql;
                        command.CommandType = CommandType.Text;
                        command.Connection = cnn;

                        cnn.Open();
                        command.ExecuteNonQuery();
                    }
                }
                if (DatabaseMode == DbMode.CreateNewCapture || DatabaseMode == DbMode.IncludeNewCapture || DatabaseMode == DbMode.CreateNewArchiveAndCapture)
                {

                    using (SqlConnection cnn = new SqlConnection(_captureConnectionStringNoneEncrypted))
                    {
                        SqlCommand command = new SqlCommand();
                        string sql = @"INSERT INTO [Setting] VALUES('ServerWorkingFolder','{0}')";
                        sql = string.Format(sql, WorkingFolder);
                        command.CommandText = sql;
                        command.CommandType = CommandType.Text;
                        command.Connection = cnn;

                        cnn.Open();
                        command.ExecuteNonQuery();
                    }
                }

                //Nhựt Nguyễn, Add config IsSaveFileInFolder
                //1 => Save In Folder
                //0 => Save In DataBase
                if (DatabaseMode == DbMode.CreateNewArchive || DatabaseMode == DbMode.IncludeNewArchive || DatabaseMode == DbMode.CreateNewArchiveAndCapture)
                {
                    using (SqlConnection cnn = new SqlConnection(_archiveConnectionStringNoneEncrypted))
                    {
                        SqlCommand command = new SqlCommand();
                        string sql = @"INSERT INTO [Setting] VALUES('IsSaveFileInFolder','{0}')";
                        sql = string.Format(sql, IsSaveFileInFolder);
                        command.CommandText = sql;
                        command.CommandType = CommandType.Text;
                        command.Connection = cnn;

                        cnn.Open();
                        command.ExecuteNonQuery();
                    }
                }
                //Nhựt Nguyễn, Add config IsSaveFileInFolder
                //1 => Save In Folder
                //0 => Save In DataBase
                if (DatabaseMode == DbMode.CreateNewCapture || DatabaseMode == DbMode.IncludeNewCapture || DatabaseMode == DbMode.CreateNewArchiveAndCapture)
                {
                    using (SqlConnection cnn = new SqlConnection(_captureConnectionStringNoneEncrypted))
                    {
                        SqlCommand command = new SqlCommand();
                        string sql = @"INSERT INTO [Setting] VALUES('IsSaveFileInFolder','{0}')";
                        sql = string.Format(sql, IsSaveFileInFolder);
                        command.CommandText = sql;
                        command.CommandType = CommandType.Text;
                        command.Connection = cnn;

                        cnn.Open();
                        command.ExecuteNonQuery();
                    }
                }

                //Nhựt Nguyễn, Add config LocationSaveFile               
                if (DatabaseMode == DbMode.CreateNewArchive || DatabaseMode == DbMode.IncludeNewArchive || DatabaseMode == DbMode.CreateNewArchiveAndCapture)
                {
                    using (SqlConnection cnn = new SqlConnection(_archiveConnectionStringNoneEncrypted))
                    {
                        SqlCommand command = new SqlCommand();
                        string sql = @"INSERT INTO [Setting] VALUES('LocationSaveFile','{0}')";
                        sql = string.Format(sql, LocationSaveFile);
                        command.CommandText = sql;
                        command.CommandType = CommandType.Text;
                        command.Connection = cnn;

                        cnn.Open();
                        command.ExecuteNonQuery();
                    }
                }

                //Nhựt Nguyễn, Add config LocationSaveFile                
                if (DatabaseMode == DbMode.CreateNewCapture || DatabaseMode == DbMode.IncludeNewCapture || DatabaseMode == DbMode.CreateNewArchiveAndCapture)
                {
                    using (SqlConnection cnn = new SqlConnection(_captureConnectionStringNoneEncrypted))
                    {
                        SqlCommand command = new SqlCommand();
                        string sql = @"INSERT INTO [Setting] VALUES('LocationSaveFile','{0}')";
                        sql = string.Format(sql, LocationSaveFile);
                        command.CommandText = sql;
                        command.CommandType = CommandType.Text;
                        command.Connection = cnn;

                        cnn.Open();
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Create default setting fail. Please contact administrator or review log file.");
                _log.Error(ex.Message, ex);
            }

        }
        private void ExecuteScript(string script, string connectionString)
        {
            var conn = new SqlConnection(connectionString);
            var server = new Server(new ServerConnection(conn));
            server.ConnectionContext.ExecuteNonQuery(script);
        }

        #endregion

        private void WriteConnectionStringToConfigFile(string targetConfigFile)
        {
            var writer = new AppSettingWriter(targetConfigFile);
            writer.SetValue("ECMConnectionString", _primaryConnectionString);

            var settings = ConfigurationManager.ConnectionStrings["ECMConnectionString"];
            var fi = typeof(ConfigurationElement).GetField("_bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fi != null)
            {
                fi.SetValue(settings, false);
            }

            settings.ConnectionString = _primaryConnectionString;
        }

        private string GetHost()
        {
            string hostName = Dns.GetHostName();
            IPAddress[] ips = Dns.GetHostAddresses(hostName);
            IPAddress host = IPAddress.None;
            foreach (IPAddress ip in ips)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    host = ip;
                    break;
                }
            }

            return hostName + " (" + host + ")";
        }

        #endregion
    }
}