using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;

namespace Ecm.CloudECMClientConfiguration
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Initialize();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            if (btnRun.Text == _closeText)
            {
                Close();
            }
            else if (btnRun.Text == _showErrorText)
            {
                var showErrorForm = new ShowError(GetErrorMessage());
                showErrorForm.ShowDialog();
            }
            else if (CheckPreprequisite())
            {
                Run();
            }
            else
            {
                Close();
            }
        }

        private bool CheckPreprequisite()
        {
            var chromeProcessList = Process.GetProcessesByName("chrome");
            var ieProcessList = Process.GetProcessesByName("iexplore");
            var fireFoxProcessList = Process.GetProcessesByName("firefox");

            if (chromeProcessList.Length > 0 || ieProcessList.Length > 0 || fireFoxProcessList.Length > 0)
            {
                if (MessageBox.Show(@"Running this tool requires the opening browsers to be closed.  Do you want to continue?", @"Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    var processList = new List<Process>(chromeProcessList);
                    processList.AddRange(ieProcessList);
                    processList.AddRange(fireFoxProcessList);
                    foreach (Process process in processList)
                    {
                        process.Kill();
                    }

                    return true;
                }

                return false;
            }

            return true;
        }

        private void Initialize()
        {
        }

        private void Run()
        {
            var checkSystemWorker = new BackgroundWorker();
            checkSystemWorker.DoWork += CheckSystemWorkerDoWork;
            checkSystemWorker.RunWorkerCompleted += CheckSystemWorkerRunWorkerCompleted;
            checkSystemWorker.RunWorkerAsync();

            ConfigureFullTrust();

            var importCertWorker = new BackgroundWorker();
            importCertWorker.DoWork += ImportCertWorkerDoWork;
            importCertWorker.RunWorkerCompleted += ImportCertWorkerRunWorkerCompleted;
            importCertWorker.RunWorkerAsync();

            var clearLocalCacheWorker = new BackgroundWorker();
            clearLocalCacheWorker.DoWork += ClearLocalCacheWorkerDoWork;
            clearLocalCacheWorker.RunWorkerCompleted += ClearLocalCacheWorkerRunWorkerCompleted;
            clearLocalCacheWorker.RunWorkerAsync();

            var supportOfficeFilesWorker = new BackgroundWorker();
            supportOfficeFilesWorker.DoWork += SupportOfficeFilesWorkerDoWork;
            supportOfficeFilesWorker.RunWorkerCompleted += SupportOfficeFilesWorkerRunWorkerCompleted;
            supportOfficeFilesWorker.RunWorkerAsync();

            var repairXbapWorker = new BackgroundWorker();
            repairXbapWorker.DoWork += RepairXbapWorkerDoWork;
            repairXbapWorker.RunWorkerCompleted += RepairXbapWorkerRunWorkerCompleted;
            repairXbapWorker.RunWorkerAsync();

            EnableXbapInIE9();
        }

        private void CheckSystemWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (!_core.HasNet4OrHigher())
                {
                    throw new Exception("Your system misses .NET 4.0." + Environment.NewLine + 
                                        "Go to http://www.microsoft.com/download/en/details.aspx?id=17851 to download and install it.");
                }

            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void CheckSystemWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is Exception)
            {
                _checkSystemRequirement = TaskStatus.Fail;
                var exception = (Exception) e.Result;
                _checkSystemRequirementEx = exception.Message + Environment.NewLine + exception.StackTrace;
                lnkDownloadNET.Visible = true;
            }
            else
            {
                _checkSystemRequirement = TaskStatus.Success;
                _checkSystemRequirementEx = string.Empty;
                lnkDownloadNET.Visible = false;
            }

            UpdateTextForRunButton();
        }

        private void ConfigureFullTrust()
        {
            try
            {
                _core.RegisterFullTrustUrl(txtURL + "/*");
                _configFullTrust = TaskStatus.Success;
                _configFullTrustEx = string.Empty;
            }
            catch (Exception ex)
            {
                _configFullTrust = TaskStatus.Fail;
                _configFullTrustEx = ex.Message + Environment.NewLine + ex.StackTrace;
            }
        }

        private void ImportCertWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                _core.ImportCertificate();
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void ImportCertWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is Exception)
            {
                _importCert = TaskStatus.Fail;
                var exception = (Exception)e.Result;
                _importCertEx = exception.Message + Environment.NewLine + exception.StackTrace;
            }
            else
            {
                _importCert = TaskStatus.Success;
                _importCertEx = string.Empty;
            }

            UpdateTextForRunButton();
        }

        private void ClearLocalCacheWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                _core.ClearLocalCache();
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void ClearLocalCacheWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is Exception)
            {
                _clearLocalCache = TaskStatus.Fail;
                var exception = (Exception)e.Result;
                _clearLocalCacheEx = exception.Message + Environment.NewLine + exception.StackTrace;
            }
            else
            {
                _clearLocalCache = TaskStatus.Success;
                _clearLocalCacheEx = string.Empty;
            }

            UpdateTextForRunButton();
        }

        private void SupportOfficeFilesWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                _core.EnableClientToDisplayOfficeFilesInsideBrowsers();
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void SupportOfficeFilesWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is Exception)
            {
                _supportOffice = TaskStatus.Fail;
                var exception = (Exception)e.Result;
                _supportOfficeEx = exception.Message + Environment.NewLine + exception.StackTrace;
            }
            else
            {
                _supportOffice = TaskStatus.Success;
                _supportOfficeEx = string.Empty;
            }

            UpdateTextForRunButton();
        }

        private void RepairXbapWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                _core.RepairXbapPermission();
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void RepairXbapWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is Exception)
            {
                _repairXbap = TaskStatus.Fail;
                var exception = (Exception)e.Result;
                _repairXbapEx = exception.Message + Environment.NewLine + exception.StackTrace;
            }
            else
            {
                _repairXbap = TaskStatus.Success;
                _repairXbapEx = string.Empty;
            }

            UpdateTextForRunButton();
        }

        private void EnableXbapInIE9()
        {
            // If on IE 9 and higher, we must add the URL to the trusted sites list to be able to run XBAP clients
            // This is a new security feature in IE9.
            using (var webBrowser = new WebBrowser())
            {
                if (webBrowser.Version.Major >= 9)
                {
                    var ie9FullTrustWorker = new BackgroundWorker();
                    ie9FullTrustWorker.DoWork += EnableIE9FullTrustWorkerDoWork;
                    ie9FullTrustWorker.RunWorkerCompleted += EnableIE9FullTrustRunWorkerCompleted;
                    ie9FullTrustWorker.RunWorkerAsync();
                }
                else
                {
                    _fullTrustIE9 = TaskStatus.Success;
                    _fullTrustIE9Ex = string.Empty;
                }
            }

            UpdateTextForRunButton();
        }

        private void EnableIE9FullTrustWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                _core.EnableFullTrustForIE9(txtURL.Text);
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void EnableIE9FullTrustRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is Exception)
            {
                _fullTrustIE9 = TaskStatus.Fail;
                var exception = (Exception)e.Result;
                _fullTrustIE9Ex = exception.Message + Environment.NewLine + exception.StackTrace;
            }
            else
            {
                _fullTrustIE9 = TaskStatus.Success;
                _fullTrustIE9Ex = string.Empty;
            }

            UpdateTextForRunButton();
        }

        private void LnkDownloadLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.microsoft.com/download/en/details.aspx?id=17851");
        }

        private void UpdateTextForRunButton()
        {
            if (_checkSystemRequirement == TaskStatus.Success &&
                _configFullTrust == TaskStatus.Success && 
                _importCert == TaskStatus.Success &&
                _clearLocalCache == TaskStatus.Success &&
                _supportOffice == TaskStatus.Success &&
                _repairXbap == TaskStatus.Success &&
                _fullTrustIE9 == TaskStatus.Success)
            {
                btnRun.Text = _closeText;
            }
            else if (_checkSystemRequirement == TaskStatus.Fail ||
                     _configFullTrust == TaskStatus.Fail ||
                     _importCert == TaskStatus.Fail ||
                     _clearLocalCache == TaskStatus.Fail ||
                     _supportOffice == TaskStatus.Fail ||
                     _repairXbap == TaskStatus.Fail ||
                     _fullTrustIE9 == TaskStatus.Fail)
            {
                btnRun.Text = _showErrorText;
            }
        }

        private string GetErrorMessage()
        {
            string error = string.Empty;
            if (!string.IsNullOrEmpty(_checkSystemRequirementEx))
            {
                error += _checkSystemRequirementEx;
            }

            if (!string.IsNullOrEmpty(_configFullTrustEx))
            {
                if (!string.IsNullOrEmpty(error))
                {
                    error += Environment.NewLine + "------------------------------------------------------------------------------------------------------" + Environment.NewLine;
                }

                error += _configFullTrustEx;
            }

            if (!string.IsNullOrEmpty(_importCertEx))
            {
                if (!string.IsNullOrEmpty(error))
                {
                    error += Environment.NewLine + "------------------------------------------------------------------------------------------------------" + Environment.NewLine;
                }

                error += _importCertEx;
            }

            if (!string.IsNullOrEmpty(_clearLocalCacheEx))
            {
                if (!string.IsNullOrEmpty(error))
                {
                    error += Environment.NewLine + "------------------------------------------------------------------------------------------------------" + Environment.NewLine;
                }

                error += _clearLocalCacheEx;
            }

            if (!string.IsNullOrEmpty(_supportOfficeEx))
            {
                if (!string.IsNullOrEmpty(error))
                {
                    error += Environment.NewLine + "------------------------------------------------------------------------------------------------------" + Environment.NewLine;
                }

                error += _supportOfficeEx;
            }

            if (!string.IsNullOrEmpty(_repairXbapEx))
            {
                if (!string.IsNullOrEmpty(error))
                {
                    error += Environment.NewLine + "------------------------------------------------------------------------------------------------------" + Environment.NewLine;
                }

                error += _repairXbapEx;
            }

            if (!string.IsNullOrEmpty(_fullTrustIE9Ex))
            {
                if (!string.IsNullOrEmpty(error))
                {
                    error += Environment.NewLine + "------------------------------------------------------------------------------------------------------" + Environment.NewLine;
                }

                error += _fullTrustIE9Ex;
            }

            return error;
        }

        private readonly Core _core = new Core();

        private TaskStatus _checkSystemRequirement = TaskStatus.NotRun;
        private TaskStatus _configFullTrust = TaskStatus.NotRun;
        private TaskStatus _importCert = TaskStatus.NotRun;
        private TaskStatus _clearLocalCache = TaskStatus.NotRun;
        private TaskStatus _supportOffice = TaskStatus.NotRun;
        private TaskStatus _repairXbap = TaskStatus.NotRun;
        private TaskStatus _fullTrustIE9 = TaskStatus.NotRun;

        private string _checkSystemRequirementEx;
        private string _configFullTrustEx;
        private string _importCertEx;
        private string _clearLocalCacheEx;
        private string _supportOfficeEx;
        private string _repairXbapEx;
        private string _fullTrustIE9Ex;
        private const string _showErrorText = "Show error";
        public const string _closeText = "Close";
    }

    public enum TaskStatus
    {
        NotRun,
        Fail,
        Success
    }
}
