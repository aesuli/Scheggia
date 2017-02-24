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

namespace Esuli.Scheggia.Indexing
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Esuli.Scheggia.IO;
    using Esuli.Scheggia.Merge;

    public class ParallelOffLineIndexer : IOffLineIndexer
    {
        private string indexName;
        private OffLineIndexer[] indexers;
        private string indexLocation;
        private string tempIndexLocation;
        private IIndexWriter indexWriter;
        private IIndexWriter tempIndexWriter;
        private IIndexReader tempIndexReader;
        private int mergeWayCount;
        private int parallelIndexes;
        private Task<int>[] indexingTasks;

        public ParallelOffLineIndexer(int parallelIndexerCount, string indexLocation, string indexName, IIndexWriter indexWriter, string tempIndexLocation, IIndexWriter tempIndexWriter, IIndexReader tempIndexReader, long inMemoryHitCountLimit, int mergeWayCount)
        {
            this.indexName = indexName;
            this.indexLocation = indexLocation;
            this.parallelIndexes = parallelIndexerCount;
            this.indexWriter = indexWriter;
            this.tempIndexLocation = tempIndexLocation;
            this.tempIndexWriter = tempIndexWriter;
            this.tempIndexReader = tempIndexReader;
            this.mergeWayCount = mergeWayCount;
            indexers = new OffLineIndexer[parallelIndexerCount];
            indexingTasks = new Task<int>[parallelIndexerCount];
            for (int i = 0; i < parallelIndexerCount; ++i)
            {
                int j = i;
                indexingTasks[i] = Task<int>.Factory.StartNew(() =>
                    {
                        indexers[j] = new OffLineIndexer(tempIndexLocation, GetIndexerIndexName(j), tempIndexWriter, tempIndexLocation, tempIndexWriter, tempIndexReader, inMemoryHitCountLimit, mergeWayCount);
                        return j;
                    }
                );
            }
            Task.WaitAll(indexingTasks);
        }

        public long HitCount
        {
            get
            {
                long total = 0;
                foreach (var indexer in indexers)
                {
                    total += indexer.HitCount;
                }
                return total;
            }
        }

        public void AddField<Titem, Tcomparer, ThitInfo>(string fieldName)
            where Tcomparer : IComparer<Titem>, new()
            where ThitInfo : IComparable<ThitInfo>
        {
            foreach (var indexer in indexers)
            {
                indexer.AddField<Titem, Tcomparer, ThitInfo>(fieldName);
            }
        }

        public void Index<Titem, Tcomparer, ThitInfo>(IEnumerator<ReaderHit<Titem, ThitInfo>> hitsEnumerator, string fieldName)
            where Tcomparer : IComparer<Titem>, new ()
            where ThitInfo : IComparable<ThitInfo>
        {
            var position = Task.WaitAny(indexingTasks);
            indexingTasks[position] = Task<int>.Factory.StartNew(() =>
            {
                indexers[position].Index<Titem, Tcomparer, ThitInfo>(hitsEnumerator, fieldName);
                return position;
            });
        }

        public long BuildIndex()
        {
            Task.WaitAll(indexingTasks);
            long hitCount = 0;
            for (int i = 0; i < parallelIndexes; ++i)
            {
                hitCount += indexers[i].HitCount;
                int j = i;
                indexingTasks[i] = Task<int>.Factory.StartNew(() => { indexers[j].BuildIndex(); return j; });
            }
            Task.WaitAll(indexingTasks);


            OffLineIndexMerger indexMerger = new OffLineIndexMerger(mergeWayCount);
            string[] sourceIndexesNames = new string[parallelIndexes];
            string[] sourceIndexesLocations = new string[parallelIndexes];
            IIndexWriter[] sourceIndexesWriters = new IIndexWriter[parallelIndexes];
            IIndexReader[] sourceIndexesReaders = new IIndexReader[parallelIndexes];
            for (int i = 0; i < parallelIndexes; ++i)
            {
                sourceIndexesNames[i] = GetIndexerIndexName(i);
                sourceIndexesLocations[i] = tempIndexLocation;
                sourceIndexesWriters[i] = tempIndexWriter;
                sourceIndexesReaders[i] = tempIndexReader;
            }
            int[] idShifts;
            indexMerger.MergeIndexes(indexName, indexLocation, indexWriter,
                sourceIndexesNames, sourceIndexesLocations, sourceIndexesWriters, sourceIndexesReaders,
                tempIndexLocation, tempIndexWriter, tempIndexReader, MergeType.merge, out idShifts, true);
            return hitCount;
        }

        private string GetIndexerIndexName(int i)
        {
            return indexName + "_" + i;
        }
    }
}
