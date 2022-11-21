namespace Microsoft.Tools.TestClient.Variables
{
    using Microsoft.Tools.TestClient;
    using System;
    using System.Reflection;

    [Serializable]
    internal class CollectionVariable : ContainerVariable
    {
        internal CollectionVariable(TypeMemberInfo declaredMember) : base(declaredMember)
        {
        }

        internal override object CreateObject()
        {
            if (base.value.Equals("(null)"))
            {
                return null;
            }
            base.GetChildVariables();
            Type type = ClientSettings.GetType(base.currentMember.TypeName);
            object obj2 = Activator.CreateInstance(type);
            if (base.childVariables != null)
            {
                MethodInfo method = type.GetMethod("Add");
                foreach (Variable variable in base.childVariables)
                {
                    object[] parameters = new object[] { variable.CreateObject() };
                    method.Invoke(obj2, parameters);
                }
            }
            return obj2;
        }
    }
}

