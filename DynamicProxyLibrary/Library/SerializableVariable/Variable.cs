namespace Microsoft.Tools.TestClient.Variables
{
    using Microsoft.Tools.TestClient;
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    [Serializable]
    internal class Variable
    {
        protected Variable[] childVariables;
        protected TypeMemberInfo currentMember;
        protected TypeMemberInfo declaredMember;
        private static readonly Variable[] empty = new Variable[0];
        [NonSerialized]
        private bool isKey;
        private bool modifiable;
        protected string name;
        [NonSerialized]
        private Variable parent;
        private static int poolSize = 1;
        [NonSerialized]
        //protected ServiceMethodInfo serviceMethodInfo;
        protected string value;
        private static IList<Variable> variablesPool = new List<Variable>();

        internal Variable(TypeMemberInfo declaredMember)
        {
            this.modifiable = true;
            this.declaredMember = this.currentMember = declaredMember;
            this.value = this.currentMember.GetDefaultValue();
        }

        internal Variable(TypeMemberInfo declaredMember, object obj) : this(declaredMember)
        {
            this.modifiable = false;
        }

        internal virtual Variable Clone()
        {
            return null;
        }

        internal virtual bool CopyFrom(Variable variable)
        {
            if ((variable == null) || object.ReferenceEquals(this, variable))
            {
                return false;
            }
            this.value = variable.value;
            return true;
        }

        internal virtual object CreateObject()
        {
            return null;
        }

        private static int GetArrayLength(string canonicalizedValue)
        {
            return int.Parse(canonicalizedValue.Substring("length=".Length), CultureInfo.CurrentCulture);
        }

        internal Variable[] GetChildVariables()
        {
            if (string.Equals(this.value, "(null)", StringComparison.Ordinal))
            {
                return empty;
            }
            if (this.modifiable)
            {
                if (this.declaredMember.HasMembers() && ((this.childVariables == null) || (this.value != this.currentMember.TypeName)))
                {
                    this.currentMember = this.declaredMember;
                    string variableName = this.declaredMember.VariableName;
                    foreach (SerializableType type in this.declaredMember.SubTypes)
                    {
                        if (type.TypeName.Equals(this.value))
                        {
                            this.currentMember = new TypeMemberInfo(variableName, type);
                            break;
                        }
                    }
                    this.childVariables = new Variable[this.currentMember.Members.Count];
                    int index = 0;
                    foreach (TypeMemberInfo info in this.currentMember.Members)
                    {
                        this.childVariables[index] = VariableFactory.CreateAssociateVariable(info);
                        if (this.currentMember.TypeProperty.IsKeyValuePair && string.Equals(info.VariableName, "Key", StringComparison.Ordinal))
                        {
                            this.childVariables[index].IsKey = true;
                        }
                        //this.childVariables[index].SetServiceMethodInfo(this.serviceMethodInfo);
                        if (this.parent != null)
                        {
                            this.childVariables[index].SetParent(this);
                        }
                        index++;
                    }
                }
                if (this.declaredMember.IsContainer())
                {
                    int arrayLength = GetArrayLength(this.value);
                    Variable[] childVariables = this.childVariables;
                    this.childVariables = new Variable[arrayLength];
                    TypeMemberInfo memberInfo = null;
                    foreach (TypeMemberInfo info3 in this.declaredMember.Members)
                    {
                        memberInfo = info3;
                        break;
                    }
                    for (int i = 0; i < arrayLength; i++)
                    {
                        if ((childVariables != null) && (i < childVariables.Length))
                        {
                            this.childVariables[i] = childVariables[i];
                        }
                        else
                        {
                            this.childVariables[i] = VariableFactory.CreateAssociateVariable("[" + i + "]", memberInfo);
                            //this.childVariables[i].SetServiceMethodInfo(this.serviceMethodInfo);
                            if (this.declaredMember.TypeProperty.IsDictionary || (this.parent != null))
                            {
                                this.childVariables[i].SetParent(this);
                                if (this.declaredMember.TypeProperty.IsDictionary)
                                {
                                    this.childVariables[i].GetChildVariables();
                                }
                            }
                        }
                    }
                }
            }
            return this.childVariables;
        }

        internal string[] GetSelectionList()
        {
            string[] selectionList = this.declaredMember.GetSelectionList();
            if (!this.isKey || (selectionList == null))
            {
                return selectionList;
            }
            int num = Array.FindIndex<string>(selectionList, new Predicate<string>(Variable.IsNullRepresentation));
            if (num < 0)
            {
                return selectionList;
            }
            string[] strArray2 = new string[selectionList.Length - 1];
            int index = 0;
            for (int i = 0; i < strArray2.Length; i++)
            {
                if (index == num)
                {
                    index++;
                }
                strArray2[i] = selectionList[index];
                index++;
            }
            return strArray2;
        }

        internal string GetValue()
        {
            if (string.Equals(this.value, this.TypeName, StringComparison.Ordinal) && this.currentMember.HasMembers())
            {
                return this.FriendlyTypeName;
            }
            return this.value;
        }

        private static bool IsNullRepresentation(string str)
        {
            return (string.CompareOrdinal(str, "(null)") == 0);
        }

        internal static void SaveToPool(Variable variable)
        {
            if (variablesPool.Count == poolSize)
            {
                variablesPool.RemoveAt(0);
            }
            Variable item = variable.Clone();
            variablesPool.Add(item);
        }

        internal void SetChildVariables(Variable[] value)
        {
            this.childVariables = value;
        }

        private void SetParent(Variable parent)
        {
            this.parent = parent;
        }

        //internal void SetServiceMethodInfo(ServiceMethodInfo serviceMethodInfo)
        //{
        //    this.serviceMethodInfo = serviceMethodInfo;
        //}

        internal virtual void ValidateAndCanonicalize(string input)
        {
            if (input == null)
            {
                this.value = null;
            }
            else if (this.isKey && string.Equals(input, "(null)", StringComparison.Ordinal))
            {
                this.value = null;
            }
            else
            {
                this.value = input;
            }
        }

        internal string FriendlyTypeName
        {
            get
            {
                return this.declaredMember.FriendlyTypeName;
            }
        }

        internal bool IsKey
        {
            set
            {
                this.isKey = value;
                if (this.isKey && this.value.Equals("(null)", StringComparison.Ordinal))
                {
                    if (this.declaredMember.HasMembers())
                    {
                        this.value = this.TypeName;
                    }
                    if (this.TypeName.Equals("System.String", StringComparison.Ordinal))
                    {
                        this.value = string.Empty;
                    }
                }
            }
        }

        internal virtual string Name
        {
            get
            {
                if (this.name == null)
                {
                    return this.declaredMember.VariableName;
                }
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        internal string TypeName
        {
            get
            {
                return this.declaredMember.TypeName;
            }
        }

        internal static IList<Variable> VariablesPool
        {
            get
            {
                return variablesPool;
            }
        }
    }
}

