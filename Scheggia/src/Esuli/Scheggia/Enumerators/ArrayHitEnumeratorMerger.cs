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
    
    public class ArrayHitEnumeratorMerger : IHitEnumerator
    {
        private IHitEnumerator[] hitEnumerators;
        private int count;
        private int progress;
        private int currentHitEnumerator;

        public ArrayHitEnumeratorMerger(IHitEnumerator[] hitEnumerators)
        {
            this.hitEnumerators = hitEnumerators;
            count = 0;
            foreach (IHitEnumerator hitEnumerator in hitEnumerators)
            {
                count += hitEnumerator.Count;
            }
            progress = 0;
            currentHitEnumerator = 0;
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
                return hitEnumerators[currentHitEnumerator].HitType;
            }
        }

        public object CurrentHit
        {
            get
            {
                return hitEnumerators[currentHitEnumerator].CurrentHit;
            }
        }

        public bool MoveNext()
        {
            bool hasNext = false;
            while(!hasNext && currentHitEnumerator < hitEnumerators.Length)
            {
                hasNext = hitEnumerators[currentHitEnumerator].MoveNext();
                if (!hasNext)
                {
                    ++currentHitEnumerator;
                }
                else
                {
                    ++progress;
                }
            }

            return hasNext;
        }
    }
}
