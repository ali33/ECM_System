namespace Microsoft.Tools.TestClient
{
    using System;

    [Serializable]
    internal class TypeProperty
    {
        private bool isArray;
        private bool isCollection;
        private bool isComposite;
        private bool isDataSet;
        private bool isDictionary;
        private bool isEnum;
        private bool isKeyValuePair;
        private bool isNullable;
        private bool isNumeric;
        private bool isStruct;

        internal bool IsArray
        {
            get
            {
                return this.isArray;
            }
            set
            {
                this.isArray = value;
            }
        }

        internal bool IsCollection
        {
            get
            {
                return this.isCollection;
            }
            set
            {
                this.isCollection = value;
            }
        }

        internal bool IsComposite
        {
            get
            {
                return this.isComposite;
            }
            set
            {
                this.isComposite = value;
            }
        }

        public bool IsDataSet
        {
            get
            {
                return this.isDataSet;
            }
            set
            {
                this.isDataSet = value;
            }
        }

        internal bool IsDictionary
        {
            get
            {
                return this.isDictionary;
            }
            set
            {
                this.isDictionary = value;
            }
        }

        internal bool IsEnum
        {
            get
            {
                return this.isEnum;
            }
            set
            {
                this.isEnum = value;
            }
        }

        internal bool IsKeyValuePair
        {
            get
            {
                return this.isKeyValuePair;
            }
            set
            {
                this.isKeyValuePair = value;
            }
        }

        internal bool IsNullable
        {
            get
            {
                return this.isNullable;
            }
            set
            {
                this.isNullable = value;
            }
        }

        public bool IsNumeric
        {
            get
            {
                return this.isNumeric;
            }
            set
            {
                this.isNumeric = value;
            }
        }

        internal bool IsStruct
        {
            get
            {
                return this.isStruct;
            }
            set
            {
                this.isStruct = value;
            }
        }
    }
}

