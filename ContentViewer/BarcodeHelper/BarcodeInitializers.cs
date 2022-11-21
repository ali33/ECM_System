namespace Ecm.ContentViewer.BarcodeHelper
{
    using SoftekBarcodeLib3;
    using System;
    using System.IO;
    using System.Reflection;

    public class BarcodeInitializers
    {
        private static void CopySoftekLibToWorkingFolder(string workingFolder, bool isX86)
        {
            string osVersion = "x86";
            if (!isX86)
            {
                osVersion = "x64";
            }
            string rootFolder = Path.Combine(workingFolder, osVersion);
            if (!Directory.Exists(rootFolder))
            {
                Directory.CreateDirectory(rootFolder);
            }
            try
            {
                Stream stream = Assembly.GetAssembly(typeof(BarcodeInitializers)).GetManifestResourceStream("Ecm.CaptureBarcodeProcessing." + osVersion + ".SoftekBarcode.dll");
                if (stream != null)
                {
                    byte[] binaries = new byte[stream.Length];
                    stream.Read(binaries, 0, binaries.Length);
                    stream.Close();
                    File.WriteAllBytes(Path.Combine(rootFolder, "SoftekBarcode.dll"), binaries);
                }
            }
            catch
            {
            }
        }

        private static BarcodeReader CreateBarcodeReaderObject(string workingFolder)
        {
            return new BarcodeReader(workingFolder) { 
                LicenseKey = "8Y34BCJWW6M0DCSC012JKIJ1PLC29DOC", DatabarOptions = 0xff, MultipleRead = true, QuietZoneSize = 0, LineJump = 1, ScanDirection = 15, SkewTolerance = 0, ColorProcessingLevel = 2, MinLength = 4, MaxLength = 0x3e7, ReadCode128 = true, ReadCodabar = true, ReadCode25 = true, ReadCode25ni = true, ReadCode39 = true, ReadCode93 = true, 
                ReadDatabar = true, ReadDataMatrix = true, ReadEAN13 = true, ReadEAN8 = true, ReadMicroPDF417 = true, ReadPatchCodes = true, ReadPDF417 = true, ReadShortCode128 = true, ReadUPCA = true, ReadUPCE = true
             };
        }

        public static BarcodeReader Initialize(string workingFolder, bool isX86)
        {
            if (!Directory.Exists(workingFolder))
            {
                Directory.CreateDirectory(workingFolder);
            }
            CopySoftekLibToWorkingFolder(workingFolder, isX86);
            return CreateBarcodeReaderObject(workingFolder);
        }
    }
}

