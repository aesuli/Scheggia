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

    public class ProximityHitEnumerator<Thit> 
        : IHitEnumerator<Thit>
        where Thit : IPositionalHit<Thit>, IComparable<Thit>
    {
        private IHitEnumerator<Thit>[] hitEnumerators;
        private int[] positions;
        private int minPositionRef;
        private int maxPositionRef;
        private int maxDistance;
        private int count;
        private int progress;
        private int currentEnumerator;
        private bool hasNext;

        public ProximityHitEnumerator(IHitEnumerator<Thit>[] hitEnumerators,int maxDistance)
        {
            this.hitEnumerators = hitEnumerators;
            positions = new int[hitEnumerators.Length];
            minPositionRef = -1;
            this.maxDistance = maxDistance;
            count = 0;
            foreach (IHitEnumerator<Thit> hitEnumerator in hitEnumerators)
            {
                count += hitEnumerator.Count;
            }
            progress = 0;
            currentEnumerator = hitEnumerators.Length - 1;
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
                return hitEnumerators[currentEnumerator].CurrentEnumeratorId;
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
                return hitEnumerators[currentEnumerator].Current;
            }
        }

        public bool MoveNext()
        {
            return MoveNextHit(true);
        }

        public bool MoveNextHit(bool advance)
        {
            if (!hasNext)
            {
                return false;
            }
            if(currentEnumerator == hitEnumerators.Length-1)
            {
                currentEnumerator = 0;
                if (advance)
                {
                    for (int i = 0; i < hitEnumerators.Length; ++i)
                    {
                        if (!hitEnumerators[i].MoveNext())
                        {
                            hasNext = false;
                            count = progress;
                            return false;
                        }
                    }
                }

                FindMaxPosition();
                FindMinPosition();

                while(true)
                {
                    if (hitEnumerators[maxPositionRef].Current.Position - hitEnumerators[minPositionRef].Current.Position <= maxDistance)
                    {
                        ++progress;
                        count = -1;
                        return true;
                    }

                    Thit minimumRequiredPosition = hitEnumerators[maxPositionRef].Current.CreateShiftedHit(-maxDistance);
                    if (!hitEnumerators[minPositionRef].MoveNext(minimumRequiredPosition))
                    {
                        hasNext = false;
                        count = progress;
                        return false;
                    }
                    if (hitEnumerators[minPositionRef].Current.Position > hitEnumerators[maxPositionRef].Current.Position)
                    {
                        maxPositionRef = minPositionRef;
                    }
                    FindMinPosition();
                }
            }
            else {
                if (advance)
                {
                    ++currentEnumerator;
                }
                return true;
            }
        }

        private void FindMinPosition()
        {
            int minPositionValue = int.MaxValue;
            for (int i = 0; i < hitEnumerators.Length; ++i)
            {
                if (minPositionValue > hitEnumerators[i].Current.Position)
                {
                    minPositionValue = hitEnumerators[i].Current.Position;
                    minPositionRef = i;
                }
            }
        }

        private void FindMaxPosition()
        {
            int maxPositionValue = int.MinValue;
            for (int i = 0; i < hitEnumerators.Length; ++i)
            {
                if (maxPositionValue < hitEnumerators[i].Current.Position)
                {
                    maxPositionValue = hitEnumerators[i].Current.Position;
                    maxPositionRef = i;
                }
            }
        }

        public bool MoveNext(Thit minHit)
        {
            foreach(var hitEnumerator in hitEnumerators) {
                if(!hitEnumerator.MoveNext(minHit))
                {
                    hasNext = false;
                    count = progress;
                    return false;
                }
            }
            currentEnumerator = hitEnumerators.Length - 1;
            return MoveNextHit(false);
        }
    }
}
