using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packager.Logic
{
    /// <summary>
    /// This class using for reading operations from package file
    /// </summary>
    class FilePackageReader : IDisposable
    {
        private int filesCount;
        private long filesDescriptionBlockOffset;
        private long currentFileDescriptionPosition;

        FileStream target;

        /// <summary>
        /// Creates reader instance 
        /// </summary>
        /// <param name="target">Stream for reading</param>
        public FilePackageReader(FileStream target)
        {
            if (target == null)
                throw new NullReferenceException("Target is null");

            this.target = target;

            filesDescriptionBlockOffset = FilePackageConstants.FilesCountBlockSize;
            currentFileDescriptionPosition = FilePackageConstants.FilesCountBlockSize;
            ReadFilesCount();
        }

        /// <summary>
        /// Reads first 4 bytes in file to find out the number of files in the package
        /// </summary>
        /// <returns>Number of files</returns>
        private int ReadFilesCount()
        {
            target.Position = FilePackageConstants.StartPosition;
            byte[] filesCountBytes = new byte[FilePackageConstants.FilesCountBlockSize];
            target.Read(filesCountBytes, 0, FilePackageConstants.FilesCountBlockSize);

            int result = IntFromBytes(filesCountBytes);

            this.filesCount = result;
            return result;
        }
        /// <summary>
        /// Reads filename and offset for every file in package
        /// </summary>
        /// <returns>Information about packaged file</returns>
        private PackagedFileInfo ReadNextFileDescription()
        {
            target.Position = currentFileDescriptionPosition;
            string filename = ReadFileName();
            long offset = ReadFileOffset();

            currentFileDescriptionPosition = target.Position;

            PackagedFileInfo result = new PackagedFileInfo
            {
                FileName = filename,
                Offset = offset
            };

            return result;
        }

        private long ReadFileOffset()
        {
            byte[] lengthBytes = new byte[FilePackageConstants.FileOffsetBlockSize];
            target.Read(lengthBytes, 0, lengthBytes.Length);
            long offset = LongFromBytes(lengthBytes);
            return offset;
        }

        private string ReadFileName()
        {
            byte[] filenameBytes = new byte[FilePackageConstants.FileNameBlockSize];
            target.Read(filenameBytes, 0, filenameBytes.Length);
            string filename = Encoding.Unicode.GetString(filenameBytes).Trim('\0');
            return filename;
        }

        /// <summary>
        /// Reads info about all files in the package
        /// </summary>
        /// <returns>List infos for every file</returns>
        public List<PackagedFileInfo> ReadFilesDescriptions()
        {
            List<PackagedFileInfo> files = new List<PackagedFileInfo>();

            for (int i = 0; i < filesCount; i++)
            {
                files.Add(ReadNextFileDescription());
                if (i != 0)
                {
                    SetPreviousFileLength(files, i);
                }
            }
            if (files.Count > 0)
            {
                SetLastFileLength(files);
            }
            return files;
        }

        private void SetLastFileLength(List<PackagedFileInfo> files)
        {
            files[filesCount - 1].Length = target.Length - files[filesCount - 1].Offset;
        }

        private static void SetPreviousFileLength(List<PackagedFileInfo> files, int i)
        {
            files[i - 1].Length = files[i].Offset - files[i - 1].Offset;
        }

        /// <summary>
        /// Converts 8 bytes array to long value
        /// </summary>
        /// <param name="longBytes">8 bytes array</param>
        /// <returns>Long value</returns>
        private long LongFromBytes(byte[] longBytes)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(longBytes);
            long result = BitConverter.ToInt64(longBytes, 0);
            return result;
        }

        /// <summary>
        /// Converts 4 bytes array to Int32 value
        /// </summary>
        /// <param name="intBytes">4 bytes array</param>
        /// <returns>Int32 value</returns>
        private int IntFromBytes(byte[] intBytes)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(intBytes);
            int result = BitConverter.ToInt32(intBytes, 0);         
            return result;
        }

        public void Dispose()
        {
            target.Dispose();
        }
    }
}
