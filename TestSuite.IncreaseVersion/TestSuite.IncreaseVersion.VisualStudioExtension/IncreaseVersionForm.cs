using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using TestSuite.IncreaseVersion.Rules;
using IoFile = System.IO.File;

namespace TestSuite.IncreaseVersion.VisualStudioExtension
{
    public partial class IncreaseVersionForm : Form
    {
        private DTE dte;
        private IVsOutputWindowPane outputWindow;

        public IncreaseVersionForm()
        {
            InitializeComponent();
        }

        public IncreaseVersionForm(IServiceProvider serviceProvider) : this()
        {
            this.dte = serviceProvider.GetService(typeof(DTE)) as DTE;
            this.outputWindow = GetOutputWindowPane(serviceProvider);
        }

        private IVsOutputWindowPane GetOutputWindowPane(IServiceProvider serviceProvider)
        {
            var outWindow = serviceProvider.GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            var generalPaneGuid = VSConstants.GUID_OutWindowDebugPane;
            outWindow.GetPane(ref generalPaneGuid, out IVsOutputWindowPane generalPane);

            return generalPane;
        }

        protected override void OnLoad(EventArgs e)
        {
            if (this.dte != null)
            {
                var version = GetFirstProjectVersion(this.dte.Solution);
                txtVersion.Text = version;
            }

            txtVersion.SelectAll();
            txtVersion.Focus();

            base.OnLoad(e);
        }

        private string GetFirstProjectVersion(Solution solution)
        {
            var projects = dte.Solution.Projects;
            if(projects != null)
            {
                for (var i = 1; i < projects.Count + 1; i++)
                {
                    var proj = projects.Item(i);
                    if (proj.Properties != null)
                    {
                        for (var j = 1; j < proj.Properties.Count; j++)
                        {
                            var prop = proj.Properties.Item(j);
                            if (prop.Name == "AssemblyVersion")
                                return prop.Value.ToString();
                        }
                    }
                }
            }

            return "1.0.0.0";
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (Form.ModifierKeys == Keys.None && keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }

            return base.ProcessDialogKey(keyData);
        }

        private void OnReplaceClick(object sender, EventArgs e)
        {
            if (dte != null)
            {
                outputWindow?.OutputString($"Updating project versions to {txtVersion.Text}.\r\n");

                var fileNames = GetAssemblyInfoFiles(dte.Solution);
                if (!fileNames.Any())
                    outputWindow?.OutputString("No Properties\\AssemblyInfo.cs found in any project. \r\n");
                else
                {
                    var increaseVersion = CreateIncreaseVersion(txtVersion.Text);
                    foreach (var fileName in fileNames)
                    {
                        outputWindow?.OutputString(fileName + "\r\n");
                        increaseVersion.Files.Add(new File(fileName));
                    }

                    increaseVersion.Execute();

                    outputWindow?.OutputString("Project versions updated.\r\n");
                }
            }

            this.Close();
        }

        private IEnumerable<string> GetAssemblyInfoFiles(Solution sln)
        {
            var result = new List<string>();

            var slnPath = Path.GetDirectoryName(sln.FileName);
            for (short i = 1; i < sln.Projects.Count + 1; i++)
            {
                try
                {
                    var proj = sln.Projects.Item(i);
                    var projPath = Path.GetDirectoryName(proj.UniqueName);
                    if (!Path.IsPathRooted(projPath))
                        projPath = Path.Combine(slnPath, projPath);

                    var assemblyInfoPath = Path.GetFullPath(Path.Combine(projPath, @"Properties\AssemblyInfo.cs"));
                    if (IoFile.Exists(assemblyInfoPath))
                        result.Add(assemblyInfoPath);
                } catch { }
            }

            return result;
        }
        
        private IncreaseVersion CreateIncreaseVersion(string version)
        {
            var result = new IncreaseVersion();
            result.Rules.Add(new RemoveAssemblyFileVersionRule());
            result.Rules.Add(new RemoveCommentedAssemblyFileVersionRule());
            result.Rules.Add(new RemoveCommentedAssemblyVersionRule());

            var updateAssemblyVersionRule = new UpdateAssemblyVersionRule();
            updateAssemblyVersionRule.NewVersion = version;
            result.Rules.Add(updateAssemblyVersionRule);

            return result;
        }
    }
}
