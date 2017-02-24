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
using Esuli.Scheggia.Indexing;
using Esuli.Scheggia.Text.Core;
using Esuli.Scheggia.Text.Indexing;
using System.Globalization;

namespace Esuli.SapirTextIndex
{
    public class SapirStringReader : TextReader
    {
        public SapirStringReader(int id, string text) :
            base(id, text)
        {
        }

        public SapirStringReader(int id, string text, CultureInfo culture, int tokenShift, int charShift):
            base(id, text,culture,tokenShift,charShift)
        {
        }

        public new bool MoveNext()
        {
            while (base.MoveNext())
            {
                ReaderHit<string, PositionHit> hit = base.Current;
                if (hit.Item.Contains("0") ||
                    hit.Item.Contains("1") ||
                    hit.Item.Contains("2") ||
                    hit.Item.Contains("3") ||
                    hit.Item.Contains("4") ||
                    hit.Item.Contains("5") ||
                    hit.Item.Contains("6") ||
                    hit.Item.Contains("7") ||
                    hit.Item.Contains("8") ||
                    hit.Item.Contains("9"))
                    continue;
                return true;
            }
            return false;
        }
    }
}
