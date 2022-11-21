namespace Microsoft.Tools.TestClient
{
    using Microsoft.Tools.TestClient.Variables;
    using System;

    [Serializable]
    internal class ServiceInvocationInputs
    {
        private string clientTypeName;
        private string contractTypeName;
        [NonSerialized]
        private AppDomain domain;
        private Variable[] inputs;
        private string methodName;
        private string endpointConfigurationName;

        internal ServiceInvocationInputs(AppDomain ClientDomain, string ClientTypeName, string ContractTypeName,
            string MethodName,
            Variable[] inputs)
        {
            //ServiceMethodInfo method = testCase.Method;
            //ClientEndpointInfo endpoint = method.Endpoint;
            //ServiceProject serviceProject = endpoint.ServiceProject;
            this.clientTypeName = ClientTypeName;
            this.contractTypeName = ContractTypeName;
            //this.endpointConfigurationName = endpoint.EndpointConfigurationName;
            endpointConfigurationName = string.Empty;
            this.methodName = MethodName;
            this.inputs = inputs;
            this.domain = ClientDomain;
        }

        internal Variable[] GetInputs()
        {
            return this.inputs;
        }

        internal string ClientTypeName
        {
            get
            {
                return this.clientTypeName;
            }
        }

        internal string ContractTypeName
        {
            get
            {
                return this.contractTypeName;
            }
        }

        internal string EndpointConfigurationName
        {
            get
            {
                return this.endpointConfigurationName;
            }
        }

        internal AppDomain Domain
        {
            get
            {
                return this.domain;
            }
        }

        internal string MethodName
        {
            get
            {
                return this.methodName;
            }
        }
    }
}

