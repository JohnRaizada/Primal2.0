using PrimalEditor.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace PrimalEditor.GameProject
{
    /// <summary>
    /// Represents a template for creating new projects.
    /// </summary>
    [DataContract]
    public class ProjectTemplate
    {
        /// <summary>
        /// Gets or sets the type of project that the template is for.
        /// </summary>
        [DataMember]
        public string? ProjectType { get; set; }
        /// <summary>
        /// Gets or sets the path to the project file for the template.
        /// </summary>
        [DataMember]
        public string? ProjectFile { get; set; }
        /// <summary>
        /// Gets or sets the list of folders to create for the new project.
        /// </summary>
        [DataMember]
        public List<string>? Folders { get; set; }
        /// <summary>
        /// Gets or sets the icon for the template.
        /// </summary>
        public byte[]? Icon { get; set; }
        /// <summary>
        /// Gets or sets the screenshot for the template.
        /// </summary>
        public byte[]? Screenshot { get; set; }
        /// <summary>
        /// Gets or sets the path to the icon file for the template.
        /// </summary>
        public string? IconFilePath { get; set; }
        /// <summary>
        /// Gets or sets the path to the screenshot file for the template.
        /// </summary>
        public string? ScreenshotFilePath { get; set; }
        /// <summary>
        /// Gets or sets the path to the project file for the template.
        /// </summary>
        public string? ProjectFilePath { get; set; }
        /// <summary>
        /// Gets or sets the path to the template.
        /// </summary>
        public string? TemplatePath { get; set; }
    }
    class NewProject : ViewModelBase
    {
        //TODO: get the path from the installation location
        private readonly string _templatePath = @"C:\Users\indus\source\repos\Primal\PrimalEditor\ProjectTemplates";
        private string _projectName = "NewProject";
        public string ProjectName
        {
            get => _projectName;
            set
            {
                if (_projectName == value) return;
                _projectName = value;
                ValidateProjectPath();
                OnPropertyChanged(nameof(ProjectName));
            }
        }
        private string _projectPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\PrimalProjects\";
        public string ProjectPath
        {
            get => _projectPath;
            set
            {
                if (_projectPath == value) return;
                _projectPath = value;
                ValidateProjectPath();
                OnPropertyChanged(nameof(ProjectPath));
            }
        }
        private bool _isValid;
        public bool IsValid
        {
            get => _isValid;
            set
            {
                if (_isValid == value) return;
                _isValid = value;
                OnPropertyChanged(nameof(IsValid));
            }
        }
        private string? _errorMsg;
        public string? ErrorMsg
        {
            get => _errorMsg;
            set
            {
                if (_errorMsg == value) return;
                _errorMsg = value;
                OnPropertyChanged(nameof(ErrorMsg));
            }
        }
        private readonly ObservableCollection<ProjectTemplate> _projectTemplates = new();
        public ReadOnlyObservableCollection<ProjectTemplate> ProjectTemplates { get; }
        private bool ValidateProjectPath()
        {
            var path = ProjectPath;
            if (!Path.EndsInDirectorySeparator(path)) path += @"\";
            path += $@"{ProjectName}\";
            var nameRegex = new Regex(@"^[A-Za-z_][A-Za-z0-9_]*$");
            IsValid = false;
            if (string.IsNullOrWhiteSpace(ProjectName.Trim())) ErrorMsg = "Type in a project name.";
            else if (!nameRegex.IsMatch(ProjectName)) ErrorMsg = "Invalid character(s) used in project name.";
            else if (string.IsNullOrWhiteSpace(ProjectPath.Trim())) ErrorMsg = "Select a valid project folder.";
            else if (ProjectPath.IndexOfAny(Path.GetInvalidPathChars()) != -1) ErrorMsg = "Invalid character(s) used in project path.";
            else if (Directory.Exists(path) && Directory.EnumerateFileSystemEntries(path).Any()) ErrorMsg = "Selected project folder already exists and is not empty.";
            else
            {
                ErrorMsg = string.Empty;
                IsValid = true;
            }
            return IsValid;
        }
        public string? CreateProject(ProjectTemplate template)
        {
            ValidateProjectPath();
            if (!IsValid) return string.Empty;
            if (!Path.EndsInDirectorySeparator(ProjectPath)) ProjectPath += @"\";
            var path = $@"{ProjectPath}{ProjectName}\";
            try
            {
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                if (template.Folders == null) return null;
                foreach (var folder in template.Folders)
                {
                    string? directoryName = Path.GetDirectoryName(path);
                    if (directoryName == null) continue;
                    Directory.CreateDirectory(Path.GetFullPath(Path.Combine(directoryName, folder)));
                }
                var dirInfo = new DirectoryInfo(path + @".Primal\");
                dirInfo.Attributes |= FileAttributes.Hidden;
                if (template.IconFilePath == null) return null;
                File.Copy(template.IconFilePath, Path.GetFullPath(Path.Combine(dirInfo.FullName, "Icon.png")));
                if (template.ScreenshotFilePath == null) return null;
                File.Copy(template.ScreenshotFilePath, Path.GetFullPath(Path.Combine(dirInfo.FullName, "Screenshot.png")));
                if (template.ProjectFilePath == null) return null;
                var projectXml = File.ReadAllText(template.ProjectFilePath);
                projectXml = string.Format(projectXml, ProjectName, path);
                var projectPath = Path.GetFullPath(Path.Combine(path, $"{ProjectName}{Project.Extension}"));
                File.WriteAllText(projectPath, projectXml);
                CreateMSVCSolution(template, path);
                return path;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Logger.Log(MessageType.Error, $"Failed to load {ProjectName}");
                throw;
            }
        }
        private void CreateMSVCSolution(ProjectTemplate template, string projectPath)
        {
            if (template.TemplatePath == null) return;
            Debug.Assert(File.Exists(Path.Combine(template.TemplatePath, "MSVCSolution")));
            Debug.Assert(File.Exists(Path.Combine(template.TemplatePath, "MSVCProject")));
            var engineAPIPath = @"$(PRIMAL_ENGINE)Engine\EngineAPI\";
            Debug.Assert(Directory.Exists(engineAPIPath));
            var _0 = ProjectName;
            var _1 = "{" + Guid.NewGuid().ToString().ToUpper() + "}";
            var _2 = engineAPIPath;
            var _3 = "$(PRIMAL_ENGINE)";
            var solution = File.ReadAllText(Path.Combine(template.TemplatePath, "MSVCSolution"));
            solution = string.Format(solution, _0, _1, "{" + Guid.NewGuid().ToString().ToUpper() + "}");
            File.WriteAllText(Path.GetFullPath(Path.Combine(projectPath, $"{_0}.sln")), solution);
            var project = File.ReadAllText(Path.Combine(template.TemplatePath, "MSVCProject"));
            project = string.Format(project, _0, _1, _2, _3);
            File.WriteAllText(Path.GetFullPath(Path.Combine(projectPath, $@"GameCode\{_0}.vcxproj")), project);
        }
        public NewProject()
        {
            ProjectTemplates = new ReadOnlyObservableCollection<ProjectTemplate>(_projectTemplates);
            try
            {
                var templatesFiles = Directory.GetFiles(_templatePath, "template.xml", SearchOption.AllDirectories);
                Debug.Assert(templatesFiles.Any());
                foreach (var file in templatesFiles)
                {
                    var template = Serializer.FromFile<ProjectTemplate>(file);
                    if (template == null) return;
                    template.TemplatePath = Path.GetDirectoryName(file);
                    if (template.TemplatePath == null) continue;
                    template.IconFilePath = Path.GetFullPath(Path.Combine(template.TemplatePath, "Icon.png"));
                    template.Icon = File.ReadAllBytes(template.IconFilePath);
                    template.ScreenshotFilePath = Path.GetFullPath(Path.Combine(template.TemplatePath, "Screenshot.png"));
                    template.Screenshot = File.ReadAllBytes(template.ScreenshotFilePath);
                    if (template.ProjectFile == null) continue;
                    template.ProjectFilePath = Path.GetFullPath(Path.Combine(template.TemplatePath, template.ProjectFile));
                    _projectTemplates.Add(template);
                }
                ValidateProjectPath();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                Logger.Log(MessageType.Error, $"Failed to read project templates");
                throw;
            }
        }
    }
}
