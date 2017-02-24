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

    public class OnLineIndexMerger
    {
        public IIndex MergeIndexes(string indexName, IIndex[] sourceIndexes, MergeType mergeType)
        {
            int[] idShifts = new int[sourceIndexes.Length];
            return MergeIndexes(indexName, sourceIndexes, mergeType, out idShifts);
        }

        public IIndex MergeIndexes(string indexName, IIndex[] sourceIndexes, MergeType mergeType, out int[] idShifts)
        {
            SortedDictionary<string, List<KeyValuePair<int, IField>>> mergingFields = new SortedDictionary<string, List<KeyValuePair<int, IField>>>();
            int lastShift = 0;
            int maxId = 0;
            idShifts = new int[sourceIndexes.Length];
            for(int i = 0; i < sourceIndexes.Length; ++i)
            {
                IEnumerator<string> fieldNames = sourceIndexes[i].GetFieldNameEnumerator();
                while(fieldNames.MoveNext())
                {
                    List<KeyValuePair<int, IField>> fieldList;
                    if (mergingFields.TryGetValue(fieldNames.Current, out fieldList))
                    {
                        fieldList.Add(new KeyValuePair<int, IField>(i, sourceIndexes[i].GetField(fieldNames.Current)));
                    }
                    else
                    {
                        fieldList = new List<KeyValuePair<int, IField>>();
                        fieldList.Add(new KeyValuePair<int, IField>(i, sourceIndexes[i].GetField(fieldNames.Current)));
                        mergingFields.Add(fieldNames.Current, fieldList);
                    }
                }
                if(mergeType == MergeType.append)
                {
                    idShifts[i] = lastShift;
                    lastShift += sourceIndexes[i].MaxId+1;
                    maxId = lastShift-1;
                }
                else
                {
                    idShifts[i] = 0;
                    maxId = Math.Max(maxId, sourceIndexes[i].MaxId);
                }
            }
            Dictionary<string, IField> fields = new Dictionary<string, IField>(mergingFields.Count);
            SortedDictionary<string, List<KeyValuePair<int, IField>>>.Enumerator fieldEnumerator = mergingFields.GetEnumerator();
            while(fieldEnumerator.MoveNext())
            {
                string fieldName = fieldEnumerator.Current.Key;
                List<KeyValuePair<int, IField>> fieldsList = fieldEnumerator.Current.Value;
                Type fieldType = fieldsList[0].Value.GetType();
                Type[] TitemAndTcomparerAndThitInfo = fieldType.GetGenericArguments();
                Type mergedFieldOpenType = typeof(MergedField<,,>);
                Type mergedFieldClosedType = mergedFieldOpenType.MakeGenericType(TitemAndTcomparerAndThitInfo);
                IField mergedField = Activator.CreateInstance(mergedFieldClosedType, fieldName, fieldsList, idShifts) as IField;
                fields.Add(fieldName, mergedField);
            }
            return new Index(indexName, maxId, fields);
        }
    }
}
