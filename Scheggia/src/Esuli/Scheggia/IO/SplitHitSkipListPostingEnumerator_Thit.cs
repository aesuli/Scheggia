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
    using Esuli.Scheggia.Scoring;
    using System.IO.MemoryMappedFiles;

    public class SplitHitSkipListPostingEnumerator<Thit> : IPostingEnumerator<Thit>
    {
        public static readonly long noNextSkipPosition = -1;

        private Stream primaryStream;
        private MemoryMappedFile secondaryMemoryMap;
        private int hitCount;
        private int progress;
        private int currentPostingId;
        private int currentHitCount;
        private long currentHitPointer;
        private long currentHitSize;
        private int enumeratorId;
        private ScoreFunction scoreFunction;
        private IHitEnumeratorSerialization<Thit> hitEnumeratorSerialization;
        private int nextSkipId;
        private long nextSkipPosition;
        private int skipSize;
        private int alreadySkippedSize;
        private byte[] buffer;

        public SplitHitSkipListPostingEnumerator(int enumeratorId, Stream primaryStream, MemoryMappedFile secondaryMemoryMap, IHitEnumeratorSerialization<Thit> hitEnumeratorSerialization)
        {
            this.hitEnumeratorSerialization = hitEnumeratorSerialization;
            this.primaryStream = primaryStream;
            this.secondaryMemoryMap = secondaryMemoryMap;
            this.enumeratorId = enumeratorId;
            hitCount = (int)VariableByteCoding.Read(primaryStream);
            progress = 0;
            currentPostingId = 0;
            currentHitCount = 0;
            currentHitPointer = 0;
            scoreFunction = ScoreFunctions.TF(this);
            buffer = new byte[sizeof(int) * 2 + sizeof(long) * 2];
            primaryStream.Read(buffer, 0, sizeof(int) * 2 + sizeof(long) * 2);
            nextSkipId = BitConverter.ToInt32(buffer, 0);
            skipSize = BitConverter.ToInt32(buffer, sizeof(int));
            alreadySkippedSize = 0;
            currentHitPointer = BitConverter.ToInt64(buffer, sizeof(int) * 2);
            currentHitSize = 0;
            nextSkipPosition = BitConverter.ToInt64(buffer, sizeof(int) * 2 + sizeof(long));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                primaryStream.Dispose();
            }
        }

        public ScoreFunction ScoreFunction
        {
            get
            {
                return scoreFunction;
            }
            set
            {
                scoreFunction = value;
            }
        }

        public int Count
        {
            get
            {
                return hitCount;
            }
        }

        public int Progress
        {
            get
            {
                return progress;
            }
        }

        public int CurrentPostingId
        {
            get
            {
                return currentPostingId;
            }
        }

        public int CurrentHitCount
        {
            get
            {
                return currentHitCount;
            }
        }

        public bool MoveNext()
        {
            if (progress >= hitCount)
            {
                return false;
            }

            if (primaryStream.Position == nextSkipPosition)
            {
                currentPostingId = nextSkipId;
                primaryStream.Read(buffer, 0, sizeof(int) * 2 + sizeof(long) * 2);
                nextSkipId = BitConverter.ToInt32(buffer, 0);
                skipSize = BitConverter.ToInt32(buffer, sizeof(int));
                alreadySkippedSize = 0;
                currentHitPointer = BitConverter.ToInt64(buffer, sizeof(int) * 2);
                currentHitSize = 0;
                nextSkipPosition = BitConverter.ToInt64(buffer, sizeof(int) * 2 + sizeof(long));
            }

            currentPostingId += (int)VariableByteCoding.Read(primaryStream);
            currentHitCount = (int)VariableByteCoding.Read(primaryStream);
            currentHitPointer += currentHitSize;
            currentHitSize = VariableByteCoding.Read(primaryStream);
            ++alreadySkippedSize;
            ++progress;
            return true;
        }

        public bool MoveNext(int minPostingId)
        {
            if (currentPostingId >= minPostingId && progress > 0)
            {
                return true;
            }

            while (nextSkipId < minPostingId)
            {
                if (nextSkipPosition == noNextSkipPosition)
                {
                    return false;
                }
                progress += skipSize - alreadySkippedSize;
                currentPostingId = nextSkipId;
                primaryStream.Position = nextSkipPosition;
                primaryStream.Read(buffer, 0, sizeof(int) * 2 + sizeof(long) * 2);
                nextSkipId = BitConverter.ToInt32(buffer, 0);
                skipSize = BitConverter.ToInt32(buffer, sizeof(int));
                alreadySkippedSize = 0;
                currentHitPointer = BitConverter.ToInt64(buffer, sizeof(int) * 2);
                currentHitSize = 0;
                nextSkipPosition = BitConverter.ToInt64(buffer, sizeof(int) * 2 + sizeof(long));
            }

            do
            {
                if (!MoveNext())
                {
                    return false;
                }
            }
            while (currentPostingId < minPostingId);
            return true;
        }

        public IHitEnumerator GetCurrentHitEnumerator()
        {
            return GetSpecializedCurrentHitEnumerator();
        }

        public IHitEnumerator<Thit> GetSpecializedCurrentHitEnumerator()
        {
            var secondaryStream = secondaryMemoryMap.CreateViewStream(currentHitPointer, currentHitSize);
            return hitEnumeratorSerialization.Read(enumeratorId, currentHitCount, secondaryStream);
        }
    }
}
