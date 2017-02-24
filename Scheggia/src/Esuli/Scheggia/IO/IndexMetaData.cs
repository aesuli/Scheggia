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
namespace Esuli.Scheggia.IO
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Esuli.Base.IO;

    public class IndexMetaData
    {
        private string name;
        private int maxId;
        private Dictionary<string, IFieldMetaData> fieldMetaDataMap;

        public IndexMetaData(string indexName, string indexLocation)
        {
            name = indexName;

            using (Stream metadataStream = new FileStream(indexLocation + Path.DirectorySeparatorChar + indexName + IndexWriter.metadataFileExtension, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                maxId = (int)VariableByteCoding.Read(metadataStream);
                int fieldCount = (int)VariableByteCoding.Read(metadataStream);
                fieldMetaDataMap = new Dictionary<string, IFieldMetaData>(fieldCount);
                for (int i = 0; i < fieldCount; ++i)
                {
                    String fieldName = StringSerialization.Read(metadataStream);
                    Type[] TitemTcomparerThitArray = new Type[3];
                    TitemTcomparerThitArray[0] = TypeSerialization.Read(metadataStream);
                    TitemTcomparerThitArray[1] = TypeSerialization.Read(metadataStream);
                    TitemTcomparerThitArray[2] = TypeSerialization.Read(metadataStream);
                    object lexiconSerialization = TypeSerialization.CreateInstance<object>(metadataStream);
                    object postingListProviderSerialization = TypeSerialization.CreateInstance<object>(metadataStream);
                    Type fieldMetaDataOpenType = typeof(FieldMetaData<,,>);
                    Type fieldMetaDataClosedType = fieldMetaDataOpenType.MakeGenericType(TitemTcomparerThitArray);
                    IFieldMetaData field = Activator.CreateInstance(fieldMetaDataClosedType, fieldName, lexiconSerialization, postingListProviderSerialization) as IFieldMetaData;
                    fieldMetaDataMap.Add(fieldName, field);
                }
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public int MaxId
        {
            get
            {
                return maxId;
            }
        }

        public int FieldCount
        {
            get
            {
                return fieldMetaDataMap.Count;
            }
        }

        public IEnumerator<string> FieldNames
        {
            get
            {
                return fieldMetaDataMap.Keys.GetEnumerator();
            }
        }

        public IFieldMetaData GetFieldMetaData(string fieldName)
        {
            return fieldMetaDataMap[fieldName];
        }
    }
}
