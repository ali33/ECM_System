namespace Microsoft.Tools.TestClient.Variables
{
    using Microsoft.Tools.TestClient;
    using System;

    [Serializable]
    internal class NullableVariable : Variable
    {
        internal NullableVariable(TypeMemberInfo declaredMember) : base(declaredMember)
        {
        }

        internal override object CreateObject()
        {
            if (base.value.Equals("(null)"))
            {
                return null;
            }
            base.GetChildVariables();
            return base.childVariables[0].CreateObject();
        }
    }
}

