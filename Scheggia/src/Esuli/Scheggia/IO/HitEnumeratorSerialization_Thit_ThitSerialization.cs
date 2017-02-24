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
    using System.IO;
    using Esuli.Base.IO;
    using Esuli.Scheggia.Core;

    public class HitEnumeratorSerialization<Thit, ThitSerialization> 
        : IHitEnumeratorSerialization<Thit> 
        where Thit : class, IComparable<Thit>
        where ThitSerialization : ISequentialObjectSerialization<Thit>, new()
    {
        private ISequentialObjectSerialization<Thit> hitSerialization;

        public HitEnumeratorSerialization()
        {
            this.hitSerialization = new ThitSerialization();
        }

        public int Write(IHitEnumerator<Thit> hitEnumerator, Stream stream)
        {
            if (!hitEnumerator.MoveNext())
            {
                return 0;
            }
            int count = 1;
            Thit previousHit = hitEnumerator.Current;
            hitSerialization.WriteFirst(previousHit, stream);
            Thit currentHit;
            while(hitEnumerator.MoveNext())
            {
                currentHit = hitEnumerator.Current;
                hitSerialization.Write(currentHit, previousHit, stream);
                previousHit = currentHit;
                ++count;
            }
            return count;
        }

        public IHitEnumerator<Thit> Read(int enumeratorId, int count, Stream stream)
        {
            return new HitEnumerator<Thit>(enumeratorId, count, hitSerialization, stream);
        }

        public IHitEnumerator<Thit> Read(int enumeratorId, int count, byte [] buffer, long startPosition)
        {
            return new ByteArrayHitEnumerator<Thit>(enumeratorId, count, hitSerialization, buffer, startPosition);
        }
    }
}
