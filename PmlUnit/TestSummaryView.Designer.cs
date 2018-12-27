﻿namespace PmlUnit
{
    partial class TestSummaryView
    {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.Label summaryLabel;
            System.Windows.Forms.FlowLayoutPanel testCountPanel;
            this.ResultLabel = new System.Windows.Forms.Label();
            this.RuntimeLabel = new System.Windows.Forms.Label();
            this.FailedTestCountLabel = new PmlUnit.IconLabel();
            this.SuccessfulTestCountLabel = new PmlUnit.IconLabel();
            this.NotExecutedTestCountLabel = new PmlUnit.IconLabel();
            summaryLabel = new System.Windows.Forms.Label();
            testCountPanel = new System.Windows.Forms.FlowLayoutPanel();
            testCountPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // summaryLabel
            // 
            summaryLabel.AutoSize = true;
            summaryLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            summaryLabel.Location = new System.Drawing.Point(3, 3);
            summaryLabel.Margin = new System.Windows.Forms.Padding(3);
            summaryLabel.Name = "summaryLabel";
            summaryLabel.Size = new System.Drawing.Size(83, 20);
            summaryLabel.TabIndex = 0;
            summaryLabel.Text = "Summary";
            // 
            // testCountPanel
            // 
            testCountPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            testCountPanel.Controls.Add(this.FailedTestCountLabel);
            testCountPanel.Controls.Add(this.SuccessfulTestCountLabel);
            testCountPanel.Controls.Add(this.NotExecutedTestCountLabel);
            testCountPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            testCountPanel.Location = new System.Drawing.Point(3, 48);
            testCountPanel.Name = "testCountPanel";
            testCountPanel.Size = new System.Drawing.Size(295, 99);
            testCountPanel.TabIndex = 3;
            testCountPanel.WrapContents = false;
            // 
            // ResultLabel
            // 
            this.ResultLabel.AutoSize = true;
            this.ResultLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ResultLabel.Location = new System.Drawing.Point(3, 29);
            this.ResultLabel.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.ResultLabel.Name = "ResultLabel";
            this.ResultLabel.Size = new System.Drawing.Size(148, 13);
            this.ResultLabel.TabIndex = 1;
            this.ResultLabel.Text = "Last test run: Successful";
            // 
            // RuntimeLabel
            // 
            this.RuntimeLabel.AutoSize = true;
            this.RuntimeLabel.Location = new System.Drawing.Point(149, 29);
            this.RuntimeLabel.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.RuntimeLabel.Name = "RuntimeLabel";
            this.RuntimeLabel.Size = new System.Drawing.Size(116, 13);
            this.RuntimeLabel.TabIndex = 2;
            this.RuntimeLabel.Text = "(Total runtime: 0:00:00)";
            // 
            // FailedTestCountLabel
            // 
            this.FailedTestCountLabel.AutoSize = true;
            this.FailedTestCountLabel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.FailedTestCountLabel.Image = global::PmlUnit.Properties.Resources.Failure;
            this.FailedTestCountLabel.Location = new System.Drawing.Point(0, 0);
            this.FailedTestCountLabel.Margin = new System.Windows.Forms.Padding(0);
            this.FailedTestCountLabel.MinimumSize = new System.Drawing.Size(16, 16);
            this.FailedTestCountLabel.Name = "FailedTestCountLabel";
            this.FailedTestCountLabel.Size = new System.Drawing.Size(82, 17);
            this.FailedTestCountLabel.TabIndex = 0;
            this.FailedTestCountLabel.Text = "0 failed tests";
            this.FailedTestCountLabel.Visible = false;
            // 
            // SuccessfulTestCountLabel
            // 
            this.SuccessfulTestCountLabel.AutoSize = true;
            this.SuccessfulTestCountLabel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.SuccessfulTestCountLabel.Image = global::PmlUnit.Properties.Resources.Success;
            this.SuccessfulTestCountLabel.Location = new System.Drawing.Point(0, 17);
            this.SuccessfulTestCountLabel.Margin = new System.Windows.Forms.Padding(0);
            this.SuccessfulTestCountLabel.MinimumSize = new System.Drawing.Size(16, 16);
            this.SuccessfulTestCountLabel.Name = "SuccessfulTestCountLabel";
            this.SuccessfulTestCountLabel.Size = new System.Drawing.Size(107, 17);
            this.SuccessfulTestCountLabel.TabIndex = 1;
            this.SuccessfulTestCountLabel.Text = "0 successful tests";
            this.SuccessfulTestCountLabel.Visible = false;
            // 
            // NotExecutedTestCountLabel
            // 
            this.NotExecutedTestCountLabel.AutoSize = true;
            this.NotExecutedTestCountLabel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.NotExecutedTestCountLabel.Image = global::PmlUnit.Properties.Resources.Unknown;
            this.NotExecutedTestCountLabel.Location = new System.Drawing.Point(0, 34);
            this.NotExecutedTestCountLabel.Margin = new System.Windows.Forms.Padding(0);
            this.NotExecutedTestCountLabel.MinimumSize = new System.Drawing.Size(16, 16);
            this.NotExecutedTestCountLabel.Name = "NotExecutedTestCountLabel";
            this.NotExecutedTestCountLabel.Size = new System.Drawing.Size(119, 17);
            this.NotExecutedTestCountLabel.TabIndex = 2;
            this.NotExecutedTestCountLabel.Text = "0 not executed tests";
            this.NotExecutedTestCountLabel.Visible = false;
            // 
            // TestSummaryView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(testCountPanel);
            this.Controls.Add(this.RuntimeLabel);
            this.Controls.Add(this.ResultLabel);
            this.Controls.Add(summaryLabel);
            this.Name = "TestSummaryView";
            this.Size = new System.Drawing.Size(301, 150);
            testCountPanel.ResumeLayout(false);
            testCountPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label ResultLabel;
        private System.Windows.Forms.Label RuntimeLabel;
        private IconLabel FailedTestCountLabel;
        private IconLabel SuccessfulTestCountLabel;
        private IconLabel NotExecutedTestCountLabel;
    }
}