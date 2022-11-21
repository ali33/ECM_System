namespace Microsoft.Tools.TestClient.Variables
{
    using Microsoft.Tools.TestClient;
    using System;
    using System.Data;
    using System.IO;

    [Serializable]
    internal class DataSetVariable : Variable
    {
        private DataSet dataSetValue;

        internal DataSetVariable(TypeMemberInfo declaredMember) : base(declaredMember)
        {
            this.CreateEmptyDataSet(declaredMember.DataSetSchema);
        }

        internal DataSetVariable(TypeMemberInfo declaredMember, object obj) : base(declaredMember, obj)
        {
            this.CreateEmptyDataSet(((DataSet) obj).GetXmlSchema());
            using (StringReader reader = new StringReader(((DataSet) obj).GetXml()))
            {
                this.dataSetValue.ReadXml(reader);
            }
        }

        internal override Variable Clone()
        {
            DataSetVariable variable = new DataSetVariable(base.currentMember);
            if (variable.CopyFrom(this))
            {
                return variable;
            }
            return null;
        }

        internal override bool CopyFrom(Variable variable)
        {
            if ((variable == null) || object.ReferenceEquals(this, variable))
            {
                return false;
            }
            this.dataSetValue.Dispose();
            DataSetVariable variable2 = variable as DataSetVariable;
            this.dataSetValue = variable2.dataSetValue.Copy();
            return true;
        }

        private void CreateEmptyDataSet(string schema)
        {
            this.dataSetValue = new DataSet();
            using (StringReader reader = new StringReader(schema))
            {
                this.dataSetValue.ReadXmlSchema(reader);
            }
            this.dataSetValue.Locale = this.dataSetValue.Locale;
        }

        internal override object CreateObject()
        {
            if (base.value.Equals("(null)"))
            {
                return null;
            }
            if (this.IsGeneralDataSet)
            {
                return this.dataSetValue;
            }
            object obj2 = Activator.CreateInstance(ClientSettings.GetType(base.currentMember.TypeName));
            using (StringReader reader = new StringReader(this.dataSetValue.GetXml()))
            {
                ((DataSet) obj2).ReadXml(reader);
            }
            return obj2;
        }

        internal object GetDataSetValue()
        {
            return this.dataSetValue;
        }

        internal string GetXmlSchema()
        {
            return this.dataSetValue.GetXmlSchema();
        }

        internal bool IsDefaultDataSet()
        {
            DataSet set = new DataSet(this.dataSetValue.DataSetName);
            DataSetVariable variable = new DataSetVariable(base.currentMember, set);
            return this.SchemaEquals(variable);
        }

        internal bool SchemaEquals(Variable variable)
        {
            DataSetVariable variable2 = variable as DataSetVariable;
            return string.Equals(this.GetXmlSchema(), variable2.GetXmlSchema(), StringComparison.Ordinal);
        }

        private bool IsGeneralDataSet
        {
            get
            {
                return string.Equals(base.currentMember.TypeName, "System.Data.DataSet", StringComparison.Ordinal);
            }
        }
    }
}

