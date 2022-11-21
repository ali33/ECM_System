namespace Ecm.CameraLib
{
    partial class CameraViewer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CameraViewer));
            this.actionButton = new System.Windows.Forms.Button();
            this.videoPanel = new System.Windows.Forms.Panel();
            this.timerLabel = new System.Windows.Forms.Label();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.closeButton = new System.Windows.Forms.Button();
            this.photoOption = new System.Windows.Forms.RadioButton();
            this.videoOption = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // actionButton
            // 
            this.actionButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.actionButton.Location = new System.Drawing.Point(225, 488);
            this.actionButton.Name = "actionButton";
            this.actionButton.Size = new System.Drawing.Size(97, 33);
            this.actionButton.TabIndex = 2;
            this.actionButton.Text = "Start";
            this.actionButton.UseVisualStyleBackColor = true;
            this.actionButton.Click += new System.EventHandler(this.ActionButtonClick);
            // 
            // videoPanel
            // 
            this.videoPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.videoPanel.BackColor = System.Drawing.SystemColors.ControlDark;
            this.videoPanel.Location = new System.Drawing.Point(0, 0);
            this.videoPanel.Name = "videoPanel";
            this.videoPanel.Size = new System.Drawing.Size(640, 480);
            this.videoPanel.TabIndex = 5;
            this.videoPanel.Resize += new System.EventHandler(this.VideoPanelResize);
            // 
            // timerLabel
            // 
            this.timerLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.timerLabel.AutoSize = true;
            this.timerLabel.Location = new System.Drawing.Point(350, 500);
            this.timerLabel.Name = "timerLabel";
            this.timerLabel.Size = new System.Drawing.Size(49, 13);
            this.timerLabel.TabIndex = 4;
            this.timerLabel.Text = "00:00:00";
            // 
            // timer
            // 
            this.timer.Interval = 1000;
            this.timer.Tick += new System.EventHandler(this.TimerTick);
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeButton.Location = new System.Drawing.Point(530, 488);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(97, 33);
            this.closeButton.TabIndex = 3;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.CloseButtonClick);
            // 
            // photoOption
            // 
            this.photoOption.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.photoOption.AutoSize = true;
            this.photoOption.Checked = true;
            this.photoOption.Location = new System.Drawing.Point(42, 496);
            this.photoOption.Name = "photoOption";
            this.photoOption.Size = new System.Drawing.Size(53, 17);
            this.photoOption.TabIndex = 0;
            this.photoOption.TabStop = true;
            this.photoOption.Text = "Photo";
            this.photoOption.UseVisualStyleBackColor = true;
            this.photoOption.CheckedChanged += new System.EventHandler(this.PhotoOptionCheckedChanged);
            // 
            // videoOption
            // 
            this.videoOption.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.videoOption.AutoSize = true;
            this.videoOption.Location = new System.Drawing.Point(101, 496);
            this.videoOption.Name = "videoOption";
            this.videoOption.Size = new System.Drawing.Size(52, 17);
            this.videoOption.TabIndex = 1;
            this.videoOption.Text = "Video";
            this.videoOption.UseVisualStyleBackColor = true;
            this.videoOption.CheckedChanged += new System.EventHandler(this.VideoOptionCheckedChanged);
            // 
            // CameraViewer
            // 
            this.AcceptButton = this.actionButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.closeButton;
            this.ClientSize = new System.Drawing.Size(640, 529);
            this.Controls.Add(this.videoOption);
            this.Controls.Add(this.photoOption);
            this.Controls.Add(this.timerLabel);
            this.Controls.Add(this.videoPanel);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.actionButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimizeBox = false;
            this.Name = "CameraViewer";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Camera Viewer";
            this.Activated += new System.EventHandler(this.CameraViewerActivated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CameraViewerFormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button actionButton;
        private System.Windows.Forms.Panel videoPanel;
        private System.Windows.Forms.Label timerLabel;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.RadioButton photoOption;
        private System.Windows.Forms.RadioButton videoOption;
    }
}