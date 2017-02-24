// Copyright (C) 2016 Andrea Esuli
// http://www.esuli.it
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
using Esuli.Base.Collections;
using Esuli.Base.IO;
using Esuli.Scheggia.Indexing;
using Esuli.Scheggia.IO;
using Esuli.Scheggia.Text.Core;
using Esuli.Scheggia.Text.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;

namespace Esuli.SapirTextIndex
{
    public class SapirTextIndex
    {
        private static Stopwatch stopwatch = new Stopwatch();

        static void Usage()
        {
            Console.Out.WriteLine("Usage:");
            Console.Out.WriteLine("Index:          -i <path-to-XML-tar-files> <tar-file-prefix> <tar-file-suffix> <first-tar-index> <last-tar-index> <output-index-name>");
        }

        [STAThread]
        static void Main(string[] args)
        {
            string selectorName = "SelectAll";

            #region aargs
            string[][] aargs = new string[][] {
                new string[] {
                    "-i",
                    @"C:\Documents and Settings\esuli\My Documents\CoPhIR\tgz\",
                    "sapir_id_",
                    "_xml_r.tgz",
                    "1",
                    "50",
                    "idx_50M_"+selectorName+"_text_bbb",
                    @"C:\Documents and Settings\esuli\My Documents\CoPhIR\index",
                },
            };
            #endregion

            if (args.Length == 0)
                args = aargs[0];


            if (args.Length == 8 && args[0].Equals("-i"))
            {
                stopwatch.Reset();
                stopwatch.Start();

                string tarPath = args[1];
                string tarPrefix = args[2];
                string tarSuffix = args[3];
                int firstTar = int.Parse(args[4]);
                int lastTar = int.Parse(args[5]) + 1;
                string indexName = args[6];
                string indexLocation = args[7];

                IndexWriter indexWriter = new IndexWriter();
                IIndexReader indexReader = new IndexReader();

                long inMemoryHitLimit = 1024 * 1024;
                int mWayMerge = 2;

                OffLineIndexer indexer = new OffLineIndexer(indexLocation, indexName, indexWriter, indexLocation, indexWriter, indexReader, inMemoryHitLimit, mWayMerge);

                indexer.AddField<string, OrdinalStringComparer, PositionHit>("description");
                indexWriter.AddField<string, OrdinalStringComparer, PositionHit>("description",
                    new DefaultLexiconSerialization<string, OrdinalStringComparer, SequentialStringSerialization>(),
                    new StreamPostingListProviderSerialization<PositionHit, SplitHitPostingEnumeratorSerialization<PositionHit, HitEnumeratorSerialization<PositionHit, PositionHitSerialization>>>());

                indexer.AddField<string, OrdinalStringComparer, PositionHit>("title");
                indexWriter.AddField<string, OrdinalStringComparer, PositionHit>("title",
                    new DefaultLexiconSerialization<string, OrdinalStringComparer, SequentialStringSerialization>(),
                    new StreamPostingListProviderSerialization<PositionHit, SplitHitPostingEnumeratorSerialization<PositionHit, HitEnumeratorSerialization<PositionHit, PositionHitSerialization>>>());

                indexer.AddField<string, OrdinalStringComparer, PositionHit>("comment");
                indexWriter.AddField<string, OrdinalStringComparer, PositionHit>("comment",
                    new DefaultLexiconSerialization<string, OrdinalStringComparer, SequentialStringSerialization>(),
                    new StreamPostingListProviderSerialization<PositionHit, SplitHitPostingEnumeratorSerialization<PositionHit, HitEnumeratorSerialization<PositionHit, PositionHitSerialization>>>());

                indexer.AddField<string, OrdinalStringComparer, PositionHit>("tag");
                indexWriter.AddField<string, OrdinalStringComparer, PositionHit>("tag",
                    new DefaultLexiconSerialization<string, OrdinalStringComparer, SequentialStringSerialization>(),
                    new StreamPostingListProviderSerialization<PositionHit, SplitHitPostingEnumeratorSerialization<PositionHit, HitEnumeratorSerialization<PositionHit, PositionHitSerialization>>>());

                indexer.AddField<string, OrdinalStringComparer, byte>("owner");
                indexWriter.AddField<string, OrdinalStringComparer, byte>("owner",
                    new DefaultLexiconSerialization<string, OrdinalStringComparer, SequentialStringSerialization>(),
                    new StreamPostingListProviderSerialization<byte, SplitHitPostingEnumeratorSerialization<byte, DummyHitEnumeratorSerialization<byte>>>());

                int count = 0;
                int shift = 20;

                for (int i = firstTar; i < lastTar; ++i)
                {
                    using (Stream stream = new FileStream(tarPath + tarPrefix + i + tarSuffix, FileMode.Open, FileAccess.Read))
                    using (StreamReader tarReader = new StreamReader(new GZipStream(stream, CompressionMode.Decompress)))
                    {
                        while (!tarReader.EndOfStream)
                        {
                            string line;
                            bool gotPhoto = false;
                            var hit = new ReaderHit<string, byte>[1];
                            while (!gotPhoto && (line = tarReader.ReadLine()) != null)
                            {
                                if (line.ToLower().Contains("<photo"))
                                {
                                    StringBuilder stringBuilder = new StringBuilder(line);
                                    while ((line = tarReader.ReadLine()) != null)
                                    {
                                        stringBuilder.Append(line);
                                        if (line.ToLower().Contains("</photo>"))
                                            break;
                                    }

                                    gotPhoto = true;

                                    int id = -1;

                                    int commentTokenCount = 0;
                                    int commentCharPosition = 0;
                                    int tagTokenCount = 0;
                                    int tagCharPosition = 0;

                                    try
                                    {
                                        using (System.IO.StringReader stringReader = new System.IO.StringReader(stringBuilder.ToString()))
                                        using (XmlReader reader = XmlReader.Create(stringReader))
                                        {
                                            while (reader.Read())
                                            {
                                                switch (reader.NodeType)
                                                {
                                                    case (XmlNodeType.Element):
                                                        switch (reader.Name)
                                                        {
                                                            case ("photo"):
                                                                id = int.Parse(reader["id"]);
                                                                break;
                                                            case ("description"):
                                                                var hitEnumerator = new SapirStringReader(count, reader.ReadString());
                                                                indexer.Index<string, OrdinalStringComparer, PositionHit>(hitEnumerator, "description");
                                                                break;
                                                            case ("title"):
                                                                hitEnumerator = new SapirStringReader(count, reader.ReadString());
                                                                indexer.Index<string, OrdinalStringComparer, PositionHit>(hitEnumerator, "title");
                                                                break;
                                                            case ("comment"):
                                                                hitEnumerator = new SapirStringReader(count, reader.ReadString(), CultureInfo.InvariantCulture, commentTokenCount, commentCharPosition);
                                                                indexer.Index<string, OrdinalStringComparer, PositionHit>(hitEnumerator, "comment");
                                                                commentTokenCount += hitEnumerator.LastReadTokenCount + shift;
                                                                commentCharPosition += hitEnumerator.LastReadCharPosition + shift;
                                                                break;
                                                            case ("tag"):
                                                                hitEnumerator = new SapirStringReader(count, reader.ReadString(), CultureInfo.InvariantCulture, tagTokenCount, tagCharPosition);
                                                                indexer.Index<string, OrdinalStringComparer, PositionHit>(hitEnumerator, "tag");
                                                                tagTokenCount += hitEnumerator.LastReadTokenCount + shift;
                                                                tagCharPosition += hitEnumerator.LastReadCharPosition + shift;
                                                                break;
                                                            case ("owner"):
                                                                hit[0] = new ReaderHit<string, byte>(count, reader["username"], 0);
                                                                indexer.Index<string, OrdinalStringComparer, byte>(((IEnumerable<ReaderHit<string, byte>>)hit).GetEnumerator(), "owner");
                                                                break;
                                                        }
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(i + "\n" + id + "\n" + e.ToString());
                                        foreach (object key in e.Data.Keys)
                                        {
                                            Console.WriteLine(key.ToString() + ":\t" + e.Data[key].ToString());
                                        }
                                    }
                                    finally
                                    {
                                        ++count;
                                    }
                                    if (count % 1000 == 0)
                                        Console.Write(".");
                                }
                            }
                        }
                    }
                }

                long hitCount = indexer.HitCount;

                indexer.BuildIndex();

                stopwatch.Stop();

                Console.WriteLine("\nData read in "
                    + stopwatch.Elapsed.TotalSeconds + " seconds ("
                    + count + " objects, "
                    + hitCount + " hits");
            }
            else
            {
                Usage();
            }
        }
    }
}
