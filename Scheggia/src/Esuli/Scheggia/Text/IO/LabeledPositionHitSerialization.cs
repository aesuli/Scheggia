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
    
    public class LabeledPositionHitSerialization
        : ISequentialObjectSerialization<LabeledPositionHit>
    {
        private PositionHitSerialization positionHitSerialization;

        public LabeledPositionHitSerialization()
        {
            positionHitSerialization = new PositionHitSerialization();
        }

        public void WriteFirst(LabeledPositionHit obj, Stream stream)
        {
            positionHitSerialization.WriteFirst(obj,stream);
            stream.WriteByte(obj.Label);
        }

        public void Write(LabeledPositionHit obj, LabeledPositionHit previousObj, Stream stream)
        {
            positionHitSerialization.Write(obj,previousObj, stream);
            stream.WriteByte(obj.Label);
        }

        public LabeledPositionHit ReadFirst(Stream stream)
        {
            PositionHit positionHit = positionHitSerialization.ReadFirst(stream);
            byte label = (byte)stream.ReadByte();
            return new LabeledPositionHit(positionHit, label);
        }

        public LabeledPositionHit Read(LabeledPositionHit previousObj, Stream stream)
        {
            PositionHit positionHit = positionHitSerialization.Read(previousObj,stream);
            byte label = (byte)stream.ReadByte();
            return new LabeledPositionHit(positionHit, label);
        }

        public LabeledPositionHit ReadFirst(byte[] buffer, ref long position)
        {
            PositionHit positionHit = positionHitSerialization.ReadFirst(buffer, ref position);
            byte label = buffer[position++];
            return new LabeledPositionHit(positionHit, label);
        }

        public LabeledPositionHit Read(LabeledPositionHit previousObj, byte[] buffer, ref long position)
        {
            PositionHit positionHit = positionHitSerialization.Read(previousObj, buffer, ref position);
            byte label = buffer[position++];
            return new LabeledPositionHit(positionHit, label);
        }
    }
}
