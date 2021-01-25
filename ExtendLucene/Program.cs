using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtendLucene
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().BuildIndexAndServe();
        }

        public void BuildIndexAndServe()
        {
            // Ensures index backward compatibility
            const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;

            var dir = new PortableRAMDirectory();
            // var dir = new SimpleFSDirectory(new System.IO.DirectoryInfo("xxx.txt"));
            // var dir = FSDirectory.Open("xxxx2.txt");
            // Create an analyzer to process the text
            var analyzer = new StandardAnalyzer(AppLuceneVersion);

            // Create an index writer
            var indexConfig = new IndexWriterConfig(AppLuceneVersion, analyzer);
            using (var writer = new IndexWriter(dir, indexConfig))
            {
                foreach(var i in Enumerable.Range(0, 100))
                {
                    AddDocument(writer);
                }
                writer.Flush(triggerMerge: false, applyAllDeletes: false);
                writer.Commit();
            }
            this.SearchIndex(dir);

            Console.WriteLine();
            Console.WriteLine("after serialization and deserialization");
            Console.WriteLine();
            var cached = dir.ToBond();
            var anotherRAMDirectory = PortableRAMDirectory.FromBond(cached);
            this.ValidateDirectory(dir, anotherRAMDirectory);
            this.SearchIndex(anotherRAMDirectory);
        }

        private void AddDocument(IndexWriter writer)
        {
            var source = new
            {
                Name = "Kermit the Frog",
                FavoritePhrase = "The quick brown fox jumps over the lazy dog"
            };

            var doc = new Document
                {
                    // StringField indexes but doesn't tokenize
                    new StringField("name",
                        source.Name,
                        Field.Store.YES),
                    new TextField("favoritePhrase",
                        source.FavoritePhrase,
                        Field.Store.YES)
                };

            writer.AddDocument(doc);
        }

        private void SearchIndex(Directory directory)
        {
            // Search with a phrase
            var phrase = new MultiPhraseQuery
            {
                new Term("favoritePhrase", "brown"),
                new Term("favoritePhrase", "fox")
            };

            using (var reader = DirectoryReader.Open(directory))
            {
                var searcher = new IndexSearcher(reader);
                var hits = searcher.Search(phrase, 20 /* top 20 */).ScoreDocs;

                // Display the output in a table
                Console.WriteLine($"{"Score",10}" +
                    $" {"Name",-15}" +
                    $" {"Favorite Phrase",-40}");
                foreach (var hit in hits)
                {
                    var foundDoc = searcher.Doc(hit.Doc);
                    Console.WriteLine($"{hit.Score:f8}" +
                        $" {foundDoc.Get("name"),-15}" +
                        $" {foundDoc.Get("favoritePhrase"),-40}");
                }
            }
        }

        private void ValidateDirectory(PortableRAMDirectory srcDir, PortableRAMDirectory dstDir)
        {
            foreach (var fileName in srcDir.ListAll())
            {
                var srcFile = srcDir.GetRAMFile(fileName);
                // Assert.IsNotNull(srcFile);
                var dstFile = dstDir.GetRAMFile(fileName);
                // Assert.IsNotNull(dstFile);
                // Assert.IsTrue(srcFile.Compare(dstFile));
                if (srcFile.Compare(dstFile))
                {
                    Console.WriteLine(srcFile.Compare(dstFile));
                }
                else
                {
                    Console.Error.WriteLine(srcFile.Compare(dstFile));
                }
            }
        }
    }
}
