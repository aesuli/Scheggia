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
    
    public class ArrayHitEnumeratorMerger<Thit> : IHitEnumerator<Thit> where Thit : IComparable<Thit>
    {
        private IHitEnumerator<Thit>[] hitEnumerators;
        private int count;
        private int progress;
        private int currentHitEnumerator;
        private Thit currentHit;
        private bool[] hasNext;

        public ArrayHitEnumeratorMerger(IHitEnumerator<Thit>[] hitEnumerators)
        {
            this.hitEnumerators = hitEnumerators;
            count = 0;
            foreach (IHitEnumerator<Thit> hitEnumerator in hitEnumerators)
            {
                count += hitEnumerator.Count;
            }
            progress = 0;
            currentHitEnumerator = -1;
            currentHit = default(Thit);
            hasNext = new bool[hitEnumerators.Length];
            for (int i = 0; i < hitEnumerators.Length; ++i)
            {
                hasNext[i] = hitEnumerators[i].MoveNext();
            }
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
                foreach (var hitEnumerator in hitEnumerators)
                {
                    hitEnumerator.Dispose();
                }
            }
        }
        
        public int Count
        {
            get
            {
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
                    for(int i = 0; i < hitEnumerators.Length; ++i) {
                        progress += hitEnumerators[i].Progress;
                        if (hasNext[i])
                        {
                            --progress;
                        }
                    }
                }
                return progress;
            }
        }

        public int CurrentEnumeratorId
        {
            get
            {
                return hitEnumerators[currentHitEnumerator].CurrentEnumeratorId;
            }
        }

        public Type HitType
        {
            get
            {
                return typeof(Thit);
            }
        }

        public object CurrentHit
        {
            get
            {
                return Current;
            }
        }
        
        public Thit Current
        {
            get
            {
                return currentHit;
            }
        }

        public bool MoveNext()
        {
            bool found = false;
            int minI = 0;
            for(int i = 0; i < hitEnumerators.Length; ++i)
            {
                if (hasNext[i])
                {
                    minI = i;
                    found = true;
                    break;
                }
            }
            if(found)
            {
                currentHitEnumerator = minI;
                Thit minHit = hitEnumerators[minI].Current;
                for(int i = minI+1; i < hitEnumerators.Length; ++i)
                {
                    if(hasNext[i])
                    {
                        if(hitEnumerators[i].Current.CompareTo(hitEnumerators[minI].Current) < 0)
                        {
                            minI = i;
                            minHit = hitEnumerators[minI].Current;
                            currentHitEnumerator = i;
                        }
                    }
                }
                currentHit = minHit;
                hasNext[minI] = hitEnumerators[minI].MoveNext();
                ++progress;
            }
            return found;
        }

        public bool MoveNext(Thit minHit)
        {
            bool found = false;
            int minI = hitEnumerators.Length;
            for(int i = 0; i < hitEnumerators.Length; ++i)
            {
                if (hitEnumerators[i].MoveNext(minHit))
                {
                    hasNext[i] = true;
                    found = true;
                    minI = Math.Min(minI, i);
                }
                else
                {
                    hasNext[i] = false;
                }
            }
            if(found)
            {
                currentHitEnumerator = minI;
                minHit = hitEnumerators[minI].Current;
                for(int i = minI + 1; i < hitEnumerators.Length; ++i)
                {
                    if(hasNext[i])
                    {
                        if(hitEnumerators[i].Current.CompareTo(hitEnumerators[minI].Current) < 0)
                        {
                            minI = i;
                            minHit = hitEnumerators[minI].Current;
                            currentHitEnumerator = i;
                        }
                    }
                }
                currentHit = minHit;
                progress = -1;
            }
            return found;
        }
    }
}
