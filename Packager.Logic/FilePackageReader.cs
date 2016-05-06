using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packager.Logic
{
    class FilePackageReader : IDisposable
    {
        private const int fileNameBlockSize = 128;

        private int filesCount;
        private long filesDescriptionBlockOffset;
        private long currentFileDescriptionPosition;

        FileStream target;

        public FilePackageReader(FileStream target)
        {
            if (target == null)
                throw new NullReferenceException("Target is null");

            this.target = target;
            filesDescriptionBlockOffset = 4;
            currentFileDescriptionPosition = filesDescriptionBlockOffset;
            ReadFilesCount();
        }

        private int ReadFilesCount()
        {
            target.Position = 0;
            byte[] filesCountBytes = new byte[4];
            target.Read(filesCountBytes, 0, 4);

            int result = IntFromBytes(filesCountBytes);

            this.filesCount = result;
            return result;
        }
        private PackagedFileInfo ReadNextFileDescription()
        {
            target.Position = currentFileDescriptionPosition;

            byte[] filenameBytes = new byte[fileNameBlockSize];
            target.Read(filenameBytes, 0, filenameBytes.Length);
            string filename = Encoding.Unicode.GetString(filenameBytes).Trim('\0');

            byte[] lengthBytes = new byte[8];
            target.Read(lengthBytes, 0, lengthBytes.Length);
            long offset = LongFromBytes(lengthBytes);            

            currentFileDescriptionPosition = target.Position;

            PackagedFileInfo result = new PackagedFileInfo
            {
                FileName = filename,
                Offset = offset
            };

            return result;
        }

        public List<PackagedFileInfo> ReadFilesDescriptions()
        {
            List<PackagedFileInfo> files = new List<PackagedFileInfo>();

            for (int i = 0; i < filesCount; i++)
            {
                files.Add(ReadNextFileDescription());
                if (i != 0)
                {
                    files[i - 1].Length = files[i].Offset - files[i - 1].Offset;
                }
            }
            if (files.Count > 0)
            {
                files[filesCount - 1].Length = target.Length - files[filesCount - 1].Offset;
            }
            return files;
        }

        private long LongFromBytes(byte[] longBytes)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(longBytes);
            long result = BitConverter.ToInt64(longBytes, 0);
            return result;
        }

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
