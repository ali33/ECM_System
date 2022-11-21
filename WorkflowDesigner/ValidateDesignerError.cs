using System;
using System.Activities.Presentation.Validation;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ecm.WorkflowDesigner
{
    public class ValidateDesignerError : IValidationErrorService
    {
        public bool HasError { get; set; }

        public void ShowValidationErrors(IList<ValidationErrorInfo> errors)
        {
            if (errors.Count > 0)
            {
                HasError = true;
            }
            else
            {
                HasError = false;
            }
        }
    }
}
