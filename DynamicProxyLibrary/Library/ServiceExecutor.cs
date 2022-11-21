using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Microsoft.Tools.TestClient.Variables;
using Microsoft.Tools.TestClient;

namespace Microsoft.Tools.TestClient
{
    class ServiceExecutor : MarshalByRefObject
    {
        private static object lockObject = new object();

        private ServiceInvocationOutputs Execute(ServiceInvocationInputs inputValues)
        {
            lock (lockObject)
            {
                MethodInfo info;
                ParameterInfo[] infoArray;
                object[] objArray;
                string clientTypeName = inputValues.ClientTypeName;
                string contractTypeName = inputValues.ContractTypeName;
                string endpointConfigurationName = inputValues.EndpointConfigurationName;
                string methodName = inputValues.MethodName;
                Variable[] inputs = inputValues.GetInputs();
                System.Type contractType = ClientSettings.ClientAssembly.GetType(contractTypeName);
                try
                {
                    PopulateInputParameters(methodName, inputs, contractType, out info, out infoArray, out objArray);
                }
                catch (TargetInvocationException exception)
                {
                    return new ServiceInvocationOutputs(ExceptionType.InvalidInput, new string[] { exception.InnerException.Message }, null); //, null);
                }
                
                object obj2 = null;
                ConstructClient(clientTypeName, endpointConfigurationName, out obj2);
              
                object result = null;
                try
                {
                    result = info.Invoke(obj2, objArray);
                }
                catch (TargetInvocationException exception2)
                {
                    string[] strArray;
                    string[] strArray2;
                    Exception innerException = exception2.InnerException;
                    if (ExceptionUtility.IsFatal(innerException))
                    {
                        throw;
                    }
                    ExtractExceptionInfos(innerException, out strArray, out strArray2);
                    return new ServiceInvocationOutputs(ExceptionType.InvokeFail, strArray, strArray2); //, behavior.InterceptedXml);
                }
                IDictionary<string, object> outValues = new Dictionary<string, object>();
                int index = 0;
                foreach (ParameterInfo info3 in infoArray)
                {
                    if (info3.ParameterType.IsByRef)
                    {
                        object obj4 = objArray[index];
                        if (obj4 == null)
                        {
                            obj4 = new NullObject();
                        }
                        outValues.Add(info3.Name, obj4);
                    }
                    index++;
                }
                if (result == null)
                {
                    result = new NullObject();
                }
                return new ServiceInvocationOutputs(DataContractAnalyzer.BuildVariableInfos(result, outValues)); //, behavior.InterceptedXml);
            }
        }

        private static ServiceInvocationOutputs ConstructClient(string clientTypeName, string endpointConfigurationName, out object client)
        {
            System.Type type = ClientSettings.ClientAssembly.GetType(clientTypeName);
            try
            {
                if (endpointConfigurationName == null)
                {
                    client = type.GetConstructor(System.Type.EmptyTypes).Invoke(null);
                }
                else
                {
                    client = type.GetConstructor(new System.Type[] { typeof(string) }).Invoke(new object[] { endpointConfigurationName });
                }
            }
            catch (TargetInvocationException exception)
            {
                if (ExceptionUtility.IsFatal(exception))
                {
                    throw;
                }
                client = null;
                return new ServiceInvocationOutputs(ExceptionType.ProxyConstructFail, new string[] { exception.InnerException.Message }, new string[] { exception.InnerException.StackTrace }); //, null);
            }
            return null;
        }

        internal static ServiceInvocationOutputs ExecuteInClientDomain(ServiceInvocationInputs inputs)
        {
            ServiceExecutor executor = (ServiceExecutor)inputs.Domain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof(ServiceExecutor).FullName);
            return executor.Execute(inputs);
        }

        internal static void ExtractExceptionInfos(Exception e, out string[] messages, out string[] stackTraces)
        {
            Exception innerException;
            int num = 0;
            for (innerException = e; innerException != null; innerException = innerException.InnerException)
            {
                num++;
            }
            messages = new string[num];
            stackTraces = new string[num];
            int index = 0;
            innerException = e;
            while (innerException != null)
            {
                messages[index] = innerException.Message;
                stackTraces[index] = innerException.StackTrace;
                innerException = innerException.InnerException;
                index++;
            }
        }

        private static void PopulateInputParameters(string methodName, Variable[] inputs, System.Type contractType, out MethodInfo method, out ParameterInfo[] parameters, out object[] parameterArray)
        {
            method = contractType.GetMethod(methodName);
            parameters = method.GetParameters();
            parameterArray = new object[parameters.Length];
            IDictionary<string, object> dictionary = BuildParameters(inputs);
            int index = 0;
            foreach (ParameterInfo info in parameters)
            {
                if (info.IsIn || !info.IsOut)
                {
                    parameterArray[index] = dictionary[info.Name];
                }
                index++;
            }
        }

        private static IDictionary<string, object> BuildParameters(Variable[] inputs)
        {
            IDictionary<string, object> dictionary = new Dictionary<string, object>();
            foreach (Variable variable in inputs)
            {
                object obj2 = variable.CreateObject();
                dictionary.Add(variable.Name, obj2);
            }
            return dictionary;
        }
    }
}
