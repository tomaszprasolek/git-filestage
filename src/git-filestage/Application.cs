using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace git_filestage
{
    internal sealed class Application
    {
        private string _repositoryPath;
        private bool _done;
        private ConsoleCommand[] _commands;
        private int _seletedLine = 1;
        private Dictionary<int, StatusEntry> _gitEntries = new Dictionary<int, StatusEntry>(8);

        public Application(string repositoryPath)
        {
            _repositoryPath = repositoryPath;
        }

        public void Run()
        {
            _commands = new[]
            {
                new ConsoleCommand(Exit, ConsoleKey.Escape),
                new ConsoleCommand(Exit, ConsoleKey.Q),
                new ConsoleCommand(SelectUp, ConsoleKey.UpArrow),
                new ConsoleCommand(SelectDown, ConsoleKey.DownArrow),
                new ConsoleCommand(DoTheAction, ConsoleKey.Enter),
                new ConsoleCommand(CheckoutFile, ConsoleKey.R)
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
            Console.SetCursorPosition(0, 0);
            ClearCurrentConsoleLine();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("");

            sb.Append("Use arrow keys to select file. Press ENTER to do the action.");
            sb.AppendLine("1. When file is in working directory, will be added to staging area.");
            sb.AppendLine("2. When file is in staging area, will be unstaged.");
            sb.AppendLine("3. When file is untracked, will start tracked and added to staging area.");
            sb.AppendLine(WriteEmptyLine());
            sb.AppendLine("Press R to checkout selected file (undo changes made in file).");
            sb.AppendLine(WriteEmptyLine());
            sb.AppendLine("Legend: S - staging area, W - working directory, N - new file, D - deleted");
            sb.AppendLine("----------");

            Console.Write(sb);

            _gitEntries.Clear();

            int idx = 1;
            using (var repo = new Repository(_repositoryPath))
            {
                foreach (StatusEntry item in repo.RetrieveStatus(new StatusOptions()))
                {
                    if (item.State == FileStatus.Ignored) continue;
                    _gitEntries.Add(idx, item);
                    WriteFile(item, idx);
                    idx++;
                }
            }
            Console.ResetColor();

            if (_gitEntries == null || _gitEntries.Count == 0)
            {
                Console.WriteLine("");
                Console.WriteLine("----------------------------");
                Console.WriteLine("No changes in the repository");
                Console.WriteLine("----------------------------");
            }
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
            if (_seletedLine == _gitEntries.Count) return;
            _seletedLine++;
            InitializeScreen();
        }

        private void DoTheAction()
        {
            if (_gitEntries == null || _gitEntries.Count == 0)
                return;

            StatusEntry entry = _gitEntries[_seletedLine];
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            switch (entry.State)
            {
                case FileStatus.NewInIndex:
                case FileStatus.ModifiedInIndex:
                case FileStatus.DeletedFromIndex:
                case FileStatus.RenamedInIndex:
                case FileStatus.TypeChangeInIndex:
                    using (var repo = new Repository(_repositoryPath))
                    {
                        Commands.Unstage(repo, entry.FilePath);
                    }
                    break;
                case FileStatus.NewInWorkdir:
                case FileStatus.ModifiedInWorkdir:
                case FileStatus.TypeChangeInWorkdir:
                case FileStatus.RenamedInWorkdir:
                    using (var repo = new Repository(_repositoryPath))
                    {
                        repo.Index.Add(entry.FilePath);
                        repo.Index.Write();
                    }
                    break;
                case FileStatus.DeletedFromWorkdir:
                    using (var repo = new Repository(_repositoryPath))
                    {
                        repo.Index.Remove(entry.FilePath);
                        repo.Index.Write();
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

            InitializeScreen();
        }

        private void CheckoutFile()
        {
            if (_gitEntries == null || _gitEntries.Count == 0)
                return;

            StatusEntry entry = _gitEntries[_seletedLine];
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            Console.Clear();

            WriteEmptyLine();
            Console.ForegroundColor = ConsoleColor.DarkRed;
            WriteFile(entry, _seletedLine);

            Console.ResetColor();
            WriteEmptyLine();

            Console.WriteLine($"Do you want to undo changes in selected file [y/n] ?");
            Console.WriteLine("Be careful: there is no undo for that operation.");

            ConsoleKey key;
            do
            {
                key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.Y)
                {
                    using (var repo = new Repository(_repositoryPath))
                    {
                        repo.CheckoutPaths(repo.Head.Tip.Sha, new[] { entry.FilePath }, new CheckoutOptions { CheckoutModifiers = CheckoutModifiers.Force });
                    }
                    break;
                }
            } while (key != ConsoleKey.N && key != ConsoleKey.Escape);

            InitializeScreen();
        }

        private void WriteFile(StatusEntry entry, int idx)
        {
            string startCharacters = "    ";
            if (idx == _seletedLine)
                startCharacters = ">>> ";
            Console.WriteLine($"{startCharacters}{GetFileStatusFriendlyDescription(entry.State)} | {entry.FilePath}");
        }

        private string WriteEmptyLine()
        {
            return "";
        }

        private string GetFileStatusFriendlyDescription(FileStatus status)
        {
            switch (status)
            {
                case FileStatus.NewInIndex:
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    return "SN";
                case FileStatus.DeletedFromIndex:
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    return "SD";
                case FileStatus.ModifiedInIndex:
                case FileStatus.RenamedInIndex:
                case FileStatus.TypeChangeInIndex:
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    return "S ";
                case FileStatus.NewInWorkdir:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    return "WN";
                case FileStatus.DeletedFromWorkdir:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    return "WD";
                case FileStatus.ModifiedInWorkdir:
                case FileStatus.TypeChangeInWorkdir:
                case FileStatus.RenamedInWorkdir:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    return "W ";
            }
            return status.ToString();
        }

        private static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }
    }
}