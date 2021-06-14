///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.exec.@base;
using com.espertech.esper.common.@internal.epl.join.rep;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.exec.outer
{
    /// <summary>
    ///     Execution for a lookup instruction to look up in one or more event streams with a supplied event
    ///     and using a given set of lookup strategies, and adding any lookup results to a lighweight repository object
    ///     for later result assembly.
    /// </summary>
    public class LookupInstructionExec
    {
        private readonly string _fromStreamName;
        private readonly JoinExecTableLookupStrategy[] _lookupStrategies;

        private readonly int _numSubStreams;
        private readonly int[] _optionalSubStreams;
        private readonly int[] _requiredSubStreams;
        private readonly ICollection<EventBean>[] _resultPerStream;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="fromStream">the stream supplying the lookup event</param>
        /// <param name="fromStreamName">the stream name supplying the lookup event</param>
        /// <param name="toStreams">the set of streams to look up in</param>
        /// <param name="lookupStrategies">the strategy to use for each stream to look up in</param>
        /// <param name="requiredPerStream">indicates which of the lookup streams are required to build a result and which are not</param>
        public LookupInstructionExec(
            int fromStream,
            string fromStreamName,
            int[] toStreams,
            JoinExecTableLookupStrategy[] lookupStrategies,
            bool[] requiredPerStream)
        {
            if (toStreams.Length != lookupStrategies.Length) {
                throw new ArgumentException("Invalid number of strategies for each stream");
            }

            if (requiredPerStream.Length < lookupStrategies.Length) {
                throw new ArgumentException("Invalid required per stream array");
            }

            if (fromStream < 0 || fromStream >= requiredPerStream.Length) {
                throw new ArgumentException("Invalid from stream");
            }

            FromStream = fromStream;
            this._fromStreamName = fromStreamName;
            _numSubStreams = toStreams.Length;
            this._lookupStrategies = lookupStrategies;

            _resultPerStream = new ISet<EventBean>[_numSubStreams];

            // Build a separate array for the required and for the optional streams
            var required = new LinkedList<int>();
            var optional = new LinkedList<int>();
            foreach (var stream in toStreams) {
                if (requiredPerStream[stream]) {
                    required.AddLast(stream);
                }
                else {
                    optional.AddLast(stream);
                }
            }

            _requiredSubStreams = required.ToArray();
            _optionalSubStreams = optional.ToArray();
            HasRequiredStream = _requiredSubStreams.Length > 0;
        }

        /// <summary>
        ///     Returns the stream number of the stream supplying the event to use for lookup.
        /// </summary>
        /// <returns>stream number</returns>
        public int FromStream { get; }

        /// <summary>
        ///     Returns true if there is one or more required substreams or false if no substreams are required joins.
        /// </summary>
        /// <value>true if any substreams are required (inner) joins, or false if not</value>
        public bool HasRequiredStream { get; }

        /// <summary>
        ///     Execute the instruction adding results to the repository and obtaining events for lookup from the
        ///     repository.
        /// </summary>
        /// <param name="repository">supplies events for lookup, and place to add results to</param>
        /// <param name="exprEvaluatorContext">expression evaluation context</param>
        /// <returns>true if one or more results, false if no results</returns>
        public bool Process(
            Repository repository,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var hasOneResultRow = false;
            var enumerator = repository.GetCursors(FromStream);

            // Loop over all events for that stream
            while (enumerator.MoveNext()) {
                var cursor = enumerator.Current;
                var lookupEvent = cursor.TheEvent;
                var streamCount = 0;

                // For that event, lookup in all required streams
                while (streamCount < _requiredSubStreams.Length) {
                    var lookupResult = _lookupStrategies[streamCount]
                        .Lookup(lookupEvent, cursor, exprEvaluatorContext);

                    // There is no result, break if this is a required stream
                    if (lookupResult == null || lookupResult.IsEmpty()) {
                        break;
                    }

                    _resultPerStream[streamCount] = lookupResult;
                    streamCount++;
                }

                // No results for a required stream, we are done with this event
                if (streamCount < _requiredSubStreams.Length) {
                    continue;
                }

                // Add results to repository
                for (var i = 0; i < _requiredSubStreams.Length; i++) {
                    hasOneResultRow = true;
                    repository.AddResult(cursor, _resultPerStream[i], _requiredSubStreams[i]);
                }

                // For that event, lookup in all optional streams
                for (var i = 0; i < _optionalSubStreams.Length; i++) {
                    var lookupResult = _lookupStrategies[streamCount].Lookup(lookupEvent, cursor, exprEvaluatorContext);

                    if (lookupResult != null) {
                        hasOneResultRow = true;
                        repository.AddResult(cursor, lookupResult, _optionalSubStreams[i]);
                    }

                    streamCount++;
                }
            }

            return hasOneResultRow;
        }

        private static int[] ToArray(IList<int> list)
        {
            var arr = new int[list.Count];
            var count = 0;
            foreach (var value in list) {
                arr[count++] = value;
            }

            return arr;
        }

        /// <summary>
        ///     Output the instruction.
        /// </summary>
        /// <param name="writer">is the write to output to</param>
        public void Print(IndentWriter writer)
        {
            writer.WriteLine(
                "LookupInstructionExec" +
                " fromStream=" +
                FromStream +
                " fromStreamName=" +
                _fromStreamName +
                " numSubStreams=" +
                _numSubStreams +
                " requiredSubStreams=" +
                _requiredSubStreams.RenderAny() +
                " optionalSubStreams=" +
                _optionalSubStreams.RenderAny());

            writer.IncrIndent();
            for (var i = 0; i < _lookupStrategies.Length; i++) {
                writer.WriteLine("lookupStrategies[" + i + "] : " + _lookupStrategies[i]);
            }

            writer.DecrIndent();
        }
    }
} // end of namespace