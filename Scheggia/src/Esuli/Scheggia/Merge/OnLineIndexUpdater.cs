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
    using System.Collections;
    using System.Collections.Generic;
    using Esuli.Scheggia.Core;

    public class OnLineIndexUpdater
    {
        public IIndex UpdateIndexes(string indexName, IIndex[] sourceIndexes, List<Dictionary<int,int>> mapping)
        {
            SortedDictionary<string, List<KeyValuePair<int, IField>>> updatedFields = new SortedDictionary<string, List<KeyValuePair<int, IField>>>();
            int maxId = 0;
            for(int i = 0; i < sourceIndexes.Length; ++i)
            {
                IEnumerator<string> fieldNames = sourceIndexes[i].GetFieldNameEnumerator();
                while(fieldNames.MoveNext())
                {
                    List<KeyValuePair<int, IField>> fieldList;
                    if (updatedFields.TryGetValue(fieldNames.Current, out fieldList))
                    {
                        fieldList.Add(new KeyValuePair<int, IField>(i, sourceIndexes[i].GetField(fieldNames.Current)));
                    }
                    else
                    {
                        fieldList = new List<KeyValuePair<int, IField>>();
                        fieldList.Add(new KeyValuePair<int, IField>(i, sourceIndexes[i].GetField(fieldNames.Current)));
                        updatedFields.Add(fieldNames.Current, fieldList);
                    }
                }
                maxId += mapping[i].Count;
            }
            Dictionary<string, IField> fields = new Dictionary<string, IField>(updatedFields.Count);
            SortedDictionary<string, List<KeyValuePair<int, IField>>>.Enumerator fieldEnumerator = updatedFields.GetEnumerator();
            while(fieldEnumerator.MoveNext())
            {
                string fieldName = fieldEnumerator.Current.Key;
                List<KeyValuePair<int, IField>> fieldsList = fieldEnumerator.Current.Value;
                Type fieldType = fieldsList[0].Value.GetType();
                Type[] TitemAndTcomparerAndThitInfo = fieldType.GetGenericArguments();
                Type updatedFieldOpenType = typeof(UpdatedField<,,>);
                Type updatedFieldClosedType = updatedFieldOpenType.MakeGenericType(TitemAndTcomparerAndThitInfo);
                IField updatedField = Activator.CreateInstance(updatedFieldClosedType, fieldName, fieldsList, mapping.ToArray()) as IField;
                fields.Add(fieldName, updatedField);
            }
            return new Index(indexName, maxId, fields);
        }
    }
}
