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
    using System;
    using Esuli.Scheggia.Enumerators;

    public class PositionHit 
        : IComparable<PositionHit>,
        IPositionalHit<PositionHit>
    {
        protected int tokenPosition;
        protected int charPosition;

        public PositionHit(int tokenPosition,int charPosition)
        {
            this.tokenPosition = tokenPosition;
            this.charPosition = charPosition;
        }

        public PositionHit(PositionHit positionHit)
        {
            this.tokenPosition = positionHit.tokenPosition;
            this.charPosition = positionHit.charPosition;
        }

        public int TokenPosition
        {
            get
            {
                return tokenPosition;
            }
        }

        public int CharPosition
        {
            get
            {
                return charPosition;
            }
        }

        public PositionHit CreateShiftedHit(int positionShift)
        {
            return new PositionHit(tokenPosition + positionShift, -1);
        }

        public int CompareTo(PositionHit other)
        {
            int diff = tokenPosition - other.tokenPosition;
            if (diff == 0)
            {
                return charPosition - other.charPosition;
            }
            return diff;
        }

        public int Position
        {
            get
            {
                return tokenPosition;
            }
        }

        public override string ToString()
        {
            return '('+tokenPosition.ToString()+','+charPosition.ToString()+')';
        }
    }
}
