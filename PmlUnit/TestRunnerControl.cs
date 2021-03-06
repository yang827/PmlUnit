﻿// Copyright (c) 2019 Florian Zimmermann.
// Licensed under the MIT License: https://opensource.org/licenses/MIT
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PmlUnit
{
    partial class TestRunnerControl : UserControl
    {
        private delegate void RunDelegate(IList<Test> tests, int index);

        private readonly TestCaseProvider TestProvider;
        private readonly AsyncTestRunner Runner;
        private readonly SettingsProvider Settings;

        public TestRunnerControl(TestCaseProvider testProvider, AsyncTestRunner runner, SettingsProvider settings)
        {
            if (testProvider == null)
                throw new ArgumentNullException(nameof(testProvider));
            if (runner == null)
                throw new ArgumentNullException(nameof(runner));
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            TestProvider = testProvider;
            Runner = runner;
            Runner.TestCompleted += OnTestCompleted;
            Runner.RunCompleted += OnRunCompleted;
            Settings = settings;

            InitializeComponent();
            ResetSplitContainerOrientation();

            TestList.Grouping = settings.TestGrouping;
            EditorDialog.Font = Font;
        }

        public void LoadTests()
        {
            TestList.TestCases.Clear();
            TestList.TestCases.AddRange(TestProvider.GetTestCases());
            TestSummary.UpdateSummary(TestList.AllTests);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                Runner.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            EditorDialog.Font = Font;
        }

        private void OnRunAllLinkClick(object sender, EventArgs e)
        {
            Run(TestList.AllTests);
        }

        private void OnRunLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            RunContextMenu.Show(RunLinkLabel, new Point(0, RunLinkLabel.Height));
        }

        private void OnRunFailedTestsMenuItemClick(object sender, EventArgs e)
        {
            Run(TestList.FailedTests);
        }

        private void OnRunNotExecutedTestsMenuItemClick(object sender, EventArgs e)
        {
            Run(TestList.NotExecutedTests);
        }

        private void OnRunPassedTestsMenuItemClick(object sender, EventArgs e)
        {
            Run(TestList.PassedTests);
        }

        private void OnRunSelectedTestsMenuItemClick(object sender, EventArgs e)
        {
            Run(TestList.SelectedTests);
        }

        private void Run(IList<Test> tests)
        {
            Enabled = false;

            ExecutionProgressBar.Value = 0;
            ExecutionProgressBar.Maximum = tests.Count;
            ExecutionProgressBar.Color = Color.Green;

            try
            {
                Runner.RunAsync(tests);
            }
            catch (Exception error)
            {
                Enabled = true;
                MessageBox.Show(error.ToString(), "Test run failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnTestCompleted(object sender, TestCompletedEventArgs e)
        {
            ExecutionProgressBar.Increment(1);
            if (e.Test.Status == TestStatus.Failed)
                ExecutionProgressBar.Color = Color.Red;

            Update();
        }

        private void OnRunCompleted(object sender, TestRunCompletedEventArgs e)
        {
            Enabled = true;
            TestSummary.UpdateSummary(e.Tests.ToList());
            if (e.Error != null)
                MessageBox.Show(e.Error.ToString(), "Test run failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void OnRefreshLinkClick(object sender, EventArgs e)
        {
            Runner.RefreshIndex();
            TestList.TestCases.Clear();
            TestList.TestCases.AddRange(TestProvider.GetTestCases().Select(Reload));
        }

        private TestCase Reload(TestCase testCase)
        {
            try
            {
                Runner.Reload(testCase);
            }
            catch (PmlException e)
            {
                Console.WriteLine("Failed to reload test case {0}", testCase.Name);
                Console.WriteLine(e);
            }

            return testCase;
        }

        private void OnGroupByLinkLabelLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            GroupByMenu.Show(GroupByLinkLabel, new Point(0, GroupByLinkLabel.Height));
        }

        private void OnGroupByTestResultMenuItemClick(object sender, EventArgs e)
        {
            TestList.Grouping = TestListGrouping.Result;
        }

        private void OnGroupByTestCaseNameMenuItemClick(object sender, EventArgs e)
        {
            TestList.Grouping = TestListGrouping.TestCase;
        }

        private void OnTestListGroupingChanged(object sender, EventArgs e)
        {
            Settings.TestGrouping = TestList.Grouping;
            GroupByTestResultToolStripMenuItem.Checked = TestList.Grouping == TestListGrouping.Result;
            GroupByTestCaseNameToolStripMenuItem.Checked = TestList.Grouping == TestListGrouping.TestCase;
        }

        private void OnTestListSelectionChanged(object sender, EventArgs e)
        {
            var selected = TestList.SelectedTests;
            TestDetails.Test = selected.FirstOrDefault();
            TestSummary.Visible = selected.Count != 1;
            TestDetails.Visible = selected.Count == 1;
        }

        private void OnTestListTestActivate(object sender, TestEventArgs e)
        {
            OpenFile(e.Test.FileName, e.Test.LineNumber);
        }

        private void OnTestDetailsFileActivate(object sender, FileEventArgs e)
        {
            OpenFile(e.FileName, e.LineNumber);
        }

        private void OpenFile(string fileName, int lineNumber)
        {
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
                return;

            var descriptor = Settings.CodeEditor;
            if (descriptor == null)
            {
                var result = EditorDialog.ShowDialog(this);
                if (result == DialogResult.OK)
                {
                    descriptor = EditorDialog.Descriptor;
                    Settings.CodeEditor = descriptor;
                }
                else
                {
                    return;
                }
            }

            try
            {
                var editor = descriptor.ToEditor();
                editor.OpenFile(fileName, lineNumber);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Failed to open file", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnSplitContainerSizeChanged(object sender, EventArgs e)
        {
            ResetSplitContainerOrientation();
        }

        private void ResetSplitContainerOrientation()
        {
            var size = TestResultSplitContainer.Size;
            var orientation = size.Width > size.Height ? Orientation.Vertical : Orientation.Horizontal;
            TestResultSplitContainer.Orientation = orientation;
        }
    }
}
