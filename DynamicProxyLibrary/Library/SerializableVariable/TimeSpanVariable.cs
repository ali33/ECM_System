namespace Microsoft.Tools.TestClient.Variables
{
    using Microsoft.Tools.TestClient;
    using System;
    using System.ComponentModel;

    [Serializable]
    internal class TimeSpanVariable : Variable
    {
        internal TimeSpanVariable(TypeMemberInfo declaredMember) : base(declaredMember)
        {
        }

        internal override object CreateObject()
        {
            return new TimeSpanConverter().ConvertFrom(base.value);
        }

        internal override void ValidateAndCanonicalize(string input)
        {
            try
            {
                base.value = new TimeSpanConverter().ConvertFrom(input).ToString();
            }
            catch (FormatException)
            {
                base.value = null;
                return;
            }
            base.ValidateAndCanonicalize(base.value);
        }
    }
}

