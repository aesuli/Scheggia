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
    using Esuli.Scheggia.Core;

    public class OnLineIndexer : IOnLineIndexer
    {
        private long hitCount;
        private string indexName;
        private SortedDictionary<string, IOnLineFieldIndexer> fieldIndexers;
        private int maxId;

        public OnLineIndexer(string indexName)
        {
            this.indexName = indexName;
            hitCount = 0;
            maxId = 0;
            fieldIndexers = new SortedDictionary<string, IOnLineFieldIndexer>();
        }

        public long HitCount
        {
            get
            {
                return hitCount;
            }
        }

        public void AddField<Titem,Tcomparer, ThitInfo>(string fieldName)
            where Tcomparer : IComparer<Titem>, new ()
            where ThitInfo : IComparable<ThitInfo>
        {
            OnLineFieldIndexer<Titem, Tcomparer, ThitInfo> fieldIndexer = new OnLineFieldIndexer<Titem, Tcomparer, ThitInfo>(fieldName);
            fieldIndexers.Add(fieldName, fieldIndexer);
        }

        public void Index<Titem, Tcomparer, ThitInfo>(IEnumerator<ReaderHit<Titem, ThitInfo>> hitEnumerator, string fieldName)
            where Tcomparer : IComparer<Titem>, new()
            where ThitInfo : IComparable<ThitInfo>
        {
            IOnLineFieldIndexer candidateFieldIndexer;
            if (!fieldIndexers.TryGetValue(fieldName, out candidateFieldIndexer))
            {
                throw new Exception("Unknown field");
            }
            OnLineFieldIndexer<Titem, Tcomparer, ThitInfo> fieldIndexer = candidateFieldIndexer as OnLineFieldIndexer<Titem, Tcomparer, ThitInfo>;

            long increment = fieldIndexer.HitCount;
            maxId = Math.Max(maxId, fieldIndexer.Index(hitEnumerator));
            increment = fieldIndexer.HitCount - increment;

            hitCount += increment;
        }

        public void Clear()
        {
            foreach (KeyValuePair<string, IOnLineFieldIndexer> indexerPair in fieldIndexers)
            {
                indexerPair.Value.Clear();
            }
            hitCount = 0;
        }

        public IIndex GetIndex()
        {
            Dictionary<string,IField> fields = new Dictionary<string,IField>(fieldIndexers.Count);
            foreach (KeyValuePair<string, IOnLineFieldIndexer> indexerPair in fieldIndexers)
            {
                fields.Add(indexerPair.Key, indexerPair.Value.GetField());
            }
            return new Index(indexName,maxId, fields);
        }
    }
}
