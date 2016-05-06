using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packager.Logic
{
    /// <summary>
    /// This class represents package with different files
    /// </summary>
    public class FilePackage : IDisposable
    {
        FileStream target;
        List<PackagedFileInfo> files;

        /// <summary>
        /// Files in package
        /// </summary>
        public List<PackagedFileInfo> Files { get { return files; } }

        /// <summary>
        /// Creates new instance of FilePackage class linked to file on disk
        /// </summary>
        /// <param name="targetPath">Path to *.pkg file</param>
        public FilePackage(string targetPath)
        {
            target = new FileStream(targetPath, FileMode.Open);
            Init();
        }

        /// <summary>
        /// Initiates state of File Package using FilePackageReader for reading files information from target stream.
        /// </summary>
        private void Init()
        {
            FilePackageReader reader = new FilePackageReader(target);
            files = reader.ReadFilesDescriptions();
        }

        /// <summary>
        /// Allows to get file from package and save it in FileStream
        /// </summary>
        /// <param name="filename">Name of file in the package</param>
        /// <param name="destFile">Destination stream for unpackaged file</param>
        public void GetFile(string filename, FileStream destFile)
        {
            if (String.IsNullOrEmpty(filename))
                throw new ArgumentException("Filename is null or empty", "filename");
            PackagedFileInfo file = FindFileInPackage(filename);
            if (file == null)
                throw new FileNotFoundException("File not found", filename);
            CopyDataToFileStream(file, destFile);
        }

        private void CopyDataToFileStream(PackagedFileInfo file, FileStream destFile)
        {
            target.Position = file.Offset;
            byte[] fileBytes = new byte[file.Length];
            target.Read(fileBytes, 0, fileBytes.Length);
            destFile.Write(fileBytes, 0, fileBytes.Length);
        }

        private PackagedFileInfo FindFileInPackage(string filename)
        {
            return files.SingleOrDefault(f => f.FileName == filename);
        }

        /// <summary>
        /// Allows to get file from package and save it in file on disk.
        /// </summary>
        /// <param name="filename">Name of file in the package</param>
        /// <param name="destFileName">Name of destination file</param>
        public void GetFile(string filename, string destFileName)
        {
            using (FileStream file = new FileStream(destFileName, FileMode.OpenOrCreate))
            {
                GetFile(filename, file);
            }
        }

        /// <summary>
        /// Add file to end of the package
        /// </summary>
        /// <param name="path">Path to file which will be added to the package</param>
        public void AddFile(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("File not found", path);

            FilePackageWriter writer = new FilePackageWriter(target);
            using (FileStream file = new FileStream(path, FileMode.Open))
            {
                writer.WriteFilesCount(files.Count + 1);
                writer.AppendFile(Path.GetFileName(path), file);
            }
            // Update state after changing schema
            Init();
        }

        public void Dispose()
        {
            target.Dispose();
        }


    }
}
