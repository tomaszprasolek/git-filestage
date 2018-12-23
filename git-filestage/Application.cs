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
                foreach (StatusEntry item in repo.RetrieveStatus(new StatusOptions()))
                {
                    if (item.State == FileStatus.Ignored) continue;
                    Console.WriteLine(item.FilePath);
                }
            }

            Console.WriteLine("");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
        }
    }
}
