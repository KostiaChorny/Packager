using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packager.Logic
{
    class FilePackageWriter : IDisposable
    {
        private const int fileNameBlockSize = 128;

        private int filesCount;
        private long filesDescriptionBlockOffset;
        private long dataBlockOffset;
        private long currentFileDescriptionPosition;
        private long currentDataBlockPosition;

        FileStream destination;

        public FilePackageWriter(FileStream destination)
        {
            if (destination == null)
                throw new NullReferenceException("Destination is null");

            this.destination = destination;
        }

        public void WriteFilesCount(int filesCount)
        {
            if (filesCount < 0)
                throw new ArgumentOutOfRangeException("filesCount", filesCount, "Files count must be greater than or equal to zero");

            byte[] result = ToBytes(filesCount);

            destination.Position = 0;
            destination.Write(result, 0, result.Length);

            this.filesCount = filesCount;
            filesDescriptionBlockOffset = 4;
            dataBlockOffset = filesCount * (fileNameBlockSize + 8) + 4;
            currentFileDescriptionPosition = filesDescriptionBlockOffset;
            currentDataBlockPosition = dataBlockOffset;
        }
         

        public void WriteFile(string filename, FileStream file)
        {
            if (string.IsNullOrEmpty(filename))
                throw new NullReferenceException("File Name is null or empty");

            if (file == null)
                throw new NullReferenceException("Destination is null");

            WriteNextFileDescription(filename);
            WriteFileData(file);
        }

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

        private void MoveDataBlock()
        {
            destination.Position = dataBlockOffset - (fileNameBlockSize + 8);
            byte[] buffer = new byte[destination.Length - destination.Position];
            destination.Read(buffer, 0, buffer.Length);
            destination.Position = dataBlockOffset;
            destination.Write(buffer, 0, buffer.Length);
        }

        private void MoveReferences()
        {
            destination.Position = filesDescriptionBlockOffset + fileNameBlockSize;
            byte[] buffer = new byte[8];
            for (int i = 0; i < filesCount; i++)
            {
                destination.Read(buffer, 0, buffer.Length);
                long value = LongFromBytes(buffer);
                value += (fileNameBlockSize + 8);
                destination.Position -= 8;
                buffer = ToBytes(value);
                destination.Write(buffer, 0, buffer.Length);
                destination.Position += fileNameBlockSize; 
            }
        }

        private void WriteNextFileDescription(string filename)
        {
            destination.Position = currentFileDescriptionPosition;

            WriteFileDescription(filename);

            currentFileDescriptionPosition = destination.Position;
        }

        private void AppendFileDescription(string filename)
        {
            destination.Position = dataBlockOffset - (fileNameBlockSize + 8);
            currentDataBlockPosition = destination.Length;

            WriteFileDescription(filename);

            currentFileDescriptionPosition = destination.Position;
        }

        private void WriteFileDescription(string filename)
        {
            byte[] filenameBytes = new byte[fileNameBlockSize];
            Encoding.Unicode.GetBytes(filename, 0, filename.Length, filenameBytes, 0);
            destination.Write(filenameBytes, 0, filenameBytes.Length);

            byte[] lengthBytes = ToBytes(currentDataBlockPosition);
            destination.Write(lengthBytes, 0, lengthBytes.Length);
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
