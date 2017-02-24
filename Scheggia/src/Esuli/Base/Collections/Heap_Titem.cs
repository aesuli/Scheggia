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

namespace Esuli.Base.Collections
{
    using System;
    using System.Collections.Generic;

    public class Heap<Titem>
    {
        private Titem[] items;
        private int count;
        private IComparer<Titem> comparer;

        public Heap(int initialCapacity, IComparer<Titem> comparer)
        {
            if (initialCapacity == 0)
            {
                initialCapacity = 1;
            }
            items = new Titem[initialCapacity];
            this.comparer = comparer;
        }

        public void Push(Titem item)
        {
            if(items.Length == count)
            {
                int resize = 2 * items.Length;
                Titem[] resizedItems = new Titem[resize];
                Array.Copy(items, 0, resizedItems, 0, items.Length);
                items = resizedItems;
            }

            int i = count;
            while(i>0) {
                int parent = (i - 1) / 2;
                if (comparer.Compare(item, items[parent]) < 0)
                {
                    items[i] = items[parent];
                    i = parent;
                }
                else
                {
                    break;
                }
            }
            items[i] = item;
            ++count;
        }

        public Titem Pop()
        {
            if (count == 0)
            {
                return default(Titem);
            }

            Titem poppedItem = items[0];
            --count;

            Titem item = items[count];
            items[count] = default(Titem);
            int i = 0;
            while(true)
            {
                int twoi = i * 2;
                int left = twoi + 1;
                if (left >= count)
                {
                    break;
                }
                else
                {
                    int right = twoi + 2;
                    int child = (right >= count || comparer.Compare(items[left], items[right]) < 0) ? left : right;
                    if (comparer.Compare(item, items[child]) > 0)
                    {
                        items[i] = items[child];
                        i = child;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            items[i] = item;
            return poppedItem;
        }

        public Titem Peek()
        {
            if (count > 0)
            {
                return items[0];
            }
            else
            {
                return default(Titem);
            }
        }

        public int Count
        {
            get
            {
                return count;
            }
        }

        public virtual void Clear()
        {
            for (int i = 0; i < count; ++i)
            {
                items[i] = default(Titem);
            }
            count = 0;
        }
    }
}