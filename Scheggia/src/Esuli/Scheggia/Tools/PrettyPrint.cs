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

namespace Esuli.Scheggia.Tools
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Esuli.Scheggia.Core;
    using Esuli.Scheggia.IO;
    using System.Text;

    public class PrettyPrint
    {
        public static void PrintIndexMetaData(IndexMetaData indexMetaData, TextWriter textWriter)
        {
            textWriter.WriteLine("Index name:\t{0}", indexMetaData.Name);
            textWriter.WriteLine("MaxId:\t{0}", indexMetaData.MaxId);
            textWriter.WriteLine("Fields count:\t{0}", indexMetaData.FieldCount);
            IEnumerator<string> fieldNamesEnumerator = indexMetaData.FieldNames;
            while (fieldNamesEnumerator.MoveNext())
            {
                string fieldName = fieldNamesEnumerator.Current;
                IFieldMetaData fieldMetaData = indexMetaData.GetFieldMetaData(fieldName);
                textWriter.WriteLine("\tField name:\t{0}", fieldMetaData.Name);
                textWriter.WriteLine("\tLexicon type:\t{0}", fieldMetaData.ItemType.FullName);
                textWriter.WriteLine("\tHit type:\t{0}", fieldMetaData.HitType.FullName);
            }
        }

        public static void PrintFields(IIndex index, TextWriter textWriter)
        {
            textWriter.WriteLine("Index name: {0}", index.Name);
            textWriter.WriteLine("Available fields:");
            IEnumerator<string> fieldNamesEnumerator = index.GetFieldNameEnumerator();
            while (fieldNamesEnumerator.MoveNext())
            {
                string fieldName = fieldNamesEnumerator.Current;
                IField field = index.GetField(fieldName);
                Type[] fieldTypes = new Type[2];
                fieldTypes = field.GetType().GetGenericArguments();
                textWriter.WriteLine("\t{0}\t{1}\t{2}", fieldName, fieldTypes[0].FullName, fieldTypes[1].FullName);
            }
        }

        public static void PrintLexicon<Titem, Tcomparer, Thit>(IField<Titem, Tcomparer, Thit> field, TextWriter textWriter)
            where Tcomparer : IComparer<Titem>
        {
            textWriter.WriteLine("Field name: {0}", field.Name);
            ILexicon<Titem,Tcomparer> lexicon = field.SpecializedLexicon;
            for (int i = 0; i < lexicon.Count; ++i)
            {
                var lexiconItem = lexicon[i];
                var count = field.PostingListProvider.GetPostingEnumerator(0, i).Count;
                textWriter.WriteLine("\t{0}\t{1}\t{2}", i, count, lexiconItem);
            }
        }

        public static void PrintPostingLists<Titem, Tcomparer, Thit>(IField<Titem, Tcomparer, Thit> field, TextWriter textWriter, IdToName idToName, SnippetBuilder<Thit> snippetBuilder, bool printHits)
        where Tcomparer : IComparer<Titem>
        {
            textWriter.WriteLine("Field name: {0}", field.Name);
            ILexicon<Titem, Tcomparer> lexicon = field.SpecializedLexicon;
            for (int i = 0; i < lexicon.Count; ++i)
            {
                var lexiconItem = lexicon[i];
                var count = field.PostingListProvider.GetPostingEnumerator(0, i).Count;
                textWriter.Write("\t{0}\t{1}\t{2}\t", i, count, lexiconItem);
                using (var postingEnumerator = field.SpecializedPostingListProvider.GetSpecializedPostingEnumerator(i, i))
                {
                    PrintPostingEnumerator<Thit>(textWriter, postingEnumerator, idToName, snippetBuilder, printHits);
                }
            }
        }

        public delegate string IdToName(int id);

        public static string IdtoId(int id)
        {
            return id.ToString();
        }

        public delegate string SnippetBuilder<Thit>(int id, IHitEnumerator<Thit> hitEnumerator);

        public static string SimpleSnippetBuilder<Thit>(int id, IHitEnumerator<Thit> hitEnumerator)
        {
            var sb = new StringBuilder();
            sb.Append("[ ");
            while (hitEnumerator.MoveNext())
            {
                sb.AppendFormat("{0} ", hitEnumerator.Current);
            }
            sb.Append("] ");
            return sb.ToString();
        }

        public static void PrintPostingEnumerator<Thit>(TextWriter textWriter, IPostingEnumerator<Thit> postingEnumerator, IdToName idToName, SnippetBuilder<Thit> snippetBuilder, bool printHits, string hitSeparator = " ")
        {
            if (printHits)
            {
                while (postingEnumerator.MoveNext())
                {
                    textWriter.Write("{0} ({1}) ", idToName(postingEnumerator.CurrentPostingId), postingEnumerator.CurrentHitCount);
                    using (var hitEnumerator = postingEnumerator.GetSpecializedCurrentHitEnumerator())
                    {
                        textWriter.Write(snippetBuilder(postingEnumerator.CurrentPostingId, hitEnumerator));
                    }
                    textWriter.Write(hitSeparator);
                }
            }
            else
            {
                while (postingEnumerator.MoveNext())
                {
                    textWriter.Write("{0} ", idToName(postingEnumerator.CurrentPostingId));
                    textWriter.Write(hitSeparator);
                }
            }
            textWriter.WriteLine();
        }

        public static void PrintPostingEnumeratorWithScore<Thit>(TextWriter textWriter, IPostingEnumerator<Thit> postingEnumerator, IdToName idToName, SnippetBuilder<Thit> snippetBuilder, bool printHits, string hitSeparator = " ")
        {
            if (printHits)
            {
                while (postingEnumerator.MoveNext())
                {
                    textWriter.Write("{0} ({1:0.###}, {2}) ", idToName(postingEnumerator.CurrentPostingId), postingEnumerator.ScoreFunction(), postingEnumerator.CurrentHitCount);
                    using (var hitEnumerator = postingEnumerator.GetSpecializedCurrentHitEnumerator())
                    {
                        textWriter.Write(snippetBuilder(postingEnumerator.CurrentPostingId, hitEnumerator));
                    }
                    textWriter.Write(hitSeparator);
                }
            }
            else
            {
                while (postingEnumerator.MoveNext())
                {
                    textWriter.Write("{0}\t{1:0.###}", idToName(postingEnumerator.CurrentPostingId), postingEnumerator.ScoreFunction());
                    textWriter.Write(hitSeparator);
                }
            }
            textWriter.WriteLine();
        }
    }
}
