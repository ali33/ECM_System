using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Description;
using System.ServiceModel;
using System.Xml;
using System.Reflection;
using System.CodeDom.Compiler;

namespace ImageSource.ILINX.DynamicProxyLibrary
{
    internal class DynamicProxyFactory
    {
        public DynamicProxyFactory(string wsdlUrl)
        {
            DynamicProxyGenerator generator = new DynamicProxyGenerator();
            //_proxyAssembly = generator.GenerateClientProxyAssemblyForService(new Uri(wsdlUrl), out _endpoints);
            _proxyAssemblyPath = generator.GenerateClientProxyForService(new Uri(wsdlUrl), out _endpoints);
        }

        public string ProxyAssemblyPath
        {
            get
            {
                return _proxyAssemblyPath;
            }
        }

        public bool LoadAssemblyForReflectionOnly
        {
            set
            {
                _loadAssemblyForReflectionOnly = value;
            }
        }

        public Assembly ProxyAssembly 
        {
            get
            {
                if (_proxyAssembly == null)
                {
                    if (_loadAssemblyForReflectionOnly)
                    {
                        _proxyAssembly = Assembly.ReflectionOnlyLoadFrom(_proxyAssemblyPath);
                    }
                    else
                    {
                        _proxyAssembly = Assembly.LoadFrom(_proxyAssemblyPath);
                    }
                }
                //else
                //{
                //    if (!_loadAssemblyForReflectionOnly)
                //    {
                //        //if (_proxyAssembly.)
                //    }
                //}
                
                return _proxyAssembly;
            }
        }

        public IEnumerable<ServiceEndpoint> Endpoints
        {
            get
            {
                return _endpoints;
            }
        }

        public DynamicProxy CreateProxy(string contractName)
        {
            return CreateProxy(contractName, null);
        }

        public DynamicProxy CreateProxy(string contractName, string contractNamespace)
        {
            ServiceEndpoint endpoint = GetEndpoint(contractName, contractNamespace);

            return CreateProxy(endpoint);
        }

        public DynamicProxy CreateProxy(ServiceEndpoint endpoint)
        {
            Type contractType = GetContractType(endpoint.Contract.Name,
                endpoint.Contract.Namespace);

            Type proxyType = GetProxyType(contractType);

            return new DynamicProxy(proxyType, endpoint.Binding,
                    endpoint.Address);
        }

        public ServiceEndpoint GetEndpoint(string contractName)
        {
            return GetEndpoint(contractName, null);
        }

        public ServiceEndpoint GetEndpoint(string contractName, string contractNamespace)
        {
            ServiceEndpoint matchingEndpoint = null;

            foreach (ServiceEndpoint endpoint in Endpoints)
            {
                if (ContractNameMatch(endpoint.Contract, contractName) &&
                    ContractNsMatch(endpoint.Contract, contractNamespace))
                {
                    matchingEndpoint = endpoint;
                    break;
                }
            }

            if (matchingEndpoint == null)
                throw new ArgumentException(string.Format(
                    Constants.ErrorMessages.EndpointNotFound,
                    contractName, contractNamespace));

            return matchingEndpoint;
        }

        public static string ToString(IEnumerable<MetadataConversionError> importErrors)
        {
            if (importErrors != null)
            {
                StringBuilder importErrStr = new StringBuilder();

                foreach (MetadataConversionError error in importErrors)
                {
                    if (error.IsWarning)
                        importErrStr.AppendLine("Warning : " + error.Message);
                    else
                        importErrStr.AppendLine("Error : " + error.Message);
                }

                return importErrStr.ToString();
            }
            else
            {
                return null;
            }
        }

        public static string ToString(IEnumerable<CompilerError> compilerErrors)
        {
            if (compilerErrors != null)
            {
                StringBuilder builder = new StringBuilder();
                foreach (CompilerError error in compilerErrors)
                    builder.AppendLine(error.ToString());

                return builder.ToString();
            }
            else
            {
                return null;
            }
        }

        private Type GetProxyType(Type contractType)
        {
            Type clientBaseType = typeof(ClientBase<>).MakeGenericType(
                    contractType);

            Type[] allTypes = ProxyAssembly.GetTypes();
            Type proxyType = null;

            foreach (Type type in allTypes)
            {
                // Look for a proxy class that implements the service 
                // contract and is derived from ClientBase<service contract>
                if (type.IsClass && contractType.IsAssignableFrom(type)
                    && type.IsSubclassOf(clientBaseType))
                {
                    proxyType = type;
                    break;
                }
            }

            if (proxyType == null)
                throw new DynamicProxyException(string.Format(
                            Constants.ErrorMessages.ProxyTypeNotFound,
                            contractType.FullName));

            return proxyType;
        }

        private bool ContractNameMatch(ContractDescription cDesc, string name)
        {
            return (string.Compare(cDesc.Name, name, true) == 0);
        }

        private bool ContractNsMatch(ContractDescription cDesc, string ns)
        {
            return ((ns == null) ||
                    (string.Compare(cDesc.Namespace, ns, true) == 0));
        }

        private Type GetContractType(string contractName, string contractNamespace)
        {
            Type[] allTypes = ProxyAssembly.GetTypes();
            ServiceContractAttribute scAttr = null;
            Type contractType = null;
            XmlQualifiedName cName;
            foreach (Type type in allTypes)
            {
                // Is it an interface?
                if (!type.IsInterface) continue;

                // Is it marked with ServiceContract attribute?
                object[] attrs = type.GetCustomAttributes(
                    typeof(ServiceContractAttribute), false);
                if ((attrs == null) || (attrs.Length == 0)) continue;

                // is it the required service contract?
                scAttr = (ServiceContractAttribute)attrs[0];
                cName = GetContractName(type, scAttr.Name, scAttr.Namespace);

                if (string.Compare(cName.Name, contractName, true) != 0)
                    continue;

                if (string.Compare(cName.Namespace, contractNamespace,
                            true) != 0)
                    continue;

                contractType = type;
                break;
            }

            if (contractType == null)
                throw new ArgumentException(
                    Constants.ErrorMessages.UnknownContract);

            return contractType;
        }

        internal const string DefaultNamespace = "http://tempuri.org/";

        internal static XmlQualifiedName GetContractName(Type contractType, string name, string ns)
        {
            if (String.IsNullOrEmpty(name))
            {
                name = contractType.Name;
            }

            if (ns == null)
            {
                ns = DefaultNamespace;
            }
            else
            {
                ns = Uri.EscapeUriString(ns);
            }

            return new XmlQualifiedName(name, ns);
        }

        private Assembly _proxyAssembly;
        private IEnumerable<ServiceEndpoint> _endpoints;
        private string _proxyAssemblyPath;
        private bool _loadAssemblyForReflectionOnly;
    }
}