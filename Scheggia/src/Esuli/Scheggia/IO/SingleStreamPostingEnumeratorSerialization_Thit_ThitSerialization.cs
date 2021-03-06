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
    using System;
    using System.IO;
    using Esuli.Base.IO;
    using Esuli.Scheggia.Core;
    using System.IO.MemoryMappedFiles;

    public class SingleStreamPostingEnumeratorSerialization<Thit, ThitSerialization>
        : IPostingEnumeratorSerialization<Thit>
        where ThitSerialization : IHitEnumeratorSerialization<Thit>, new()
    {
        //TODO use a more flexible check for the use of skiplists
        private static readonly int minSkipListJump = 512;

        private IHitEnumeratorSerialization<Thit> hitEnumeratorSerialization;

        public SingleStreamPostingEnumeratorSerialization()
        {
            this.hitEnumeratorSerialization = new ThitSerialization();
        }

        public void Write(IPostingEnumerator<Thit> postingEnumerator, Stream primaryStream, Stream secondaryStream)
        {
            long tempPosition;
            int skipListJump = (int)Math.Sqrt(postingEnumerator.Count);
            bool hasSkipList = false;
            if (skipListJump >= minSkipListJump)
            {
                hasSkipList = true;
            }
            long baseSkipPosition = primaryStream.Position;

            VariableByteCoding.Write((hasSkipList ? 1 : 0), primaryStream);

            long countPosition = primaryStream.Position;
            int reservedCountSize = VariableByteCoding.ByteSize(postingEnumerator.Count);
            VariableByteCoding.Write(postingEnumerator.Count, primaryStream);

            using (var memStream = new MemoryStream())
            {
                int hitCount = 0;
                if (hasSkipList)
                {
                    byte[] bytes;
                    int skipEntrySize = sizeof(int) * 2 + sizeof(long);
                    long skipPosition = primaryStream.Position;
                    primaryStream.Position += skipEntrySize;

                    int skipSize = 0;
                    int previousHitId = 0;
                    while (postingEnumerator.MoveNext())
                    {
                        int deltaId = postingEnumerator.CurrentPostingId - previousHitId;
                        VariableByteCoding.Write(deltaId, primaryStream);
                        memStream.SetLength(0);
                        int hitInfoCount = 0;
                        using (var hitInfoEnumerator = postingEnumerator.GetSpecializedCurrentHitEnumerator())
                        {
                            hitInfoCount = hitEnumeratorSerialization.Write(hitInfoEnumerator, memStream);
                        }
                        long deltaPointer = memStream.Position;
                        VariableByteCoding.Write(hitInfoCount, primaryStream);
                        VariableByteCoding.Write(deltaPointer, primaryStream);
                        primaryStream.Write(memStream.GetBuffer(), 0, (int)memStream.Position);
                        previousHitId += deltaId;
                        ++hitCount;
                        ++skipSize;
                        if (hitCount % skipListJump == 0)
                        {
                            tempPosition = primaryStream.Position;
                            primaryStream.Position = skipPosition;
                            skipPosition = tempPosition;
                            bytes = BitConverter.GetBytes(previousHitId);
                            primaryStream.Write(bytes, 0, bytes.Length);
                            bytes = BitConverter.GetBytes(skipSize);
                            primaryStream.Write(bytes, 0, bytes.Length);
                            bytes = BitConverter.GetBytes(tempPosition - baseSkipPosition);
                            primaryStream.Write(bytes, 0, bytes.Length);
                            primaryStream.Position = tempPosition + skipEntrySize;
                            skipSize = 0;
                        }
                    }
                    tempPosition = primaryStream.Position;
                    primaryStream.Position = skipPosition;
                    skipPosition = tempPosition;
                    bytes = BitConverter.GetBytes(previousHitId);
                    primaryStream.Write(bytes, 0, bytes.Length);
                    bytes = BitConverter.GetBytes(skipSize);
                    primaryStream.Write(bytes, 0, bytes.Length);
                    bytes = BitConverter.GetBytes(SingleStreamSkipListPostingEnumerator<Thit>.noNextSkipPosition);
                    primaryStream.Write(bytes, 0, bytes.Length);
                    primaryStream.Position = tempPosition;
                }
                else
                {
                    int previousHitId = 0;
                    while (postingEnumerator.MoveNext())
                    {
                        int deltaId = postingEnumerator.CurrentPostingId - previousHitId;
                        VariableByteCoding.Write(deltaId, primaryStream);
                        VariableByteCoding.Write(postingEnumerator.CurrentHitCount, primaryStream);
                        memStream.SetLength(0);
                        using (var hitInfoEnumerator = postingEnumerator.GetSpecializedCurrentHitEnumerator())
                        {
                            hitEnumeratorSerialization.Write(hitInfoEnumerator, memStream);
                        }
                        long deltaPointer = memStream.Position;
                        VariableByteCoding.Write(deltaPointer, primaryStream);
                        primaryStream.Write(memStream.GetBuffer(), 0, (int)memStream.Position);
                        previousHitId += deltaId;
                        ++hitCount;
                    }
                }
                tempPosition = primaryStream.Position;
                primaryStream.Position = countPosition;
                VariableByteCoding.Write(hitCount, reservedCountSize, primaryStream);
                primaryStream.Position = tempPosition;
            }
        }

        public IPostingEnumerator Read(int enumeratorId, Stream primaryStream, MemoryMappedFile secondaryMemoryMap)
        {
            return SpecializedRead(enumeratorId, primaryStream, secondaryMemoryMap);
        }

        public IPostingEnumerator<Thit> SpecializedRead(int enumeratorId, Stream primaryStream, MemoryMappedFile secondaryMemoryMap)
        {
            bool hasSkipList = (VariableByteCoding.Read(primaryStream) == 1 ? true : false);
            if (hasSkipList)
            {
                return new SingleStreamSkipListPostingEnumerator<Thit>(enumeratorId, primaryStream, hitEnumeratorSerialization);
            }
            else
            {
                return new SingleStreamPostingEnumerator<Thit>(enumeratorId, primaryStream, hitEnumeratorSerialization);
            }
        }
    }
}
