using PrimalEditor.Components;
using PrimalEditor.Dependencies;
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
using System.Windows.Controls;
using System.Windows.Input;

namespace PrimalEditor.GameProject
{
    [DataContract(Name = "Game")]
    public class Project : ViewModelBase
    {
        public static string Extension => ".primal";
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
        public string Solution => $@"{Path}{Name}.sln";
        public string TempFolder => $@"{Path}.Primal\Temp\";
        // In your view model
        private StackPanel _selectedSdk;
        public StackPanel SelectedSdk
        {
            get => _selectedSdk;
            set
            {
                SetSelectedSdk(value);
            }
        }

        private StackPanel SetSelectedSdk(StackPanel value)
        {
            if (_selectedSdk != value)
            {
                _selectedSdk = value;
                switch ((_selectedSdk.Children[1] as TextBlock).Text)
                {
                    case "Android":
                        SelectedSdkDetails = new AndroidSdkDetails();
                        break;
                    default:
                        SelectedSdkDetails = null;
                        break;
                }
                OnPropertyChanged(nameof(SelectedSdk));
            }
            return _selectedSdk;
        }

        public object SelectedSdkDetails { get; set; }
        private int _buildConfig;
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
        public BuildConfiguration StandAloneBuildConfig => BuildConfig == 0 ? BuildConfiguration.Debug : BuildConfiguration.Release;
        public BuildConfiguration DLLBuildConfig => BuildConfig == 0 ? BuildConfiguration.DebugEditor : BuildConfiguration.ReleaseEditor;
        private string[] _availableScripts;
        public string[] AvailableScripts
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
        public ReadOnlyObservableCollection<Scene> Scenes
        {
            get; private set;
        }

        private Scene _activeScene;
        public Scene ActiveScene
        {
            get => _activeScene;
            set
            {
                if (_activeScene == value) return;
                _activeScene = value;
                OnPropertyChanged(nameof(ActiveScene));
            }
        }
        public static Project Current => Application.Current.MainWindow?.DataContext as Project;
        public static UndoRedo UndoRedo { get; } = new UndoRedo();
        public ICommand UndoCommand { get; private set; }
        public ICommand RedoCommand { get; private set; }
        public ICommand AddSceneCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand DebugStartCommand { get; private set; }
        public ICommand DebugStartWithoutDebuggingCommand { get; private set; }
        public ICommand DebugStopCommand { get; private set; }
        public ICommand RemoveSceneCommand { get; private set; }
        public ICommand BuildCommand { get; private set; }
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
            OnPropertyChanged(nameof(AddSceneCommand));
            OnPropertyChanged(nameof(RemoveSceneCommand));
            OnPropertyChanged(nameof(UndoCommand));
            OnPropertyChanged(nameof(RedoCommand));
            OnPropertyChanged(nameof(SaveCommand));
            OnPropertyChanged(nameof(DebugStartCommand));
            OnPropertyChanged(nameof(DebugStartWithoutDebuggingCommand));
            OnPropertyChanged(nameof(DebugStopCommand));
            OnPropertyChanged(nameof(BuildCommand));
        }
        private void AddScene(string sceneName)
        {
            Debug.Assert(!string.IsNullOrEmpty(sceneName.Trim()));
            _scenes.Add(new Scene(this, sceneName));
        }

        private void RemoveScene(Scene scene)
        {
            Debug.Assert(_scenes.Contains(scene));
            _scenes.Remove(scene);
        }
        public static Project Load(string file)
        {
            Debug.Assert(File.Exists(file));
            return Serializer.FromFile<Project>(file);
        }
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
            catch (DirectoryNotFoundException ex)
            {
                SystemOperations.CreateNewFolderWithName($@"{Path}x64\{configName}");
                bw = new BinaryWriter(File.Open(bin, FileMode.Create, FileAccess.Write));
            }
            using (bw)
            {
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
        private static void Copy(string path)
        {
            Debug.Assert(path != null);
            Debug.Assert(!string.IsNullOrEmpty(path));
            Debug.Assert(path.Length > 0);
            try
            {
                Debug.Assert(Directory.Exists(path));

            }
            catch (DirectoryNotFoundException ex)
            {
                Debug.Assert(File.Exists(path));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Logger.Log(MessageType.Warning, $"Failed to copy {path}");
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
                ActiveScene.GameEntities.Where(x => x.GetComponent<Script>() != null).ToList().ForEach(x => x.IsActive = true);
                Logger.Log(MessageType.Info, "Game code Dll loaded successfully");
            }
            else
            {
                Logger.Log(MessageType.Warning, "Failed to load game code DLL file. Try to build the project first");
            }
        }

        private void UnloadGameCodeDLL()
        {
            ActiveScene.GameEntities.Where(x => x.GetComponent<Script>() != null).ToList().ForEach(x => x.IsActive = false);
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
            ActiveScene = _scenes.FirstOrDefault(x => x.IsActive);
            Debug.Assert(ActiveScene != null);
            await BuildGameCodeDLL(false);
            SetCommands();
        }
        public Project(string name, string path)
        {
            Name = name;
            Path = path;
            Debug.Assert(File.Exists((Path + Name + Extension).ToLower()));
            OnDeserialized(new StreamingContext());
        }

    }
}
