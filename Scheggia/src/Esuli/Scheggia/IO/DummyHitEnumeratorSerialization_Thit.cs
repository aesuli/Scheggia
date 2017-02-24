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
    using System.IO;
    using Esuli.Scheggia.Core;
    using Esuli.Scheggia.Enumerators;
    
    public class DummyHitEnumeratorSerialization<Thit> :
        IHitEnumeratorSerialization<Thit>
    {
        public DummyHitEnumeratorSerialization()
        {
        }

        public int Write(IHitEnumerator<Thit> hitEnumerator, Stream stream)
        {
            return 0;
        }

        public IHitEnumerator<Thit> Read(int enumeratorId, int count, Stream stream)
        {
            return new EmptyHitEnumerator<Thit>();
        }

        public IHitEnumerator<Thit> Read(int enumeratorId, int count, byte[] buffer, long startPosition)
        {
            return new EmptyHitEnumerator<Thit>();
        }
    }
}
