///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.join.rep;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.join.exec.@base
{
    /// <summary>
    /// Execution for a lookup instruction to look up in one or more event streams with a supplied
    /// event and using a given set of lookup strategies, and adding any lookup results to a lighweight
    /// repository object for later result assembly.
    /// </summary>
    public class LookupInstructionExec
    {
        private readonly int _fromStream;
        private readonly String _fromStreamName;
        private readonly JoinExecTableLookupStrategy[] _lookupStrategies;
    
        private readonly int _numSubStreams;
        private readonly ICollection<EventBean>[] _resultPerStream;
        private readonly int[] _requiredSubStreams;
        private readonly int[] _optionalSubStreams;
        private readonly bool _hasRequiredSubStreams;
    
        /// <summary>Ctor. </summary>
        /// <param name="fromStream">the stream supplying the lookup event</param>
        /// <param name="fromStreamName">the stream name supplying the lookup event</param>
        /// <param name="toStreams">the set of streams to look up in</param>
        /// <param name="lookupStrategies">the strategy to use for each stream to look up in</param>
        /// <param name="requiredPerStream">indicates which of the lookup streams are required to build a result and which are not</param>
        public LookupInstructionExec(int fromStream, String fromStreamName, int[] toStreams, JoinExecTableLookupStrategy[] lookupStrategies, bool[] requiredPerStream)
        {
            if (toStreams.Length != lookupStrategies.Length)
            {
                throw new ArgumentException("Invalid number of strategies for each stream");
            }
            if (requiredPerStream.Length < lookupStrategies.Length)
            {
                throw new ArgumentException("Invalid required per stream array");
            }
            if ((fromStream < 0) || (fromStream >= requiredPerStream.Length))
            {
                throw new ArgumentException("Invalid from stream");
            }
    
            _fromStream = fromStream;
            _fromStreamName = fromStreamName;
            _numSubStreams = toStreams.Length;
            _lookupStrategies = lookupStrategies;
    
            _resultPerStream = new ICollection<EventBean>[_numSubStreams];
    
            // Build a separate array for the required and for the optional streams
            var required = new List<int>();
            var optional = new List<int>();
            foreach (int stream in toStreams)
            {
                if (requiredPerStream[stream])
                {
                    required.Add(stream);
                }
                else
                {
                    optional.Add(stream);
                }
            }
            _requiredSubStreams = required.ToArray();
            _optionalSubStreams = optional.ToArray();
            _hasRequiredSubStreams = _requiredSubStreams.Length > 0;
        }

        /// <summary>Returns the stream number of the stream supplying the event to use for lookup. </summary>
        /// <value>stream number</value>
        public int FromStream
        {
            get { return _fromStream; }
        }

        /// <summary>Returns true if there is one or more required substreams or false if no substreams are required joins. </summary>
        /// <value>true if any substreams are required (inner) joins, or false if not</value>
        public bool HasRequiredStream
        {
            get { return _hasRequiredSubStreams; }
        }

        /// <summary>Execute the instruction adding results to the repository and obtaining events for lookup from the repository. </summary>
        /// <param name="repository">supplies events for lookup, and place to add results to</param>
        /// <param name="exprEvaluatorContext">expression evaluation context</param>
        /// <returns>true if one or more results, false if no results</returns>
        public bool Process(Repository repository, ExprEvaluatorContext exprEvaluatorContext)
        {
            bool hasOneResultRow = false;
            IEnumerator<Cursor> it = repository.GetCursors(_fromStream);
    
            // Loop over all events for that stream
            for (;it.MoveNext();)
            {
                Cursor cursor = it.Current;
                EventBean lookupEvent = cursor.Event;
                int streamCount = 0;
    
                // For that event, lookup in all required streams
                while (streamCount < _requiredSubStreams.Length)
                {
                    ICollection<EventBean> lookupResult = _lookupStrategies[streamCount].Lookup(lookupEvent, cursor, exprEvaluatorContext);
    
                    // There is no result, break if this is a required stream
                    if (lookupResult == null || lookupResult.IsEmpty())
                    {
                        break;
                    }
                    _resultPerStream[streamCount] = lookupResult;
                    streamCount++;
                }
    
                // No results for a required stream, we are done with this event
                if (streamCount < _requiredSubStreams.Length)
                {
                    continue;
                }
                else
                {
                    // Add results to repository
                    for (int i = 0; i < _requiredSubStreams.Length; i++)
                    {
                        hasOneResultRow = true;
                        repository.AddResult(cursor, _resultPerStream[i], _requiredSubStreams[i]);
                    }
                }
    
                // For that event, lookup in all optional streams
                for (int i = 0; i < _optionalSubStreams.Length; i++)
                {
                    ICollection<EventBean> lookupResult = _lookupStrategies[streamCount].Lookup(lookupEvent, cursor, exprEvaluatorContext);
    
                    if (lookupResult != null)
                    {
                        hasOneResultRow = true;
                        repository.AddResult(cursor, lookupResult, _optionalSubStreams[i]);
                    }
                    streamCount++;
                }
            }
    
            return hasOneResultRow;
        }

        /// <summary>Output the instruction. </summary>
        /// <param name="writer">is the write to output to</param>
        public void Print(IndentWriter writer)
        {
            writer.WriteLine("LookupInstructionExec" +
                    " fromStream=" + _fromStream +
                    " fromStreamName=" + _fromStreamName +
                    " numSubStreams=" + _numSubStreams +
                    " requiredSubStreams=" + _requiredSubStreams.Render() +
                    " optionalSubStreams=" + _optionalSubStreams.Render());
    
            writer.IncrIndent();
            for (int i = 0; i < _lookupStrategies.Length; i++)
            {
                writer.WriteLine("lookupStrategies[" + i + "] : " + _lookupStrategies[i]);
            }
            writer.DecrIndent();
        }
    }
}
