namespace ExcelImport
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
            this.btnOpenView = this.Factory.CreateRibbonButton();
            this.group1 = this.Factory.CreateRibbonGroup();
            this.btnSave = this.Factory.CreateRibbonButton();
            this.btnSaveAs = this.Factory.CreateRibbonButton();
            this.tab1 = this.Factory.CreateRibbonTab();
            this.ArchiveTab.SuspendLayout();
            this.ArchiveGroup.SuspendLayout();
            this.group1.SuspendLayout();
            this.tab1.SuspendLayout();
            // 
            // ArchiveTab
            // 
            this.ArchiveTab.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
            this.ArchiveTab.Groups.Add(this.ArchiveGroup);
            this.ArchiveTab.Groups.Add(this.group1);
            this.ArchiveTab.Label = "Ecm Archive";
            this.ArchiveTab.Name = "ArchiveTab";
            // 
            // ArchiveGroup
            // 
            this.ArchiveGroup.Items.Add(this.btnSendToArchive);
            this.ArchiveGroup.Items.Add(this.btnOpenView);
            this.ArchiveGroup.Label = "Archive";
            this.ArchiveGroup.Name = "ArchiveGroup";
            // 
            // btnSendToArchive
            // 
            this.btnSendToArchive.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnSendToArchive.Label = "Send to Archive";
            this.btnSendToArchive.Name = "btnSendToArchive";
            this.btnSendToArchive.ShowImage = true;
            this.btnSendToArchive.Tag = "SendToArchive";
            // 
            // btnOpenView
            // 
            this.btnOpenView.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnOpenView.Label = "Open";
            this.btnOpenView.Name = "btnOpenView";
            this.btnOpenView.ShowImage = true;
            // 
            // group1
            // 
            this.group1.Items.Add(this.btnSave);
            this.group1.Items.Add(this.btnSaveAs);
            this.group1.Label = "group1";
            this.group1.Name = "group1";
            // 
            // btnSave
            // 
            this.btnSave.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnSave.Label = "Save";
            this.btnSave.Name = "btnSave";
            this.btnSave.ShowImage = true;
            // 
            // btnSaveAs
            // 
            this.btnSaveAs.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnSaveAs.Label = "Save As";
            this.btnSaveAs.Name = "btnSaveAs";
            this.btnSaveAs.ShowImage = true;
            // 
            // tab1
            // 
            this.tab1.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
            this.tab1.ControlId.OfficeId = "Capture";
            this.tab1.Label = "Capture";
            this.tab1.Name = "tab1";
            // 
            // CustomRibbon
            // 
            this.Name = "CustomRibbon";
            this.RibbonType = "Microsoft.Word.Document";
            this.Tabs.Add(this.ArchiveTab);
            this.Tabs.Add(this.tab1);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.CustomRibbon_Load);
            this.ArchiveTab.ResumeLayout(false);
            this.ArchiveTab.PerformLayout();
            this.ArchiveGroup.ResumeLayout(false);
            this.ArchiveGroup.PerformLayout();
            this.group1.ResumeLayout(false);
            this.group1.PerformLayout();
            this.tab1.ResumeLayout(false);
            this.tab1.PerformLayout();

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab ArchiveTab;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup ArchiveGroup;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnSendToArchive;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnOpenView;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup group1;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnSave;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnSaveAs;
        private Microsoft.Office.Tools.Ribbon.RibbonTab tab1;
    }

    partial class ThisRibbonCollection
    {
        internal CustomRibbon CustomRibbon
        {
            get { return this.GetRibbon<CustomRibbon>(); }
        }
    }
}
