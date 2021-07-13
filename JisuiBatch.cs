using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JisuiBatch
{
    public class JisuiBatch
    {
        private AppSettings AppSettings { get; }

        public JisuiBatch(AppSettings appSettings)
        {
            AppSettings = appSettings;
        }

        public void Execute()
        {
            CheckDirectoryExists();

            var bookTitles = GetBookTitles();

            foreach (var bookTitle in bookTitles)
            {
                var p = new BookProcessor(AppSettings, bookTitle);
                p.Execute();
            }
        }

        private void CheckDirectoryExists()
        {
            if (!Directory.Exists(AppSettings.WorkingDirectory.Root))
            {
                throw new DirectoryNotFoundException(AppSettings.WorkingDirectory.Root);
            }
        }

        private IEnumerable<string> GetBookTitles()
        {
            return Directory.GetDirectories(AppSettings.WorkingDirectory.Root)
                .Select(x => new DirectoryInfo(x).Name)
                .Where(x => !x.StartsWith("_"));
        }
    }
}
