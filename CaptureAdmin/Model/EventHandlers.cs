using Ecm.CaptureModel;
using System;

namespace Ecm.CaptureAdmin.Model
{
    public delegate void SaveOcrTemplateEventHandler(Guid docTypeId, OCRTemplateModel ocrTemplate);
    public delegate void SaveBarcodeEventHandler(Guid docTypeId, BarcodeConfigurationModel barcode);
    public delegate void DeleteBarcodeEventHandler(Guid barcodeId);
}
