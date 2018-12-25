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
        private int _seletedLine = 1;
        /// <summary>
        /// Modified files count.
        /// </summary>
        private int _filesCount = 0;

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
                new ConsoleCommand(Exit, ConsoleKey.Q),
                new ConsoleCommand(SelectUp, ConsoleKey.UpArrow),
                new ConsoleCommand(SelectDown, ConsoleKey.DownArrow),
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

        private void InitializeScreen()
        {
            Console.Clear();
            Console.WriteLine("Use arrow keys to select file. Press ENTER to do the action.");
            Console.WriteLine("1. When file is in working directory, will be added to staging area.");
            Console.WriteLine("2. When file is in staging area, will be unstaged.");
            Console.WriteLine("3. When file is not ... TODO");
            Console.WriteLine("----------");

            _filesCount = 0;
            int idx = 1;
            using (var repo = new Repository(_repositoryPath))
            {
                foreach (StatusEntry item in repo.RetrieveStatus(new StatusOptions() { Show = StatusShowOption.IndexOnly }))
                {
                    if (item.State == FileStatus.Ignored) continue;
                    WriteFile(item, idx);
                    idx++;
                    _filesCount = _filesCount + 1;
                }

                foreach (StatusEntry item in repo.RetrieveStatus(new StatusOptions() { Show = StatusShowOption.WorkDirOnly }))
                {
                    if (item.State == FileStatus.Ignored) continue;
                    WriteFile(item, idx);
                    idx++;
                    _filesCount = _filesCount + 1;
                }
            }
            Console.ResetColor();
        }

        private void Exit()
        {
            _done = true;
        }

        private void SelectUp()
        {
            if (_seletedLine - 1 == 0) return;
            _seletedLine--;
            InitializeScreen();
        }

        private void SelectDown()
        {
            if (_seletedLine == _filesCount) return;
            _seletedLine++;
            InitializeScreen();
        }

        private void WriteFile(StatusEntry entry, int idx)
        {
            string startCharacters = "    ";
            if (idx == _seletedLine)
                startCharacters = ">>> ";
            Console.WriteLine($"{startCharacters}{GetFileStatusFriendlyDescription(entry.State)} | {entry.FilePath}");
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