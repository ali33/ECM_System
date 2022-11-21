namespace WordImport
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
            this.ArchiveTab = this.Factory.CreateRibbonTab();
            this.ArchiveGroup = this.Factory.CreateRibbonGroup();
            this.btnSendToArchive = this.Factory.CreateRibbonButton();
            this.btnSendToArchive1 = this.Factory.CreateRibbonButton();
            this.ArchiveTab.SuspendLayout();
            this.ArchiveGroup.SuspendLayout();
            // 
            // ArchiveTab
            // 
            this.ArchiveTab.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
            this.ArchiveTab.Groups.Add(this.ArchiveGroup);
            this.ArchiveTab.Label = "Ecm Archive";
            this.ArchiveTab.Name = "ArchiveTab";
            // 
            // ArchiveGroup
            // 
            this.ArchiveGroup.Items.Add(this.btnSendToArchive);
            this.ArchiveGroup.Label = "Archive";
            this.ArchiveGroup.Name = "ArchiveGroup";
            // 
            // btnSendToArchive
            // 
            this.btnSendToArchive.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnSendToArchive.Image = global::Ecm.WordImport.Resources.Logo;
            this.btnSendToArchive.Label = "Send to Archive";
            this.btnSendToArchive.Name = "btnSendToArchive";
            this.btnSendToArchive.ShowImage = true;
            this.btnSendToArchive.Tag = "SendToArchive";
            // 
            // btnSendToArchive1
            // 
            this.btnSendToArchive1.Image = global::Ecm.WordImport.Resources.Logo;
            this.btnSendToArchive1.Label = "Send to Archive";
            this.btnSendToArchive1.Name = "btnSendToArchive1";
            this.btnSendToArchive1.ShowImage = true;
            this.btnSendToArchive1.Tag = "SendToArchive1";
            // 
            // CustomRibbon
            // 
            this.Name = "CustomRibbon";
            // 
            // CustomRibbon.OfficeMenu
            // 
            this.OfficeMenu.Items.Add(this.btnSendToArchive1);
            this.RibbonType = "Microsoft.Word.Document";
            this.Tabs.Add(this.ArchiveTab);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.CustomRibbon_Load);
            this.ArchiveTab.ResumeLayout(false);
            this.ArchiveTab.PerformLayout();
            this.ArchiveGroup.ResumeLayout(false);
            this.ArchiveGroup.PerformLayout();

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab ArchiveTab;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup ArchiveGroup;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnSendToArchive;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnSendToArchive1;
    }

    partial class ThisRibbonCollection
    {
        internal CustomRibbon CustomRibbon
        {
            get { return this.GetRibbon<CustomRibbon>(); }
        }
    }
}
