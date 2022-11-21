namespace Microsoft.Tools.TestClient.Variables
{
    using Microsoft.Tools.TestClient;
    using System;
    using System.Globalization;

    [Serializable]
    internal class DateTimeOffsetVariable : Variable
    {
        internal DateTimeOffsetVariable(TypeMemberInfo declaredMember) : base(declaredMember)
        {
        }

        internal override object CreateObject()
        {
            return DateTimeOffset.Parse(base.value, CultureInfo.CurrentCulture);
        }

        internal override void ValidateAndCanonicalize(string input)
        {
            base.ValidateAndCanonicalize(input);
            if (base.value != null)
            {
                DateTimeOffset offset;
                if (DateTimeOffset.TryParse(input, out offset))
                {
                    base.value = offset.ToString();
                }
                else
                {
                    base.value = null;
                }
            }
        }
    }
}

