using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packager.Logic
{
    public class PackagedFileInfo
    {
        public string FileName { get; set; }
        public long Offset { get; set; }
        public long Length { get; set; }
    }
}
