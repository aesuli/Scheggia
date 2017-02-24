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
    using System.Collections.Generic;
    using Esuli.Scheggia.Core;

    public class OrPostingEnumerator : IPostingEnumerator
    {
        private IPostingEnumerator[] postingEnumerators;
        private int count;
        private int progress;
        private int currentPostingId;
        private int currentHitCount;
        private bool[] hasNext;
        protected List<int> currentHitEnumerators;
        private ScoreFunction scoreFunction;

        public static IPostingEnumerator Build(IPostingEnumerator[] postingEnumerators)
        {
            if (postingEnumerators.Length == 0)
            {
                return new EmptyPostingEnumerator();
            }

            if (postingEnumerators.Length == 1)
            {
                return postingEnumerators[0];
            }

            return new OrPostingEnumerator(postingEnumerators);
        }

        protected OrPostingEnumerator(IPostingEnumerator[] postingEnumerators)
        {
            Array.Sort<IPostingEnumerator>(postingEnumerators, new DescendingPostingEnumeratorComparer());
            this.postingEnumerators = postingEnumerators;
            foreach (IPostingEnumerator hitListEnumerator in postingEnumerators)
            {
                count += hitListEnumerator.Count;
            }
            progress = 0;
            currentPostingId = -1;
            currentHitCount = -1;
            hasNext = new bool[postingEnumerators.Length];
            for (int i = 0; i < postingEnumerators.Length; ++i)
            {
                hasNext[i] = postingEnumerators[i].MoveNext();
            }
            currentHitEnumerators = new List<int>();
            scoreFunction = delegate()
            {
                double score = 0;
                foreach (int i in currentHitEnumerators)
                {
                    score += postingEnumerators[i].ScoreFunction();
                }
                return score;
            };
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var hitListEnumerator in postingEnumerators)
                {
                    hitListEnumerator.Dispose();
                }
            }
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

        public int Count
        {
            get
            {
                if (count < 0)
                {
                    count = progress
                        + (int)((postingEnumerators[0].Count - postingEnumerators[0].Progress)
                        * ((double)progress / Math.Max(1, postingEnumerators[0].Progress)));
                }
                return count;
            }
        }

        public int Progress
        {
            get
            {
                if(progress < 0)
                {
                    progress = 0;
                    for(int i = 0; i < postingEnumerators.Length; ++i)
                    {
                        progress += postingEnumerators[i].Progress;
                        if (hasNext[i])
                        {
                            --progress;
                        }
                    }
                }
                return progress;
            }
        }

        public int CurrentPostingId
        {
            get
            {
                return currentPostingId;
            }
        }

        public int CurrentHitCount
        {
            get
            {
                if(currentHitCount < 0)
                {
                    currentHitCount = 0;
                    foreach (int i in currentHitEnumerators)
                    {
                        currentHitCount += postingEnumerators[i].CurrentHitCount;
                    }
                }
                return currentHitCount;
            }
        }

        public bool MoveNext()
        {
            return MoveNextHit(true);
        }

        private bool MoveNextHit(bool advanceCurrentEnumerators)
        {
            currentHitCount = -1;
            bool found = false;
            int minI = 0;
            if(advanceCurrentEnumerators)
            {
                foreach (int i in currentHitEnumerators)
                {
                    hasNext[i] = postingEnumerators[i].MoveNext();
                }
                currentHitEnumerators.Clear();
            }
            for(int i = 0; i < postingEnumerators.Length; ++i)
            {
                if(hasNext[i])
                {
                    minI = i;
                    found = true;
                    break;
                }
            }
            if(found)
            {
                currentPostingId = postingEnumerators[minI].CurrentPostingId;
                currentHitEnumerators.Clear();
                currentHitEnumerators.Add(minI);
                for(int i = minI + 1; i < postingEnumerators.Length; ++i)
                {
                    if(hasNext[i])
                    {
                        if(postingEnumerators[i].CurrentPostingId < currentPostingId)
                        {
                            minI = i;
                            currentPostingId = postingEnumerators[minI].CurrentPostingId;
                            currentHitEnumerators.Clear();
                            currentHitEnumerators.Add(minI);
                        }
                        else if (postingEnumerators[i].CurrentPostingId == currentPostingId)
                        {
                            currentHitEnumerators.Add(i);
                        }
                    }
                }

                ++progress;
                count = -1;
            }
            return found;
        }

        public bool MoveNext(int minPostingId)
        {
            if(minPostingId > currentPostingId)
            {
                for (int i = 0; i < postingEnumerators.Length; ++i)
                {
                    hasNext[i] = postingEnumerators[i].MoveNext(minPostingId);
                }
                bool found = MoveNextHit(false);
                progress = -1;
                return found;
            }
            return true;
        }

        public IHitEnumerator GetCurrentHitEnumerator()
        {
            if (currentHitEnumerators.Count == 0)
            {
                return null;
            }
            IHitEnumerator[] hitEnumerators = new IHitEnumerator[currentHitEnumerators.Count];
            int j = 0;
            foreach(int i in currentHitEnumerators)
            {
                hitEnumerators[j] = postingEnumerators[i].GetCurrentHitEnumerator();
                ++j;
            }
            return new ArrayHitEnumeratorMerger(hitEnumerators);
        }
    }
}
