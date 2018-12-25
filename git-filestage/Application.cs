using LibGit2Sharp;
using System;
using System.Linq;

namespace git_filestage
{
    internal sealed class Application
    {
        private string _repositoryPath;
        private string _pathToGit;
        private bool _done;
        private ConsoleCommand[] _commands;
        private int seletedLine = 1;

        public Application(string repositoryPath, string pathToGit)
        {
            _repositoryPath = repositoryPath;
            _pathToGit = pathToGit;
        }

        public void Run()
        {
            _commands = new[]
            {
                new ConsoleCommand(Exit, ConsoleKey.Escape),
                new ConsoleCommand(Exit, ConsoleKey.Q)
            };

            Console.CursorVisible = false;
            Console.Clear();

            InitializeScreen();

            while (_done == false)
            {
                var width = Console.WindowWidth;
                var height = Console.WindowHeight;

                var key = Console.ReadKey(true);
                var command = _commands.FirstOrDefault(c => c.MatchesKey(key));
                command?.Execute();

                if (width != Console.WindowWidth || height != Console.WindowHeight)
                    InitializeScreen();
            }

            Console.Clear();
            Console.CursorVisible = true;
        }

        private void Exit()
        {
            _done = true;
        }

        private void InitializeScreen()
        {
            Console.WriteLine("Use arrow keys to select file. Press ENTER to do the action.");
            Console.WriteLine("1. When file is in working directory, will be added to staging area.");
            Console.WriteLine("2. When file is in staging area, will be unstaged.");
            Console.WriteLine("3. When file is not ... TODO");
            Console.WriteLine("----------");

            using (var repo = new Repository(_repositoryPath))
            {
                foreach (StatusEntry item in repo.RetrieveStatus(new StatusOptions() { Show = StatusShowOption.IndexOnly }))
                {
                    if (item.State == FileStatus.Ignored) continue;
                    WriteFile(item);
                }

                foreach (StatusEntry item in repo.RetrieveStatus(new StatusOptions() { Show = StatusShowOption.WorkDirOnly }))
                {
                    if (item.State == FileStatus.Ignored) continue;
                    WriteFile(item);
                }
            }
            Console.ResetColor();
        }

        private void WriteFile(StatusEntry entry)
        {
            Console.WriteLine($"{GetFileStatusFriendlyDescription(entry.State)} | {entry.FilePath}");
        }

        private string GetFileStatusFriendlyDescription(FileStatus status)
        {
            switch (status)
            {
                case FileStatus.NewInIndex:
                case FileStatus.ModifiedInIndex:
                case FileStatus.DeletedFromIndex:
                case FileStatus.RenamedInIndex:
                case FileStatus.TypeChangeInIndex:
                    //return "Staging area";
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    return "S";
                case FileStatus.NewInWorkdir:
                case FileStatus.ModifiedInWorkdir:
                case FileStatus.DeletedFromWorkdir:
                case FileStatus.TypeChangeInWorkdir:
                case FileStatus.RenamedInWorkdir:
                    //return "Working directory";
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    return "W";
            }
            return status.ToString();
        }
    }
}