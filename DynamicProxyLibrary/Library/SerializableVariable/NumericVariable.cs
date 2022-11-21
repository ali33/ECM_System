namespace Microsoft.Tools.TestClient.Variables
{
    using Microsoft.Tools.TestClient;
    using System;
    using System.Globalization;
    using System.Reflection;

    [Serializable]
    internal class NumericVariable : Variable
    {
        private MethodInfo parseMethod;

        internal NumericVariable(TypeMemberInfo declaredMember) : base(declaredMember)
        {
            this.parseMethod = Type.GetType(base.currentMember.TypeName).GetMethod("Parse", new Type[] { typeof(string), typeof(IFormatProvider) });
        }

        internal override object CreateObject()
        {
            return this.parseMethod.Invoke(null, new object[] { base.value, CultureInfo.CurrentUICulture });
        }

        internal override void ValidateAndCanonicalize(string input)
        {
            Type type = Type.GetType(base.currentMember.TypeName);
            object[] objArray2 = new object[2];
            objArray2[0] = input;
            object[] parameters = objArray2;
            if ((bool) type.GetMethod("TryParse", new Type[] { typeof(string), Type.GetType(base.currentMember.TypeName + "&") }).Invoke(null, parameters))
            {
                base.value = parameters[1].ToString();
            }
            else
            {
                base.value = null;
            }
        }
    }
}

