namespace Microsoft.Tools.TestClient.Variables
{
    using Microsoft.Tools.TestClient;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [Serializable]
    internal class DictionaryVariable : ContainerVariable
    {
        internal DictionaryVariable(TypeMemberInfo declaredMember) : base(declaredMember)
        {
        }

        internal static object CreateAndValidateDictionary(string typeName, Variable[] variables, out List<int> invalidList)
        {
            Type type = DataContractAnalyzer.TypesCache[typeName];
            object obj2 = Activator.CreateInstance(type);
            invalidList = new List<int>();
            if (variables != null)
            {
                MethodInfo method = type.GetMethod("Add");
                if (method == null)
                {
                    return null;
                }
                int item = 0;
                foreach (KeyValuePairVariable variable in variables)
                {
                    if ((variable != null) && variable.IsValid)
                    {
                        object[] parameters = new object[2];
                        Variable[] childVariables = variable.GetChildVariables();
                        parameters[0] = childVariables[0].CreateObject();
                        parameters[1] = childVariables[1].CreateObject();
                        try
                        {
                            method.Invoke(obj2, parameters);
                        }
                        catch (TargetInvocationException)
                        {
                            invalidList.Add(item);
                        }
                        item++;
                    }
                }
            }
            return obj2;
        }

        internal override object CreateObject()
        {
            if (base.value.Equals("(null)"))
            {
                return null;
            }
            base.GetChildVariables();
            List<int> invalidList = null;
            return CreateAndValidateDictionary(base.currentMember.TypeName, base.childVariables, out invalidList);
        }

        private void Validate()
        {
            if (base.childVariables != null)
            {
                foreach (KeyValuePairVariable variable in base.childVariables)
                {
                    variable.IsValid = true;
                }
                //foreach (int num in ServiceExecutor.ValidateDictionary(this, base.serviceMethodInfo.Endpoint.ServiceProject.ClientDomain))
                //{
                //    ((KeyValuePairVariable) base.childVariables[num]).IsValid = false;
                //}
            }
        }

        internal override void ValidateAndCanonicalize(string input)
        {
            base.ValidateAndCanonicalize(input);
            if (base.value != null)
            {
                base.GetChildVariables();
                this.Validate();
            }
        }

        internal IList<int> ValidateDictionary()
        {
            List<int> invalidList = null;
            CreateAndValidateDictionary(base.TypeName, base.childVariables, out invalidList);
            return invalidList;
        }
    }
}

