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
namespace Esuli.Scheggia.Text.Core
{
    public class LabeledPositionHit
        : PositionHit
    {
        private byte label;

        public LabeledPositionHit(int tokenPosition, int charPosition,byte label)
            : base(tokenPosition,charPosition)
        {
            this.label = label;
        }

        public LabeledPositionHit(PositionHit positionHit, byte label)
            : base(positionHit)
        {
            this.label = label;
        }

        public byte Label
        {
            get
            {
                return label;
            }
        }

        public new LabeledPositionHit CreateShiftedHit(int positionShift)
        {
            return new LabeledPositionHit(tokenPosition + positionShift, -1,label);
        }

        public int CompareTo(LabeledPositionHit other)
        {
            return base.CompareTo(other);
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", tokenPosition, charPosition, label);
        }
    }
}
