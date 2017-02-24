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
namespace Esuli.Scheggia.Merge
{
    using System;
    using System.Collections.Generic;
    using Esuli.Scheggia.Core;
    using Esuli.Scheggia.IO;

    public class OffLineIndexUpdater
    {
        public void UpdateIndexes(string indexName, string indexLocation, IIndexWriter indexWriter,
            string[] sourceIndexesNames, string[] sourceIndexesLocations, IIndexWriter[] sourceIndexesWriters, IIndexReader[] sourceIndexesReaders,
            List<Dictionary<int,int>> mapping, bool deleteSourceIndexes)
        {
            OnLineIndexUpdater indexUpdater = new OnLineIndexUpdater();

            IIndex[] sourceIndexes = new IIndex[sourceIndexesNames.Length];
            try
            {
                for (int i = 0; i < sourceIndexesNames.Length; ++i)
                {
                    sourceIndexes[i] = sourceIndexesReaders[i].Read(sourceIndexesNames[i], sourceIndexesLocations[i]);
                }
                using (IIndex index = indexUpdater.UpdateIndexes(indexName, sourceIndexes, mapping))
                {
                    indexWriter.Write(index, indexLocation);
                }
            }
            finally
            {
                for(int i = 0; i < sourceIndexesNames.Length; ++i)
                {
                    if (sourceIndexes[i] != null)
                    {
                        sourceIndexes[i].Dispose();
                    }
                    if (deleteSourceIndexes)
                    {
                        sourceIndexesWriters[i].Delete(sourceIndexesNames[i], sourceIndexesLocations[i]);
                    }
                }
            }
        }
    }
}
