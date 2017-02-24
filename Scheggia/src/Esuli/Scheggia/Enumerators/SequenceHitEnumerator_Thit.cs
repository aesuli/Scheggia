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
    
    public class SequenceHitEnumerator<Thit> 
        : IHitEnumerator<Thit>
        where Thit : IPositionalHit<Thit>, IComparable<Thit>
    {
        private IHitEnumerator<Thit>[] hitEnumerators;
        private int count;
        private int progress;
        private int currentHitEnumerator;
        private bool hasNext;

        public SequenceHitEnumerator(IHitEnumerator<Thit>[] hitEnumerators)
        {
            this.hitEnumerators = hitEnumerators;
            count = 0;
            foreach (var hitEnumerator in hitEnumerators)
            {
                count += hitEnumerator.Count;
            }
            progress = 0;
            currentHitEnumerator = hitEnumerators.Length - 1;
            hasNext = true;
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
                //TODO improve estimate
                return count;
            }
        }

        public int Progress
        {
            get
            {
                //TODO improve estimate
                if (progress < 0)
                {
                    progress = 0;
                    for (int i = 0; i < hitEnumerators.Length; ++i)
                    {
                        progress += hitEnumerators[i].Progress;
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
                return hitEnumerators[currentHitEnumerator].Current;
            }
        }

        public bool MoveNext()
        {
            return MoveNextHit(true);
        }

        public bool MoveNextHit(bool advance)
        {
            if(!hasNext)
                return false;
            if(currentHitEnumerator == hitEnumerators.Length-1)
            {
                currentHitEnumerator = 0;
                while(true)
                {
                    if (advance)
                    {
                        if (!hitEnumerators[0].MoveNext())
                        {
                            hasNext = false;
                            count = progress;
                            return false;
                        }
                    }
                    else
                    {
                        advance = true;
                    }

                    Thit lastPosition = hitEnumerators[0].Current;
                    int i = 1;
                    while(i < hitEnumerators.Length)
                    {
                        if(!hitEnumerators[i].MoveNext(lastPosition))
                        {
                            hasNext = false;
                            count = progress;
                            return false;
                        }
                        if (hitEnumerators[i].Current.Position - lastPosition.Position > 1)
                        {
                            break;
                        }
                        else if (hitEnumerators[i].Current.Position - lastPosition.Position < 1)
                        {
                            if (!hitEnumerators[i].MoveNext())
                            {
                                hasNext = false;
                                count = progress;
                                return false;
                            }
                            continue;
                        }
                        lastPosition = hitEnumerators[i].Current;
                        ++i;
                    }
                    if(i == hitEnumerators.Length)
                    {
                        ++progress;
                        count = -1;
                        return true;
                    }
                }
            }
            else {
                if (advance)
                {
                    ++currentHitEnumerator;
                }
                return true;
            }
        }

        public bool MoveNext(Thit minHit)
        {
            foreach (var hitEnumerator in hitEnumerators)
            {
                if (!hitEnumerator.MoveNext(minHit))
                {
                    hasNext = false;
                    count = progress;
                    return false;
                }
            }
            currentHitEnumerator = hitEnumerators.Length - 1;
            return MoveNextHit(false);
        }
    }
}
