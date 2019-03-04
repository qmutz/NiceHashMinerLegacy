namespace MinerSmokeTest
{
    partial class PluginForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btn_pluginStart = new System.Windows.Forms.Button();
            this.dgv_pluginDevices = new System.Windows.Forms.DataGridView();
            this.dgv_deviceEnabled = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.dgv_deviceName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgv_pluginAlgo = new System.Windows.Forms.DataGridView();
            this.Enabled = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Algorithm = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Plugin = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tbx_pluginInfo = new System.Windows.Forms.TextBox();
            this.gb_pluginOptionStop = new System.Windows.Forms.GroupBox();
            this.rb_pluginStopMining = new System.Windows.Forms.RadioButton();
            this.rb_pluginEndMining = new System.Windows.Forms.RadioButton();
            this.tbx_pluginStopDelayMS = new System.Windows.Forms.TextBox();
            this.tbx_pluginStopDelayS = new System.Windows.Forms.TextBox();
            this.tbx_pluginStopDelayM = new System.Windows.Forms.TextBox();
            this.lbl_pluginlabel6 = new System.Windows.Forms.Label();
            this.lbl_pluginLabel7 = new System.Windows.Forms.Label();
            this.lbl_pluginLabel8 = new System.Windows.Forms.Label();
            this.lbl_pluginStopTime = new System.Windows.Forms.Label();
            this.tbx_pluginMinTimeMS = new System.Windows.Forms.TextBox();
            this.tbx_pluginMinTimeS = new System.Windows.Forms.TextBox();
            this.tbx_pluginMinTimeM = new System.Windows.Forms.TextBox();
            this.lbl_pluginLabel5 = new System.Windows.Forms.Label();
            this.lbl_pluginLabel4 = new System.Windows.Forms.Label();
            this.lbl_pluginLabel3 = new System.Windows.Forms.Label();
            this.lbl_pluginMiningTime = new System.Windows.Forms.Label();
            this.lbl_pluginSteps = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_pluginDevices)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_pluginAlgo)).BeginInit();
            this.gb_pluginOptionStop.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_pluginStart
            // 
            this.btn_pluginStart.Location = new System.Drawing.Point(13, 13);
            this.btn_pluginStart.Name = "btn_pluginStart";
            this.btn_pluginStart.Size = new System.Drawing.Size(75, 23);
            this.btn_pluginStart.TabIndex = 0;
            this.btn_pluginStart.Text = "Start test";
            this.btn_pluginStart.UseVisualStyleBackColor = true;
            this.btn_pluginStart.Click += new System.EventHandler(this.btn_pluginStart_Click);
            // 
            // dgv_pluginDevices
            // 
            this.dgv_pluginDevices.AllowUserToAddRows = false;
            this.dgv_pluginDevices.AllowUserToDeleteRows = false;
            this.dgv_pluginDevices.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_pluginDevices.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dgv_deviceEnabled,
            this.dgv_deviceName});
            this.dgv_pluginDevices.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dgv_pluginDevices.Location = new System.Drawing.Point(12, 183);
            this.dgv_pluginDevices.Name = "dgv_pluginDevices";
            this.dgv_pluginDevices.ReadOnly = true;
            this.dgv_pluginDevices.RowHeadersVisible = false;
            this.dgv_pluginDevices.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv_pluginDevices.Size = new System.Drawing.Size(310, 255);
            this.dgv_pluginDevices.TabIndex = 13;
            // 
            // dgv_deviceEnabled
            // 
            this.dgv_deviceEnabled.FalseValue = "\"NO\"";
            this.dgv_deviceEnabled.HeaderText = "Enabled";
            this.dgv_deviceEnabled.Name = "dgv_deviceEnabled";
            this.dgv_deviceEnabled.ReadOnly = true;
            this.dgv_deviceEnabled.Width = 50;
            // 
            // dgv_deviceName
            // 
            this.dgv_deviceName.HeaderText = "Name";
            this.dgv_deviceName.Name = "dgv_deviceName";
            this.dgv_deviceName.ReadOnly = true;
            this.dgv_deviceName.Width = 200;
            // 
            // dgv_pluginAlgo
            // 
            this.dgv_pluginAlgo.AllowUserToAddRows = false;
            this.dgv_pluginAlgo.AllowUserToDeleteRows = false;
            this.dgv_pluginAlgo.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_pluginAlgo.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Enabled,
            this.Algorithm,
            this.Plugin});
            this.dgv_pluginAlgo.Location = new System.Drawing.Point(350, 183);
            this.dgv_pluginAlgo.Name = "dgv_pluginAlgo";
            this.dgv_pluginAlgo.ReadOnly = true;
            this.dgv_pluginAlgo.RowHeadersVisible = false;
            this.dgv_pluginAlgo.Size = new System.Drawing.Size(375, 256);
            this.dgv_pluginAlgo.TabIndex = 14;
            // 
            // Enabled
            // 
            this.Enabled.HeaderText = "Enabled";
            this.Enabled.Name = "Enabled";
            this.Enabled.ReadOnly = true;
            // 
            // Algorithm
            // 
            this.Algorithm.HeaderText = "Algorithm";
            this.Algorithm.Name = "Algorithm";
            this.Algorithm.ReadOnly = true;
            // 
            // Plugin
            // 
            this.Plugin.HeaderText = "Plugin";
            this.Plugin.Name = "Plugin";
            this.Plugin.ReadOnly = true;
            // 
            // tbx_pluginInfo
            // 
            this.tbx_pluginInfo.Location = new System.Drawing.Point(742, 183);
            this.tbx_pluginInfo.Multiline = true;
            this.tbx_pluginInfo.Name = "tbx_pluginInfo";
            this.tbx_pluginInfo.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbx_pluginInfo.Size = new System.Drawing.Size(591, 256);
            this.tbx_pluginInfo.TabIndex = 15;
            // 
            // gb_pluginOptionStop
            // 
            this.gb_pluginOptionStop.Controls.Add(this.rb_pluginStopMining);
            this.gb_pluginOptionStop.Controls.Add(this.rb_pluginEndMining);
            this.gb_pluginOptionStop.Location = new System.Drawing.Point(691, 12);
            this.gb_pluginOptionStop.Name = "gb_pluginOptionStop";
            this.gb_pluginOptionStop.Size = new System.Drawing.Size(200, 100);
            this.gb_pluginOptionStop.TabIndex = 32;
            this.gb_pluginOptionStop.TabStop = false;
            this.gb_pluginOptionStop.Text = "Stop by";
            // 
            // rb_pluginStopMining
            // 
            this.rb_pluginStopMining.AutoSize = true;
            this.rb_pluginStopMining.Location = new System.Drawing.Point(6, 43);
            this.rb_pluginStopMining.Name = "rb_pluginStopMining";
            this.rb_pluginStopMining.Size = new System.Drawing.Size(47, 17);
            this.rb_pluginStopMining.TabIndex = 29;
            this.rb_pluginStopMining.TabStop = true;
            this.rb_pluginStopMining.Text = "Stop";
            this.rb_pluginStopMining.UseVisualStyleBackColor = true;
            // 
            // rb_pluginEndMining
            // 
            this.rb_pluginEndMining.AutoSize = true;
            this.rb_pluginEndMining.Location = new System.Drawing.Point(6, 20);
            this.rb_pluginEndMining.Name = "rb_pluginEndMining";
            this.rb_pluginEndMining.Size = new System.Drawing.Size(44, 17);
            this.rb_pluginEndMining.TabIndex = 28;
            this.rb_pluginEndMining.TabStop = true;
            this.rb_pluginEndMining.Text = "End";
            this.rb_pluginEndMining.UseVisualStyleBackColor = true;
            // 
            // tbx_pluginStopDelayMS
            // 
            this.tbx_pluginStopDelayMS.Location = new System.Drawing.Point(509, 67);
            this.tbx_pluginStopDelayMS.Name = "tbx_pluginStopDelayMS";
            this.tbx_pluginStopDelayMS.Size = new System.Drawing.Size(40, 20);
            this.tbx_pluginStopDelayMS.TabIndex = 46;
            this.tbx_pluginStopDelayMS.Text = "0";
            this.tbx_pluginStopDelayMS.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tbx_pluginStopDelayS
            // 
            this.tbx_pluginStopDelayS.Location = new System.Drawing.Point(453, 67);
            this.tbx_pluginStopDelayS.Name = "tbx_pluginStopDelayS";
            this.tbx_pluginStopDelayS.Size = new System.Drawing.Size(40, 20);
            this.tbx_pluginStopDelayS.TabIndex = 45;
            this.tbx_pluginStopDelayS.Text = "0";
            this.tbx_pluginStopDelayS.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tbx_pluginStopDelayM
            // 
            this.tbx_pluginStopDelayM.Location = new System.Drawing.Point(382, 67);
            this.tbx_pluginStopDelayM.Name = "tbx_pluginStopDelayM";
            this.tbx_pluginStopDelayM.Size = new System.Drawing.Size(40, 20);
            this.tbx_pluginStopDelayM.TabIndex = 44;
            this.tbx_pluginStopDelayM.Text = "0";
            this.tbx_pluginStopDelayM.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lbl_pluginlabel6
            // 
            this.lbl_pluginlabel6.AutoSize = true;
            this.lbl_pluginlabel6.Location = new System.Drawing.Point(555, 74);
            this.lbl_pluginlabel6.Name = "lbl_pluginlabel6";
            this.lbl_pluginlabel6.Size = new System.Drawing.Size(20, 13);
            this.lbl_pluginlabel6.TabIndex = 43;
            this.lbl_pluginlabel6.Text = "ms";
            // 
            // lbl_pluginLabel7
            // 
            this.lbl_pluginLabel7.AutoSize = true;
            this.lbl_pluginLabel7.Location = new System.Drawing.Point(424, 74);
            this.lbl_pluginLabel7.Name = "lbl_pluginLabel7";
            this.lbl_pluginLabel7.Size = new System.Drawing.Size(23, 13);
            this.lbl_pluginLabel7.TabIndex = 42;
            this.lbl_pluginLabel7.Text = "min";
            // 
            // lbl_pluginLabel8
            // 
            this.lbl_pluginLabel8.AutoSize = true;
            this.lbl_pluginLabel8.Location = new System.Drawing.Point(491, 74);
            this.lbl_pluginLabel8.Name = "lbl_pluginLabel8";
            this.lbl_pluginLabel8.Size = new System.Drawing.Size(12, 13);
            this.lbl_pluginLabel8.TabIndex = 41;
            this.lbl_pluginLabel8.Text = "s";
            // 
            // lbl_pluginStopTime
            // 
            this.lbl_pluginStopTime.AutoSize = true;
            this.lbl_pluginStopTime.Location = new System.Drawing.Point(294, 74);
            this.lbl_pluginStopTime.Name = "lbl_pluginStopTime";
            this.lbl_pluginStopTime.Size = new System.Drawing.Size(82, 13);
            this.lbl_pluginStopTime.TabIndex = 40;
            this.lbl_pluginStopTime.Text = "Stop delay time:";
            // 
            // tbx_pluginMinTimeMS
            // 
            this.tbx_pluginMinTimeMS.Location = new System.Drawing.Point(509, 25);
            this.tbx_pluginMinTimeMS.Name = "tbx_pluginMinTimeMS";
            this.tbx_pluginMinTimeMS.Size = new System.Drawing.Size(40, 20);
            this.tbx_pluginMinTimeMS.TabIndex = 39;
            this.tbx_pluginMinTimeMS.Text = "0";
            this.tbx_pluginMinTimeMS.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tbx_pluginMinTimeS
            // 
            this.tbx_pluginMinTimeS.Location = new System.Drawing.Point(453, 25);
            this.tbx_pluginMinTimeS.Name = "tbx_pluginMinTimeS";
            this.tbx_pluginMinTimeS.Size = new System.Drawing.Size(40, 20);
            this.tbx_pluginMinTimeS.TabIndex = 38;
            this.tbx_pluginMinTimeS.Text = "0";
            this.tbx_pluginMinTimeS.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tbx_pluginMinTimeM
            // 
            this.tbx_pluginMinTimeM.Location = new System.Drawing.Point(382, 25);
            this.tbx_pluginMinTimeM.Name = "tbx_pluginMinTimeM";
            this.tbx_pluginMinTimeM.Size = new System.Drawing.Size(40, 20);
            this.tbx_pluginMinTimeM.TabIndex = 37;
            this.tbx_pluginMinTimeM.Text = "0";
            this.tbx_pluginMinTimeM.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lbl_pluginLabel5
            // 
            this.lbl_pluginLabel5.AutoSize = true;
            this.lbl_pluginLabel5.Location = new System.Drawing.Point(555, 32);
            this.lbl_pluginLabel5.Name = "lbl_pluginLabel5";
            this.lbl_pluginLabel5.Size = new System.Drawing.Size(20, 13);
            this.lbl_pluginLabel5.TabIndex = 36;
            this.lbl_pluginLabel5.Text = "ms";
            // 
            // lbl_pluginLabel4
            // 
            this.lbl_pluginLabel4.AutoSize = true;
            this.lbl_pluginLabel4.Location = new System.Drawing.Point(424, 32);
            this.lbl_pluginLabel4.Name = "lbl_pluginLabel4";
            this.lbl_pluginLabel4.Size = new System.Drawing.Size(23, 13);
            this.lbl_pluginLabel4.TabIndex = 35;
            this.lbl_pluginLabel4.Text = "min";
            // 
            // lbl_pluginLabel3
            // 
            this.lbl_pluginLabel3.AutoSize = true;
            this.lbl_pluginLabel3.Location = new System.Drawing.Point(491, 32);
            this.lbl_pluginLabel3.Name = "lbl_pluginLabel3";
            this.lbl_pluginLabel3.Size = new System.Drawing.Size(12, 13);
            this.lbl_pluginLabel3.TabIndex = 34;
            this.lbl_pluginLabel3.Text = "s";
            // 
            // lbl_pluginMiningTime
            // 
            this.lbl_pluginMiningTime.AutoSize = true;
            this.lbl_pluginMiningTime.Location = new System.Drawing.Point(313, 32);
            this.lbl_pluginMiningTime.Name = "lbl_pluginMiningTime";
            this.lbl_pluginMiningTime.Size = new System.Drawing.Size(63, 13);
            this.lbl_pluginMiningTime.TabIndex = 33;
            this.lbl_pluginMiningTime.Text = "Mining time:";
            // 
            // lbl_pluginSteps
            // 
            this.lbl_pluginSteps.AutoSize = true;
            this.lbl_pluginSteps.Location = new System.Drawing.Point(132, 22);
            this.lbl_pluginSteps.Name = "lbl_pluginSteps";
            this.lbl_pluginSteps.Size = new System.Drawing.Size(0, 13);
            this.lbl_pluginSteps.TabIndex = 47;
            // 
            // PluginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1351, 450);
            this.Controls.Add(this.lbl_pluginSteps);
            this.Controls.Add(this.tbx_pluginStopDelayMS);
            this.Controls.Add(this.tbx_pluginStopDelayS);
            this.Controls.Add(this.tbx_pluginStopDelayM);
            this.Controls.Add(this.lbl_pluginlabel6);
            this.Controls.Add(this.lbl_pluginLabel7);
            this.Controls.Add(this.lbl_pluginLabel8);
            this.Controls.Add(this.lbl_pluginStopTime);
            this.Controls.Add(this.tbx_pluginMinTimeMS);
            this.Controls.Add(this.tbx_pluginMinTimeS);
            this.Controls.Add(this.tbx_pluginMinTimeM);
            this.Controls.Add(this.lbl_pluginLabel5);
            this.Controls.Add(this.lbl_pluginLabel4);
            this.Controls.Add(this.lbl_pluginLabel3);
            this.Controls.Add(this.lbl_pluginMiningTime);
            this.Controls.Add(this.gb_pluginOptionStop);
            this.Controls.Add(this.tbx_pluginInfo);
            this.Controls.Add(this.dgv_pluginAlgo);
            this.Controls.Add(this.dgv_pluginDevices);
            this.Controls.Add(this.btn_pluginStart);
            this.Name = "PluginForm";
            this.Text = "PluginForm";
            ((System.ComponentModel.ISupportInitialize)(this.dgv_pluginDevices)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_pluginAlgo)).EndInit();
            this.gb_pluginOptionStop.ResumeLayout(false);
            this.gb_pluginOptionStop.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_pluginStart;
        private System.Windows.Forms.DataGridView dgv_pluginDevices;
        private System.Windows.Forms.DataGridViewCheckBoxColumn dgv_deviceEnabled;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgv_deviceName;
        private System.Windows.Forms.DataGridView dgv_pluginAlgo;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Enabled;
        private System.Windows.Forms.DataGridViewTextBoxColumn Algorithm;
        private System.Windows.Forms.DataGridViewTextBoxColumn Plugin;
        private System.Windows.Forms.TextBox tbx_pluginInfo;
        private System.Windows.Forms.GroupBox gb_pluginOptionStop;
        private System.Windows.Forms.RadioButton rb_pluginStopMining;
        private System.Windows.Forms.RadioButton rb_pluginEndMining;
        private System.Windows.Forms.TextBox tbx_pluginStopDelayMS;
        private System.Windows.Forms.TextBox tbx_pluginStopDelayS;
        private System.Windows.Forms.TextBox tbx_pluginStopDelayM;
        private System.Windows.Forms.Label lbl_pluginlabel6;
        private System.Windows.Forms.Label lbl_pluginLabel7;
        private System.Windows.Forms.Label lbl_pluginLabel8;
        private System.Windows.Forms.Label lbl_pluginStopTime;
        private System.Windows.Forms.TextBox tbx_pluginMinTimeMS;
        private System.Windows.Forms.TextBox tbx_pluginMinTimeS;
        private System.Windows.Forms.TextBox tbx_pluginMinTimeM;
        private System.Windows.Forms.Label lbl_pluginLabel5;
        private System.Windows.Forms.Label lbl_pluginLabel4;
        private System.Windows.Forms.Label lbl_pluginLabel3;
        private System.Windows.Forms.Label lbl_pluginMiningTime;
        private System.Windows.Forms.Label lbl_pluginSteps;
    }
}
