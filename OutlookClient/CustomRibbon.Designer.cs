namespace OutlookClient
{
    partial class CustomRibbon : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public CustomRibbon()
            : base(Globals.Factory.GetRibbonFactory())
        {
            InitializeComponent();
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tab3 = this.Factory.CreateRibbonTab();
            this.group1 = this.Factory.CreateRibbonGroup();
            this.btnSendToArchive2 = this.Factory.CreateRibbonButton();
            this.tab3.SuspendLayout();
            this.group1.SuspendLayout();
            // 
            // tab3
            // 
            this.tab3.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
            this.tab3.Groups.Add(this.group1);
            this.tab3.Label = "Ecm Archive";
            this.tab3.Name = "tab3";
            // 
            // group1
            // 
            this.group1.Items.Add(this.btnSendToArchive2);
            this.group1.Label = "Archive";
            this.group1.Name = "group1";
            // 
            // btnSendToArchive2
            // 
            this.btnSendToArchive2.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnSendToArchive2.Image = global::Ecm.OutlookClient.Resources.Logo;
            this.btnSendToArchive2.Label = "Send to Archive";
            this.btnSendToArchive2.Name = "btnSendToArchive2";
            this.btnSendToArchive2.ShowImage = true;
            this.btnSendToArchive2.Tag = "SendToArchive";
            // 
            // CustomRibbon
            // 
            this.Name = "CustomRibbon";
            this.RibbonType = "Microsoft.Outlook.Mail.Read";
            this.Tabs.Add(this.tab3);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.CustomRibbon_Load);
            this.tab3.ResumeLayout(false);
            this.tab3.PerformLayout();
            this.group1.ResumeLayout(false);
            this.group1.PerformLayout();

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab tab3;
        public Microsoft.Office.Tools.Ribbon.RibbonGroup group1;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnSendToArchive2;
    }

    partial class ThisRibbonCollection
    {
        internal CustomRibbon CustomRibbon
        {
            get { return this.GetRibbon<CustomRibbon>(); }
        }
    }
}
