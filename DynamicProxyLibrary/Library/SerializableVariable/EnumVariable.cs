namespace Microsoft.Tools.TestClient.Variables
{
    using Microsoft.Tools.TestClient;
    using System;

    [Serializable]
    internal class EnumVariable : Variable
    {
        internal EnumVariable(TypeMemberInfo declaredMember) : base(declaredMember)
        {
        }

        internal override object CreateObject()
        {
            return Enum.Parse(ClientSettings.GetType(base.currentMember.TypeName), base.value);
        }
    }
}

