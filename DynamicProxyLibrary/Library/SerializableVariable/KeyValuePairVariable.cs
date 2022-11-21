namespace Microsoft.Tools.TestClient.Variables
{
    using Microsoft.Tools.TestClient;
    using System;

    [Serializable]
    internal class KeyValuePairVariable : Variable
    {
        private const string duplicateKeyMark = "[ # ]";
        private bool isValid;

        internal KeyValuePairVariable(TypeMemberInfo declaredMember) : base(declaredMember)
        {
            this.isValid = true;
        }

        internal override object CreateObject()
        {
            base.GetChildVariables();
            Type type = DataContractAnalyzer.TypesCache[base.currentMember.TypeName];
            object[] args = new object[] { base.childVariables[0].CreateObject(), base.childVariables[1].CreateObject() };
            return Activator.CreateInstance(type, args);
        }

        internal bool IsValid
        {
            get
            {
                return this.isValid;
            }
            set
            {
                this.isValid = value;
            }
        }

        internal override string Name
        {
            get
            {
                if (this.isValid)
                {
                    return base.Name;
                }
                return "[ # ]";
            }
            set
            {
                base.Name = value;
            }
        }
    }
}

