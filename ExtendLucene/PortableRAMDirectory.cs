
namespace Lucene.Net.Store
{
    using A;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using LNS = Lucene.Net.Store;

    public class PortableRAMDirectory : RAMDirectory
    {
        // *****
        // Lock acquisition sequence:  RAMDirectory, then RAMFile
        // *****

        /// <summary>
        /// Constructs an empty <see cref="Directory"/>. </summary>
        public PortableRAMDirectory()
        {
            try
            {
                SetLockFactory(new LNS.SingleInstanceLockFactory());
            }
            catch (IOException) // LUCENENET: IDE0059: Remove unnecessary value assignment
            {
                // Cannot happen
            }
        }

        /// <summary>
        /// Creates a new <see cref="RAMDirectory"/> instance from a different
        /// <see cref="Directory"/> implementation.  This can be used to load
        /// a disk-based index into memory.
        ///
        /// <para/><b>Warning:</b> this class is not intended to work with huge
        /// indexes. Everything beyond several hundred megabytes will waste
        /// resources (GC cycles), because it uses an internal buffer size
        /// of 1024 bytes, producing millions of <see cref="T:byte[1024]"/> arrays.
        /// this class is optimized for small memory-resident indexes.
        /// It also has bad concurrency on multithreaded environments.
        ///
        /// <para/>For disk-based indexes it is recommended to use
        /// <see cref="MMapDirectory"/>, which is a high-performance directory
        /// implementation working directly on the file system cache of the
        /// operating system, so copying data to heap space is not useful.
        ///
        /// <para/>Note that the resulting <see cref="RAMDirectory"/> instance is fully
        /// independent from the original <see cref="Directory"/> (it is a
        /// complete copy).  Any subsequent changes to the
        /// original <see cref="Directory"/> will not be visible in the
        /// <see cref="RAMDirectory"/> instance.
        /// </summary>
        /// <param name="dir"> a <see cref="Directory"/> value </param>
        /// <param name="context">io context</param>
        /// <exception cref="IOException"> if an error occurs </exception>
        public PortableRAMDirectory(LNS.Directory dir, LNS.IOContext context)
            : this(dir, false, context)
        {
        }

        private PortableRAMDirectory(LNS.Directory dir, bool closeDir, LNS.IOContext context)
            : this()
        {
            foreach (string file in dir.ListAll())
            {
                dir.Copy(this, file, file, context);
            }
            if (closeDir)
            {
                dir.Dispose();
            }
        }

        /// <summary>
        /// Returns a new <see cref="RAMFile"/> for storing data. this method can be
        /// overridden to return different <see cref="RAMFile"/> impls, that e.g. override
        /// <see cref="RAMFile.NewBuffer(int)"/>.
        /// </summary>
        protected override RAMFile NewRAMFile()
        {
            return new PortableRAMFile(this);
        }

        public PortableRAMFile GetRAMFile(string fileName)
        {
            if (m_fileMap.ContainsKey(fileName))
            {
                return m_fileMap[fileName] as PortableRAMFile;
            }
            else
            {
                return null;
            }
        }

        public CachedRAMDirectory ToBond()
        {
            var cachedRAMDirectory = new CachedRAMDirectory();
            cachedRAMDirectory.files = new Dictionary<string, CachedRAMFile>();
            foreach (var kvp in m_fileMap)
            {
                cachedRAMDirectory.files.Add(kvp.Key, (kvp.Value as PortableRAMFile).ToBond());
            }
            return cachedRAMDirectory;
        }

        public static PortableRAMDirectory FromBond(CachedRAMDirectory cached)
        {
            var portableRAMDirectory = new PortableRAMDirectory();
            foreach (var kvp in cached.files)
            {
                using (var indexOutput = portableRAMDirectory.CreateOutput(kvp.Key, IOContext.DEFAULT))
                {
                    long remainingLength = kvp.Value.Length;
                    foreach (var buffer in kvp.Value.buffers)
                    {
                        if (remainingLength > buffer.Count)
                        {
                            indexOutput.WriteBytes(buffer.ToArray(), buffer.Count);
                            remainingLength -= buffer.Count;
                        }
                        else
                        {
                            indexOutput.WriteBytes(buffer.ToArray(), (int)remainingLength);
                        }
                    }
                }
            }
            return portableRAMDirectory;
        }
    }
}
