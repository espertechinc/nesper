///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.epl.variable;

namespace com.espertech.esper.epl.core
{
    /// <summary>An limit-processor for use with "limit" and "offset".</summary>
    public class RowLimitProcessor
    {
        private readonly VariableReader _numRowsVariableReader;
        private readonly VariableReader _offsetVariableReader;
        private int _currentRowLimit;
        private int _currentOffset;

        public RowLimitProcessor(
            VariableReader numRowsVariableReader,
            VariableReader offsetVariableReader,
            int currentRowLimit,
            int currentOffset)
        {
            _numRowsVariableReader = numRowsVariableReader;
            _offsetVariableReader = offsetVariableReader;
            _currentRowLimit = currentRowLimit;
            _currentOffset = currentOffset;
        }

        public int CurrentRowLimit
        {
            get { return _currentRowLimit; }
        }

        public int CurrentOffset
        {
            get { return _currentOffset; }
        }

        /// <summary>
        /// Determine the current limit and applies the limiting function to outgoing events.
        /// </summary>
        /// <param name="outgoingEvents">unlimited</param>
        /// <returns>limited</returns>
        internal EventBean[] DetermineLimitAndApply(EventBean[] outgoingEvents)
        {
            if (outgoingEvents == null)
            {
                return null;
            }
            DetermineCurrentLimit();
            return ApplyLimit(outgoingEvents);
        }

        internal void DetermineCurrentLimit()
        {
            if (_numRowsVariableReader != null)
            {
                var varValue = _numRowsVariableReader.Value;
                if (varValue != null)
                {
                    _currentRowLimit = varValue.AsInt();
                }
                else
                {
                    _currentRowLimit = Int32.MaxValue;
                }
                if (_currentRowLimit < 0)
                {
                    _currentRowLimit = Int32.MaxValue;
                }
            }

            if (_offsetVariableReader != null)
            {
                var varValue = _offsetVariableReader.Value;
                if (varValue != null)
                {
                    _currentOffset = varValue.AsInt();
                }
                else
                {
                    _currentOffset = 0;
                }
                if (_currentOffset < 0)
                {
                    _currentOffset = 0;
                }
            }
        }

        internal EventBean[] ApplyLimit(EventBean[] outgoingEvents)
        {
            // no offset
            if (_currentOffset == 0)
            {
                if (outgoingEvents.Length <= _currentRowLimit)
                {
                    return outgoingEvents;
                }

                if (_currentRowLimit == 0)
                {
                    return null;
                }

                var limited = new EventBean[_currentRowLimit];
                Array.Copy(outgoingEvents, 0, limited, 0, _currentRowLimit);
                return limited;
            }
            else
            {
                // with offset
                int maxInterested = _currentRowLimit + _currentOffset;
                if (_currentRowLimit == Int32.MaxValue)
                {
                    maxInterested = Int32.MaxValue;
                }

                // more rows then requested
                if (outgoingEvents.Length > maxInterested)
                {
                    var limitedX = new EventBean[_currentRowLimit];
                    Array.Copy(outgoingEvents, _currentOffset, limitedX, 0, _currentRowLimit);
                    return limitedX;
                }

                // less or equal rows to offset
                if (outgoingEvents.Length <= _currentOffset)
                {
                    return null;
                }

                int size = outgoingEvents.Length - _currentOffset;
                var limited = new EventBean[size];
                Array.Copy(outgoingEvents, _currentOffset, limited, 0, size);
                return limited;
            }
        }
    }
} // end of namespace
