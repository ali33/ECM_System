namespace Microsoft.Tools.TestClient
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    [Serializable]
    internal class ClientEndpointInfo
    {
        private string clientTypeName;
        private string endpointConfigurationName;
        [NonSerialized]
        private string invalidReason;
        private IList<ServiceMethodInfo> methods = new List<ServiceMethodInfo>();
        private string operationContractTypeName;
        private string proxyIdentifier;
       
        private bool valid;

        internal ClientEndpointInfo(string operationContractTypeName)
        {
            this.operationContractTypeName = operationContractTypeName;
        }

        public override string ToString()
        {
            if (this.endpointConfigurationName == null)
            {
                return this.operationContractTypeName;
            }
            return string.Format(CultureInfo.CurrentUICulture, "{0} ({1})", new object[] { this.OperationContractTypeName, this.endpointConfigurationName });
        }

        internal string ClientTypeName
        {
            get
            {
                return this.clientTypeName;
            }
            set
            {
                this.clientTypeName = value;
            }
        }

        internal string EndpointConfigurationName
        {
            get
            {
                return this.endpointConfigurationName;
            }
            set
            {
                this.endpointConfigurationName = value;
            }
        }

        public string InvalidReason
        {
            get
            {
                return this.invalidReason;
            }
            set
            {
                this.invalidReason = value;
            }
        }

        internal IList<ServiceMethodInfo> Methods
        {
            get
            {
                return this.methods;
            }
        }

        internal string OperationContractTypeName
        {
            get
            {
                return this.operationContractTypeName;
            }
        }

        internal string ProxyIdentifier
        {
            get
            {
                return this.proxyIdentifier;
            }
            set
            {
                this.proxyIdentifier = value;
            }
        }

        public bool Valid
        {
            get
            {
                return this.valid;
            }
            set
            {
                this.valid = value;
            }
        }
    }
}

