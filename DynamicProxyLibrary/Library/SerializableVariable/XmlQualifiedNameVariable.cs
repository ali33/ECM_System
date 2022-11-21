namespace Microsoft.Tools.TestClient.Variables
{
    using Microsoft.Tools.TestClient;
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;

    [Serializable]
    internal class XmlQualifiedNameVariable : Variable
    {
        internal XmlQualifiedNameVariable(TypeMemberInfo declaredMember) : base(declaredMember)
        {
        }

        internal override object CreateObject()
        {
            XmlQualifiedName name;
            if (base.value.Equals("(null)"))
            {
                return null;
            }
            if (!TryParseXmlQualifiedName(base.value, out name))
            {
                name = null;
            }
            return name;
        }

        private static bool TryParseXmlQualifiedName(string stringRepresentation, out XmlQualifiedName result)
        {
            int length = stringRepresentation.LastIndexOf(":", StringComparison.Ordinal);
            if (length == -1)
            {
                result = new XmlQualifiedName(stringRepresentation);
                return true;
            }
            string ns = stringRepresentation.Substring(0, length);
            string str2 = stringRepresentation.Substring(length + 1);
            if (string.IsNullOrEmpty(str2))
            {
                result = null;
                return false;
            }
            result = new XmlQualifiedName(str2, ns);
            return true;
        }

        internal override void ValidateAndCanonicalize(string input)
        {
            if (input.Equals("(null)"))
            {
                base.ValidateAndCanonicalize(input);
            }
            else if (base.value != null)
            {
                XmlQualifiedName name;
                if (TryParseXmlQualifiedName(input, out name))
                {
                    base.value = name.ToString();
                }
                else
                {
                    base.value = null;
                }
            }
        }
    }
}

