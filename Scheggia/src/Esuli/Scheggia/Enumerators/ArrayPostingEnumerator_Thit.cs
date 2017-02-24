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

namespace Esuli.Scheggia.Enumerators
{
    using System;
    using Esuli.Scheggia.Core;
    using Esuli.Scheggia.Scoring;
    
    public class ArrayPostingEnumerator<Thit> : IPostingEnumerator<Thit>
    {
        private ScoreFunction scoreFunction;
        private int [] postingList;
        private Thit [] [] hitLists;
        private int enumeratorId;
        private int pos;

        public ArrayPostingEnumerator(int enumeratorId, int [] postingList, Thit [] [] hitLists) 
           
        {
            this.enumeratorId = enumeratorId;
            this.postingList = postingList;
            this.hitLists = hitLists;
            scoreFunction = ScoreFunctions.ScoreOne();
            pos = -1;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public int Count
        {
            get
            {
                return postingList.Length;
            }
        }

        public int Progress
        {
            get
            {
                return pos;
            }
        }

        public int CurrentPostingId
        {
            get
            {
                return postingList[pos];
            }
        }

        public int CurrentHitCount
        {
            get
            {
                return hitLists[pos].Length;
            }
        }

        public bool MoveNext()
        {
            ++pos;
            return pos < postingList.Length;
        }

        public bool MoveNext(int minPostingId)
        {
            pos = Array.BinarySearch<int>(postingList,pos,postingList.Length-pos, minPostingId);
            if (pos < 0)
            {
                pos = ~pos;
            }
            return pos < postingList.Length;
        }

        public IHitEnumerator GetCurrentHitEnumerator() {
            return GetSpecializedCurrentHitEnumerator();
        }

        public IHitEnumerator<Thit> GetSpecializedCurrentHitEnumerator()
        {
            return new ArrayHitEnumerator<Thit>(enumeratorId, hitLists[pos]);
        }

        public ScoreFunction ScoreFunction
        {
            get
            {
                return scoreFunction;
            }
            set
            {
                scoreFunction = value;
            }
        }
    }
}
