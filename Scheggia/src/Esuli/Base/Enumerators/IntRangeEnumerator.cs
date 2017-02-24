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

namespace Esuli.Base.Enumerators
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Enumerator that return all the integer values from first to last, inclusive.
    /// </summary>
    public class IntRangeEnumerator : IEnumerator<int>
    {
        private int first;
        private int last;
        private int current;

        public IntRangeEnumerator(int first, int last)
        {
            this.first = first;
            this.last = last;
            current = first-1;
        }

        public int Current
        {
            get
            {
                return current;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        object System.Collections.IEnumerator.Current
        {
            get
            {
                return current;
            }
        }

        public bool MoveNext()
        {
            ++current;
            return current <= last;
        }

        public void Reset()
        {
            current = first - 1;
        }
    }
}