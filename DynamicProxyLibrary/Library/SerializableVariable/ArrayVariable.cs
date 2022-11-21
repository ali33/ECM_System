namespace Microsoft.Tools.TestClient.Variables
{
    using Microsoft.Tools.TestClient;
    using System;
    using System.Globalization;

    [Serializable]
    internal class ArrayVariable : ContainerVariable
    {
        internal ArrayVariable(TypeMemberInfo declaredMember) : base(declaredMember)
        {
        }

        internal override object CreateObject()
        {
            if (base.value.Equals("(null)"))
            {
                return null;
            }
            Array array = Array.CreateInstance(ClientSettings.GetType(base.currentMember.TypeName.Substring(0, base.currentMember.TypeName.Length - 2)), int.Parse(base.value.Substring("length=".Length), CultureInfo.CurrentCulture));
            int num = 0;
            base.GetChildVariables();
            if (base.childVariables != null)
            {
                foreach (Variable variable in base.childVariables)
                {
                    array.SetValue(variable.CreateObject(), num++);
                }
            }
            return array;
        }
    }
}

