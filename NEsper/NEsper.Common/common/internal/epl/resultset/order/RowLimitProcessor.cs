///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.resultset.order
{
    /// <summary>
    ///     An limit-processor for use with "limit" and "offset".
    /// </summary>
    public class RowLimitProcessor
    {
        private readonly VariableReader numRowsVariableReader;
        private readonly VariableReader offsetVariableReader;

        public RowLimitProcessor(
            VariableReader numRowsVariableReader, VariableReader offsetVariableReader, int currentRowLimit,
            int currentOffset)
        {
            this.numRowsVariableReader = numRowsVariableReader;
            this.offsetVariableReader = offsetVariableReader;
            CurrentRowLimit = currentRowLimit;
            CurrentOffset = currentOffset;
        }

        public int CurrentRowLimit { get; private set; }

        public int CurrentOffset { get; private set; }

        public void DetermineCurrentLimit()
        {
            if (numRowsVariableReader != null)
            {
                var varValue = numRowsVariableReader.Value;
                if (varValue != null)
                {
                    CurrentRowLimit = varValue.AsInt();
                }
                else
                {
                    CurrentRowLimit = int.MaxValue;
                }

                if (CurrentRowLimit < 0)
                {
                    CurrentRowLimit = int.MaxValue;
                }
            }

            if (offsetVariableReader != null)
            {
                var varValue = offsetVariableReader.Value;
                if (varValue != null)
                {
                    CurrentOffset = varValue.AsInt();
                }
                else
                {
                    CurrentOffset = 0;
                }

                if (CurrentOffset < 0)
                {
                    CurrentOffset = 0;
                }
            }
        }

        public EventBean[] ApplyLimit(EventBean[] outgoingEvents)
        {
            // no offset
            if (CurrentOffset == 0)
            {
                if (outgoingEvents.Length <= CurrentRowLimit)
                {
                    return outgoingEvents;
                }

                if (CurrentRowLimit == 0)
                {
                    return null;
                }

                var limited = new EventBean[CurrentRowLimit];
                Array.Copy(outgoingEvents, 0, limited, 0, CurrentRowLimit);
                return limited;
            }
            else
            {
                // with offset
                var maxInterested = CurrentRowLimit + CurrentOffset;
                if (CurrentRowLimit == int.MaxValue)
                {
                    maxInterested = int.MaxValue;
                }

                // more rows then requested
                if (outgoingEvents.Length > maxInterested)
                {
                    var limitedX = new EventBean[CurrentRowLimit];
                    Array.Copy(outgoingEvents, CurrentOffset, limitedX, 0, CurrentRowLimit);
                    return limitedX;
                }

                // less or equal rows to offset
                if (outgoingEvents.Length <= CurrentOffset)
                {
                    return null;
                }

                var size = outgoingEvents.Length - CurrentOffset;
                var limited = new EventBean[size];
                Array.Copy(outgoingEvents, CurrentOffset, limited, 0, size);
                return limited;
            }
        }

        /// <summary>
        ///     Determine the current limit and applies the limiting function to outgoing events.
        /// </summary>
        /// <param name="outgoingEvents">unlimited</param>
        /// <returns>limited</returns>
        public EventBean[] DetermineLimitAndApply(EventBean[] outgoingEvents)
        {
            if (outgoingEvents == null)
            {
                return null;
            }

            DetermineCurrentLimit();
            return ApplyLimit(outgoingEvents);
        }

        public EventBean[] DetermineApplyLimit2Events(EventBean first, EventBean second)
        {
            DetermineCurrentLimit();
            if (CurrentRowLimit == 0)
            {
                return null;
            }

            if (CurrentRowLimit == 1)
            {
                return new[] { first };
            }

            return new[] { first, second };
        }
    }
} // end of namespace