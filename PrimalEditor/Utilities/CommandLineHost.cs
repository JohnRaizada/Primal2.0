using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace PrimalEditor.Utilities
{
    enum CommandLineMessageType
    {
        Command,
        Output
    }
    interface ICommandLineHostSubject : ISubject
    {
        string? GetLatestCommand();
        ObservableCollection<string>? GetAllCommands();
        string? GetLatestOutput();
        ObservableCollection<string>? GetAllOutputs();
        string? GetCurrentWorkingDirectory();
        void RunCommand(string command, CancellationToken ct);
        string? CurrentWorkingDirectory { get; set; }
        ObservableCollection<string> Commands { get; }
        ObservableCollection<string> Outputs { get; }
        CommandLineMessageType CommandLineMessageType { get; }
    }
    class CommandLineHost : Subject, ICommandLineHostSubject
    {
        private static CommandLineHost? _instance;
        private static readonly object _lock = new();
        internal static CommandLineHost Instance
        {
            get
            {
                if (_instance == null) lock (_lock) _instance ??= new CommandLineHost();
                return _instance;
            }
        }
        public string? CurrentWorkingDirectory { get; set; } = MainWindow.PrimalPath;
        public ObservableCollection<string> Commands { get; } = new();
        public ObservableCollection<string> Outputs { get; } = new();
        public CommandLineMessageType CommandLineMessageType { get; private set; }
        public ObservableCollection<string>? GetAllCommands() => Commands;
        public ObservableCollection<string>? GetAllOutputs() => Outputs;
        public string? GetLatestCommand() => Commands.LastOrDefault();
        public string? GetLatestOutput() => Outputs.LastOrDefault();
        public string? GetCurrentWorkingDirectory() => CurrentWorkingDirectory;
        public async void RunCommand(string command, CancellationToken ct = default)
        {
            Commands.Add(command);
            CommandLineMessageType = CommandLineMessageType.Command;
            Notify();
            var cdRegex = new Regex(@"\s*cd\s+(.+)$", RegexOptions.IgnoreCase);
            var cdMatch = cdRegex.Match(command);
            if (cdMatch.Success)
            {
                var stringOfInterest = cdMatch.Groups[1].Value;
                CurrentWorkingDirectory ??= Path.GetPathRoot(Environment.SystemDirectory);
                if (CurrentWorkingDirectory == null) return;
                if (stringOfInterest.StartsWith("..")) CurrentWorkingDirectory = Directory.GetParent(CurrentWorkingDirectory)?.FullName;
                else if (Path.IsPathRooted(stringOfInterest)) CurrentWorkingDirectory = stringOfInterest;
                else CurrentWorkingDirectory = Path.Combine(CurrentWorkingDirectory, stringOfInterest);
            }
            Process process = new();
            process.StartInfo.WorkingDirectory = CurrentWorkingDirectory;
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = "/C " + command;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.OutputDataReceived += async (sender, e) =>
            {
                if (e.Data == null) return;
                await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Outputs.Add(e.Data);
                    CommandLineMessageType = CommandLineMessageType.Output;
                    Notify();
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
                process.Kill();
            }
            catch (OperationCanceledException)
            {
                GenerateConsoleCtrlEvent(0, (uint)process.Id);
                process.Kill();
            }
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);
    }
}
