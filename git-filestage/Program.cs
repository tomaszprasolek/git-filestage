﻿using LibGit2Sharp;
using System;
using System.IO;
using System.Linq;

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

            var pathToGit = ResolveGitPath();
            if (string.IsNullOrEmpty(pathToGit))
            {
                Console.WriteLine("fatal: git is not in your path");
                return;
            }

            var application = new Application(repositoryPath, pathToGit);
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

        private static string ResolveGitPath()
        {
            var path = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrEmpty(path))
                return null;

            var paths = path.Split(Path.PathSeparator);

            // In order to have this work across all operating systems, we
            // need to include other extensions.
            //
            // NOTE: On .NET Core, we should use RuntimeInformation in order
            //       to limit the extensions based on operating system.

            var fileNames = new[] { "git.exe", "git" };
            var searchPaths = paths.SelectMany(p => fileNames.Select(f => Path.Combine(p, f)));

            return searchPaths.FirstOrDefault(File.Exists);
        }
    }
}
