namespace Microsoft.Tools.TestClient.Variables
{
    using Microsoft.Tools.TestClient;
    using System;
    using System.ComponentModel;

    [Serializable]
    internal class DateTimeVariable : Variable
    {
        internal DateTimeVariable(TypeMemberInfo declaredMember) : base(declaredMember)
        {
        }

        internal override object CreateObject()
        {
            return new DateTimeConverter().ConvertFrom(base.value);
        }

        internal override void ValidateAndCanonicalize(string input)
        {
            try
            {
                base.value = new DateTimeConverter().ConvertFrom(input).ToString();
            }
            catch (FormatException)
            {
                base.value = null;
            }
        }
    }
}

