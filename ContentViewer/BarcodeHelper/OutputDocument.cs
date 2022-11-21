using System;
using System.Collections.Generic;

namespace Ecm.CaptureBarcodeProcessing
{

    public class OutputDocument
    {
        private bool _isNew = true;

        public byte[] Binary { get; internal set; }

        public bool IsNew
        {
            get
            {
                return this._isNew;
            }
            set
            {
                this._isNew = value;
            }
        }

        public List<string> PageFiles { get; set; }
    }
}

