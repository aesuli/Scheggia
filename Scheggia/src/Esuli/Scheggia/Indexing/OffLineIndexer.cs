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
    using Esuli.Scheggia.Core;
    using Esuli.Scheggia.IO;
    using Esuli.Scheggia.Merge;
    using Esuli.Scheggia.Tools;
    using System;
    using System.Collections.Generic;

    public class OffLineIndexer : IOffLineIndexer
    {
        private string indexName;
        private List<string> tempIndexNames;
        private long hitCount;
        private IOnLineIndexer indexer;
        private long inMemoryHitCountLimit;
        private string indexLocation;
        private string tempIndexLocation;
        private IIndexWriter indexWriter;
        private IIndexWriter tempIndexWriter;
        private IIndexReader tempIndexReader;
        private int mergeWayCount;

        public OffLineIndexer(string indexLocation, string indexName, IIndexWriter indexWriter, string tempIndexLocation, IIndexWriter tempIndexWriter, IIndexReader tempIndexReader, long inMemoryHitCountLimit, int mergeWayCount)
        {
            this.indexName = indexName;
            this.indexLocation = indexLocation;
            this.indexWriter = indexWriter;
            this.tempIndexLocation = tempIndexLocation;
            this.inMemoryHitCountLimit = inMemoryHitCountLimit;
            this.tempIndexWriter = tempIndexWriter;
            this.tempIndexReader = tempIndexReader;
            this.mergeWayCount = mergeWayCount;
            tempIndexNames = new List<string>();
            hitCount = 0;
            indexer = new OnLineIndexer(indexName);
        }

        public long HitCount
        {
            get
            {
                return hitCount + indexer.HitCount;
            }
        }

        public void AddField<Titem, Tcomparer, ThitInfo>(string fieldName)
            where Tcomparer : IComparer<Titem>, new()
            where ThitInfo : IComparable<ThitInfo>
        {
            indexer.AddField<Titem, Tcomparer, ThitInfo>(fieldName);
        }

        public string GetTempIndexName(string name, int count)
        {
            return name + "_t" + count + '_' + DateTime.Now.Ticks;
        }

        public void Index<Titem, Tcomparer, ThitInfo>(IEnumerator<ReaderHit<Titem, ThitInfo>> hitsEnumerator, string fieldName)
            where Tcomparer : IComparer<Titem>, new ()
            where ThitInfo : IComparable<ThitInfo>
        {
            indexer.Index<Titem, Tcomparer, ThitInfo>(hitsEnumerator, fieldName);
            if(indexer.HitCount > inMemoryHitCountLimit)
            {
                hitCount += indexer.HitCount;
                var tempIndexName = GetTempIndexName(indexName, tempIndexNames.Count);
                tempIndexNames.Add(tempIndexName);
                IIndex renamedIndex = new RenamedIndex(tempIndexName, indexer.GetIndex());
                tempIndexWriter.Write(renamedIndex, tempIndexLocation);
                indexer.Clear();
            }
        }

        public long BuildIndex()
        {
            if (tempIndexNames.Count == 0)
            {
                indexWriter.Write(indexer.GetIndex(), indexLocation);
                return indexer.HitCount;
            }
            else
            {
                if (indexer.HitCount > 0)
                {
                    hitCount += indexer.HitCount;
                    var tempIndexName = GetTempIndexName(indexName, tempIndexNames.Count);
                    tempIndexNames.Add(tempIndexName);
                    IIndex renamedIndex = new RenamedIndex(tempIndexName, indexer.GetIndex());
                    tempIndexWriter.Write(renamedIndex, tempIndexLocation);
                    indexer.Clear();
                }

                OffLineIndexMerger indexMerger = new OffLineIndexMerger(mergeWayCount);
                string[] sourceIndexesNames = new string[tempIndexNames.Count];
                string[] sourceIndexesLocations = new string[tempIndexNames.Count];
                IIndexWriter[] sourceIndexesWriters = new IIndexWriter[tempIndexNames.Count];
                IIndexReader[] sourceIndexesReaders = new IIndexReader[tempIndexNames.Count];
                for (int i = 0; i < tempIndexNames.Count; ++i)
                {
                    sourceIndexesNames[i] = tempIndexNames[i];
                    sourceIndexesLocations[i] = tempIndexLocation;
                    sourceIndexesWriters[i] = tempIndexWriter;
                    sourceIndexesReaders[i] = tempIndexReader;
                }
                int[] idShifts;
                indexMerger.MergeIndexes(indexName, indexLocation, indexWriter,
                    sourceIndexesNames, sourceIndexesLocations, sourceIndexesWriters, sourceIndexesReaders,
                    tempIndexLocation, tempIndexWriter, tempIndexReader, MergeType.merge, out idShifts, true);
                tempIndexNames.Clear();
                long prevHitCount = hitCount;
                hitCount = 0;
                return prevHitCount;
            }
        }
    }
}
