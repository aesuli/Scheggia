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
    
    public class EmptyHitEnumerator : IHitEnumerator
    {
        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        
        public bool MoveNext()
        {
            return false;
        }

        public int Count
        {
            get
            {
                return 0;
            }
        }

        public int Progress
        {
            get
            {
                return 0;
            }
        }

        public int CurrentEnumeratorId
        {
            get
            {
                return 0;
            }
        }

        public Type HitType
        {
            get
            {
                return typeof(object);
            }
        }

        public object CurrentHit
        {
            get
            {
                return null;
            }
        }
    }
}
