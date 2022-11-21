namespace Microsoft.Tools.TestClient
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Text;

    [Serializable]
    internal class SerializableType
    {
        private string dataSetSchema;
        private string[] enumChoices;
        private string friendlyName;
        private bool isInvalid;
        internal const string LengthRepresentation = "length=";
        private ICollection<TypeMemberInfo> members = new List<TypeMemberInfo>();
        internal const string NullRepresentation = "(null)";
        private static List<string> numericTypes = new List<string>(new string[] { typeof(short).FullName, typeof(int).FullName, typeof(long).FullName, typeof(ushort).FullName, typeof(uint).FullName, typeof(ulong).FullName, typeof(byte).FullName, typeof(sbyte).FullName, typeof(float).FullName, typeof(double).FullName, typeof(decimal).FullName });
        private ICollection<SerializableType> subTypes = new List<SerializableType>();
        private string typeName;
        private Microsoft.Tools.TestClient.TypeProperty typeProperty = new Microsoft.Tools.TestClient.TypeProperty();

        internal SerializableType(Type type)
        {
            this.typeName = type.FullName;
            if (type.IsEnum)
            {
                this.enumChoices = Enum.GetNames(type);
                this.typeProperty.IsEnum = true;
            }
            else if (numericTypes.Contains(this.typeName))
            {
                this.typeProperty.IsNumeric = true;
            }
            else if (DataContractAnalyzer.IsDataSet(type))
            {
                this.typeProperty.IsDataSet = true;
                this.dataSetSchema = (Activator.CreateInstance(type) as DataSet).GetXmlSchema();
            }
            else if (type.IsArray)
            {
                this.typeProperty.IsArray = true;
            }
            else if (DataContractAnalyzer.IsNullableType(type))
            {
                this.typeProperty.IsNullable = true;
            }
            else if (DataContractAnalyzer.IsCollectionType(type))
            {
                this.typeProperty.IsCollection = true;
            }
            else if (DataContractAnalyzer.IsDictionaryType(type))
            {
                this.typeProperty.IsDictionary = true;
            }
            else if (DataContractAnalyzer.IsKeyValuePairType(type))
            {
                this.typeProperty.IsKeyValuePair = true;
            }
            else if (DataContractAnalyzer.IsSupportedType(type))
            {
                this.typeProperty.IsComposite = true;
                if (type.IsValueType)
                {
                    this.typeProperty.IsStruct = true;
                }
            }
        }

        private void ComposeFriendlyName()
        {
            int index = this.TypeName.IndexOf('`');
            if (index > -1)
            {
                StringBuilder builder = new StringBuilder(this.TypeName.Substring(0, index));
                builder.Append("<");
                ICollection<TypeMemberInfo> members = this.members;
                if (this.typeProperty.IsDictionary)
                {
                    members = ((List<TypeMemberInfo>) this.members)[0].Members;
                }
                int num2 = 0;
                foreach (TypeMemberInfo info in members)
                {
                    if (num2++ > 0)
                    {
                        builder.Append(",");
                    }
                    builder.Append(info.FriendlyTypeName);
                }
                builder.Append(">");
                this.friendlyName = builder.ToString();
            }
            else
            {
                this.friendlyName = this.TypeName;
            }
        }

        internal string[] GetSelectionList()
        {
            string[] strArray;
            
                if (this.typeName.Equals("System.Boolean"))
                {
                    strArray = new string[] { "True", "False" };
                }
                else
                {
                    if (this.enumChoices != null)
                    {
                        return this.enumChoices;
                    }
                    if (this.typeProperty.IsKeyValuePair || this.typeProperty.IsStruct)
                    {
                        strArray = new string[] { this.typeName };
                    }
                    else if (this.typeProperty.IsDataSet)
                    {
                        strArray = new string[] { "(null)", "StringResources.EditDataSet" };
                    }
                    else
                    {
                        strArray = new string[0];
                    }
                }
            if ((strArray != null) && (strArray.Length == 0))
            {
                List<string> list = new List<string> {
                    "(null)",
                    this.typeName
                };
                foreach (SerializableType type in this.subTypes)
                {
                    if (!type.IsInvalid)
                    {
                        list.Add(type.TypeName);
                    }
                }
                strArray = new string[list.Count];
                list.CopyTo(strArray);
            }
            return strArray;
        }


        internal string GetDefaultValue()
        {
            if (this.enumChoices != null)
            {
                return this.enumChoices[0];
            }
            if (numericTypes.Contains(this.typeName))
            {
                return "0";
            }
            if (this.typeName.Equals("System.Boolean", StringComparison.Ordinal))
            {
                return bool.FalseString;
            }
            if (this.typeName.Equals("System.Char", StringComparison.Ordinal))
            {
                return "A";
            }
            if (this.typeName.Equals("System.Guid", StringComparison.Ordinal))
            {
                return Guid.NewGuid().ToString();
            }
            if (this.typeName.Equals("System.DateTime", StringComparison.Ordinal))
            {
                return (DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
            }
            if (this.typeName.Equals("System.DateTimeOffset", StringComparison.Ordinal))
            {
                return DateTimeOffset.Now.ToString();
            }
            if (this.typeName.Equals("System.TimeSpan", StringComparison.Ordinal))
            {
                return TimeSpan.Zero.ToString();
            }
            if (this.typeName.Equals("System.Uri", StringComparison.Ordinal))
            {
                return "http://localhost";
            }
            if (this.typeName.Equals("System.Xml.XmlQualifiedName", StringComparison.Ordinal))
            {
                return "namespace:name";
            }
            if (this.IsContainer())
            {
                return "length=0";
            }
            if (!this.typeProperty.IsKeyValuePair && !this.typeProperty.IsStruct)
            {
                return "(null)";
            }
            return this.typeName;
        }

        internal bool HasMembers()
        {
            if (!this.typeProperty.IsComposite && !this.typeProperty.IsNullable)
            {
                return this.typeProperty.IsKeyValuePair;
            }
            return true;
        }

        internal bool IsContainer()
        {
            if (!this.typeProperty.IsArray && !this.typeProperty.IsDictionary)
            {
                return this.typeProperty.IsCollection;
            }
            return true;
        }

        internal static bool IsNullRepresentation(string value)
        {
            return string.Equals(value, "(null)", StringComparison.Ordinal);
        }

        internal void MarkAsInvalid()
        {
            this.isInvalid = true;
        }

        internal string DataSetSchema
        {
            get
            {
                return this.dataSetSchema;
            }
        }
      
        internal string FriendlyName
        {
            get
            {
                if (this.friendlyName == null)
                {
                    this.ComposeFriendlyName();
                }
                return this.friendlyName;
            }
        }

        internal bool IsInvalid
        {
            get
            {
                return this.isInvalid;
            }
        }

        internal ICollection<TypeMemberInfo> Members
        {
            get
            {
                return this.members;
            }
        }

        internal ICollection<SerializableType> SubTypes
        {
            get
            {
                return this.subTypes;
            }
        }

        internal string TypeName
        {
            get
            {
                return this.typeName;
            }
        }

        internal Microsoft.Tools.TestClient.TypeProperty TypeProperty
        {
            get
            {
                return this.typeProperty;
            }
        }
    }
}

