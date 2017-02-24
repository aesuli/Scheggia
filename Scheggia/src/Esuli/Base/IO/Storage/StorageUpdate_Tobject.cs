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

    public class StorageUpdate<Tobject, Tkey>
    {
        public delegate Tkey KeyExtraction(Tobject obj);

        public static List<Dictionary<int, int>> Update(string targetStorageName, string targetStorageLocation, IReadOnlyList<Tobject>[] sourceStorages, KeyExtraction keyExtractor)
        {
            using (var storage = new SequentialWriteOnlyStorage<Tobject>(targetStorageName, targetStorageLocation, true))
            {
                var sourceStoragesList = new List<IReadOnlyList<Tobject>>(sourceStorages.Length);
                var sourceIdsEnumerators = new List<IEnumerator<int>>(sourceStorages.Length);

                var keys = new HashSet<Tkey>();
                var mapping = new List<Dictionary<int, int>>();

                for (int i = 0; i < sourceStorages.Length; ++i)
                {
                    var map = new Dictionary<int, int>();
                    mapping.Add(map);
                    for (int j = 0; j < sourceStorages[i].Count; ++j)
                    {
                        var key = keyExtractor(sourceStorages[i][j]);
                        if (!keys.Contains(key))
                        {
                            keys.Add(key);
                            int newId = storage.Write(sourceStorages[i][j]);
                            map.Add(j, newId);
                        }
                    }
                }
                return mapping;
            }
        }
    }
}
