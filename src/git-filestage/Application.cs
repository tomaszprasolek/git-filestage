﻿using LibGit2Sharp;
using System;
using System.Collections.Generic;
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
        private Dictionary<int, StatusEntry> _gitEntries = new Dictionary<int, StatusEntry>(8);

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
                new ConsoleCommand(DoTheAction, ConsoleKey.Enter),
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
            Console.WriteLine("3. When file is untracked, will start tracked and added to staging area.");
            Console.WriteLine("");
            Console.WriteLine("Legend: S - staging area, W - working directory, N - new file.");
            Console.WriteLine("----------");

            _gitEntries.Clear();

            int idx = 1;
            using (var repo = new Repository(_repositoryPath))
            {
                foreach (StatusEntry item in repo.RetrieveStatus(new StatusOptions() { Show = StatusShowOption.IndexOnly }))
                {
                    if (item.State == FileStatus.Ignored) continue;
                    _gitEntries.Add(idx, item);
                    WriteFile(item, idx);
                    idx++;
                }

                foreach (StatusEntry item in repo.RetrieveStatus(new StatusOptions() { Show = StatusShowOption.WorkDirOnly }))
                {
                    if (item.State == FileStatus.Ignored) continue;
                    _gitEntries.Add(idx, item);
                    WriteFile(item, idx);
                    idx++;
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
                case FileStatus.DeletedFromWorkdir:
                case FileStatus.TypeChangeInWorkdir:
                case FileStatus.RenamedInWorkdir:
                    using (var repo = new Repository(_repositoryPath))
                    {
                        repo.Index.Add(entry.FilePath);
                        repo.Index.Write();
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

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
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    return "SN";
                case FileStatus.ModifiedInIndex:
                case FileStatus.DeletedFromIndex:
                case FileStatus.RenamedInIndex:
                case FileStatus.TypeChangeInIndex:
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    return "S ";
                case FileStatus.NewInWorkdir:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    return "WN";
                case FileStatus.ModifiedInWorkdir:
                case FileStatus.DeletedFromWorkdir:
                case FileStatus.TypeChangeInWorkdir:
                case FileStatus.RenamedInWorkdir:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    return "W ";
            }
            return status.ToString();
        }
    }
}