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

    public class OnLineFieldIndexer<Titem, Tcomparer, Thit>
        : IOnLineFieldIndexer
        where Tcomparer : IComparer<Titem>, new ()
        where Thit : IComparable<Thit>
    {
        private static readonly int DefaultHitListInitialCapacity = 1;

        private int hitCount;
        private LexiconBuilder<Titem, Tcomparer> lexiconBuilder;
        private List<SortedDictionary<int, List<Thit>>> postingLists;
        private string name;

        public OnLineFieldIndexer(string name)
        {
            this.name = name;
            hitCount = 0;
            lexiconBuilder = new LexiconBuilder<Titem, Tcomparer>();
            postingLists = new List<SortedDictionary<int, List<Thit>>>();
        }

        public long HitCount
        {
            get
            {
                return hitCount;
            }
        }

        public int Index(IEnumerator<ReaderHit<Titem, Thit>> hitEnumerator)
        {
            int maxId = 0;
            while(hitEnumerator.MoveNext())
            {
                ReaderHit<Titem, Thit> readerHit = hitEnumerator.Current;
                Index(readerHit);
                maxId = Math.Max(maxId, readerHit.Id);
            }
            return maxId;
        }

        private void Index(ReaderHit<Titem, Thit> readerHit)
        {
            int position = lexiconBuilder.Hit(readerHit.Item);
            SortedDictionary<int, List<Thit>> postingList;
            List<Thit> hitList;
            if (position<postingLists.Count)
            {
                postingList = postingLists[position];
                if (!postingList.TryGetValue(readerHit.Id, out hitList))
                {
                    hitList = new List<Thit>(DefaultHitListInitialCapacity);
                    postingList.Add(readerHit.Id, hitList);
                }
            }
            else
            {
                postingList = new SortedDictionary<int, List<Thit>>();
                postingLists.Add(postingList);
                hitList = new List<Thit>(DefaultHitListInitialCapacity);
                postingList.Add(readerHit.Id, hitList);
            }
            hitList.Add(readerHit.Hit);
            ++hitCount;
        }

        public void Clear()
        {
            hitCount = 0;
            lexiconBuilder.Clear();
            postingLists.Clear();
        }

        public IField GetField()
        {
            return GetSpecializedField();
        }

        public IField<Titem, Tcomparer, Thit> GetSpecializedField()
        {
            int[] idRemapping;
            ArrayLexicon<Titem, Tcomparer> lexicon = lexiconBuilder.GetLexicon(out idRemapping);

            KeyValuePair<int[], Thit[][]>[] realPostingLists = new KeyValuePair<int[], Thit[][]>[lexicon.Count];
            for(int i = 0; i < postingLists.Count; ++i)
            {
                if (idRemapping[i] >= 0)
                {
                    int[] postingIds = new int[postingLists[i].Count];
                    Thit[][] hitLists = new Thit[postingLists[i].Count][];
                    postingLists[i].Keys.CopyTo(postingIds, 0);
                    for (int j = 0; j < postingIds.Length; ++j)
                    {
                        hitLists[j] = postingLists[i][postingIds[j]].ToArray();
                        Array.Sort<Thit>(hitLists[j]);
                    }
                    realPostingLists[idRemapping[i]] = new KeyValuePair<int[], Thit[][]>(postingIds, hitLists);
                }
            }
            ArrayPostingListProvider<Thit> postingListProvider = new ArrayPostingListProvider<Thit>(realPostingLists);
            Field<Titem, Tcomparer, Thit> field = new Field<Titem, Tcomparer, Thit>(name, lexicon, postingListProvider);
            return field;
        }
    }
}
