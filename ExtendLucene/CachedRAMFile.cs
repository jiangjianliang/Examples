
namespace A
{ 
    using System.Collections.Generic;
    public class CachedRAMFile
    {

        public CachedRAMFile()
        { }

        public IList<IList<byte>> buffers { get; set; }

        public long Length { get; set; }

    }
}