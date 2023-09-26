///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.queryplanouter
{
    /// <summary> This class represents outer-join relationships between outer and inner tables.
    /// To add a left outer join between streams 0 and 1 use "Add(0, 1)".
    /// To add a full outer join between streams 0 and 1 use "Add(0, 1)" and "Add(1, 0)".
    /// To add a right outer join between streams 0 and 1 use "Add(1, 0)".
    /// </summary>
    public class OuterInnerDirectionalGraph
    {
        private readonly IDictionary<int, ICollection<int>> _streamToInnerMap;
        private readonly IDictionary<int, ICollection<int>> _unqualifiedNavigableStreams;
        private readonly int _numStreams;

        /// <summary> Ctor.</summary>
        /// <param name="numStreams">number of streams
        /// </param>
        public OuterInnerDirectionalGraph(int numStreams)
        {
            _numStreams = numStreams;
            _streamToInnerMap = new Dictionary<int, ICollection<int>>();
            _unqualifiedNavigableStreams = new Dictionary<int, ICollection<int>>();
        }

        /// <summary> Add an outer-to-inner join stream relationship.</summary>
        /// <param name="outerStream">is the stream number of the outer stream
        /// </param>
        /// <param name="innerStream">is the stream number of the inner stream
        /// </param>
        /// <returns> graph object
        /// </returns>
        public virtual OuterInnerDirectionalGraph Add(
            int outerStream,
            int innerStream)
        {
            CheckArgs(outerStream, innerStream);

            // add set
            var innerSet = _streamToInnerMap.Get(outerStream, null);
            if (innerSet == null) {
                innerSet = new HashSet<int>();
                _streamToInnerMap[outerStream] = innerSet;
            }

            // populate
            if (innerSet.Contains(innerStream)) {
                throw new ArgumentException("Inner stream already in collection");
            }

            innerSet.Add(innerStream);

            return this;
        }

        /// <summary> Returns the set of inner streams for the given outer stream number.</summary>
        /// <param name="outerStream">is the stream number of the outer stream
        /// </param>
        /// <returns> set of inner streams, or null if empty
        /// </returns>
        public ICollection<int> GetInner(int outerStream)
        {
            CheckArgs(outerStream);
            return _streamToInnerMap.Get(outerStream, null);
        }

        /// <summary> Returns the set of outer streams for the given inner stream number.</summary>
        /// <param name="innerStream">is the stream number of the inner stream
        /// </param>
        /// <returns> set of outer streams, or null if empty
        /// </returns>
        public ICollection<int> GetOuter(int innerStream)
        {
            CheckArgs(innerStream);

            ICollection<int> result = new HashSet<int>();
            foreach (var keyValuePair in _streamToInnerMap) {
                var key = keyValuePair.Key;
                var set = keyValuePair.Value;

                if (set.Contains(innerStream)) {
                    result.Add(key);
                }
            }

            if (result.Count == 0) {
                return null;
            }

            return result;
        }

        /// <summary> Returns true if the outer stream has an optional relationship to the inner stream.</summary>
        /// <param name="outerStream">is the stream number of the outer stream
        /// </param>
        /// <param name="innerStream">is the stream number of the inner stream
        /// </param>
        /// <returns> true if outer-inner relationship between streams, false if not
        /// </returns>
        public virtual bool IsInner(
            int outerStream,
            int innerStream)
        {
            CheckArgs(outerStream, innerStream);

            var innerSet = _streamToInnerMap.Get(outerStream, null);
            if (innerSet == null) {
                return false;
            }

            return innerSet.Contains(innerStream);
        }

        /// <summary> Returns true if the inner stream has a relationship to the outer stream.</summary>
        /// <param name="outerStream">is the stream number of the outer stream
        /// </param>
        /// <param name="innerStream">is the stream number of the inner stream
        /// </param>
        /// <returns> true if outer-inner relationship between streams, false if not
        /// </returns>
        public virtual bool IsOuter(
            int outerStream,
            int innerStream)
        {
            CheckArgs(outerStream, innerStream);
            var outerStreams = GetOuter(innerStream);
            if (outerStreams == null) {
                return false;
            }

            return outerStreams.Contains(outerStream);
        }

        /// <summary> Prints out collection.</summary>
        /// <returns> textual output of keys and values
        /// </returns>
        public virtual string Print()
        {
            var buffer = new StringBuilder();
            var delimiter = "";

            foreach (var kvPair in _streamToInnerMap) {
                var set = kvPair.Value;

                buffer.Append(delimiter);
                buffer.Append(kvPair.Key);
                buffer.Append('=');
                buffer.Append(set.ToString());

                delimiter = ", ";
            }

            return buffer.ToString();
        }

        public IDictionary<int, ICollection<int>> UnqualifiedNavigableStreams => _unqualifiedNavigableStreams;

        public void AddUnqualifiedNavigable(
            int streamOne,
            int streamTwo)
        {
            AddUnqualifiedInternal(streamOne, streamTwo);
            AddUnqualifiedInternal(streamTwo, streamOne);
        }

        private void AddUnqualifiedInternal(
            int streamOne,
            int streamTwo)
        {
            var set = _unqualifiedNavigableStreams.Get(streamOne);
            if (set == null) {
                set = new HashSet<int>();
                _unqualifiedNavigableStreams.Put(streamOne, set);
            }

            set.Add(streamTwo);
        }

        private void CheckArgs(int stream)
        {
            if (stream >= _numStreams || stream < 0) {
                throw new ArgumentException("Out of bounds parameter for stream num");
            }
        }

        private void CheckArgs(
            int outerStream,
            int innerStream)
        {
            if (outerStream >= _numStreams ||
                innerStream >= _numStreams ||
                outerStream < 0 ||
                innerStream < 0) {
                throw new ArgumentException("Out of bounds parameter for inner or outer stream num");
            }

            if (outerStream == innerStream) {
                throw new ArgumentException("Unexpected equal stream num for inner and outer stream");
            }
        }
    }
}