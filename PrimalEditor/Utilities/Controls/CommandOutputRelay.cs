
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PrimalEditor.Utilities.Controls
{
    class CommandOutputRelay : Control, IObserver
    {
        private static string? _currentWorkingDirectory;
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
        public void RunCommand(CancellationToken ct) => CommandLineHost.Instance.RunCommand(Command, ct);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);
        public void Update(ISubject subject)
        {
            if (subject is not CommandLineHost commandLineHost) return;
            if (commandLineHost.CommandLineMessageType == CommandLineMessageType.Command)
            {
                _messages.Add(new CommandLineMessage { Text = commandLineHost.GetLatestCommand(), HorizontalAlignment = HorizontalAlignment.Right, Background = (Brush)Application.Current.FindResource("Editor.BlueBrush") });
                return;
            }
            var latestOutput = commandLineHost.GetLatestOutput();
            if (latestOutput == null) return;
            _messages.Add(new CommandLineMessage { Text = latestOutput, HorizontalAlignment = HorizontalAlignment.Left, Background = (Brush)Application.Current.FindResource("Editor.YellowBrush") });
            if (!latestOutput.StartsWith("[") || !latestOutput.Contains(']')) return;
            var success = double.TryParse(latestOutput.Split(']').LastOrDefault()?.Trim().Split('%').FirstOrDefault(), out double progress);
            if (success) Progress = progress;
            Status = latestOutput.Split('%').LastOrDefault()?.Trim();
            CurrentPath = commandLineHost.GetLatestCommand()?.Contains("uninstall") == true ? null : commandLineHost.GetLatestCommand()?.Split(new string[] { "--install" }, StringSplitOptions.None)[1].Trim().Trim('"');
            //process.OutputDataReceived += async (sender, e) =>
            //{
            //    if (e.Data == null) return;
            //    await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            //    {
            //        // Parse the output of the sdkmanager command
            //    }));
            //};
        }
        public CommandOutputRelay() => CommandLineHost.Instance.Attach(this);
        ~CommandOutputRelay() => CommandLineHost.Instance.Detach(this);
    }
}
