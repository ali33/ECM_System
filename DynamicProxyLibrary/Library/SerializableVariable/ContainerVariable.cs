namespace Microsoft.Tools.TestClient.Variables
{
    using Microsoft.Tools.TestClient;
    using System;

    [Serializable]
    internal class ContainerVariable : Variable
    {
        internal ContainerVariable(TypeMemberInfo declaredMember) : base(declaredMember)
        {
        }

        internal override void ValidateAndCanonicalize(string input)
        {
            base.ValidateAndCanonicalize(input);
            int result = -1;
            if ((base.value != null) && !input.Equals("(null)"))
            {
                if (!input.TrimStart(new char[] { ' ' }).StartsWith("length", StringComparison.OrdinalIgnoreCase))
                {
                    base.value = null;
                }
                else
                {
                    base.value = input.Replace(" ", "");
                    if (base.value.StartsWith("length=", StringComparison.OrdinalIgnoreCase))
                    {
                        input = base.value.Substring("length=".Length);
                        if (int.TryParse(input, out result) && (result >= 0))
                        {
                            base.value = "length=" + input;
                            return;
                        }
                    }
                    base.value = null;
                }
            }
        }
    }
}

