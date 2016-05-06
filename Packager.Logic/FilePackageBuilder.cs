using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packager.Logic
{
    public class FilePackageBuilder
    {
        List<string> filesPaths = new List<string>();

        public void AddFile(string path)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentException("Path is null or empty", "path");
            if (!File.Exists(path))
                throw new FileNotFoundException("File doesn't exists", path);

            filesPaths.Add(path);
        }

        public FilePackage Build(string destinationPath)
        {
            if (String.IsNullOrEmpty(destinationPath))
                throw new ArgumentException("Destination Path is null or empty", "path");
            if (!File.Exists(destinationPath))
                throw new FileNotFoundException("File doesn't exists", destinationPath);

            using (FileStream destination = new FileStream(destinationPath, FileMode.OpenOrCreate))
            {
                FilePackageWriter writer = new FilePackageWriter(destination);
                writer.WriteFilesCount(filesPaths.Count);
                foreach (string path in filesPaths)
                {
                    using (FileStream file = new FileStream(path, FileMode.Open))
                    {
                        writer.WriteFile(Path.GetFileName(path), file);
                    }
                }
            }

            FilePackage package = new FilePackage(destinationPath);
            return package;
        }
    }
}
