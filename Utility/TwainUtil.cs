using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using Ecm.Utility.Exceptions;
using Ecm.Utility.Victor;

namespace Ecm.Utility
{
    public class TwainUtil
    {
        #region Member variables

        private string _appName;

        private Action<string> _completeEachScanedPage;

        private int _hWnd;

        private List<string> _scanFiles;

        private string _scanFolder;

        #endregion

        #region Public methods

        public void Inititlize(int hWnd, string scanFolder, string applicationName, Action<string> completeEachScanedPage)
        {
            _hWnd = hWnd;
            _completeEachScanedPage = completeEachScanedPage;
            _appName = applicationName;
            _scanFolder = scanFolder;
            _scanFiles = new List<string>();
            VicWin.TWsetproductname(ref applicationName);
        }

        public void Scan(bool showSettingsDialog)
        {
            int rCode = VicWin.TWopen(_hWnd);
            if (rCode != VicWin.NO_ERROR)
            {
                ProcessError(rCode);
                return;
            }

            try
            {
                var scanRc = new VicWin.RECT { left = 0, top = 0, right = 8500, bottom = 11000 }; // lf,tp,rt,bt

                if (showSettingsDialog)
                {
                    DoScanningWithDialog(scanRc);
                }
                else
                {
                    DoScanning(scanRc);
                }
            }
            finally
            {
                VicWin.TWclose();
            }
        }

        public void SetDefaultScanner()
        {
            int rCode = VicWin.TWselectsource(_hWnd);
            if (rCode != VicWin.NO_ERROR)
            {
                ProcessError(rCode);
            }
        }

        #endregion

        #region Helper methods

        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        private static extern void CopyBmh(ref VicWin.BITMAPINFOHEADER des, int src, int count);

        private void ContinueScan(VicWin.RECT scanRc)
        {
            MessageBoxResult result = MessageBox.Show("Continue scanning?", _appName, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                DoScanning(scanRc);
            }
        }

        private void DoScanning(VicWin.RECT scanRc)
        {
            var scannedImage = new VicWin.imgdes();

            try
            {
                int rCode = VicWin.TWscanmultipleimagesex(
                    _hWnd, ref scannedImage, ref scanRc, 0, OnePageScannedCompleteCallback);
                if (rCode == VicWin.NO_ERROR || rCode == VicWin.TWAIN_NO_PAPER)
                {
                    ContinueScan(scanRc);
                }
                else
                {
                    ProcessError(rCode);
                }
            }
            finally
            {
                VicWin.freeimage(ref scannedImage);
            }
        }

        private void DoScanningWithDialog(VicWin.RECT scanRc)
        {
            var scannedImage = new VicWin.imgdes();

            try
            {
                int rCode = VicWin.TWscanmultipleimages(_hWnd, ref scannedImage, OnePageScannedCompleteCallback);
                if (rCode == VicWin.NO_ERROR || rCode == VicWin.TWAIN_NO_PAPER)
                {
                    ContinueScan(scanRc);
                }
                else
                {
                    ProcessError(rCode);
                }
            }
            finally
            {
                VicWin.freeimage(ref scannedImage);
            }
        }

        private string GetErrorMessage(int errorCode)
        {
            switch (errorCode)
            {
                case VicWin.TWAIN_BUSY:
                    return "TWAIN module is busy";
                case VicWin.TWAIN_ERR:
                    switch (VicWin.TWgeterror())
                    {
                        case VicWin.TWCC_BUMMER:
                            return "Failure due to unknown causes";
                        case VicWin.TWCC_LOWMEMORY:
                            return "Not enough memory to perform operation";
                        case VicWin.TWCC_NODS:
                            return "No Data Source";
                        case VicWin.TWCC_MAXCONNECTIONS:
                            return "Source already in use";
                        case VicWin.TWCC_BADCAP:
                            return "Unknown capability requested";
                        case VicWin.TWCC_BADPROTOCOL:
                            return "Unrecognized DataGroup / Data ArgType / Msg combination";
                        case VicWin.TWCC_BADVALUE:
                            return "Parameter out of range";
                        case VicWin.TWCC_BADDEST:
                            return "Unknown destination App/Src in DSM_Entry";
                        case VicWin.TWAIN_NODSM:
                            return "TWAIN Source Manager not found";
                        default:
                            return string.Empty;
                    }
                case VicWin.TWAIN_NODS:
                    return "Could not open TWAIN Data Source";
                case VicWin.TWAIN_NODSM:
                    return "Could not open TWAIN Source Manager";
                case VicWin.BAD_MEM:
                    return "Insufficient memory";
                default:
                    return string.Empty;
            }
        }

        private int OnePageScannedCompleteCallback(ref VicWin.imgdes resimg)
        {
            try
            {
                var info = new VicWin.BITMAPINFOHEADER();
                CopyBmh(ref info, resimg.bmh, 40);

                var xres = (int)(info.biXPelsPerMeter / 39.37); // convert meter to inch, 1 meter = 39.37 inch
                var yres = (int)(info.biXPelsPerMeter / 39.37); // convert meter to inch, 1 meter = 39.37 inch
                VicWin.tiffsetxyresolution(xres, yres, 2);
                if (!Directory.Exists(_scanFolder))
                {
                    Directory.CreateDirectory(_scanFolder);
                }

                string fileName = Path.Combine(_scanFolder, "scan_page_" + Guid.NewGuid().GetHashCode() + "_" + (_scanFiles.Count + 1) + ".tiff");
                int rCode;
                if (info.biBitCount == 1)
                {
                    rCode = VicWin.savetif(ref fileName, ref resimg, 4);
                    if (rCode != VicWin.NO_ERROR)
                    {
                        ProcessError(rCode);
                    }
                }
                else
                {
                    rCode = VicWin.savetif(ref fileName, ref resimg, 6);
                    if (rCode != VicWin.NO_ERROR)
                    {
                        ProcessError(rCode);
                    }
                }

                _scanFiles.Add(fileName);
                _completeEachScanedPage(fileName);

                return VicWin.NO_ERROR;
            }
            finally
            {
                VicWin.freeimage(ref resimg);
            }
        }

        private void ProcessError(int errorCode)
        {
            string errMsg = GetErrorMessage(errorCode);

            if (!string.IsNullOrEmpty(errMsg))
            {
                throw new TwainException(errMsg);
            }
        }

        #endregion
    }
}