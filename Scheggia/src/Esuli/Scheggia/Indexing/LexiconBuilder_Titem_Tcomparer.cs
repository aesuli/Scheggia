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

namespace Esuli.Scheggia.Indexing
{
    using System;
    using System.Collections.Generic;
    using Esuli.Scheggia.Core;

    public class LexiconBuilder<Titem, Tcomparer> where Tcomparer : IComparer<Titem>, new()
    {
        Dictionary<Titem, KeyValuePair<int, Titem>> items;

        public LexiconBuilder()
        {
            items = new Dictionary<Titem, KeyValuePair<int, Titem>>();
        }

        public void Clear()
        {
            items.Clear();
        }

        public int Hit(Titem item)
        {
            KeyValuePair<int,Titem> pair;
            if(items.TryGetValue(item, out pair))
            {
                return pair.Key;
            }
            else
            {
                int position = items.Count;
                items.Add(item, new KeyValuePair<int, Titem>(position, item));
                return position;
            }
        }

        public ArrayLexicon<Titem,Tcomparer> GetLexicon(out int [] idRemapping)
        {
            Titem[] items = new Titem[this.items.Count];
            idRemapping = new int[items.Length];
            int [] sorted = new int[items.Length];
            Dictionary<Titem, KeyValuePair<int, Titem>>.ValueCollection.Enumerator enumerator = this.items.Values.GetEnumerator();
            while(enumerator.MoveNext()) 
            {
                KeyValuePair<int, Titem> pair = enumerator.Current;
                int position = pair.Key;
                items[position] = pair.Value;
                sorted[position] = position;
            }
            Array.Sort<Titem, int>(items, sorted, new Tcomparer());
            for (int i = 0; i < items.Length; ++i)
            {
                idRemapping[sorted[i]] = i;
            }

            ArrayLexicon<Titem, Tcomparer> lexicon = new ArrayLexicon<Titem, Tcomparer>(items);
            return lexicon;
        }
    }
}
