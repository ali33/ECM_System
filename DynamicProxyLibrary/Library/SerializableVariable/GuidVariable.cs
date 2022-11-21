namespace Microsoft.Tools.TestClient.Variables
{
    using Microsoft.Tools.TestClient;
    using System;

    [Serializable]
    internal class GuidVariable : Variable
    {
        internal GuidVariable(TypeMemberInfo declaredMember) : base(declaredMember)
        {
        }

        internal override object CreateObject()
        {
            return new Guid(base.value);
        }

        internal override void ValidateAndCanonicalize(string input)
        {
            try
            {
                base.value = new Guid(input).ToString();
            }
            catch (FormatException)
            {
                base.value = null;
                return;
            }
            catch (OverflowException)
            {
                base.value = null;
                return;
            }
            catch (ArgumentException)
            {
                base.value = null;
                return;
            }
            base.ValidateAndCanonicalize(base.value);
        }
    }
}

