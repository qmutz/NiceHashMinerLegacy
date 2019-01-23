namespace NiceHashMiner.Forms.Components
{
    partial class DevicesMainBoard
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.devicesDataGridView = new System.Windows.Forms.DataGridView();
            this.Enable = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.deviceHeader = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.StatusColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnTemperature = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnLoad = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnRMP = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Run = new System.Windows.Forms.DataGridViewButtonColumn();
            ((System.ComponentModel.ISupportInitialize)(this.devicesDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // devicesDataGridView
            // 
            this.devicesDataGridView.AllowUserToAddRows = false;
            this.devicesDataGridView.AllowUserToDeleteRows = false;
            this.devicesDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.devicesDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Enable,
            this.deviceHeader,
            this.StatusColumn,
            this.ColumnTemperature,
            this.ColumnLoad,
            this.ColumnRMP,
            this.Run});
            this.devicesDataGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.devicesDataGridView.Location = new System.Drawing.Point(0, 0);
            this.devicesDataGridView.Name = "devicesDataGridView";
            this.devicesDataGridView.ReadOnly = true;
            this.devicesDataGridView.RowHeadersVisible = false;
            this.devicesDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.devicesDataGridView.Size = new System.Drawing.Size(766, 181);
            this.devicesDataGridView.TabIndex = 114;
            // 
            // Enable
            // 
            this.Enable.FalseValue = "NO";
            this.Enable.HeaderText = "Enabled";
            this.Enable.Name = "Enable";
            this.Enable.ReadOnly = true;
            this.Enable.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Enable.TrueValue = "Ye43s";
            // 
            // deviceHeader
            // 
            this.deviceHeader.HeaderText = "Device";
            this.deviceHeader.Name = "deviceHeader";
            this.deviceHeader.ReadOnly = true;
            // 
            // StatusColumn
            // 
            this.StatusColumn.HeaderText = "Status";
            this.StatusColumn.Name = "StatusColumn";
            this.StatusColumn.ReadOnly = true;
            // 
            // ColumnTemperature
            // 
            this.ColumnTemperature.HeaderText = "Temp (°C)";
            this.ColumnTemperature.Name = "ColumnTemperature";
            this.ColumnTemperature.ReadOnly = true;
            // 
            // ColumnLoad
            // 
            this.ColumnLoad.HeaderText = "Load (%)";
            this.ColumnLoad.Name = "ColumnLoad";
            this.ColumnLoad.ReadOnly = true;
            // 
            // ColumnRMP
            // 
            this.ColumnRMP.HeaderText = "RPM";
            this.ColumnRMP.Name = "ColumnRMP";
            this.ColumnRMP.ReadOnly = true;
            // 
            // Run
            // 
            this.Run.HeaderText = "Start";
            this.Run.Name = "Run";
            this.Run.ReadOnly = true;
            this.Run.Text = "Start";
            // 
            // DevicesMainBoard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.devicesDataGridView);
            this.Name = "DevicesMainBoard";
            this.Size = new System.Drawing.Size(766, 181);
            ((System.ComponentModel.ISupportInitialize)(this.devicesDataGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView devicesDataGridView;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Enable;
        private System.Windows.Forms.DataGridViewTextBoxColumn deviceHeader;
        private System.Windows.Forms.DataGridViewTextBoxColumn StatusColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnTemperature;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnLoad;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnRMP;
        private System.Windows.Forms.DataGridViewButtonColumn Run;
    }
}
