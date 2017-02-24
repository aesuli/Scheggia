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

    /// <summary>
    /// Simple enumerator of hits stored into an array.
    /// </summary>
    /// <typeparam name="Thit">The type of the hit.</typeparam>
    public class ArrayHitEnumerator<Thit> : IHitEnumerator<Thit>
    {
        private int enumeratorId;
        private Thit[] hitList;
        private int pos;

        public ArrayHitEnumerator(int enumeratorId,Thit[] hitList)
        {
            this.enumeratorId = enumeratorId;
            this.hitList = hitList;
            pos = -1;
        }

        public void Dispose()
        {
        }

        public int Count
        {
            get
            {
                return hitList.Length;
            }
        }

        public int Progress
        {
            get
            {
                return pos;
            }
        }

        public int CurrentEnumeratorId
        {
            get
            {
                return enumeratorId;
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
                return hitList[pos];
            }
        }

        public bool MoveNext()
        {
            ++pos;
            return pos < hitList.Length;
        }

        public bool MoveNext(Thit minHit)
        {
            pos = Array.BinarySearch<Thit>(hitList, pos, hitList.Length - pos, minHit);
            if (pos < 0)
            {
                pos = ~pos;
            }
            return pos < hitList.Length;
        }
    }
}
