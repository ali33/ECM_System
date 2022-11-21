using System;
using System.Runtime.Serialization;

namespace Ecm.CaptureDomain
{
    /// <summary>
    /// Represent for the annotation on each page of document.
    /// </summary>
    [DataContract]
    public class Annotation
    {
        [DataMember]
        public Guid Id { get; set; }

        /// <summary>
        /// Identifier of the page which contains the annotation.
        /// </summary>
        [DataMember]
        public Guid PageId { get; set; }

        /// <summary>
        /// Identifier of the document which contains the annotation.
        /// </summary>
        [DataMember]
        public Guid DocId { get; set; }

        /// <summary>
        /// Identifier of the document type contains the document.
        /// </summary>
        [DataMember]
        public Guid DocTypeId { get; set; }

        /// <summary>
        /// Type of annotation. There are 5 types of annotation: Redaction, Highlight, Text, Line, OCRZone.
        /// </summary>
        [DataMember]
        public string Type { get; set; }

        /// <summary>
        /// Height of annotation
        /// </summary>
        [DataMember]
        public double Height { get; set; }

        /// <summary>
        /// Width of annotation
        /// </summary>
        [DataMember]
        public double Width { get; set; }

        /// <summary>
        /// Left position of annotation on the page.
        /// </summary>
        [DataMember]
        public double Left { get; set; }

        /// <summary>
        /// Only apply for Line annotation. The value can be: TopLeft, TopRight, BottomLeft, BottomRight
        /// </summary>
        [DataMember]
        public string LineEndAt { get; set; }

        /// <summary>
        /// Only apply for Line annotation. The value can be: TopLeft, TopRight, BottomLeft, BottomRight
        /// </summary>
        [DataMember]
        public string LineStartAt { get; set; }

        /// <summary>
        /// Only apply for Line annotation. The value can be: ArrowAtEnd, ArrowAtStart, ArrowAtBoth, NoArrow
        /// </summary>
        [DataMember]
        public string LineStyle { get; set; }

        /// <summary>
        /// Only apply for Line annotation. This property define the thickness of the line.
        /// </summary>
        [DataMember]
        public int LineWeight { get; set; }

        /// <summary>
        /// The rotation angle of the annotation. The angle can be positive or negative and must be divisible by 90. 
        /// </summary>
        [DataMember]
        public double RotateAngle { get; set; }

        /// <summary>
        /// Define the top position of the annotation
        /// </summary>
        [DataMember]
        public double Top { get; set; }

        /// <summary>
        /// Only apply for Line annotation. This property define the color of the line.
        /// </summary>
        [DataMember]
        public string LineColor { get; set; }

        /// <summary>
        /// The content of the Text annotation.
        /// </summary>
        [DataMember]
        public string Content { get; set; }

        /// <summary>
        /// The user name of the user create the annotation
        /// </summary>
        [DataMember]
        public string CreatedBy { get; set; }

        /// <summary>
        /// The time when the annotation is created
        /// </summary>
        [DataMember]
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// The user name of the user modify the annotation. The modification can be the change of position or change content,...
        /// </summary>
        [DataMember]
        public string ModifiedBy { get; set; }

        /// <summary>
        /// The time when the annotation is modified.
        /// </summary>
        [DataMember]
        public DateTime? ModifiedOn { get; set; }

        /// <summary>
        /// This function is use for set default value of type DateTime to universal time (for Json serialization not throw exception). 
        /// </summary>
        /// <param name="context"></param>
        [OnSerializing()]
        internal void OnSerializingMethod(StreamingContext context)
        {
            // Adjust all member have type of DateTime

            if (DateTime.MinValue == ModifiedOn)
            {
                ModifiedOn = DateTime.MinValue.ToUniversalTime();
            }
        }

    }
}
