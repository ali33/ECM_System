namespace Microsoft.Tools.TestClient
{
    using Microsoft.Tools.TestClient.Variables;
    using System;
    using System.Collections.Generic;

    [Serializable]
    internal class ServiceMethodInfo
    {
        private List<TypeMemberInfo> invalidParameters;
        private bool isOneWay;
        private string methodName;
        private List<TypeMemberInfo> otherParameters = new List<TypeMemberInfo>();
        private List<TypeMemberInfo> parameters = new List<TypeMemberInfo>();

        internal ServiceMethodInfo(string methodName, bool isOneWay)
        {
            this.methodName = methodName;
            this.isOneWay = isOneWay;
        }

        private bool CheckAndSaveInvalidMembers(TypeMemberInfo member)
        {
            if (member.IsInvalid)
            {
                this.invalidParameters.Add(member);
            }
            return false;
        }

        internal Variable[] GetVariables()
        {
            Variable[] variableArray = new Variable[this.parameters.Count];
            int index = 0;
            foreach (TypeMemberInfo info in this.parameters)
            {
                variableArray[index] = VariableFactory.CreateAssociateVariable(info);
                string[] selectionList = variableArray[index].GetSelectionList();
                //if (((selectionList != null) && (selectionList.Length == 2)) && (selectionList[0] == "(null)"))
                //{
                //    variableArray[index].SetValue(selectionList[1]);
                //}
                index++;
            }
            return variableArray;
        }

       

        internal IList<TypeMemberInfo> InputParameters
        {
            get
            {
                return this.parameters;
            }
        }

        internal List<TypeMemberInfo> InvalidMembers
        {
            get
            {
                if (this.invalidParameters == null)
                {
                    this.invalidParameters = new List<TypeMemberInfo>();
                    this.parameters.Find(new Predicate<TypeMemberInfo>(this.CheckAndSaveInvalidMembers));
                    this.otherParameters.Find(new Predicate<TypeMemberInfo>(this.CheckAndSaveInvalidMembers));
                }
                return this.invalidParameters;
            }
        }

        internal bool IsOneWay
        {
            get
            {
                return this.isOneWay;
            }
        }

        internal string MethodName
        {
            get
            {
                return this.methodName;
            }
        }

        internal IList<TypeMemberInfo> OtherParameters
        {
            get
            {
                return this.otherParameters;
            }
        }

        internal bool Valid
        {
            get
            {
                return (this.InvalidMembers.Count == 0);
            }
        }
    }
}

