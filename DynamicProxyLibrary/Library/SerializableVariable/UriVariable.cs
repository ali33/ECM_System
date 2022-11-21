namespace Microsoft.Tools.TestClient.Variables
{
    using Microsoft.Tools.TestClient;
    using System;

    [Serializable]
    internal class UriVariable : Variable
    {
        internal UriVariable(TypeMemberInfo declaredMember) : base(declaredMember)
        {
        }

        internal override object CreateObject()
        {
            if (base.value.Equals("(null)"))
            {
                return null;
            }
            return new Uri(base.value);
        }

        internal override void ValidateAndCanonicalize(string input)
        {
            if (input.Equals("(null)"))
            {
                base.ValidateAndCanonicalize(input);
            }
            else
            {
                Uri uri;
                if (Uri.TryCreate(input, UriKind.Absolute, out uri))
                {
                    base.value = uri.ToString();
                }
                else
                {
                    base.value = null;
                }
            }
        }
    }
}

