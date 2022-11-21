namespace Microsoft.Tools.TestClient.Variables
{
    using Microsoft.Tools.TestClient;
    using System;

    [Serializable]
    internal class BooleanVariable : Variable
    {
        internal BooleanVariable(TypeMemberInfo declaredMember) : base(declaredMember)
        {
        }

        internal override object CreateObject()
        {
            return bool.Parse(base.value);
        }

        internal override void ValidateAndCanonicalize(string input)
        {
            bool flag;
            if (bool.TryParse(input, out flag))
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

