namespace Microsoft.Tools.TestClient
{
    using Microsoft.Tools.TestClient.Variables;
    using System;

    [Serializable]
    internal class ServiceInvocationOutputs
    {
        private string[] exceptionMessages;
        private string[] exceptionStacks;
        private Microsoft.Tools.TestClient.ExceptionType exceptionType;
        private string responseXml;
        private Variable[] serviceInvocationResult;

        internal ServiceInvocationOutputs(Variable[] serviceInvocationResult) //, string responseXml)
        {
            this.serviceInvocationResult = serviceInvocationResult;
            //this.responseXml = responseXml;
        }

        internal ServiceInvocationOutputs(Microsoft.Tools.TestClient.ExceptionType exceptionType, string[] exceptionMessages, string[] exceptionStacks)//, string responseXml)
        {
            this.exceptionType = exceptionType;
            this.exceptionMessages = exceptionMessages;
            this.exceptionStacks = exceptionStacks;
            //this.responseXml = responseXml;
        }

        internal Variable[] GetServiceInvocationResult()
        {
            return this.serviceInvocationResult;
        }

        internal string[] ExceptionMessages
        {
            get
            {
                return this.exceptionMessages;
            }
        }

        internal string[] ExceptionStacks
        {
            get
            {
                return this.exceptionStacks;
            }
        }

        internal Microsoft.Tools.TestClient.ExceptionType ExceptionType
        {
            get
            {
                return this.exceptionType;
            }
        }

        internal string ResponseXml
        {
            get
            {
                return this.responseXml;
            }
        }
    }
}

