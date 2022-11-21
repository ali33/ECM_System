using SoftekBarcodeLib3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ecm.BarcodeDomain;

namespace Ecm.CaptureBarcodeProcessing
{
    public class BarcodeUtils
    {
        private readonly BarcodeReader _barcodeReader;

        public BarcodeUtils(string workingFolder)
        {
            this.WorkingFolder = workingFolder;
            this._barcodeReader = BarcodeInitializers.Initialize(workingFolder, true);
            if (!Directory.Exists(workingFolder))
            {
                Directory.CreateDirectory(workingFolder);
            }
        }

        public bool ContainsBarcode(string filename, BarcodeType barcodeType)
        {
            int nCode = this._barcodeReader.ScanBarCode(filename);
            if (nCode <= -6)
            {
                throw new BarcodeException("License key error: either an evaluation key has expired or the license key is not valid for processing pdf documents");
            }
            if (nCode < 0)
            {
                throw new BarcodeException("Error reading barcode");
            }
            for (int i = 1; i <= nCode; i++)
            {
                string barcodeTypeString = this._barcodeReader.GetBarStringType(i);
                BarcodeType tmpBarcodeType = (BarcodeType) Enum.Parse(typeof(BarcodeType), barcodeTypeString.ToUpper());
                if (tmpBarcodeType == barcodeType)
                {
                    return true;
                }
            }
            return false;
        }

        public List<string> GetBarcodeValue(string filename, BarcodeType barcodeType, int barcodePositonToScan, string startsWith)
        {
            List<string> values = new List<string>();
            int nCode = this._barcodeReader.ScanBarCode(filename);
            if (nCode <= -6)
            {
                throw new BarcodeException("License key error: either an evaluation key has expired or the license key is not valid for processing pdf documents");
            }
            if (nCode < 0)
            {
                throw new BarcodeException("Error reading barcode");
            }
            for (int i = 1; i <= nCode; i++)
            {
                string barcodeTypeString = this._barcodeReader.GetBarStringType(i);
                BarcodeType tmpBarcodeType = (BarcodeType) Enum.Parse(typeof(BarcodeType), barcodeTypeString.ToUpper());
                if (tmpBarcodeType == barcodeType)
                {
                    string barcodeValue;
                    if (barcodePositonToScan > 0)
                    {
                        if (barcodePositonToScan == i)
                        {
                            barcodeValue = this._barcodeReader.GetBarString(i);
                            if (string.IsNullOrEmpty(startsWith) || (barcodeValue + string.Empty).StartsWith(startsWith, StringComparison.OrdinalIgnoreCase))
                            {
                                values.Add(barcodeValue);
                                return values;
                            }
                        }
                    }
                    else
                    {
                        barcodeValue = this._barcodeReader.GetBarString(i);
                        if (string.IsNullOrEmpty(startsWith) || (barcodeValue + string.Empty).StartsWith(startsWith, StringComparison.OrdinalIgnoreCase))
                        {
                            values.Add(barcodeValue);
                        }
                    }
                }
            }
            return values;
        }

        public List<BarcodeType> GetSupportedBarcodeTypes()
        {
            return Enum.GetValues(typeof(BarcodeType)).Cast<BarcodeType>().ToList<BarcodeType>();
        }

        public string WorkingFolder { get; private set; }
    }
}

