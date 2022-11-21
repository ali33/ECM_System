using System;
using System.Runtime.Serialization;
namespace Ecm.Workflow.Activities.CustomActivityDomain
{
    public class LookupParameter
    {
        private string _parameterValue;

        public string ParameterName { get; set; }

        public string ParameterType { get; set; }

        public string OrderIndex { get; set; }

        public string Mode { get; set; }

        public string ParameterValue { get; set; }
    
        public bool IsRequired { get; set; }
    }
}
