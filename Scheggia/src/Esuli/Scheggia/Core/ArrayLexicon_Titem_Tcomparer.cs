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

namespace Esuli.Scheggia.Core
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Simple in-memory array-based implementation of <see cref="ILexicon{Titem,Tcomparer}" />.
    /// </summary>
    /// <typeparam name="Titem">The type of the item.</typeparam>
    /// <typeparam name="Tcomparer">The type of the comparer.</typeparam>
    public class ArrayLexicon<Titem, Tcomparer> : ILexicon<Titem, Tcomparer> 
        where Tcomparer : IComparer<Titem>, new()
    {
        private Tcomparer comparer;
        private Titem[] lexiconItems;

        public ArrayLexicon(Titem[] lexiconItems)
        {
            this.lexiconItems = lexiconItems;
            comparer = new Tcomparer();
        }

        public int Search(Titem query)
        {
            int position = Array.BinarySearch<Titem>(lexiconItems, query, comparer);
            return position;
        }

        public int Count
        {
            get
            {
                return lexiconItems.Length;
            }
        }

        public Titem this[int index]
        {
            get
            {
                return lexiconItems[index];
            }
        }
    }
}
