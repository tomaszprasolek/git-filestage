using LibGit2Sharp;
using System;

namespace git_filestage
{
    internal sealed class Application
    {
        private string _repositoryPath;
        private string _pathToGit;

        public Application(string repositoryPath, string pathToGit)
        {
            _repositoryPath = repositoryPath;
            _pathToGit = pathToGit;
        }

        public void Run()
        {
            using (var repo = new Repository(_repositoryPath))
            {
                foreach (StatusEntry item in repo.RetrieveStatus(new StatusOptions() { Show = StatusShowOption.IndexOnly } ))
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

            Console.WriteLine("");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
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