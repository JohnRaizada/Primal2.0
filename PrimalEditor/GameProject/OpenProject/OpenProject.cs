using PrimalEditor.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace PrimalEditor.GameProject
{
    /// <summary>
    /// Represents data for a project.
    /// </summary>
    [DataContract]
    public class ProjectData
    {
        /// <summary>
        /// Gets or sets the name of the project.
        /// </summary>
        [DataMember]
        public string? ProjectName { get; set; }
        /// <summary>
        /// Gets or sets the path to the project.
        /// </summary>
        [DataMember]
        public string? ProjectPath { get; set; }
        /// <summary>
        /// Gets or sets the date that the project was last modified.
		/// </summary>
        [DataMember]
        public DateTime Date { get; set; }
        /// <summary>
        /// Gets the full path to the project.
        /// </summary>
        public string FullPath
        {
            get => $"{ProjectPath}{ProjectName}{Project.Extension}";
        }
        /// <summary>
        /// Gets or sets the icon for the project.
        /// </summary>
        public byte[]? Icon { get; set; }
        /// <summary>
        /// Gets or sets the screenshot for the project.
        /// </summary>
        public byte[]? Screenshot { get; set; }
    }
    /// <summary>
    /// Represents a list of project data.
    /// </summary>
    [DataContract]
    public class ProjectDataList
    {
        /// <summary>
        /// Gets or sets the list of projects.
        /// </summary>
        [DataMember]
        public List<ProjectData>? Projects { get; set; }
    }
    class OpenProject
    {
        private static readonly string _applicationDataPath =
            $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\PrimalEditor\";

        private static readonly string _projectDataPath;
        private static readonly ObservableCollection<ProjectData> _projects = new ObservableCollection<ProjectData>();
        public static ReadOnlyObservableCollection<ProjectData> Projects { get; }

        private static void ReadProjectData()
        {
            if (!File.Exists(_projectDataPath)) return;
            var projects = Serializer.FromFile<ProjectDataList>(_projectDataPath).Projects?
                .OrderByDescending(x => x.Date);
            _projects.Clear();
            if (projects == null) return;
            foreach (var project in projects)
            {
                if (!File.Exists(project.FullPath)) continue;
                project.Icon = File.ReadAllBytes($@"{project.ProjectPath}\.Primal\Icon.png");
                project.Screenshot = File.ReadAllBytes($@"{project.ProjectPath}\.Primal\Screenshot.png");
                _projects.Add(project);
            }
        }
        private static void WriteProjectData()
        {
            var projects = _projects.OrderBy(x => x.Date).ToList();
            Serializer.ToFile(new ProjectDataList() { Projects = projects }, _projectDataPath);
        }
        public static Project Open(ProjectData data)
        {
            ReadProjectData();
            var project = _projects.FirstOrDefault(x => x.FullPath == data.FullPath);
            if (project != null)
            {
                project.Date = DateTime.Now;
            }
            else
            {
                project = data;
                project.Date = DateTime.Now;
                _projects.Add(project);
            }

            WriteProjectData();
            return Project.Load(project.FullPath);
        }
        static OpenProject()
        {
            try
            {
                if (!Directory.Exists(_applicationDataPath)) Directory.CreateDirectory(_applicationDataPath);
                _projectDataPath = $@"{_applicationDataPath}ProjectData.xml";
                Projects = new ReadOnlyObservableCollection<ProjectData>(_projects);
                ReadProjectData();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Logger.Log(MessageType.Error, $"Failed to read project data");
                throw;
            }
        }
    }
}
