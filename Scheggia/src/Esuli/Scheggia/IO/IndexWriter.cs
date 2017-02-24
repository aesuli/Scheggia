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
    using System.Reflection;
    using Esuli.Base.IO;
    using Esuli.Scheggia.Core;

    public class IndexWriter : IIndexWriter
    {
        public static readonly string fieldPrefix = "_";
        public static readonly string metadataFileExtension = ".idx";
        public static readonly string lexiconsFileExtension = ".lex";
        public static readonly string primaryPostingsFileExtension = ".pri";
        public static readonly string secondaryPostingsFileExtension = ".sec";

        private Dictionary<String, object> lexiconSerializationMap;
        private Dictionary<String, object> postingListProviderSerializationMap;

        public IndexWriter()
        {
            lexiconSerializationMap = new Dictionary<string, object>();
            postingListProviderSerializationMap = new Dictionary<string, object>();
        }

        public void CopyField(IFieldMetaData fieldMetaData)
        {
            Type[] TitemAndTcomparerAndThitInfo = new Type[3];
            TitemAndTcomparerAndThitInfo[0] = fieldMetaData.ItemType;
            TitemAndTcomparerAndThitInfo[1] = fieldMetaData.ComparerType;
            TitemAndTcomparerAndThitInfo[2] = fieldMetaData.HitType;
            MethodInfo methodInfo = typeof(IndexWriter).GetMethod("AddField");
            methodInfo = methodInfo.MakeGenericMethod(TitemAndTcomparerAndThitInfo);
            methodInfo.Invoke(this, new object[] { fieldMetaData.Name, fieldMetaData.LexiconSerialization, fieldMetaData.PostingListProviderSerialization });
        }

        public void AddField<Titem, Tcomparer, Thit>(string fieldName, ILexiconSerialization<Titem, Tcomparer> lexiconSerialization,
            IPostingListProviderSerialization<Thit> postingListProviderSerialization)
            where Tcomparer : IComparer<Titem>, new ()
        {
            if (lexiconSerializationMap.ContainsKey(fieldName))
            {
                lexiconSerializationMap.Remove(fieldName);
            }
            lexiconSerializationMap.Add(fieldName, lexiconSerialization);
            if (postingListProviderSerializationMap.ContainsKey(fieldName))
            {
                postingListProviderSerializationMap.Remove(fieldName);
            }
            postingListProviderSerializationMap.Add(fieldName, postingListProviderSerialization);
        }

        public void Write(IIndex index, string indexLocation)
        {
            using (Stream metadataStream = new FileStream(indexLocation + Path.DirectorySeparatorChar + index.Name + metadataFileExtension, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                VariableByteCoding.Write(index.MaxId, metadataStream);
                VariableByteCoding.Write(index.FieldCount, metadataStream);

                IEnumerator<string> fieldNames = index.GetFieldNameEnumerator();
                while (fieldNames.MoveNext())
                {
                    IField field = index.GetField(fieldNames.Current);
                    StringSerialization.Write(fieldNames.Current, metadataStream);
                    Type[] TitemAndTcomparerAndThitInfo = field.GetType().GetGenericArguments();
                    TypeSerialization.Write(TitemAndTcomparerAndThitInfo[0], metadataStream);
                    TypeSerialization.Write(TitemAndTcomparerAndThitInfo[1], metadataStream);
                    TypeSerialization.Write(TitemAndTcomparerAndThitInfo[2], metadataStream);
                    object lexiconSerialization = lexiconSerializationMap[field.Name];
                    TypeSerialization.Write(lexiconSerialization.GetType(), metadataStream);
                    object postingListProviderSerialization = postingListProviderSerializationMap[field.Name];
                    TypeSerialization.Write(postingListProviderSerialization.GetType(), metadataStream);
                    Type fieldHandlerOpenType = typeof(FieldSerialization<,,>);
                    Type fieldHandlerClosedType = fieldHandlerOpenType.MakeGenericType(TitemAndTcomparerAndThitInfo);
                    IFieldSerialization fieldSerialization = Activator.CreateInstance(fieldHandlerClosedType,
                        lexiconSerialization, postingListProviderSerialization) as IFieldSerialization;
                    fieldSerialization.Write(field, index.Name, indexLocation);
                }
            }
        }

        public void Delete(string indexName, string indexLocation)
        {
            var metadata = new IndexMetaData(indexName, indexLocation);
            IEnumerator<string> fieldNames = metadata.FieldNames;
            while (fieldNames.MoveNext())
            {
                File.Delete(indexLocation + Path.DirectorySeparatorChar + indexName + IndexWriter.fieldPrefix + fieldNames.Current + IndexWriter.lexiconsFileExtension);
                File.Delete(indexLocation + Path.DirectorySeparatorChar + indexName + IndexWriter.fieldPrefix + fieldNames.Current + IndexWriter.primaryPostingsFileExtension);
                File.Delete(indexLocation + Path.DirectorySeparatorChar + indexName + IndexWriter.fieldPrefix + fieldNames.Current + IndexWriter.secondaryPostingsFileExtension);
            }
            File.Delete(indexLocation + Path.DirectorySeparatorChar + indexName + IndexWriter.metadataFileExtension);
        }
    }
}
