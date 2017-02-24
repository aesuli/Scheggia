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
    using System.Collections.Generic;

    /// <summary>
    /// Functions to manage a list as a heap.
    /// </summary>
    /// <typeparam name="Titem"></typeparam>
    public class HeapUtils<Titem>
    {
        public static void Heapify(List<Titem> items, IComparer<Titem> comparer)
        {
            if (items.Count < 2)
            {
                return;
            }
            int parent = (items.Count - 2) / 2;
            while (parent >= 0)
            {
                SubHeapify(items, parent, items.Count, comparer);
                --parent;
            }
        }

        private static void SubHeapify(List<Titem> items, int parent, int size, IComparer<Titem> comparer)
        {
            int lastChild = 2 * (parent + 1);
            while (lastChild < size)
            {
                if (comparer.Compare(items[lastChild - 1], items[lastChild]) > 0)
                {
                    --lastChild;
                }
                if (comparer.Compare(items[parent], items[lastChild]) > 0)
                {
                    break;
                }

                Titem temp = items[parent];
                items[parent] = items[lastChild];
                items[lastChild] = temp;

                parent = lastChild;
                lastChild = 2 * (parent + 1);
            }
            if (lastChild == size)
            {
                --lastChild;
                if (comparer.Compare(items[parent], items[lastChild]) < 0)
                {
                    Titem temp = items[parent];
                    items[parent] = items[lastChild];
                    items[lastChild] = temp;
                }
            }
        }

        public static void HeapSort(List<Titem> items, IComparer<Titem> comparer, bool isHeap)
        {
            if (!isHeap)
            {
                Heapify(items, comparer);
            }

            int lastPosition = items.Count - 1;
            while (lastPosition > 0)
            {
                Titem temp = items[lastPosition];
                items[lastPosition] = items[0];
                items[0] = temp;
                SubHeapify(items, 0, lastPosition, comparer);
                --lastPosition;
            }
        }

        public static void ReplaceItem(Titem newItem, int position, List<Titem> items, IComparer<Titem> comparer)
        {
            items[position] = newItem;
            while (true)
            {
                SubHeapify(items, position, items.Count, comparer);
                if (position == 0)
                {
                    return;
                }
                position = (position - 1) / 2;
            }
        }

        public static List<Titem> GetTopK(int k, IEnumerator<Titem> resultsEnumerator, IComparer<Titem> comparer)
        {
            List<Titem> topResults = new List<Titem>(k);
            while (resultsEnumerator.MoveNext())
            {
                Titem result = resultsEnumerator.Current;
                if (topResults.Count < k)
                {
                    topResults.Add(result);
                    if (topResults.Count == k)
                    {
                        HeapUtils<Titem>.Heapify(topResults, comparer);
                    }
                }
                else if (comparer.Compare(result, topResults[0]) < 0)
                {
                    HeapUtils<Titem>.ReplaceItem(result, 0, topResults, comparer);
                }
            }

            if (topResults.Count < k)
            {
                HeapUtils<Titem>.Heapify(topResults, comparer);
            }
            HeapUtils<Titem>.HeapSort(topResults, comparer, true);
            return topResults;
        }
    }
}
