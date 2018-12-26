using LibGit2Sharp;
using System;
using System.IO;

namespace git_filestage
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            if (Win32Console.IsSupported)
                Win32Console.Initialize();

            var repositoryPath = ResolveRepositoryPath();
            if (string.IsNullOrEmpty(repositoryPath))
            {
                Console.WriteLine("fatal: Not a git repository");
                return;
            }

            var application = new Application(repositoryPath);   
            application.Run();
        }

        private static string ResolveRepositoryPath()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var repositoryPath = Repository.Discover(currentDirectory);

            if (string.IsNullOrEmpty(repositoryPath))
                return null;

            if (!Repository.IsValid(repositoryPath))
                return null;

            return repositoryPath;
        }
    }
}
