using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using TestSuite.IncreaseVersion.Rules;

namespace TestSuite.IncreaseVersion.VisualStudioExtension
{
    public partial class IncreaseVersionForm : Form
    {
        private DTE dte;
        private IVsOutputWindowPane outputWindow;
        private IEnumerable<Project> projects;

        public IncreaseVersionForm()
        {
            InitializeComponent();
        }

        public IncreaseVersionForm(IServiceProvider serviceProvider) : this()
        {
            this.dte = serviceProvider.GetService(typeof(DTE)) as DTE;
            this.outputWindow = GetOutputWindowPane(serviceProvider);
            this.projects = GetProjects(dte.Solution);
        }

        private IVsOutputWindowPane GetOutputWindowPane(IServiceProvider serviceProvider)
        {
            var outWindow = serviceProvider.GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            var generalPaneGuid = VSConstants.GUID_OutWindowDebugPane;
            outWindow.GetPane(ref generalPaneGuid, out IVsOutputWindowPane generalPane);

            return generalPane;
        }

        private IEnumerable<Project> GetProjects(Solution solution)
        {
            var result = new List<Project>();

            for (int i = 1; i < solution.Projects.Count + 1; i++)
            {
                var proj = solution.Projects.Item(i);
                if (proj.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                    result.AddRange(GetSolutionFolderProjects(proj));
                else
                    result.Add(proj);
            }

            return result;
        }

        private IEnumerable<Project> GetSolutionFolderProjects(Project project)
        {
            var result = new List<Project>();

            if (project.ProjectItems != null)
            {
                for (int i = 1; i < project.ProjectItems.Count + 1; i++)
                {
                    var subProj = project.ProjectItems.Item(i).SubProject;
                    if (subProj != null)
                    {
                        if (subProj.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                            result.AddRange(GetSolutionFolderProjects(subProj));
                        else
                            result.Add(subProj);
                    }
                }
            }

            return result;
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
            if (this.projects != null)
            {
                foreach (var proj in this.projects)
                {
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

                var fileNames = GetProjectAssemblyInfoFiles();
                if (!fileNames.Any())
                {
                    outputWindow?.OutputString("No AssemblyInfo.cs found in any project. " +
                        "Lightweight solution load might be enabled. " +
                        "Please disable lightweight solution load and try again.\r\n");
                }
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

        private IEnumerable<string> GetProjectAssemblyInfoFiles()
        {
            var result = new List<string>();

            if(this.projects != null)
            {
                foreach (var proj in this.projects)
                {
                    try
                    {
                        var fileName = proj.FileName;
                        var projDirectory = Path.GetDirectoryName(fileName);
                        var assemblyInfoFile = Path.GetFullPath(Path.Combine(projDirectory, @"Properties\AssemblyInfo.cs"));
                        if(System.IO.File.Exists(assemblyInfoFile))
                            result.Add(assemblyInfoFile);

                    }
                    catch { }
                }
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
