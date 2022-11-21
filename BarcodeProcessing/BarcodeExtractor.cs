using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Ecm.Domain;
using Ecm.Model;
using SoftekBarcodeLib3;

namespace Ecm.BarcodeProcessing
{
    public class BarcodeExtractor
    {
        public static void Initialize(string workingFolder)
        {
            WorkingFolder = workingFolder;
            CopySoftekLibToWorkingFolder();
        }

        public static string WorkingFolder { get; private set; }

        public BarcodeExtractor(DocumentTypeModel documentType, Func<FieldMetaDataModel, string, DataTable> getLookupData)
        {
            DocumentType = documentType;
            GetLookupData = getLookupData;
            InitReader();
        }

        public List<DocumentModel> Process(List<string> pageFilePaths)
        {
            var foundBarcodes = new Dictionary<BarcodeTypeModel, int>();
            var outputs = new List<DocumentModel>();
            foreach (string filePath in pageFilePaths)
            {
                DocumentModel preDoc = outputs.Count > 0 ? outputs.Last() : null;
                DocumentModel postDoc = ProcessOnePage(filePath, foundBarcodes, preDoc);
                if (postDoc != preDoc)
                {
                    outputs.Add(postDoc);
                }
            }

            return outputs;
        }

        public List<DocumentModel> Process(Dictionary<string, string> pageFiles)
        {
            var foundBarcodes = new Dictionary<BarcodeTypeModel, int>();
            var outputs = new List<DocumentModel>();
            foreach (var filePath in pageFiles)
            {
                DocumentModel preDoc = outputs.Count > 0 ? outputs.Last() : null;
                DocumentModel postDoc = ProcessOnePage(filePath.Key, filePath.Value, foundBarcodes, preDoc);
                if (postDoc != preDoc)
                {
                    outputs.Add(postDoc);
                }
            }

            return outputs;
        }

        public DocumentModel ProcessOnePage(string pageFile, Dictionary<BarcodeTypeModel, int> foundBarcodes, DocumentModel preDocument)
        {
            int nCode = _barcodeReader.ScanBarCode(pageFile);
            if (nCode <= -6)
            {
                throw new BarcodeException("License key error: either an evaluation key has expired or the license key is not valid for processing pdf documents");
            }

            if (nCode < 0)
            {
                throw new BarcodeException("Error reading barcode");
            }

            if (nCode == 0)
            {
                if (preDocument == null)
                {
                    preDocument = new DocumentModel(DateTime.Now, string.Empty, DocumentType);
                }

                preDocument.Pages.Add(new PageModel { FilePath = pageFile });
            }
            else
            {
                List<BarcodeData> values;
                bool hasSeparator = DetectSeparator(nCode, out values);
                if (preDocument == null || hasSeparator)
                {
                    preDocument = new DocumentModel(DateTime.Now, string.Empty, DocumentType);
                    foundBarcodes.Clear();
                }

                bool removeThisPage = ProcessBarcodes(foundBarcodes, preDocument, values);
                if (removeThisPage)
                {
                    try
                    {
                        File.Delete(pageFile);
                    }
                    catch { }
                }
                else
                {
                    preDocument.Pages.Add(new PageModel { FilePath = pageFile });
                }
            }

            return preDocument;
        }

        public DocumentModel ProcessOnePage(string pageFile, string originalFileName, Dictionary<BarcodeTypeModel, int> foundBarcodes, DocumentModel preDocument)
        {
            int nCode = _barcodeReader.ScanBarCode(pageFile);
            if (nCode <= -6)
            {
                throw new BarcodeException("License key error: either an evaluation key has expired or the license key is not valid for processing pdf documents");
            }

            if (nCode < 0)
            {
                throw new BarcodeException("Error reading barcode");
            }

            if (nCode == 0)
            {
                if (preDocument == null)
                {
                    preDocument = new DocumentModel(DateTime.Now, string.Empty, DocumentType);
                }

                preDocument.Pages.Add(new PageModel { FilePath = pageFile, OriginalFileName = originalFileName });
            }
            else
            {
                List<BarcodeData> values;
                bool hasSeparator = DetectSeparator(nCode, out values);
                if (preDocument == null || hasSeparator)
                {
                    preDocument = new DocumentModel(DateTime.Now, string.Empty, DocumentType);
                    foundBarcodes.Clear();
                }

                bool removeThisPage = ProcessBarcodes(foundBarcodes, preDocument, values);
                if (removeThisPage)
                {
                    try
                    {
                        File.Delete(pageFile);
                    }
                    catch { }
                }
                else
                {
                    preDocument.Pages.Add(new PageModel { FilePath = pageFile, OriginalFileName = originalFileName });
                }
            }

            return preDocument;
        }

        public DocumentTypeModel DocumentType { get; private set; }

        public Func<FieldMetaDataModel, string, DataTable> GetLookupData;

        private bool DetectSeparator(int numberOfBarcodes, out List<BarcodeData> values)
        {
            values = new List<BarcodeData>();
            bool hasSeparator = false;
            for (int i = 1; i <= numberOfBarcodes; i++)
            {
                var barcodeType = (BarcodeTypeModel)Enum.Parse(typeof(BarcodeTypeModel), _barcodeReader.GetBarStringType(i).ToUpper());
                string barcodeValue = _barcodeReader.GetBarString(i);
                if (DocumentType.BarcodeConfigurations.Any(p => p.BarcodeType == barcodeType &&
                                                                p.BarcodePosition == i &&
                                                                p.IsDocumentSeparator))
                {
                    hasSeparator = true;
                }

                values.Add(new BarcodeData { Type = barcodeType, Value = barcodeValue });
            }

            return hasSeparator;
        }

        private bool ProcessBarcodes(Dictionary<BarcodeTypeModel, int> foundBarcodes, DocumentModel preDocument, List<BarcodeData> barcodeValues)
        {
            bool removeThisPage = false;
            for (int i = 1; i <= barcodeValues.Count; i++)
            {
                var barcodeType = barcodeValues[i - 1].Type;
                UpdatePositionCache(foundBarcodes, barcodeType);
                var configuration = DocumentType.BarcodeConfigurations.FirstOrDefault(p => p.BarcodeType == barcodeType &&
                                                                                           p.BarcodePosition == foundBarcodes[barcodeType]);
                if (configuration != null)
                {
                    if (!removeThisPage)
                    {
                        removeThisPage = configuration.RemoveSeparatorPage;
                    }

                    if (configuration.MapValueToFieldId != null)
                    {
                        var fieldValue = preDocument.FieldValues.FirstOrDefault(p => p.Field.Id == configuration.MapValueToFieldId);
                        if (fieldValue != null)
                        {
                            CopyDataToField(fieldValue, barcodeValues[i - 1].Value);
                            if (configuration.HasDoLookup && !string.IsNullOrEmpty(fieldValue.Value))
                            {
                                DoLookup(preDocument, fieldValue);
                            }
                        }
                    }
                }
            }

            return removeThisPage;
        }

        private void InitReader()
        {
            _barcodeReader = new BarcodeReader(WorkingFolder)
                                 {
                                     //LicenseKey = "8H34A60EC2ZS602B012J7BE7MMSX42F5",
                                     //DatabarOptions = 255,
                                     //MultipleRead = true,
                                     //QuietZoneSize = 0,
                                     //LineJump = 1,
                                     //ScanDirection = 15,
                                     //SkewTolerance = 0,
                                     //ColorProcessingLevel = 2,
                                     //MinLength = 4,
                                     //MaxLength = 999
                                     LicenseKey = "8Y34BCJWW6M0DCSC012JKIJ1PLC29DOC",
                                     DatabarOptions = 0xff,
                                     MultipleRead = true,
                                     QuietZoneSize = 0,
                                     LineJump = 1,
                                     ScanDirection = 15,
                                     SkewTolerance = 0,
                                     ColorProcessingLevel = 2,
                                     MinLength = 4,
                                     MaxLength = 0x3e7,
                                     ReadCode128 = true,
                                     ReadCodabar = true,
                                     ReadCode25 = true,
                                     ReadCode25ni = true,
                                     ReadCode39 = true,
                                     ReadCode93 = true,
                                     ReadDatabar = true,
                                     ReadDataMatrix = true,
                                     ReadEAN13 = true,
                                     ReadEAN8 = true,
                                     ReadMicroPDF417 = true,
                                     ReadPatchCodes = true,
                                     ReadPDF417 = true,
                                     ReadShortCode128 = true,
                                     ReadUPCA = true,
                                     ReadUPCE = true
                                 };

            ConfigurateReader(DocumentType.BarcodeConfigurations.Select(p => p.BarcodeType.Value).Distinct().ToList());
        }

        private void ConfigurateReader(List<BarcodeTypeModel> barCodeTypes)
        {
            _barcodeReader.ReadCode128 = barCodeTypes.Contains(BarcodeTypeModel.CODE128);
            _barcodeReader.ReadCodabar = barCodeTypes.Contains(BarcodeTypeModel.CODABAR);
            _barcodeReader.ReadCode25 = barCodeTypes.Contains(BarcodeTypeModel.CODE25);
            _barcodeReader.ReadCode25ni = barCodeTypes.Contains(BarcodeTypeModel.CODE25NI);
            _barcodeReader.ReadCode39 = barCodeTypes.Contains(BarcodeTypeModel.CODE39);
            _barcodeReader.ReadCode93 = barCodeTypes.Contains(BarcodeTypeModel.CODE93);
            _barcodeReader.ReadDatabar = barCodeTypes.Contains(BarcodeTypeModel.DATABAR);
            _barcodeReader.ReadDataMatrix = barCodeTypes.Contains(BarcodeTypeModel.DATAMATRIX);
            _barcodeReader.ReadEAN13 = barCodeTypes.Contains(BarcodeTypeModel.EAN13);
            _barcodeReader.ReadEAN8 = barCodeTypes.Contains(BarcodeTypeModel.EAN8);
            _barcodeReader.ReadMicroPDF417 = barCodeTypes.Contains(BarcodeTypeModel.MICROPDF417);
            _barcodeReader.ReadPatchCodes = barCodeTypes.Contains(BarcodeTypeModel.PATCH);
            _barcodeReader.ReadPDF417 = barCodeTypes.Contains(BarcodeTypeModel.PDF417);
            _barcodeReader.ReadShortCode128 = barCodeTypes.Contains(BarcodeTypeModel.SHORTCODE128);
            _barcodeReader.ReadUPCA = barCodeTypes.Contains(BarcodeTypeModel.UPCA);
            _barcodeReader.ReadUPCE = barCodeTypes.Contains(BarcodeTypeModel.UPCE);
        }

        private void UpdatePositionCache(Dictionary<BarcodeTypeModel, int> foundBarcodes, BarcodeTypeModel barcodeType)
        {
            if (foundBarcodes.ContainsKey(barcodeType))
            {
                foundBarcodes[barcodeType] += 1;
            }
            else
            {
                foundBarcodes.Add(barcodeType, 1);
            }
        }

        private void CopyDataToField(FieldValueModel fieldValue, string value)
        {
            if (fieldValue != null)
            {
                switch (fieldValue.Field.DataType)
                {
                    case FieldDataType.String:
                    case FieldDataType.Picklist:
                        fieldValue.Value = value;
                        break;
                    case FieldDataType.Decimal:
                        decimal decVal;
                        if (decimal.TryParse(value, out decVal))
                        {
                            fieldValue.Value = decVal.ToString();
                        }

                        break;
                    case FieldDataType.Integer:
                        int intVal;
                        if (int.TryParse(value, out intVal))
                        {
                            fieldValue.Value = intVal.ToString();
                        }
                        break;
                    case FieldDataType.Date:
                        DateTime dateValue;
                        if (DateTime.TryParseExact(value, "MM/dd/yyyy", null, DateTimeStyles.None, out dateValue))
                        {
                            fieldValue.Value = dateValue.ToString("MM/dd/yyyy");
                        }

                        break;
                }
            }
        }

        private void DoLookup(DocumentModel output, FieldValueModel fieldValue)
        {
            DataTable lookupData = GetLookupData(fieldValue.Field, fieldValue.Value);
            if (lookupData != null && lookupData.Rows.Count > 0)
            {
                foreach (FieldValueModel autoPopulatedField in output.FieldValues)
                {
                    if (lookupData.Columns.Contains(autoPopulatedField.Field.Name))
                    {
                        autoPopulatedField.Value = lookupData.Rows[0][autoPopulatedField.Field.Name] + string.Empty;
                    }
                }
            }
        }

        private static void CopySoftekLibToWorkingFolder()
        {
            const string osVersion = "x86";
            // 64 bit
            //if (IntPtr.Size == 8)
            //{
            //    osVersion = "x64";
            //}

            string rootFolder = Path.Combine(WorkingFolder, osVersion);
            if (!Directory.Exists(rootFolder))
            {
                Directory.CreateDirectory(rootFolder);
            }

            if (!File.Exists(Path.Combine(rootFolder, "SoftekBarcode.dll")))
            {
                Stream stream = Assembly.GetAssembly(typeof(BarcodeExtractor)).GetManifestResourceStream("Ecm.BarcodeProcessing." + osVersion + ".SoftekBarcode.dll");
                if (stream != null)
                {
                    var binaries = new byte[stream.Length];
                    stream.Read(binaries, 0, binaries.Length);
                    stream.Close();
                    File.WriteAllBytes(Path.Combine(rootFolder, "SoftekBarcode.dll"), binaries);
                }
            }
        }

        private BarcodeReader _barcodeReader;
    }
}
