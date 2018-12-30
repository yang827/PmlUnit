﻿using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using PmlUnit.Properties;

namespace PmlUnit
{
    partial class TestDetailsView : UserControl
    {
        private Test TestField;
        private TestResult ResultField;

        public TestDetailsView()
        {
            InitializeComponent();

            var builder = new TestCaseBuilder("Test");
            builder.AddTest("Test");
            Test = builder.Build().Tests[0];
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Test Test
        {
            get { return TestField; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                TestField = value;
                TestNameLabel.Text = value.Name;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TestResult Result
        {
            get { return ResultField; }
            set
            {
                ResultField = value;
                if (value == null)
                {
                    TestResultIconLabel.Image = Resources.Unknown;
                    TestResultIconLabel.Text = "Not executed";
                    StackTraceLabel.Text = "";
                    ElapsedTimeLabel.Text = "";
                }
                else
                {
                    if (value.Error == null)
                    {
                        TestResultIconLabel.Image = Resources.Success;
                        TestResultIconLabel.Text = "Successful";
                        StackTraceLabel.Text = "";
                    }
                    else
                    {
                        TestResultIconLabel.Image = Resources.Failure;
                        TestResultIconLabel.Text = "Failed";
                        StackTraceLabel.Text = value.Error.Message;
                    }
                    ElapsedTimeLabel.Text = FormatDuration(value);
                }
            }
        }

        private static string FormatDuration(TestResult testResult)
        {
            var result = new StringBuilder("Elapsed Time: ");

            var millis = testResult.Duration.TotalMilliseconds;
            if (millis < 1)
                return result.Append(": < 1 ms").ToString();
            else if (millis < 1000)
                return result.AppendFormat(CultureInfo.CurrentCulture, "{0} ms", (int)millis).ToString();
            else if (millis < 10000)
                return result.AppendFormat(CultureInfo.CurrentCulture, "{0:N1} s", ((int)millis / 100) / 10.0).ToString();
            else
                return result.AppendFormat(CultureInfo.CurrentCulture, "{0:N0} s", millis / 1000).ToString();
        }
    }
}