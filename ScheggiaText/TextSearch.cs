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

namespace Esuli.Scheggia.Text
{
    using Esuli.Base.Collections;
    using Esuli.Base.IO;
    using Esuli.Base.IO.Storage;
    using Esuli.Scheggia.Core;
    using Esuli.Scheggia.Enumerators;
    using Esuli.Scheggia.Indexing;
    using Esuli.Scheggia.IO;
    using Esuli.Scheggia.Merge;
    using Esuli.Scheggia.Search;
    using Esuli.Scheggia.Text.Core;
    using Esuli.Scheggia.Text.IO;
    using Esuli.Scheggia.Text.Search;
    using Esuli.Scheggia.Tools;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    public class TextSearch
    {
        static void Usage()
        {
            Console.Out.WriteLine("Usage:");
            Console.Out.WriteLine("Index:                -i  <number of parallel indexers> <merge way count> <million of hits per temporary index> single|split <[path]indexName> <pathToIndex>+");
            Console.Out.WriteLine("Search:               -s  <[path]indexName>");
            Console.Out.WriteLine("Print fields:         -df <[path]indexName>");
            Console.Out.WriteLine("Print lexicon:        -dl <[path]indexName> <fieldname>");
            Console.Out.WriteLine("Print posting lists:  -dp <[path]indexName> <fieldname>");
            Console.Out.WriteLine("Print all hits:       -dh <[path]indexName> <fieldname>");
            Console.Out.WriteLine("Merge indexes:        -m  <[path]targetIndexName> <[path]indexToMerge>+");
            Console.Out.WriteLine("Update indexes:       -u  <[path]targetIndexName> <[path]indexToUpdate>+");
            Console.Out.WriteLine("Append indexes:       -a  <[path]targetIndexName> <[path]indexToAppend>+");
        }

        static void Process(FileSystemInfo fsInfo, ISequentiallyWriteableStorage<TextFile> storage, IIndexer indexer)
        {
            var directoryInfo = fsInfo as DirectoryInfo;
            if (directoryInfo != null)
            {
                Console.Write("+(" + directoryInfo.Name + ")");
                foreach (FileSystemInfo subfsInfo in directoryInfo.GetFileSystemInfos())
                {
                    Process(subfsInfo, storage, indexer);
                }
            }
            else
            {
                var fileInfo = fsInfo as FileInfo;
                if (fileInfo != null)
                {
                    Console.Write(".");
                    foreach (var doc in TextFile.ReadFile(fileInfo.FullName))
                    {
                        int id = storage.Write(doc);
                        indexer.Index<string, OrdinalStringComparer, PositionHit>(new Scheggia.Text.Indexing.TextReader(id, doc.Name), "name");
                        indexer.Index<string, OrdinalStringComparer, PositionHit>(new Scheggia.Text.Indexing.TextReader(id, doc.URL), "url");
                        indexer.Index<string, OrdinalStringComparer, PositionHit>(new Scheggia.Text.Indexing.TextReader(id, doc.Text), "text");
                    }
                }
            }
        }

        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Out.Write("Command: (just hit return to exit) ");
                var tokens = Console.In.ReadLine().Split(new char[] { ' ' });
                var list = new List<string>();
                string token = "";
                bool inQuotes = false;
                for (int i = 0; i < tokens.Length; ++i)
                {
                    if (tokens[i].StartsWith("\""))
                    {
                        token = tokens[i].Substring(1);
                        inQuotes = true;
                    }
                    else if (inQuotes)
                    {
                        token += " " + tokens[i];
                        if (token.EndsWith("\""))
                        {
                            list.Add(token.Remove(token.Length - 1));
                            inQuotes = false;
                            token = "";
                        }
                    }
                    else
                    {
                        list.Add(tokens[i]);
                    }
                }
                args = list.ToArray();
            }
            if (args.Length >= 4 && args[0].Equals("-i"))
            {
                int parallelIndexerCount = int.Parse(args[1]);
                int mergeWayCount = int.Parse(args[2]);
                int millionHits = int.Parse(args[3]) * 1024 * 1024;
                bool split = args[4].Equals("split");
                FileInfo fileInfo = new FileInfo(args[5]);
                string indexName = fileInfo.Name;
                string indexLocation = fileInfo.DirectoryName;

                using (var storage = new SequentialWriteOnlyStorage<TextFile>(indexName, indexLocation, true))
                {
                    IndexWriter indexWriter = new IndexWriter();
                    IIndexReader indexReader = new IndexReader();
                    ParallelOffLineIndexer indexer = new ParallelOffLineIndexer(parallelIndexerCount, indexLocation, indexName, indexWriter, indexLocation, indexWriter, indexReader, millionHits, mergeWayCount);
                    indexer.AddField<string, OrdinalStringComparer, PositionHit>("name");
                    indexer.AddField<string, OrdinalStringComparer, PositionHit>("url");
                    indexer.AddField<string, OrdinalStringComparer, PositionHit>("text");
                    IPostingListProviderSerialization<PositionHit> postingListProviderSerialization;
                    if (split)
                    {
                        postingListProviderSerialization = new StreamPostingListProviderSerialization<PositionHit, SplitHitPostingEnumeratorSerialization<PositionHit, HitEnumeratorSerialization<PositionHit, PositionHitSerialization>>>();
                    }
                    else
                    {
                        postingListProviderSerialization = new StreamPostingListProviderSerialization<PositionHit, SingleStreamPostingEnumeratorSerialization<PositionHit, HitEnumeratorSerialization<PositionHit, PositionHitSerialization>>>();
                    }
                    indexWriter.AddField<string, OrdinalStringComparer, PositionHit>("name",
                        new DefaultLexiconSerialization<string, OrdinalStringComparer, SequentialStringSerialization>(), postingListProviderSerialization
                        );
                    indexWriter.AddField<string, OrdinalStringComparer, PositionHit>("url",
                        new DefaultLexiconSerialization<string, OrdinalStringComparer, SequentialStringSerialization>(), postingListProviderSerialization
                        );
                    indexWriter.AddField<string, OrdinalStringComparer, PositionHit>("text",
                        new DefaultLexiconSerialization<string, OrdinalStringComparer, SequentialStringSerialization>(), postingListProviderSerialization
                        );

                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    for (int i = 6; i < args.Length; ++i)
                    {
                        if (File.Exists(args[i]))
                        {
                            Process(new FileInfo(args[i]), storage, indexer);
                        }
                        else if (Directory.Exists(args[i]))
                        {
                            Process(new DirectoryInfo(args[i]), storage, indexer);
                        }
                    }

                    sw.Stop();

                    Console.Out.WriteLine();
                    Console.Out.WriteLine("Input data acquired in " + sw.Elapsed.TotalSeconds + " seconds.");

                    sw.Start();

                    long hitCount = indexer.BuildIndex();

                    sw.Stop();

                    Console.Out.WriteLine();
                    Console.Out.WriteLine(storage.Count + " files (" + hitCount + " hits) indexed in "
                        + sw.Elapsed.TotalSeconds + " seconds.");

                }
            }
            else if (args.Length == 2 && args[0].Equals("-s"))
            {
                FileInfo fileInfo = new FileInfo(args[1]);
                string indexName = fileInfo.Name;
                string indexLocation = fileInfo.DirectoryName;

                IIndexReader indexReader = new IndexReader();
                using (IIndex index = indexReader.Read(indexName, indexLocation))
                using (var storage = new ReadOnlyStorage<TextFile>(indexName, indexLocation))
                {
                    IQueryParser<string> queryParser = new TextQueryParser<OrdinalStringComparer>("text");

                    string queryString;
                    bool sortByScore = false;
                    PrettyPrint.IdToName idToName = PrettyPrint.IdtoId;
                    PrettyPrint.SnippetBuilder<PositionHit> snippetBuilder = PrettyPrint.SimpleSnippetBuilder<PositionHit>;
                    bool printDetails = false;
                    int rangeFilterStart = -1;
                    int rangeFilterEnd = -1;
                    int count = 100;
                    while (true)
                    {
                        try
                        {
                            Console.Out.Write(">");
                            queryString = Console.In.ReadLine();

                            if (queryString.Length == 0)
                            {
                                continue;
                            }

                            if (queryString == "x")
                            {
                                Console.Out.WriteLine("bye");
                                break;
                            }

                            if (queryString[0] == 'C')
                            {
                                var values = queryString.Split(new char[] { ' ' });
                                if (values.Length == 2)
                                {
                                    count = int.Parse(values[1]);
                                }
                                else
                                {
                                    count = 100;
                                }
                                Console.Out.WriteLine("returning {0} results", count);
                            }
                            else if (queryString[0] == 'R')
                            {
                                var values = queryString.Split(new char[] { ' ' });
                                if (values.Length == 3)
                                {
                                    rangeFilterStart = int.Parse(values[1]);
                                    rangeFilterEnd = int.Parse(values[2]);
                                    Console.Out.WriteLine("results range: from {0} to {1}", rangeFilterStart, rangeFilterEnd);
                                }
                                else
                                    rangeFilterStart = -1;
                            }
                            else if (queryString[0] == 'S')
                            {
                                if (queryString[1] == 'y')
                                {
                                    sortByScore = true;
                                    Console.Out.WriteLine("Sort by score");
                                }
                                else
                                {
                                    sortByScore = false;
                                    Console.Out.WriteLine("Sort by docId");
                                }
                            }
                            else if (queryString[0] == 'D')
                            {
                                if (queryString[1] == 'y')
                                {
                                    printDetails = true;
                                    Console.Out.WriteLine("Detailed results");
                                }
                                else
                                {
                                    printDetails = false;
                                    Console.Out.WriteLine("No details");
                                }
                            }
                            else if (queryString[0] == 'N')
                            {
                                if (queryString[1] == 'y')
                                {
                                    idToName = delegate (int id)
                                    {
                                        var obj = storage[id];
                                        return id + "\t" + obj.Name + "\t" + obj.URL;
                                    };
                                    Console.Out.WriteLine("Name in results");
                                }
                                else
                                {
                                    idToName = PrettyPrint.IdtoId;
                                    Console.Out.WriteLine("Only Id in results");
                                }
                            }
                            else if (queryString[0] == 'T')
                            {
                                if (queryString[1] == 'y')
                                {
                                    snippetBuilder = delegate (int id, IHitEnumerator<PositionHit> hitEnumerator)
                                    {
                                        string text = storage[id].Text;
                                        int window = 20;
                                        int max = 60;
                                        var sb = new StringBuilder();
                                        if(!hitEnumerator.MoveNext())
                                        {
                                            return "no snippet";
                                        }
                                        int first = hitEnumerator.Current.CharPosition;
                                        int end = first;
                                        while (hitEnumerator.MoveNext())
                                        {
                                            int pos = hitEnumerator.Current.CharPosition;
                                            if(pos-first>max)
                                            {
                                                break;
                                            }
                                            else
                                            {
                                                end = pos;
                                            }
                                        }
                                        first = Math.Max(0, first - window);
                                        end = Math.Min(text.Length, end + window + queryString.Length);
                                        sb.Append(text.Substring(first, end - first));
                                        return sb.ToString();
                                    };
                                    Console.Out.WriteLine("Print snippets");
                                }
                                else
                                {
                                    snippetBuilder = PrettyPrint.SimpleSnippetBuilder<PositionHit>;
                                    Console.Out.WriteLine("No snippets");
                                }
                            }
                            else if (queryString[0] == '[')
                            {
                                int objId = 0;
                                if (queryString[1] == '!')
                                {
                                    objId = Int32.Parse(queryString.Substring(2, queryString.Length - 3));
                                }
                                else
                                {
                                    objId = Int32.Parse(queryString.Substring(1, queryString.Length - 2));
                                }

                                TextFile textFile = storage[objId];

                                Console.Out.WriteLine(objId + "\t" + textFile.Name + "\t" + textFile.URL);
                                if (queryString[1] == '!')
                                {
                                    Console.Out.WriteLine("\n-------\n" + textFile.Text + "\n-------\n");
                                }
                            }
                            else
                            {
                                Stopwatch sw = new Stopwatch();
                                sw.Start();

                                IQuery<PositionHit> query = queryParser.Parse(queryString) as IQuery<PositionHit>;

                                IPostingEnumerator<PositionHit> baseEnumerator = null;
                                var resultsEnumerator = baseEnumerator;
                                try
                                {
                                    baseEnumerator = query.ApplySpecialized(index);
                                    if (rangeFilterStart >= 0)
                                    {
                                        var range = RangePostingEnumerator<PositionHit>.Build(rangeFilterStart, rangeFilterEnd);
                                        baseEnumerator = AndPostingEnumerator<PositionHit>.Build(new IPostingEnumerator<PositionHit>[] { range, baseEnumerator }, new IPostingEnumerator<PositionHit>[0]);
                                    }

                                    if (sortByScore)
                                    {
                                        resultsEnumerator = new ScoreSortedPostingEnumerator<PositionHit>(baseEnumerator, count, true);
                                    }
                                    else
                                    {
                                        resultsEnumerator = new CutoffPostingEnumerator<PositionHit>(baseEnumerator, count);
                                    }

                                    sw.Stop();

                                    Console.Out.WriteLine(query.Describe());

                                    PrettyPrint.PrintPostingEnumeratorWithScore<PositionHit>(Console.Out, resultsEnumerator, idToName, snippetBuilder, printDetails, "\n");

                                    if (resultsEnumerator.Count == 0)
                                    {
                                        Console.Out.WriteLine("No results");
                                    }
                                    else
                                    {
                                        Console.Out.WriteLine(resultsEnumerator.Count + " results of " + baseEnumerator.Count + " shown");
                                    }
                                }
                                finally
                                {
                                    baseEnumerator.Dispose();
                                    resultsEnumerator.Dispose();
                                }

                                Console.Out.WriteLine("Search time: " + sw.Elapsed.TotalSeconds);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.Out.WriteLine("Exception: " + e.GetType());
                            Console.Out.WriteLine("Error: " + e.Message);
                            Console.Out.WriteLine("StackTrace: " + e.StackTrace);
                        }
                    }
                }
            }
            else if (args.Length >= 3 && (args[0].Equals("-m") || args[0].Equals("-a")))
            {
                FileInfo fileInfo = new FileInfo(args[1]);
                string indexName = fileInfo.Name;
                string indexLocation = fileInfo.DirectoryName;

                int mWayMerge = 5;
                OffLineIndexMerger merger = new OffLineIndexMerger(mWayMerge);

                IndexWriter indexWriter = new IndexWriter();
                IIndexReader indexReader = new IndexReader();

                IIndexWriter[] indexWriters = new IIndexWriter[args.Length - 2];
                IIndexReader[] indexReaders = new IIndexReader[args.Length - 2];
                string[] mergedNames = new string[args.Length - 2];
                string[] mergedLocations = new string[args.Length - 2];
                var merged = new IReadOnlyList<TextFile>[args.Length - 2];

                for (int i = 0; i < mergedNames.Length; ++i)
                {
                    indexWriters[i] = indexWriter;
                    indexReaders[i] = indexReader;
                    FileInfo fi = new FileInfo(args[i + 2]);
                    mergedNames[i] = fi.Name;
                    mergedLocations[i] = fi.DirectoryName;
                    IndexMetaData indexMetaData = new IndexMetaData(fi.Name, fi.DirectoryName);
                    IEnumerator<string> fieldNames = indexMetaData.FieldNames;
                    while (fieldNames.MoveNext())
                    {
                        IFieldMetaData fieldMetaData = indexMetaData.GetFieldMetaData(fieldNames.Current);
                        indexWriter.CopyField(fieldMetaData);
                    }
                }

                MergeType mergeType;
                if (args[0].Equals("-m"))
                {
                    Console.Out.Write("Merging: ");
                    mergeType = MergeType.merge;
                }
                else
                {
                    Console.Out.Write("Appending: ");
                    mergeType = MergeType.append;
                }

                foreach (string m in mergedNames)
                {
                    Console.Out.Write(m + ", ");
                }
                Console.Out.Write(" into " + indexName);

                Stopwatch sw = new Stopwatch();
                sw.Start();

                int[] baseIds;
                merger.MergeIndexes(indexName, indexLocation, indexWriter,
                    mergedNames, mergedLocations, indexWriters, indexReaders,
                    indexLocation, indexWriter, indexReader, mergeType, out baseIds, false);

                for (int i = 0; i < mergedNames.Length; ++i)
                {
                    merged[i] = new ReadOnlyStorage<TextFile>(mergedNames[i], mergedLocations[i]);
                }

                StorageMerge<TextFile>.Merge(indexName, indexLocation, merged, baseIds);

                sw.Stop();

                Console.Out.WriteLine();
                Console.Out.WriteLine("Process completed in " + sw.Elapsed.TotalSeconds + " seconds.");
            }
            else if (args.Length >= 3 && args[0].Equals("-u"))
            {
                FileInfo fileInfo = new FileInfo(args[1]);
                string indexName = fileInfo.Name;
                string indexLocation = fileInfo.DirectoryName;

                IndexWriter indexWriter = new IndexWriter();
                IIndexReader indexReader = new IndexReader();

                IIndexWriter[] indexWriters = new IIndexWriter[args.Length - 2];
                IIndexReader[] indexReaders = new IIndexReader[args.Length - 2];
                string[] updatedNames = new string[args.Length - 2];
                string[] updatedLocations = new string[args.Length - 2];

                for (int i = 0; i < updatedNames.Length; ++i)
                {
                    indexWriters[i] = indexWriter;
                    indexReaders[i] = indexReader;
                    FileInfo fi = new FileInfo(args[i + 2]);
                    updatedNames[i] = fi.Name;
                    updatedLocations[i] = fi.DirectoryName;
                    IndexMetaData indexMetaData = new IndexMetaData(fi.Name, fi.DirectoryName);
                    IEnumerator<string> fieldNames = indexMetaData.FieldNames;
                    while (fieldNames.MoveNext())
                    {
                        IFieldMetaData fieldMetaData = indexMetaData.GetFieldMetaData(fieldNames.Current);
                        indexWriter.CopyField(fieldMetaData);
                    }
                }

                var updated = new IReadOnlyList<TextFile>[args.Length - 2];

                for (int i = 0; i < updatedNames.Length; ++i)
                {
                    updated[i] = new ReadOnlyStorage<TextFile>(updatedNames[i], updatedLocations[i]);
                }

                var keyExtractor = new StorageUpdate<TextFile, String>.KeyExtraction(delegate (TextFile textFile) { return textFile.URL.ToLower(); });

                var mapping = StorageUpdate<TextFile, String>.Update(indexName, indexLocation, updated, keyExtractor);

                OffLineIndexUpdater updater = new OffLineIndexUpdater();

                Console.Out.Write("Updating: ");

                foreach (string m in updatedNames)
                {
                    Console.Out.Write(m + ", ");
                }
                Console.Out.Write(" into " + indexName);

                Stopwatch sw = new Stopwatch();
                sw.Start();

                updater.UpdateIndexes(indexName, indexLocation, indexWriter,
                   updatedNames, updatedLocations, indexWriters, indexReaders,
                   mapping, false);

                sw.Stop();

                Console.Out.WriteLine();
                Console.Out.WriteLine("Process completed in " + sw.Elapsed.TotalSeconds + " seconds.");
            }
            else if (args.Length == 2 && args[0] == "-df")
            {
                FileInfo fileInfo = new FileInfo(args[1]);
                string indexName = fileInfo.Name;
                string indexLocation = fileInfo.DirectoryName;

                IndexMetaData indexMetaData = new IndexMetaData(indexName, indexLocation);
                PrettyPrint.PrintIndexMetaData(indexMetaData, Console.Out);
            }
            else if (args.Length == 3 && (args[0] == "-dl" || args[0] == "-dp" || args[0] == "-dh"))
            {
                FileInfo fileInfo = new FileInfo(args[1]);
                string indexName = fileInfo.Name;
                string indexLocation = fileInfo.DirectoryName;

                IIndexReader indexReader = new IndexReader();
                using (IIndex index = indexReader.Read(indexName, indexLocation))
                {
                    string fieldName = args[2];
                    IField<string, OrdinalStringComparer, PositionHit> field = index.GetSpecializedField<string, OrdinalStringComparer, PositionHit>(fieldName);

                    if (args[0] == "-dl")
                    {
                        PrettyPrint.PrintLexicon<string, OrdinalStringComparer, PositionHit>(field, Console.Out);
                    }
                    if (args[0] == "-dp")
                    {
                        PrettyPrint.PrintPostingLists<string, OrdinalStringComparer, PositionHit>(field, Console.Out, PrettyPrint.IdtoId, PrettyPrint.SimpleSnippetBuilder<PositionHit>, false);
                    }
                    if (args[0] == "-dh")
                    {
                        PrettyPrint.PrintPostingLists<string, OrdinalStringComparer, PositionHit>(field, Console.Out, PrettyPrint.IdtoId, PrettyPrint.SimpleSnippetBuilder<PositionHit>, true);
                    }
                }
            }
            else
            {
                Usage();
            }

            Console.Out.Write("Press enter to exit.");
            Console.In.ReadLine();
        }

    }
}
