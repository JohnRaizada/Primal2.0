using PrimalEditor.Components;
using PrimalEditor.DLLWrapper;
using PrimalEditor.GameDev;
using PrimalEditor.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PrimalEditor.GameProject
{
    /// <summary>
    /// Represents a game project.
    /// </summary>
    [DataContract(Name = "Game")]
    public class Project : ViewModelBase
    {
        private static bool _isModified = false;
        /// <summary>
        /// Gets or sets a value indicating whether the project has been modified.
        /// </summary>
        public bool IsModified
        {
            get => _isModified;
            set => _isModified = value;
        }
        /// <summary>
        /// Gets the file extension for project files.
        /// </summary>
        public static string Extension => ".primal";
        /// <summary>
        /// Gets or sets the name of the project.
        /// </summary>
        [DataMember]
        public string Name { get; private set; } = "New Project";
        /// <summary>
        /// Gets the root folder that contains the current project.
        /// </summary>
        [DataMember]
        public string Path { get; private set; }
        /// <summary>
        /// Gets the full path of the current Primal project file, including its file name and extension.
        /// </summary>
        public string FullPath => $@"{Path}\{Name}{Extension}";
        /// <summary>
        /// Gets the path to the solution file for the project.
        /// </summary>
        public string Solution => $@"{Path}{Name}.sln";
        /// <summary>
        /// Gets the path to the temporary folder for the project.
        /// </summary>
        public string TempFolder => $@"{Path}.Primal\Temp\";
        private int _buildConfig;
        /// <summary>
        /// Gets or sets the build configuration for the project.
        /// </summary>
        [DataMember]
        public int BuildConfig
        {
            get => _buildConfig;
            set
            {
                if (_buildConfig != value)
                {
                    _buildConfig = value;
                    OnPropertyChanged(nameof(BuildConfig));
                }
            }
        }
        /// <summary>
        /// Gets the build configuration for standalone builds of the project.
        /// </summary>
        public BuildConfiguration StandAloneBuildConfig => BuildConfig == 0 ? BuildConfiguration.Debug : BuildConfiguration.Release;
        /// <summary>
        /// Gets the build configuration for DLL builds of the project.
        /// </summary>
        public BuildConfiguration DLLBuildConfig => BuildConfig == 0 ? BuildConfiguration.DebugEditor : BuildConfiguration.ReleaseEditor;
        private string[]? _availableScripts;
        /// <summary>
        /// Gets or sets the available scripts for the project.
        /// </summary>
        public string[]? AvailableScripts
        {
            get => _availableScripts;
            private set
            {
                if (_availableScripts != value)
                {
                    _availableScripts = value;
                    OnPropertyChanged(nameof(AvailableScripts));
                }
            }
        }
        [DataMember(Name = nameof(Scenes))]
        private readonly ObservableCollection<Scene> _scenes = new ObservableCollection<Scene>();
        /// <summary>
        /// Gets the collection of scenes in the project.
        /// </summary>
        public ReadOnlyObservableCollection<Scene>? Scenes
        {
            get; private set;
        }

        private Scene? _activeScene;
        /// <summary>
        /// Gets or sets the active scene in the project.
        /// </summary>

        public Scene? ActiveScene
        {
            get => _activeScene;
            set
            {
                if (_activeScene == value) return;
                _activeScene = value;
                OnPropertyChanged(nameof(ActiveScene));
            }
        }
        /// <summary>
        /// Gets the current project.
        /// </summary>
        public static Project? Current => Application.Current.MainWindow?.DataContext as Project;
        /// <summary>
        /// Gets the undo/redo manager for the project.
        /// </summary>
        public static UndoRedo UndoRedo { get; } = new UndoRedo();
        /// <summary>
        /// Gets the command for undoing the last action.
        /// </summary>
        public ICommand? UndoCommand { get; private set; }
        /// <summary>
        /// Gets the command for redoing the last undone action.
        /// </summary>
        public ICommand? RedoCommand { get; private set; }
        /// <summary>
        /// Gets the command for adding a new scene to the project.
        /// </summary>
        public ICommand? AddSceneCommand { get; private set; }
        /// <summary>
        /// Gets the command for saving the project.
        /// </summary>
        public ICommand? SaveCommand { get; private set; }
        /// <summary>
        /// Gets the command for starting debugging of the project.
        /// </summary>
        public ICommand? DebugStartCommand { get; private set; }
        /// <summary>
        /// Gets the command for starting the project without debugging.
		/// </summary>
        public ICommand? DebugStartWithoutDebuggingCommand { get; private set; }
        /// <summary>
        /// Gets the command for stopping debugging of the project.
        /// </summary>
        public ICommand? DebugStopCommand { get; private set; }
        /// <summary>
        /// Gets the command for removing a scene from the project.
        /// </summary>
        public ICommand? RemoveSceneCommand { get; private set; }
        /// <summary>
        /// Gets the command for building the project.
        /// </summary>
        public ICommand? BuildCommand { get; private set; }
        /// <summary>
        /// Gets the command for exiting the project.
        /// </summary>
        public ICommand? ExitCommand { get; private set; }
        /// <summary>
        /// Gets the path to the content folder for the project.
        /// </summary>
        public string ContentPath => $@"{Path}Content\";
        private void SetCommands()
        {
            AddSceneCommand = new RelayCommand<object>(x =>
            {
                AddScene($"New Scene {_scenes.Count}");
                var newScene = _scenes.Last();
                var sceneIndex = _scenes.Count - 1;
                UndoRedo.Add(new UndoRedoAction(
                    () => RemoveScene(newScene),
                    () => _scenes.Insert(sceneIndex, newScene),
                    $"Add {newScene.Name}"
                ));
            });
            RemoveSceneCommand = new RelayCommand<Scene>(x =>
            {
                var sceneIndex = _scenes.IndexOf(x);
                RemoveScene(x);
                UndoRedo.Add(new UndoRedoAction(
                    () => _scenes.Insert(sceneIndex, x),
                    () => RemoveScene(x),
                    $"Remove {x.Name}"
                ));
            }, x => !x.IsActive);
            UndoCommand = new RelayCommand<object>(x => UndoRedo.Undo(), x => UndoRedo.UndoList.Any());
            RedoCommand = new RelayCommand<object>(x => UndoRedo.Redo(), x => UndoRedo.RedoList.Any());
            SaveCommand = new RelayCommand<object>(x => Save(this));
            DebugStartCommand = new RelayCommand<object>(async x => await RunGame(true), x => !VisualStudio.IsDebugging() && VisualStudio.BuildDone);
            DebugStartWithoutDebuggingCommand = new RelayCommand<object>(async x => await RunGame(false), x => !VisualStudio.IsDebugging() && VisualStudio.BuildDone);
            DebugStopCommand = new RelayCommand<object>(async x => await StopGame(), x => VisualStudio.IsDebugging());
            BuildCommand = new RelayCommand<bool>(async x => await BuildGameCodeDLL(x), x => !VisualStudio.IsDebugging() && VisualStudio.BuildDone);
            ExitCommand = new RelayCommand<object>(x => InitiatializeExitIntent());
            OnPropertyChanged(nameof(AddSceneCommand));
            OnPropertyChanged(nameof(RemoveSceneCommand));
            OnPropertyChanged(nameof(UndoCommand));
            OnPropertyChanged(nameof(RedoCommand));
            OnPropertyChanged(nameof(SaveCommand));
            OnPropertyChanged(nameof(DebugStartCommand));
            OnPropertyChanged(nameof(DebugStartWithoutDebuggingCommand));
            OnPropertyChanged(nameof(DebugStopCommand));
            OnPropertyChanged(nameof(BuildCommand));
            OnPropertyChanged(nameof(ExitCommand));
        }
        private void InitiatializeExitIntent()
        {
            if (_isModified)
            {
                switch (MessageBox.Show("Do you want to save the changes before you exit?", "Exit", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Cancel))
                {
                    case MessageBoxResult.Yes:
                        Save(this);
                        break;
                    case MessageBoxResult.Cancel:
                        return;
                    default:
                        break;
                }
                Application.Current.Shutdown();
                return;
            }
            if (MessageBox.Show("Are you sure you really want to exit?", "Exit", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No) return;
            Application.Current.Shutdown();
        }
        private void AddScene(string sceneName)
        {
            Debug.Assert(!string.IsNullOrEmpty(sceneName.Trim()));
            _scenes.Add(new Scene(this, sceneName));
            _isModified = true;
        }

        private void RemoveScene(Scene scene)
        {
            Debug.Assert(_scenes.Contains(scene));
            _scenes.Remove(scene);
            _isModified = true;
        }
        /// <summary>
        /// Loads a project from a file.
        /// </summary>
        /// <param name="file">The path to the file to load the project from.</param>
        /// <returns>The loaded project.</returns>
        public static Project? Load(string file)
        {
            Debug.Assert(File.Exists(file));
            return Serializer.FromFile<Project>(file);
        }
        /// <summary>
        /// Unloads the project.
        /// </summary>
        public void Unload()
        {
            UnloadGameCodeDLL();
            VisualStudio.CloseVisualStudio();
            UndoRedo.Reset();
            Logger.Clear();
            DeleteTempFolder();
        }
        private void DeleteTempFolder()
        {
            if (Directory.Exists(TempFolder))
            {
                Directory.Delete(TempFolder, true);
            }
        }
        private static void Save(Project project)
        {
            Serializer.ToFile(project, project.FullPath);
            Logger.Log(MessageType.Info, $"Saved project to {project.FullPath}");
            _isModified = false;
        }
        private void SaveToBinary()
        {
            var configName = VisualStudio.GetConfigurationName(StandAloneBuildConfig);
            var bin = $@"{Path}x64\{configName}\game.bin";
            BinaryWriter bw;
            try
            {
                bw = new BinaryWriter(File.Open(bin, FileMode.Create, FileAccess.Write));
            }
            catch (DirectoryNotFoundException)
            {
                SystemOperations.CreateNewFolderWithName($@"{Path}x64\{configName}");
                bw = new BinaryWriter(File.Open(bin, FileMode.Create, FileAccess.Write));
            }
            using (bw)
            {
                if (ActiveScene == null) return;
                if (ActiveScene.GameEntities == null) return;
                bw.Write(ActiveScene.GameEntities.Count);
                foreach (var entity in ActiveScene.GameEntities)
                {
                    bw.Write(0); //entity type (reserved for later)
                    bw.Write(entity.Components.Count);
                    foreach (var component in entity.Components)
                    {
                        bw.Write((int)component.ToEnumType());
                        component.WriteToBinary(bw);
                    }
                }
            }
        }
        private async Task RunGame(bool debug)
        {
            await Task.Run(() => VisualStudio.BuildSolution(this, StandAloneBuildConfig, debug));
            if (VisualStudio.BuildSucceeded)
            {
                await Task.Run(() => VisualStudio.Run(this, StandAloneBuildConfig, debug));
            }
        }
        private async Task StopGame() => await Task.Run(() => VisualStudio.Stop());
        private async Task BuildGameCodeDLL(bool showWindow = true)
        {
            try
            {
                UnloadGameCodeDLL();
                await Task.Run(() => VisualStudio.BuildSolution(this, DLLBuildConfig, showWindow));
                if (VisualStudio.BuildSucceeded)
                {
                    try
                    {
                        await Task.Run(() => SaveToBinary());
                    }
                    catch (Exception)
                    {
                        Logger.Log(MessageType.Warning, "Failed to create a game binary. Try running the game first.");
                    }
                    LoadGameCodeDLL();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw;
            }
        }
        private void LoadGameCodeDLL()
        {
            var configName = VisualStudio.GetConfigurationName(DLLBuildConfig);
            var dll = $@"{Path}x64\{configName}\{Name}.dll";
            AvailableScripts = null;
            if (File.Exists(dll) && EngineAPI.LoadGameCodeDll(dll) != 0)
            {
                AvailableScripts = EngineAPI.GetScriptNames();
                ActiveScene?.GameEntities?.Where(x => x.GetComponent<Script>() != null).ToList().ForEach(x => x.IsActive = true);
                Logger.Log(MessageType.Info, "Game code Dll loaded successfully");
            }
            else
            {
                Logger.Log(MessageType.Warning, "Failed to load game code DLL file. Try to build the project first");
            }
        }
        private void UnloadGameCodeDLL()
        {
            ActiveScene?.GameEntities?.Where(x => x.GetComponent<Script>() != null).ToList().ForEach(x => x.IsActive = false);
            if (EngineAPI.UnloadGameCodeDll() != 0)
            {
                Logger.Log(MessageType.Info, "Game code Dll unloaded successfully");
                AvailableScripts = null;
            }
        }
        [OnDeserialized]
        private async void OnDeserialized(StreamingContext context)
        {
            if (_scenes != null)
            {
                Scenes = new ReadOnlyObservableCollection<Scene>(_scenes);
                OnPropertyChanged(nameof(Scenes));
            }
            ActiveScene = _scenes?.FirstOrDefault(x => x.IsActive);
            Debug.Assert(ActiveScene != null);
            await BuildGameCodeDLL(false);
            SetCommands();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Project"/> class with the specified name and path.
		/// </summary>
        public Project(string name, string path)
        {
            Name = name;
            Path = path;
            Debug.Assert(File.Exists((Path + Name + Extension).ToLower()));
            OnDeserialized(new StreamingContext());
        }
    }
}
