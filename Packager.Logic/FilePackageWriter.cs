using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packager.Logic
{
    /// <summary>
    /// This class using for writing to the package file
    /// </summary>
    class FilePackageWriter : IDisposable
    {
        private int filesCount;
        private long filesDescriptionBlockOffset;
        private long dataBlockOffset;
        private long currentFileDescriptionPosition;
        private long currentDataBlockPosition;

        FileStream destination;

        /// <summary>
        /// Create writer instance for specified File Stream
        /// </summary>
        /// <param name="destination">File Stream for writing data</param>
        public FilePackageWriter(FileStream destination)
        {
            if (destination == null)
                throw new NullReferenceException("Destination is null");

            this.destination = destination;
        }

        /// <summary>
        /// Writes in first 4 bytes in file count of files in the package 
        /// </summary>
        /// <param name="filesCount">Number of files</param>
        public void WriteFilesCount(int filesCount)
        {
            if (filesCount < 0)
                throw new ArgumentOutOfRangeException("filesCount", filesCount, "Files count must be greater than or equal to zero");

            byte[] result = ToBytes(filesCount);

            destination.Position = 0;
            destination.Write(result, 0, result.Length);

            this.filesCount = filesCount;
            InitPositions(filesCount);
        }

        private void InitPositions(int filesCount)
        {
            filesDescriptionBlockOffset = FilePackageConstants.FilesCountBlockSize;
            currentFileDescriptionPosition = FilePackageConstants.FilesCountBlockSize;
            SetDataBlockOffset(filesCount);
            currentDataBlockPosition = dataBlockOffset;
        }

        private void SetDataBlockOffset(int filesCount)
        {
            dataBlockOffset = filesCount * FilePackageConstants.FileDescriptionBlockSize + FilePackageConstants.FilesCountBlockSize;
        }

        /// <summary>
        /// Writes to the stream information and body of the file 
        /// </summary>
        /// <param name="filename">A name that will be stored in the package</param>
        /// <param name="file">A file that will be stored in the package</param>
        public void WriteFile(string filename, FileStream file)
        {
            if (string.IsNullOrEmpty(filename))
                throw new NullReferenceException("File Name is null or empty");

            if (file == null)
                throw new NullReferenceException("Destination is null");

            WriteNextFileDescription(filename);
            WriteFileData(file);
        }

        /// <summary>
        /// Appends file to existing package
        /// </summary>
        /// <param name="filename">A name that will be stored in the package</param>
        /// <param name="file">A file that will be stored in the package</param>
        public void AppendFile(string filename, FileStream file)
        {
            if (string.IsNullOrEmpty(filename))
                throw new NullReferenceException("File Name is null or empty");

            if (file == null)
                throw new NullReferenceException("Destination is null");

            MoveDataBlock();
            MoveReferences();
            AppendFileDescription(filename);
            currentDataBlockPosition = destination.Length;
            WriteFileData(file);
        }

        /// <summary>
        /// It moves a block of data to make space for service information
        /// </summary>
        private void MoveDataBlock()
        {
            destination.Position = dataBlockOffset - FilePackageConstants.FileDescriptionBlockSize;
            byte[] buffer = new byte[destination.Length - destination.Position];
            destination.Read(buffer, 0, buffer.Length);
            destination.Position = dataBlockOffset;
            destination.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// It changes offset values for existing files in the package
        /// </summary>
        private void MoveReferences()
        {
            destination.Position = filesDescriptionBlockOffset;
            for (int i = 0; i < filesCount; i++)
            {
                MoveReference();
            }
        }

        private void MoveReference()
        {
            destination.Position += FilePackageConstants.FileNameBlockSize;
            long value = ReadOffsetValue();
            value += FilePackageConstants.FileDescriptionBlockSize;
            destination.Position -= FilePackageConstants.FileOffsetBlockSize;
            WriteOffsetValue(value);            
        }

        private void WriteOffsetValue(long value)
        {
            byte[] buffer = ToBytes(value);
            destination.Write(buffer, 0, buffer.Length);
        }

        private long ReadOffsetValue()
        {
            byte[] buffer = new byte[FilePackageConstants.FileOffsetBlockSize];
            destination.Read(buffer, 0, buffer.Length);
            long value = LongFromBytes(buffer);
            return value;
        }

        /// <summary>
        /// It is used for sequential recording information about files
        /// </summary>
        /// <param name="filename">The file name that will be added</param>
        private void WriteNextFileDescription(string filename)
        {
            destination.Position = currentFileDescriptionPosition;

            WriteFileDescription(filename);

            currentFileDescriptionPosition = destination.Position;
        }

        /// <summary>
        /// It is used for appending information about files
        /// </summary>
        /// <param name="filename">The file name that will be added</param>
        private void AppendFileDescription(string filename)
        {
            destination.Position = dataBlockOffset - FilePackageConstants.FileDescriptionBlockSize;
            currentDataBlockPosition = destination.Length;

            WriteFileDescription(filename);

            currentFileDescriptionPosition = destination.Position;
        }

        private void WriteFileDescription(string filename)
        {
            WriteFileName(filename);
            WriteOffsetValue(currentDataBlockPosition);
        }

        private void WriteFileName(string filename)
        {
            byte[] filenameBytes = new byte[FilePackageConstants.FileNameBlockSize];
            Encoding.Unicode.GetBytes(filename, 0, filename.Length, filenameBytes, 0);
            destination.Write(filenameBytes, 0, filenameBytes.Length);
        }

        private void WriteFileData(FileStream file)
        {
            destination.Position = currentDataBlockPosition;

            using (file)
            {
                file.CopyTo(destination);
            }

            currentDataBlockPosition = destination.Position;
        }

        private static byte[] ToBytes(int number)
        {
            byte[] intBytes = BitConverter.GetBytes(number);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(intBytes);
            byte[] result = intBytes;
            return result;
        }

        private static byte[] ToBytes(long number)
        {
            byte[] longBytes = BitConverter.GetBytes(number);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(longBytes);
            byte[] result = longBytes;
            return result;
        }

        private long LongFromBytes(byte[] longBytes)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(longBytes);
            long result = BitConverter.ToInt64(longBytes, 0);
            return result;
        }

        public void Dispose()
        {
            destination.Dispose();
        }
    }
}
