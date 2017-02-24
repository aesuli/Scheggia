// Copyright (C) 2016 Andrea Esuli (andrea@esuli.it)
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

namespace Esuli.Base.IO.Storage
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Esuli.Base.Enumerators;

    public class StorageMerge<Tobject>
    {
        public static void Merge(string targetStorageName, string targetStorageLocation, IReadOnlyList<Tobject>[] sourceStorages, int[] baseIds)
        {
            using (var storage = new SequentialWriteOnlyStorage<Tobject>(targetStorageName, targetStorageLocation, true))
            {

                List<IReadOnlyList<Tobject>> sourceStoragesList = new List<IReadOnlyList<Tobject>>(sourceStorages.Length);
                List<IEnumerator<int>> sourceIdsEnumerators = new List<IEnumerator<int>>(sourceStorages.Length);
                List<int> shifts = new List<int>(sourceStorages.Length);
                for (int i = 0; i < sourceStorages.Length; ++i)
                {
                    IEnumerator<int> idEnumerator = IntEnumeratorOffset.Offset(new IntRangeEnumerator(0, sourceStorages[i].Count - 1), baseIds[i]);
                    if (idEnumerator.MoveNext())
                    {
                        sourceStoragesList.Add(sourceStorages[i]);
                        sourceIdsEnumerators.Add(idEnumerator);
                        shifts.Add(baseIds[i]);
                    }
                }

                while (sourceStoragesList.Count > 0)
                {
                    int min = 0;
                    int minIdx = -1;
                    int i = 0;
                    while (i < sourceStoragesList.Count)
                    {
                        int value = sourceIdsEnumerators[i].Current;
                        if (value < min || minIdx == -1)
                        {
                            min = value;
                            minIdx = i;
                        }
                        else if (value == min)
                        {
                            if (!sourceIdsEnumerators[i].MoveNext())
                            {
                                sourceIdsEnumerators.RemoveAt(i);
                                sourceStoragesList.RemoveAt(i);
                                shifts.RemoveAt(i);
                                --i;
                            }
                        }
                        ++i;
                    }

                    while (storage.Count < min)
                    {
                        storage.Write(default(Tobject));
                    }

                    Debug.Assert(storage.Count == min);

                    storage.Write(sourceStoragesList[minIdx][min - shifts[minIdx]]);

                    if (!sourceIdsEnumerators[minIdx].MoveNext())
                    {
                        sourceIdsEnumerators.RemoveAt(minIdx);
                        sourceStoragesList.RemoveAt(minIdx);
                        shifts.RemoveAt(minIdx);
                    }
                }
            }
        }
    }
}
