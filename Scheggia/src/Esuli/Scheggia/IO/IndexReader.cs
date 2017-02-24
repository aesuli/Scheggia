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
    using Esuli.Scheggia.Core;

    public class IndexReader : IIndexReader
    {
        public IIndex Read(string indexName, string indexLocation)
        {
            var metadata = new IndexMetaData(indexName, indexLocation);

            int maxId = metadata.MaxId;
            Dictionary<string, IField> fields = new Dictionary<string, IField>(metadata.FieldCount);

            IEnumerator<string> fieldNames = metadata.FieldNames;
            while (fieldNames.MoveNext())
            {
                IFieldMetaData fieldMetadata = metadata.GetFieldMetaData(fieldNames.Current);
                string fieldName = fieldNames.Current;
                Type[] TitemAndTcomparerAndThitInfo = new Type[3];
                TitemAndTcomparerAndThitInfo[0] = fieldMetadata.ItemType;
                TitemAndTcomparerAndThitInfo[1] = fieldMetadata.ComparerType;
                TitemAndTcomparerAndThitInfo[2] = fieldMetadata.HitType;
                object lexiconSerialization = fieldMetadata.LexiconSerialization;
                object postingListProviderSerialization = fieldMetadata.PostingListProviderSerialization;
                Type fieldHandlerOpenType = typeof(FieldSerialization<,,>);
                Type fieldHandlerClosedType = fieldHandlerOpenType.MakeGenericType(TitemAndTcomparerAndThitInfo);
                IFieldSerialization fieldSerialization = Activator.CreateInstance(fieldHandlerClosedType,
                    lexiconSerialization, postingListProviderSerialization) as IFieldSerialization;
                IField field = fieldSerialization.Read(indexName, indexLocation, fieldName);
                fields.Add(fieldName, field);
            }
            return new Index(indexName, maxId, fields);
        }

        public bool Exists(string indexName, string indexLocation)
        {
            return File.Exists(indexLocation + Path.DirectorySeparatorChar + indexName + IndexWriter.metadataFileExtension);
        }
    }
}
