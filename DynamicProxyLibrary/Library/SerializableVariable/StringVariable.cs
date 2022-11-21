namespace Microsoft.Tools.TestClient.Variables
{
    using Microsoft.Tools.TestClient;
    using System;

    [Serializable]
    internal class StringVariable : Variable
    {
        internal StringVariable(TypeMemberInfo declaredMember) : base(declaredMember)
        {
        }

        internal override object CreateObject()
        {
            if (base.value.Equals("(null)"))
            {
                return null;
            }
            return StringFormatter.FromEscapeCode(base.value);
        }

        internal override void ValidateAndCanonicalize(string input)
        {
            base.ValidateAndCanonicalize(input);
        }
    }
}

