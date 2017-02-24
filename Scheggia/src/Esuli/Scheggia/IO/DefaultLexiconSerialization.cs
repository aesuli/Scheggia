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
    using Esuli.Base.IO;
    using Esuli.Scheggia.Core;
    using System.Collections.Generic;
    using System.IO;

    public class DefaultLexiconSerialization<Titem, Tcomparer, TitemSerialization> : ILexiconSerialization<Titem, Tcomparer>
        where Tcomparer : IComparer<Titem>, new()
        where TitemSerialization : ISequentialObjectSerialization<Titem>, new()
    {
        private ISequentialObjectSerialization<Titem> itemSerialization;

        public DefaultLexiconSerialization()
        {
            this.itemSerialization = new TitemSerialization();
        }

        public void Write(ILexicon<Titem, Tcomparer> lexicon, string indexName, string indexLocation, string fieldName)
        {
            int bufferSize = 1024 * 1024;
            using (var stream = new FileStream(indexLocation + Path.DirectorySeparatorChar + indexName + IndexWriter.fieldPrefix + fieldName + IndexWriter.lexiconsFileExtension, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, bufferSize))
            {
                VariableByteCoding.Write(lexicon.Count, stream);
                if (lexicon.Count == 0)
                {
                    return;
                }
                Titem previousLexiconItem = lexicon[0];
                itemSerialization.WriteFirst(previousLexiconItem, stream);
                for (int i = 1; i < lexicon.Count; ++i)
                {
                    Titem currentLexiconItem = lexicon[i];
                    itemSerialization.Write(currentLexiconItem, previousLexiconItem, stream);
                    previousLexiconItem = currentLexiconItem;
                }
            }
        }

        public ILexicon<Titem, Tcomparer> Read(string indexName, string indexLocation, string fieldName)
        {
            int bufferSize = 1024 * 1024;
            using (Stream stream = new FileStream(indexLocation + Path.DirectorySeparatorChar + indexName + IndexWriter.fieldPrefix + fieldName + IndexWriter.lexiconsFileExtension, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize))
            {
                int itemCount = (int)VariableByteCoding.Read(stream);
                var lexiconItems = new Titem[itemCount];
                Titem item = default(Titem);
                Titem previousItem = default(Titem);
                if (itemCount > 0)
                {
                    previousItem = itemSerialization.ReadFirst(stream);
                    lexiconItems[0] = previousItem;
                }
                for (int i = 1; i < itemCount; ++i)
                {
                    item = itemSerialization.Read(previousItem, stream);
                    lexiconItems[i] = item;
                    previousItem = item;
                }
                return new ArrayLexicon<Titem, Tcomparer>(lexiconItems);
            }
        }
    }
}
