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
    using Esuli.Base.Collections;
    using Esuli.Base.Comparers;
    using Esuli.Scheggia.Core;

    public class ScoreSortedPostingEnumerator<Thit>
        : IPostingEnumerator<Thit>
        where Thit : IComparable<Thit>
    {
        private List<KeyValuePair<double, KeyValuePair<int, List<KeyValuePair<int, Thit>>>>> results;
        int progress;

        public ScoreSortedPostingEnumerator(IPostingEnumerator<Thit> basePostingEnumerator, int cutoff, bool higherScoreIsBetter)
        {
            results = new List<KeyValuePair<double, KeyValuePair<int, List<KeyValuePair<int, Thit>>>>>(cutoff);
            IComparer<KeyValuePair<double, KeyValuePair<int, List<KeyValuePair<int, Thit>>>>> comparer;
            if (higherScoreIsBetter)
            {
                comparer = new KeyValuePairKeyReverseComparer<double, KeyValuePair<int, List<KeyValuePair<int, Thit>>>>();
            }
            else
            {
                comparer = new KeyValuePairKeyComparer<double, KeyValuePair<int, List<KeyValuePair<int, Thit>>>>();
            }
            Comparison<double> scoreComparer;
            if (higherScoreIsBetter)
            {
                scoreComparer = delegate(double lhs, double rhs)
                {
                    if (lhs > rhs)
                    {
                        return 1;
                    }
                    else if (rhs > lhs)
                    {
                        return -1;
                    }
                    else
                    {
                        return 0;
                    }
                };
            }
            else
            {
                scoreComparer = delegate(double lhs, double rhs)
                {
                    if (lhs > rhs)
                    {
                        return -1;
                    }
                    else if (rhs > lhs)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                };
            }

            while (basePostingEnumerator.MoveNext())
            {
                int id = basePostingEnumerator.CurrentPostingId;
                int hitCount = basePostingEnumerator.CurrentHitCount;
                double score = basePostingEnumerator.ScoreFunction();
                if (results.Count < cutoff)
                {
                    List<KeyValuePair<int, Thit>> hitList = new List<KeyValuePair<int, Thit>>(hitCount);
                    using (var hitEnumerator = basePostingEnumerator.GetSpecializedCurrentHitEnumerator())
                    {
                        while (hitEnumerator.MoveNext())
                        {
                            hitList.Add(new KeyValuePair<int, Thit>(hitEnumerator.CurrentEnumeratorId, hitEnumerator.Current));
                        }
                        results.Add(new KeyValuePair<double, KeyValuePair<int, List<KeyValuePair<int, Thit>>>>(score, new KeyValuePair<int, List<KeyValuePair<int, Thit>>>(id, hitList)));
                    }
                    if (results.Count == cutoff)
                    {
                        HeapUtils<KeyValuePair<double, KeyValuePair<int, List<KeyValuePair<int, Thit>>>>>.Heapify(results, comparer);
                    }
                }
                else if (scoreComparer(score, results[0].Key) > 0)
                {
                    List<KeyValuePair<int, Thit>> hitList = new List<KeyValuePair<int, Thit>>(hitCount);
                    using (var hitEnumerator = basePostingEnumerator.GetSpecializedCurrentHitEnumerator())
                    {
                        while (hitEnumerator.MoveNext())
                        {
                            hitList.Add(new KeyValuePair<int, Thit>(hitEnumerator.CurrentEnumeratorId, hitEnumerator.Current));
                        }
                    }
                    HeapUtils<KeyValuePair<double, KeyValuePair<int, List<KeyValuePair<int, Thit>>>>>.ReplaceItem(new KeyValuePair<double, KeyValuePair<int, List<KeyValuePair<int, Thit>>>>(score, new KeyValuePair<int, List<KeyValuePair<int, Thit>>>(id, hitList)),
                        0, results, comparer);
                }

            }
            if (results.Count < cutoff)
            {
                HeapUtils<KeyValuePair<double, KeyValuePair<int, List<KeyValuePair<int, Thit>>>>>.Heapify(results, comparer);
            }
            HeapUtils<KeyValuePair<double, KeyValuePair<int, List<KeyValuePair<int, Thit>>>>>.HeapSort(results, comparer, true);
            progress = -1;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public ScoreFunction ScoreFunction
        {
            get
            {
                return delegate()
                {
                    return results[progress].Key;
                };
            }
            set
            {
                throw new Exception("Unsupported method");
            }
        }

        public int Count
        {
            get
            {
                return results.Count;
            }
        }

        public int Progress
        {
            get
            {
                return progress + 1;
            }
        }

        public int CurrentPostingId
        {
            get
            {
                return results[progress].Value.Key;
            }
        }

        public int CurrentHitCount
        {
            get
            {
                return results[progress].Value.Value.Count;
            }
        }

        public bool MoveNext()
        {
            ++progress;
            return progress < results.Count;
        }

        public bool MoveNext(int minPostingId)
        {
            throw new Exception("Unsupported method");
        }

        public IHitEnumerator GetCurrentHitEnumerator()
        {
            return GetSpecializedCurrentHitEnumerator();
        }

        public IHitEnumerator<Thit> GetSpecializedCurrentHitEnumerator()
        {
            return new HitListEnumerator<Thit>(results[progress].Value.Value);
        }
    }
}
