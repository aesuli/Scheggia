// Copyright (C) 2016 Andrea Esuli (andrea@esuli.it)
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

namespace Esuli.Base.IO.Storage
{
    using System;

    /// <summary>
    /// Creates a storage by sequentially adding objects.
    /// </summary>
    /// <typeparam name="Tobject">The object to added to the storage</typeparam>
    /// <remarks>Returns the id to be used by the relative <see cref="IReadableStorage"/>.</remarks>
    public interface ISequentiallyWriteableStorage<Tobject> : IDisposable
    {
        int Write(Tobject obj);

        int Count
        {
            get;
        }
    }
}
