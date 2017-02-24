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

    /// <summary>
    /// AND operator that also supports negated enumerators.
    /// </summary>
    public class AndPostingEnumerator : IPostingEnumerator
    {
        private IPostingEnumerator[] postingEnumerators;
        private IPostingEnumerator notPostingEnumerator;
        private int count;
        private int progress;
        private int currentPostingId;
        private int currentPostingHitCount;
        private ScoreFunction scoreFunction;

        public static IPostingEnumerator Build(IPostingEnumerator[] postingEnumerators, IPostingEnumerator[] notPostingEnumerators)
        {
            if (postingEnumerators.Length == 0)
            {
                return new EmptyPostingEnumerator();
            }

            if (postingEnumerators.Length == 1 && notPostingEnumerators.Length == 0)
            {
                return postingEnumerators[0];
            }

            return new AndPostingEnumerator(postingEnumerators, notPostingEnumerators);
        }

        protected AndPostingEnumerator(IPostingEnumerator[] postingEnumerators, IPostingEnumerator[] notPostingEnumerators)
        {
            Array.Sort<IPostingEnumerator>(postingEnumerators, new AscendingPostingEnumeratorComparer());
            this.postingEnumerators = postingEnumerators;
            notPostingEnumerator = OrPostingEnumerator.Build(notPostingEnumerators);
            count = postingEnumerators[0].Count / postingEnumerators.Length;
            progress = 0;
            currentPostingId = -1;
            currentPostingHitCount = -1;
            scoreFunction = ScoreFunctions.AddScore(postingEnumerators);
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
                notPostingEnumerator.Dispose();
                foreach (var postingEnumerator in postingEnumerators)
                {
                    postingEnumerator.Dispose();
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
                if(currentPostingHitCount < 0)
                {
                    currentPostingHitCount = 0;
                    foreach (IPostingEnumerator postingListEnumerator in postingEnumerators)
                    {
                        currentPostingHitCount += postingListEnumerator.CurrentHitCount;
                    }
                }
                return currentPostingHitCount;
            }
        }

        public bool MoveNext()
        {
            currentPostingHitCount = -1;

            int minId = currentPostingId + 1;

            int last = 0;
            while(last < postingEnumerators.Length)
            {
                if(!postingEnumerators[last].MoveNext(minId))
                {
                    count = progress;
                    currentPostingId = int.MaxValue;
                    return false;
                }

                if(last == 0)
                {
                    minId = postingEnumerators[last].CurrentPostingId;
                    ++last;
                }
                else
                {
                    if (postingEnumerators[last].CurrentPostingId == minId)
                    {
                        ++last;
                    }
                    else
                    {
                        minId = postingEnumerators[last].CurrentPostingId;
                        last = 0;
                    }
                }
                if(last == postingEnumerators.Length)
                {
                    if(notPostingEnumerator.MoveNext(minId))
                    {
                        if(notPostingEnumerator.CurrentPostingId == minId)
                        {
                            ++minId;
                            last = 0;
                        }
                    }
                }
            }

            currentPostingId = minId;

            ++progress;
            count = -1;

            return true;
        }

        public bool MoveNext(int minPostingId)
        {
            if (currentPostingId >= minPostingId && currentPostingId != int.MaxValue)
            {
                return true;
            }

            currentPostingHitCount = -1;

            int firstPostingListEnumeratorProgress = postingEnumerators[0].Progress;
            if(postingEnumerators[0].MoveNext(minPostingId))
            {
                double progressRatio = progress / (double)Math.Max(1, firstPostingListEnumeratorProgress);
                int estDeltaProgress = (int)(progressRatio * (postingEnumerators[0].Progress - firstPostingListEnumeratorProgress));
                progress += Math.Max(0, estDeltaProgress - 1);
                currentPostingId = postingEnumerators[0].CurrentPostingId - 1;
                return MoveNext();
            }

            currentPostingId = int.MaxValue;
            return false;
        }

        public IHitEnumerator GetCurrentHitEnumerator()
        {
            IHitEnumerator[] hitEnumerators = new IHitEnumerator[postingEnumerators.Length];
            for (int i = 0; i < postingEnumerators.Length; ++i)
            {
                hitEnumerators[i] = postingEnumerators[i].GetCurrentHitEnumerator();
            }
            return new ArrayHitEnumeratorMerger(hitEnumerators);
        }
    }
}
