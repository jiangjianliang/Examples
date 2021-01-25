

namespace Lucene.Net.Store
{
    using A;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    public class PortableRAMFile : RAMFile
    {
        internal PortableRAMDirectory directory;

        /// <summary>
        /// File used as buffer, in no <see cref="PortableRAMDirectory"/>
        /// </summary>
        public PortableRAMFile()
        {
        }

        public PortableRAMFile(PortableRAMDirectory directory)
        {
            this.directory = directory;
        }

        public bool Compare(PortableRAMFile targetFile)
        {
            bool areEqual = this.m_sizeInBytes == targetFile.m_sizeInBytes;
            areEqual &= this.NumBuffers == targetFile.NumBuffers;
            areEqual &= this.Length == targetFile.Length;
            if (areEqual)
            {
                foreach (var index in Enumerable.Range(0, this.NumBuffers))
                {
                    var srcBuffer = this.GetBuffer(index);
                    var dstBuffer = targetFile.GetBuffer(index);
                    areEqual &= srcBuffer.Length == dstBuffer.Length;
                    if (areEqual)
                    {
                        foreach (var byteIndex in Enumerable.Range(0, srcBuffer.Length))
                        {
                            areEqual &= srcBuffer[byteIndex] == dstBuffer[byteIndex];
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return areEqual;
        }

        public CachedRAMFile ToBond()
        {
            var cachedRAMFile = new CachedRAMFile();
            cachedRAMFile.buffers = new List<IList<byte>>();
            foreach (var buffer in m_buffers)
            {
                cachedRAMFile.buffers.Add(buffer.ToList());
            }
            cachedRAMFile.Length = this.Length;
            return cachedRAMFile;
        }
    }
}
