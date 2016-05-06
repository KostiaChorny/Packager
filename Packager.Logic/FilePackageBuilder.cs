using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packager.Logic
{
    /// <summary>
    /// This class allows prepare files to packing and transform them to package
    /// </summary>
    public class FilePackageBuilder
    {
        List<string> filesPaths = new List<string>();

        /// <summary>
        /// Add file in list for packing
        /// </summary>
        /// <param name="path">Path to file</param>
        public void AddFile(string path)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentException("Path is null or empty", "path");
            if (!File.Exists(path))
                throw new FileNotFoundException("File doesn't exists", path);

            filesPaths.Add(path);
        }

        /// <summary>
        /// Buids package from listed files
        /// </summary>
        /// <param name="destinationPath">Path to package file</param>
        /// <returns>Instance of package</returns>
        public FilePackage Build(string destinationPath)
        {
            if (String.IsNullOrEmpty(destinationPath))
                throw new ArgumentException("Destination Path is null or empty", "path");

            using (FileStream destination = new FileStream(destinationPath, FileMode.OpenOrCreate))
            {
                FilePackageWriter writer = new FilePackageWriter(destination);
                writer.WriteFilesCount(filesPaths.Count);
                WriteAllFiles(writer);
            }

            FilePackage package = new FilePackage(destinationPath);
            return package;
        }

        private void WriteAllFiles(FilePackageWriter writer)
        {
            foreach (string path in filesPaths)
            {
                using (FileStream file = new FileStream(path, FileMode.Open))
                {
                    writer.WriteFile(Path.GetFileName(path), file);
                }
            }
        }
    }
}
