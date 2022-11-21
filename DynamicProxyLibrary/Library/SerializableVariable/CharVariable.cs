namespace Microsoft.Tools.TestClient.Variables
{
    using Microsoft.Tools.TestClient;
    using System;

    [Serializable]
    internal class CharVariable : Variable
    {
        internal CharVariable(TypeMemberInfo declaredMember) : base(declaredMember)
        {
        }

        internal override object CreateObject()
        {
            return base.value[0];
        }

        internal override void ValidateAndCanonicalize(string input)
        {
            if (input.Length == 1)
            {
                base.value = input;
            }
            else
            {
                base.value = null;
            }
        }
    }
}

