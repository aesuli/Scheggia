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
namespace Esuli.Scheggia.Text.IO
{
    using System.IO;
    using Esuli.Base.IO;
    using Esuli.Scheggia.Text.Core;

    public class PositionHitSerialization
        : ISequentialObjectSerialization<PositionHit>
    {
        public void WriteFirst(PositionHit obj, Stream stream)
        {
            VariableByteCoding.Write(obj.TokenPosition, stream);
            VariableByteCoding.Write(obj.CharPosition, stream);
        }

        public void Write(PositionHit obj, PositionHit previousObj, Stream stream)
        {
            VariableByteCoding.Write(obj.TokenPosition - previousObj.TokenPosition, stream);
            VariableByteCoding.Write(obj.CharPosition - previousObj.CharPosition, stream);
        }

        public PositionHit ReadFirst(Stream stream)
        {
            int tokenPosition = (int)VariableByteCoding.Read(stream);
            int charPosition = (int)VariableByteCoding.Read(stream);
            return new PositionHit(tokenPosition,charPosition);
        }

        public PositionHit Read(PositionHit previousObj, Stream stream)
        {
            int deltaTokenPosition = (int)VariableByteCoding.Read(stream);
            int deltaCharPosition = (int)VariableByteCoding.Read(stream);
            return new PositionHit(deltaTokenPosition + previousObj.TokenPosition,deltaCharPosition+previousObj.CharPosition);
        }

        public PositionHit ReadFirst(byte[] buffer, ref long position)
        {
            int tokenPosition = (int)VariableByteCoding.Read(buffer,ref position);
            int charPosition = (int)VariableByteCoding.Read(buffer, ref position);
            return new PositionHit(tokenPosition, charPosition);
        }

        public PositionHit Read(PositionHit previousObj, byte[] buffer, ref long position)
        {
            int deltaTokenPosition = (int)VariableByteCoding.Read(buffer, ref position);
            int deltaCharPosition = (int)VariableByteCoding.Read(buffer, ref position);
            return new PositionHit(deltaTokenPosition + previousObj.TokenPosition, deltaCharPosition + previousObj.CharPosition);
        }
    }
}
