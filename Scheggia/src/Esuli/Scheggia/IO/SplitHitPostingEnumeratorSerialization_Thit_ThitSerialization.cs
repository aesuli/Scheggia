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

    public class SplitHitPostingEnumeratorSerialization<Thit, ThitEnumerationSerialization> 
        : IPostingEnumeratorSerialization<Thit>
        where ThitEnumerationSerialization : IHitEnumeratorSerialization<Thit>,new()
    {
        //TODO use a more flexible check for the use of skiplists
        private static readonly int minSkipListJump = 512;
			
        private IHitEnumeratorSerialization<Thit> hitEnumeratorSerialization;

        public SplitHitPostingEnumeratorSerialization()
        {
            this.hitEnumeratorSerialization = new ThitEnumerationSerialization();
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
			
			int hitCount = 0;
			if(hasSkipList)
			{
				byte [] bytes;
				int skipEntrySize = sizeof(int)*2+sizeof(long)*2;
				long skipPosition = primaryStream.Position;
				primaryStream.Position += skipEntrySize;

				int skipSize = 0;
	            int previousPostingId = 0;
	            long previousHitPointer = secondaryStream.Position;
                long skipHitPointer = previousHitPointer;
	            while(postingEnumerator.MoveNext())
	            {
	                int deltaId = postingEnumerator.CurrentPostingId - previousPostingId;
                    VariableByteCoding.Write(deltaId, primaryStream);
	                VariableByteCoding.Write(postingEnumerator.CurrentHitCount, primaryStream);
                    using (var hitEnumerator = postingEnumerator.GetSpecializedCurrentHitEnumerator())
                    {
                        hitEnumeratorSerialization.Write(hitEnumerator, secondaryStream);
                    }
                    long deltaPointer = secondaryStream.Position - previousHitPointer;
                    VariableByteCoding.Write(deltaPointer, primaryStream);
                    previousHitPointer += deltaPointer;
	                previousPostingId += deltaId;
	                ++hitCount;
					++skipSize;
                    if (hitCount % skipListJump == 0)
                    {
                        tempPosition = primaryStream.Position;
                        primaryStream.Position = skipPosition;
                        skipPosition = tempPosition;
                        bytes = BitConverter.GetBytes(previousPostingId);
                        primaryStream.Write(bytes, 0, bytes.Length);
                        bytes = BitConverter.GetBytes(skipSize);
                        primaryStream.Write(bytes, 0, bytes.Length);
                        bytes = BitConverter.GetBytes(skipHitPointer);
                        primaryStream.Write(bytes, 0, bytes.Length);
                        bytes = BitConverter.GetBytes(tempPosition - baseSkipPosition);
                        primaryStream.Write(bytes, 0, bytes.Length);
                        primaryStream.Position = tempPosition + skipEntrySize;
                        skipSize = 0;
                        skipHitPointer = previousHitPointer;
                    }
	            }
				tempPosition = primaryStream.Position;
				primaryStream.Position = skipPosition;
				skipPosition = tempPosition;
				bytes = BitConverter.GetBytes(previousPostingId);
				primaryStream.Write(bytes,0,bytes.Length);
				bytes = BitConverter.GetBytes(skipSize);
				primaryStream.Write(bytes,0,bytes.Length);
                bytes = BitConverter.GetBytes(skipHitPointer);
                primaryStream.Write(bytes, 0, bytes.Length);
                bytes = BitConverter.GetBytes(SplitHitSkipListPostingEnumerator<Thit>.noNextSkipPosition);
				primaryStream.Write(bytes,0,bytes.Length);
				primaryStream.Position = tempPosition;
			}
			else 
			{
	            int previousPostingId = 0;
	            long previousHitPointer = secondaryStream.Position;
                VariableByteCoding.Write(previousHitPointer, primaryStream);
                while (postingEnumerator.MoveNext())
	            {
	                int deltaId = postingEnumerator.CurrentPostingId - previousPostingId;
	                VariableByteCoding.Write(deltaId, primaryStream);
	                VariableByteCoding.Write(postingEnumerator.CurrentHitCount, primaryStream);
                    using (var hitInfoEnumerator = postingEnumerator.GetSpecializedCurrentHitEnumerator())
                    {
                        hitEnumeratorSerialization.Write(hitInfoEnumerator, secondaryStream);
                    }
                    long deltaPointer = secondaryStream.Position - previousHitPointer;
                    VariableByteCoding.Write(deltaPointer, primaryStream);
                    previousHitPointer += deltaPointer;
                    previousPostingId += deltaId;
	                ++hitCount;
	            }
			}
            tempPosition = primaryStream.Position;
            primaryStream.Position = countPosition;
            VariableByteCoding.Write(hitCount, reservedCountSize, primaryStream);
            primaryStream.Position = tempPosition;
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
                return new SplitHitSkipListPostingEnumerator<Thit>(enumeratorId, primaryStream, secondaryMemoryMap, hitEnumeratorSerialization);
            }
            else
            {
                return new SplitHitPostingEnumerator<Thit>(enumeratorId, primaryStream, secondaryMemoryMap, hitEnumeratorSerialization);
            }
        }
    }
}
