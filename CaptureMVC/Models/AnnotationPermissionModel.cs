using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CaptureMVC.Models
{
    public class AnnotationPermissionModel
    {
        // Properties
        public bool CanAddHighlight { get; set; }

        public bool CanAddRedaction { get; set; }

        public bool CanAddText { get; set; }

        public bool CanDeleteHighlight { get; set; }

        public bool CanDeleteRedaction { get; set; }

        public bool CanDeleteText { get; set; }

        public bool CanHideRedaction { get; set; }

        public bool CanSeeHighlight { get; set; }

        public bool CanSeeText { get; set; }

        public Guid DocTypeId { get; set; }

        public Guid UserGroupId { get; set; }

        public Guid Id { get; set; }

        // Methods
        public static AnnotationPermissionModel GetAllowAll()
        {
            return new AnnotationPermissionModel
            {
                CanAddHighlight = true,
                CanAddRedaction = true,
                CanAddText = true,
                CanDeleteHighlight = true,
                CanDeleteRedaction = true,
                CanDeleteText = true,
                CanHideRedaction = true,
                CanSeeHighlight = true,
                CanSeeText = true
            };
        }
    }
}