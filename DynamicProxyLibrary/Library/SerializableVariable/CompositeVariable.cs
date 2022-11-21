namespace Microsoft.Tools.TestClient.Variables
{
    using Microsoft.Tools.TestClient;
    using System;
    using System.Reflection;

    [Serializable]
    internal class CompositeVariable : Variable
    {
        internal CompositeVariable(TypeMemberInfo declaredMember) : base(declaredMember)
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
            foreach (Variable variable in base.childVariables)
            {
                PropertyInfo property = type.GetProperty(variable.Name);
                if (property != null)
                {
                    property.SetValue(obj2, variable.CreateObject(), null);
                }
                else
                {
                    type.GetField(variable.Name).SetValue(obj2, variable.CreateObject());
                }
            }
            return obj2;
        }
    }
}

