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
namespace Esuli.Scheggia.Text.Indexing
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using Esuli.Scheggia.Indexing;
    using Esuli.Scheggia.Text.Core;

    public class TextReader : IEnumerator<ReaderHit<string, PositionHit>>
    {
        private CultureInfo culture;
        private StringBuilder stringBuilder;
        private int tokenShift;
        private int charShift;
        private int tokenCount;
        private int charPosition;
        private ReaderHit<string, PositionHit> currentHit;
        private string text;
        private int position;
        private int id;

        public TextReader(int id, string text) :
            this(id, text, CultureInfo.InvariantCulture, 0, 0)
        {
        }

        public TextReader(int id, string text, CultureInfo culture, int tokenShift,int charShift)
        {
            this.id = id;
            this.culture = culture;
            stringBuilder = new StringBuilder(10);
            this.tokenShift = tokenShift;
            this.charShift = charShift;
            currentHit = default(ReaderHit<string,PositionHit>);
            this.text = text.ToLower(culture);
            this.text = this.text.Normalize();
            Reset();
        }

        public int TokenShift
        {
            set
            {
                tokenShift = value;
            }
            get
            {
                return tokenShift;
            }
        }

        public int CharShift
        {
            set
            {
                charShift = value;
            }
            get
            {
                return charShift;
            }
        }

        public int LastReadTokenCount
        {
            get
            {
                return tokenCount;
            }
        }

        public int LastReadCharPosition
        {
            get
            {
                return charPosition;
            }
        }


        public ReaderHit<string, PositionHit> Current
        {
            get
            { return currentHit; }
        }

        public void Dispose()
        {
        }

        object System.Collections.IEnumerator.Current
        {
            get { return currentHit; }
        }

        public bool MoveNext()
        {
            while (true)
            {
                stringBuilder.Length = 0;
                while (position < text.Length)
                {
                    char c = text[position];
                    if (char.IsLetterOrDigit(c))
                    {
                        charPosition = position;
                        stringBuilder.Append(c);
                        ++position;
                        break;
                    }
                    ++position;
                }

                if (stringBuilder.Length == 0)
                {
                    return false;
                }

                while (position < text.Length)
                {
                    char c = text[position];
                    if (!char.IsLetterOrDigit(c))
                    {
                        ++position;
                        break;
                    }
                    stringBuilder.Append(c);
                    ++position;
                }

                string token = stringBuilder.ToString();

                currentHit = new ReaderHit<string, PositionHit>(id, token, new PositionHit(tokenCount + tokenShift, charPosition + charShift));
                ++tokenCount;
                return true;
            }
        }

        public void Reset()
        {
            tokenCount = 0;
            position = 0;
            charPosition = 0;
        }
    }
}
