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
namespace Esuli.Scheggia.Indexing
{
    using Esuli.Scheggia.Core;

    /// <summary>
    /// An <see cref="IIndexer"/> which requires off-line processing
    /// to produce the <see cref="IIndex"/> of the indexed data.
    /// </summary>
    /// <seealso cref="IIndex"/>
    public interface IOffLineIndexer : IIndexer
    {
        /// <summary>
        /// Starts the final processing of the indexed data and produces the
        /// relative <see cref="IIndex"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The invocation of this method is the final step of the
        /// indexing process.
        /// </para>
        /// <para>
        /// The <see cref="BuildIndex"/> method have to be invoked only once.
        /// </para>
        /// <returns>The number of indexed hits.</returns>
        /// </remarks>
        /// <seealso cref="IIndex"/>
        long BuildIndex();
    }
}
