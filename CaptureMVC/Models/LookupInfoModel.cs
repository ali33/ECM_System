using System;
using System.Collections.Generic;
using Ecm.CaptureDomain;

namespace CaptureMVC.Models
{
    [Serializable]
    public class LookupInfoModel
    {
        public LookupInfoModel()
        {
            ConnectionInfo = new LookupConnectionModel();
            FieldMappings = new List<LookupMapModel>();
            Parameters = new List<ParameterModel>();
        }

        public Guid FieldId { get; set; }

        public LookupConnectionModel ConnectionInfo { get; set; }

        public int LookupType { get; set; }

        public string SqlCommand { get; set; }

        public int MaxLookupRow { get; set; }

        public int MinPrefixLength { get; set; }

        public string SourceName { get; set; }

        public string LookupColumn { get; set; }
        public string LookupOperator { get; set; }

        public string ConnectionString { get; set; }

        public List<ParameterModel> Parameters { get; set; }

        public List<LookupMapModel> FieldMappings { get; set; }
    }
}