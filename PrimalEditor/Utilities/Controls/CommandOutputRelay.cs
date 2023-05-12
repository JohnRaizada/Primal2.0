
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PrimalEditor.Utilities.Controls
{
    class CommandOutputRelay : Control
    {
        private static string? _currentWorkingDirectory;
        private bool _isCancelled;
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(nameof(Command), typeof(string), typeof(CommandOutputRelay), new PropertyMetadata(null));
        public static readonly DependencyProperty OutputProperty = DependencyProperty.Register(nameof(Output), typeof(string), typeof(CommandOutputRelay), new PropertyMetadata(null));
        public static readonly DependencyProperty CurrentPathProperty = DependencyProperty.Register(nameof(CurrentPath), typeof(string), typeof(CommandOutputRelay), new PropertyMetadata(null));
        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(nameof(Progress), typeof(double), typeof(CommandOutputRelay), new PropertyMetadata(double.NaN, OnProgressChanged));
        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(nameof(Status), typeof(string), typeof(CommandOutputRelay), new PropertyMetadata("Preparing", OnStatusChanged));
        public event EventHandler<EventArgs>? ProgressChanged;
        public event EventHandler<EventArgs>? StatusChanged;
        public string Command
        {
            get => (string)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }
        public string Output
        {
            get => (string)GetValue(OutputProperty);
            set => SetValue(OutputProperty, value);
        }
        public double Progress
        {
            get => (double)GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }
        public string? Status
        {
            get => (string)GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }
        public string? CurrentPath
        {
            get => (string)GetValue(CurrentPathProperty);
            set => SetValue(CurrentPathProperty, value);
        }
        private static void OnProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CommandOutputRelay)d;
            control.ProgressChanged?.Invoke(control, EventArgs.Empty);
        }

        private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CommandOutputRelay)d;
            control.StatusChanged?.Invoke(control, EventArgs.Empty);
        }
        private static readonly ObservableCollection<CommandLineMessage> _messages = new();
        public static ReadOnlyObservableCollection<CommandLineMessage> Messages { get; } = new ReadOnlyObservableCollection<CommandLineMessage>(_messages);
        public async Task RunCommandAsync(CancellationToken ct)
        {
            _currentWorkingDirectory ??= MainWindow.PrimalPath;
            _messages.Add(new CommandLineMessage { Text = Command, HorizontalAlignment = HorizontalAlignment.Right, Background = (Brush)Application.Current.FindResource("Editor.BlueBrush") });
            var process = new Process();
            if (Command.ToLower().StartsWith("cd "))
            {
                var stringOfInterest = Command[3..];
                _currentWorkingDirectory = stringOfInterest.StartsWith("..") ? Directory.GetParent(_currentWorkingDirectory)?.FullName : Path.Combine(_currentWorkingDirectory, stringOfInterest);
            }
            process.StartInfo.WorkingDirectory = _currentWorkingDirectory;
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = "/C " + Command;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            GenerateConsoleCtrlEvent(0, (uint)process.Id);
            process.OutputDataReceived += async (sender, e) =>
            {
                if (e.Data == null) return;
                await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _messages.Add(new CommandLineMessage { Text = e.Data, HorizontalAlignment = HorizontalAlignment.Left, Background = (Brush)Application.Current.FindResource("Editor.YellowBrush") });
                    // Parse the output of the sdkmanager command
                    if (!e.Data.StartsWith("[") || !e.Data.Contains(']')) return;
                    var progress = double.NaN;
                    var success = double.TryParse(e.Data.Split(']').LastOrDefault()?.Trim().Split('%').FirstOrDefault(), out progress);
                    if (success) Progress = progress;
                    Status = e.Data.Split('%').LastOrDefault()?.Trim();
                    CurrentPath = Command.Contains("uninstall") ? null : Command.Split(new string[] { "--install" }, StringSplitOptions.None)[1].Trim().Trim('"');
                }));
            };
            process.BeginOutputReadLine();
            try
            {
                await process.WaitForExitAsync(ct);
            }
            catch (TaskCanceledException)
            {
                GenerateConsoleCtrlEvent(0, (uint)process.Id);
                process.Kill(true);
            }
            catch (OperationCanceledException)
            {
                GenerateConsoleCtrlEvent(0, (uint)process.Id);
                process.Kill(true);
            }
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);
    }
}
