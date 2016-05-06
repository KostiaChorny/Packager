using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packager.Logic
{
    static class FilePackageConstants
    {
        //Number of bytes for filename
        public const int FileNameBlockSize = 128;

        public const int FilesCountBlockSize = 4;

        public const int FileOffsetBlockSize = 8;

        public const int StartPosition = 0;

        public const int FileDescriptionBlockSize = FileNameBlockSize + FileOffsetBlockSize;
    }
}
