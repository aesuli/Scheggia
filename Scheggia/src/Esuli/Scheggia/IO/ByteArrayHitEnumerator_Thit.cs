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

namespace Esuli.Scheggia.IO
{
    using System;
    using Esuli.Base.IO;
    using Esuli.Scheggia.Core;

    public class ByteArrayHitEnumerator<Thit> : IHitEnumerator<Thit> 
        where Thit : class, IComparable<Thit>
    {
        private int enumeratorId;
        private int hitCount;
        private byte [] buffer;
        private long position;
        private int progress;
        private ISequentialObjectSerialization<Thit> hitSerialization;
        private Thit currentHit;
        private Thit nextHit;

        public ByteArrayHitEnumerator(int enumeratorId, int count, ISequentialObjectSerialization<Thit> hitSerialization, byte[] buffer, long startPosition)
        {
            this.enumeratorId = enumeratorId;
            this.hitCount = count;
            this.hitSerialization = hitSerialization;
            this.buffer = buffer;
            position = startPosition;
            progress = 0;
            if (hitCount > 0)
            {
                nextHit = hitSerialization.ReadFirst(buffer, ref position);
            }
            else
            {
                nextHit = default(Thit);
            }
            currentHit = default(Thit);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public int Count
        {
            get
            {
                return hitCount;
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
                return enumeratorId;
            }
        }

        public System.Type HitType
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

        public bool MoveNext(Thit minHit)
        {
            do
            {
                if (currentHit != null && currentHit.CompareTo(minHit) >= 0)
                {
                    return true;
                }
            }
            while (MoveNext());
            return false;
        }


        public bool MoveNext()
        {
            if (progress == hitCount)
            {
                currentHit = null;
                return false;
            }
            ++progress;
            currentHit = nextHit;
            if (progress < hitCount)
            {
                nextHit = hitSerialization.Read(nextHit, buffer, ref position);
            }
            else
            {
                nextHit = default(Thit);
            }
            return true;
        }
    }
}