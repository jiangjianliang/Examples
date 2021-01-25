

namespace A
{ 
    using System.Collections.Generic;
    public class CachedRAMDirectory
    {
        public CachedRAMDirectory()
        { 
        }

        public IDictionary<string, CachedRAMFile> files { get; set; }
    }
}