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
    using Esuli.Scheggia.Core;
    using Esuli.Scheggia.IO;

    public class OffLineIndexMerger
    {
        private int mWayMerge;

        public OffLineIndexMerger(int mWayMerge)
        {
            this.mWayMerge = mWayMerge;
        }

        public void MergeIndexes(string indexName, string indexLocation, IIndexWriter indexWriter,
            string[] sourceIndexesNames, string[] sourceIndexesLocations, IIndexWriter[] sourceIndexesWriters, IIndexReader[] sourceIndexesReaders,
            string tempIndexesLocation, IIndexWriter tempIndexesWriter, IIndexReader tempIndexesReader,
            MergeType mergeType, out int[] idShifts, bool deleteSourceIndexes)
        {
            OnLineIndexMerger indexMerger = new OnLineIndexMerger();
            int blocksCount = (int)Math.Ceiling(((double)sourceIndexesNames.Length) / mWayMerge);
            int maxBlockSize = (int)Math.Ceiling(((double)sourceIndexesNames.Length) / blocksCount);

            if(blocksCount == 1)
            {
                IIndex[] sourceIndexes = new IIndex[sourceIndexesNames.Length];
                try
                {
                    for (int i = 0; i < sourceIndexesNames.Length; ++i)
                    {
                        sourceIndexes[i] = sourceIndexesReaders[i].Read(sourceIndexesNames[i], sourceIndexesLocations[i]);
                    }
                    using (IIndex index = indexMerger.MergeIndexes(indexName, sourceIndexes, mergeType, out idShifts))
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
            else
            {
                idShifts = new int[sourceIndexesNames.Length];
                for (int i = 0; i < sourceIndexesNames.Length; ++i)
                {
                    idShifts[i] = 0;
                }

                string[] tempIndexesNames = new string[blocksCount];
                string[] tempIndexesLocations = new string[tempIndexesNames.Length];
                for(int i = 0; i < blocksCount; ++i)
                {
                    string tempIndexName;
                    do
                    {
                        tempIndexName = indexName + "_m" + i + '_' + DateTime.Now.Ticks;
                    }
                    while(tempIndexesReader.Exists(tempIndexName, tempIndexesLocation));
                    tempIndexesNames[i] = tempIndexName;
                    tempIndexesLocations[i] = tempIndexesLocation;
                }

                for(int k = 0; k < blocksCount; ++k)
                {
                    int currentBlockStart = k * maxBlockSize;
                    int currentBlockSize = Math.Min(maxBlockSize, sourceIndexesNames.Length - currentBlockStart);

                    if(currentBlockSize == 1)
                    {
                        tempIndexesNames[k] = sourceIndexesNames[currentBlockStart];
                        tempIndexesLocations[k] = sourceIndexesLocations[currentBlockStart];
                    }
                    else
                    {
                        int[] blockIdShifts;
                        IIndex[] sourceIndexes = new IIndex[currentBlockSize];
                        try
                        {
                            for (int i = 0; i < currentBlockSize; ++i)
                            {
                                sourceIndexes[i] = sourceIndexesReaders[i + currentBlockStart].Read(sourceIndexesNames[i + currentBlockStart], sourceIndexesLocations[i + currentBlockStart]);
                            }
                            using (IIndex tempIndex = indexMerger.MergeIndexes(tempIndexesNames[k], sourceIndexes, mergeType, out blockIdShifts))
                            {
                                tempIndexesWriter.Write(tempIndex, tempIndexesLocation);
                            }
                            for (int i = 0; i < currentBlockSize; ++i)
                            {
                                idShifts[i + currentBlockStart] = blockIdShifts[i];
                            }
                        }
                        finally
                        {
                            for(int i = 0; i < currentBlockSize; ++i)
                            {
                                if (sourceIndexes[i] != null)
                                {
                                    sourceIndexes[i].Dispose();
                                }
                                if (deleteSourceIndexes)
                                {
                                    sourceIndexesWriters[i + currentBlockStart].Delete(sourceIndexesNames[i + currentBlockStart], sourceIndexesLocations[i + currentBlockStart]);
                                }
                            }
                        }
                    }
                }

                int[] tempIdShifts;
                if(blocksCount <= mWayMerge)
                {
                    IIndex[] tempIndexes = new IIndex[blocksCount];
                    try
                    {
                        for (int i = 0; i < blocksCount; ++i)
                        {
                            tempIndexes[i] = tempIndexesReader.Read(tempIndexesNames[i], tempIndexesLocation);
                        }
                        using (IIndex index = indexMerger.MergeIndexes(indexName, tempIndexes, mergeType, out tempIdShifts))
                        {
                            indexWriter.Write(index, indexLocation);
                        }
                    }
                    finally
                    {
                        for(int i = 0; i < blocksCount; ++i)
                        {
                            if (tempIndexes[i] != null)
                            {
                                tempIndexes[i].Dispose();
                            }
                            tempIndexesWriter.Delete(tempIndexesNames[i], tempIndexesLocation);
                        }
                    }
                }
                else
                {
                    IIndexWriter[] tempIndexesWriters = new IIndexWriter[tempIndexesNames.Length];
                    IIndexReader[] tempIndexesReaders = new IIndexReader[tempIndexesNames.Length];
                    for(int i = 0; i < tempIndexesNames.Length; ++i)
                    {
                        tempIndexesWriters[i] = tempIndexesWriter;
                        tempIndexesReaders[i] = tempIndexesReader;
                    }
                    MergeIndexes(indexName, indexLocation, indexWriter, tempIndexesNames, tempIndexesLocations, tempIndexesWriters, tempIndexesReaders,
                        tempIndexesLocation, tempIndexesWriter, tempIndexesReader, mergeType, out tempIdShifts, true);
                }

                for(int k = 0; k < blocksCount; ++k)
                {
                    int currentBlockStart = k * maxBlockSize;
                    int currentBlockSize = Math.Min(maxBlockSize, sourceIndexesNames.Length - currentBlockStart);

                    for (int i = 0; i < currentBlockSize; ++i)
                    {
                        idShifts[i + currentBlockStart] += tempIdShifts[k];
                    }
                }
            }
        }
    }
}
