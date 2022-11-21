using System;
namespace Ecm.Workflow.Activities.CustomActivityModel
{
    public class ParameterModel : ICloneable
    {
        public string ParameterName { get; set; }

        public string ParameterType { get; set; }

        public string OrderIndex { get; set; }

        public string Mode { get; set; }

        public string ParameterValue { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
