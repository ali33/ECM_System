using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Win32;

namespace Ecm.CloudECMClientConfiguration
{
    /// <summary>
    /// Business class for the UI
    /// </summary>
    internal sealed class Core
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string moduleName);

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule,
                                                    [MarshalAs(UnmanagedType.LPStr)] string procName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process(IntPtr hProcess, out bool wow64Process);

        public void RegisterFullTrustUrl(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                // We are calling caspol directly instead of programmatically
                //       because this seems to resolve our issue not being to register some time on Win 7 x64

                string caspolApp = Path.Combine(Path.Combine(Environment.GetEnvironmentVariable("windir"), @"Microsoft.NET\Framework\v2.0.50727"), "caspol.exe");
                string caspolOutput = string.Empty;
                using (Process caspolExecProcess = new Process())
                {
                    caspolExecProcess.StartInfo.UseShellExecute = false;
                    caspolExecProcess.StartInfo.RedirectStandardOutput = true;
                    caspolExecProcess.StartInfo.Arguments = " -m -lg";
                    caspolExecProcess.StartInfo.FileName = caspolApp;
                    caspolExecProcess.StartInfo.Verb = "runas";
                    caspolExecProcess.Start();

                    // Call StandardOutput.ReadToEnd() before the WaitForExit() to prevent from deadlock
                    //       Detail at http://msdn.microsoft.com/en-us/library/system.diagnostics.process.standardoutput(v=vs.71).aspx
                    caspolOutput = caspolExecProcess.StandardOutput.ReadToEnd();
                    caspolExecProcess.WaitForExit();
                }

                // Check if the url is already added to full trust of machine level
                if (!string.IsNullOrEmpty(caspolOutput))
                {
                    string[] lines = caspolOutput.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string line in lines)
                    {
                        if (line.ToLower().Contains(url.ToLower()))
                        {
                            string[] items = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            string removeParam = " -q -m -remgroup " + items[0];

                            ProcessStartInfo removeInfo = new ProcessStartInfo
                            {
                                FileName = caspolApp,
                                UseShellExecute = true,
                                Verb = "runas",
                                Arguments = removeParam
                            };
                            // Provides Run as Administrator
                            Process.Start(removeInfo);
                        }
                    }
                }

                // Add FullTrust
                string param = string.Format(" -q -m -ag 1 -url \"{0}\" FullTrust -exclusive on", url);
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = caspolApp;
                info.UseShellExecute = true;
                info.Verb = "runas"; // Provides Run as Administrator
                info.Arguments = param;
                Process.Start(info);
            }
        }

        public bool HasNet4OrHigher()
        {
            var windowFolder = Environment.GetEnvironmentVariable("windir");
            var folderNet40 = Path.Combine(windowFolder, "Microsoft.NET\\Framework\\v4.0.30319\\csc.exe");
            return File.Exists(folderNet40);
        }

        public void ImportCertificate()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Ecm.CloudECMClientConfiguration.signClickOnceKey.pfx"))
            {
                if (stream != null)
                {
                    var rawData = new byte[stream.Length];
                    stream.Read(rawData, 0, rawData.Length);

                    var certificate = new X509Certificate2(rawData, "ECM");
                    var storeTrusted = new X509Store(StoreName.TrustedPublisher, StoreLocation.LocalMachine);
                    storeTrusted.Open(OpenFlags.ReadWrite);
                    storeTrusted.Add(certificate);
                    storeTrusted.Close();

                    var storeRoot = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                    storeRoot.Open(OpenFlags.ReadWrite);
                    storeRoot.Add(certificate);
                    storeRoot.Close();
                }
            }

            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Ecm.CloudECMClientConfiguration.eBusinessProcess.pfx"))
            {
                if (stream != null)
                {
                    var rawData = new byte[stream.Length];
                    stream.Read(rawData, 0, rawData.Length);

                    var certificate = new X509Certificate2(rawData, "saigonnets@20132014");
                    var storeTrusted = new X509Store(StoreName.TrustedPublisher, StoreLocation.LocalMachine);
                    storeTrusted.Open(OpenFlags.ReadWrite);
                    storeTrusted.Add(certificate);
                    storeTrusted.Close();

                    var storeRoot = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                    storeRoot.Open(OpenFlags.ReadWrite);
                    storeRoot.Add(certificate);
                    storeRoot.Close();
                }
            }

        }

        public void ClearLocalCache()
        {
            string windirVariable = Environment.GetEnvironmentVariable("windir", EnvironmentVariableTarget.Machine);
            using (Process runDllProcess = Process.Start("rundll32.exe ", windirVariable + "\\system32\\dfshim.dll CleanOnlineAppCache"))
            {
                if (runDllProcess != null)
                {
                    runDllProcess.WaitForExit();
                }
            }
        }

        public void EnableClientToRunInsideFirefox()
        {
            if (IsWindowsVistaAndHigher() && IsInstalled("Firefox", "Mozilla Firefox", out _fireFoxInstalledLocation))
            {
                string firefoxSetupFile = ExtractResourceFileToDisk("FirefoxPlugin4Win7.msi");
                string param = " /i \"" + firefoxSetupFile + "\" /quiet";

                using (Process msiexecProcess = Process.Start("msiexec", param))
                {
                    if (msiexecProcess != null)
                    {
                        msiexecProcess.WaitForExit();
                    }

                    File.Delete(firefoxSetupFile);
                }
            }
        }

        /// <summary>
        /// Chrome is not really a supported browser but this hack will allow us to run inside Chrome -- not sure when things will stop working
        /// </summary>
        public void EnableClientToRunInsideChrome()
        {
            string chromeBrowserInstalledLocation;
            if (IsInstalled("Chrome", "Google Chrome", out chromeBrowserInstalledLocation))
            {
                // Hack: Basically we need to copy Firefox files to the Chrome installation folder.
                //       Depending on what version of Firefox you have installed, some of the files below may not exist.
                //       That said, Phong was able to get Chrome running by skipping any files that Firefox does not ship
                var firefoxFiles = new[]{
                                            "js3250.dll", "mozcrt19.dll", "nspr4.dll",
                                            "nss3.dll", "nssutil3.dll", "plds4.dll",
                                            "smime3.dll", "sqlite3.dll", "ssl3.dll",
                                            "xpcom.dll", "xul.dll"
                                        };
                foreach (string file in firefoxFiles)
                {
                    string sourceFile = Path.Combine(_fireFoxInstalledLocation, file);
                    if (File.Exists(sourceFile))
                    {
                        File.Copy(sourceFile, chromeBrowserInstalledLocation + "\\" + file, true);
                    }
                }

                // Add Firefox installation path to the PATH var
                string pathVariable = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
                if (pathVariable != null && !pathVariable.ToLower().Contains(_fireFoxInstalledLocation.ToLower()))
                {
                    Environment.SetEnvironmentVariable("PATH", pathVariable + ";" + _fireFoxInstalledLocation,
                                                       EnvironmentVariableTarget.Machine);
                }
            }
        }

        public void EnableClientToDisplayOfficeFilesInsideBrowsers()
        {
            string officeRegFile = ExtractResourceFileToDisk("officeHotfix.reg");
            using (Process proc = Process.Start("regedit.exe", "/s " + officeRegFile))
            {
                if (proc != null)
                {
                    proc.WaitForExit();
                }

                File.Delete(officeRegFile);
            }
        }

        public void RepairXbapPermission()
        {
            try
            {
                string repairTool = ExtractResourceFileToDisk("XbapPermFix.exe");
                using (Process proc = Process.Start(repairTool, "/q"))
                {
                    if (proc != null)
                    {
                        proc.WaitForExit();
                    }

                    File.Delete(repairTool);
                }
            }
            catch
            {
            }
        }

        public void EnableFullTrustForIE9(string url)
        {
            string regFile = ExtractResourceFileToDisk("ie9FullTrust.reg");
            string nonHttpUrl = url.ToLower().Replace("http://", "");
            string text = File.ReadAllText(regFile);
            text = text.Replace("<<URL>>", nonHttpUrl);
            File.WriteAllText(regFile, text);
            using (Process proc = Process.Start("regedit.exe", "/s " + regFile))
            {
                if (proc != null)
                {
                    proc.WaitForExit();
                }

                File.Delete(regFile);
            }
        }

        private bool IsWindowsVistaAndHigher()
        {
            // Vista and higher should be much more compatible than previous generations of Windows (2003 and XP)
            return Environment.OSVersion.Version.Major >= 6;
        }

        private bool Is64BitOperatingSystem()
        {
            if (IntPtr.Size == 8) // 64-bit programs run only on Win64
            {
                return true;
            }
            
            // Detect whether the current process is a 32-bit process 
            // running on a 64-bit system.
            bool flag;
            return ((DoesWin32MethodExist("kernel32.dll", "IsWow64Process") &&
                     IsWow64Process(GetCurrentProcess(), out flag)) && flag);
        }

        /// <summary>
        /// The function determins whether a method exists in the export 
        /// table of a certain module.
        /// </summary>
        private bool DoesWin32MethodExist(string moduleName, string methodName)
        {
            IntPtr moduleHandle = GetModuleHandle(moduleName);
            if (moduleHandle == IntPtr.Zero)
            {
                return false;
            }

            return (GetProcAddress(moduleHandle, methodName) != IntPtr.Zero);
        }

        private bool IsInstalled(string searchKey1, string searchKey2, out string installLocation)
        {
            string keyPath = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall";

            if (Is64BitOperatingSystem())
            {
                keyPath = "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall";
            }

            RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath);
            Debug.Assert(key != null);
            string[] subKeyNames = key.GetSubKeyNames();

            // Search on machine key
            foreach (string keyName in subKeyNames)
            {
                if (keyName.StartsWith(searchKey1, StringComparison.OrdinalIgnoreCase) ||
                    keyName.StartsWith(searchKey2, StringComparison.OrdinalIgnoreCase))
                {
                    RegistryKey firefoxKey = key.OpenSubKey(keyName);
                    if (firefoxKey != null)
                    {
                        installLocation = firefoxKey.GetValue("InstallLocation").ToString();
                        return true;
                    }
                }
            }

            // Search on current user
            keyPath = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall";
            key = Registry.CurrentUser.OpenSubKey(keyPath);

            if (key != null)
            {
                subKeyNames = key.GetSubKeyNames();
                foreach (string keyName in subKeyNames)
                {
                    if (keyName.StartsWith(searchKey1, StringComparison.OrdinalIgnoreCase) ||
                        keyName.StartsWith(searchKey2, StringComparison.OrdinalIgnoreCase))
                    {
                        using (RegistryKey firefoxKey = key.OpenSubKey(keyName))
                        {
                            if (firefoxKey != null)
                            {
                                installLocation = firefoxKey.GetValue("InstallLocation").ToString();
                            }
                            else
                            {
                                installLocation = string.Empty;
                            }
                        }

                        return true;
                    }
                }
            }

            installLocation = "";
            return false;
        }

        private string ExtractResourceFileToDisk(string resourceFileName)
        {
            string extractedFile = Path.Combine(Path.GetTempPath(), resourceFileName);

            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Ecm.CloudECMClientConfiguration." + resourceFileName))
            {
                if (stream != null)
                {
                    var rawData = new byte[stream.Length];
                    stream.Read(rawData, 0, rawData.Length);
                    using (var file = new FileStream(extractedFile, FileMode.Create))
                    {
                        file.Write(rawData, 0, rawData.Length);
                    }
                }
            }

            //Debug.Assert(File.Exists(extractedFile));
            return extractedFile;
        }

        private string _fireFoxInstalledLocation = "";
    }
}