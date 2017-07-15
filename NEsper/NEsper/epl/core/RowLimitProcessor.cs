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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.variable;

namespace com.espertech.esper.epl.core
{
    /// <summary>An limit-processor for use with "limit" and "offset".</summary>
    public class RowLimitProcessor {
    
        private readonly VariableReader numRowsVariableReader;
        private readonly VariableReader offsetVariableReader;
        private int currentRowLimit;
        private int currentOffset;
    
        public RowLimitProcessor(VariableReader numRowsVariableReader, VariableReader offsetVariableReader, int currentRowLimit, int currentOffset) {
            this.numRowsVariableReader = numRowsVariableReader;
            this.offsetVariableReader = offsetVariableReader;
            this.currentRowLimit = currentRowLimit;
            this.currentOffset = currentOffset;
        }
    
        public int GetCurrentRowLimit() {
            return currentRowLimit;
        }
    
        public int GetCurrentOffset() {
            return currentOffset;
        }
    
        /// <summary>
        /// Determine the current limit and applies the limiting function to outgoing events.
        /// </summary>
        /// <param name="outgoingEvents">unlimited</param>
        /// <returns>limited</returns>
        protected EventBean[] DetermineLimitAndApply(EventBean[] outgoingEvents) {
            if (outgoingEvents == null) {
                return null;
            }
            DetermineCurrentLimit();
            return ApplyLimit(outgoingEvents);
        }
    
        protected void DetermineCurrentLimit() {
            if (numRowsVariableReader != null) {
                Number varValue = (Number) numRowsVariableReader.Value;
                if (varValue != null) {
                    currentRowLimit = varValue.IntValue();
                } else {
                    currentRowLimit = Int32.MaxValue;
                }
                if (currentRowLimit < 0) {
                    currentRowLimit = Int32.MaxValue;
                }
            }
    
            if (offsetVariableReader != null) {
                Number varValue = (Number) offsetVariableReader.Value;
                if (varValue != null) {
                    currentOffset = varValue.IntValue();
                } else {
                    currentOffset = 0;
                }
                if (currentOffset < 0) {
                    currentOffset = 0;
                }
            }
        }
    
        protected EventBean[] ApplyLimit(EventBean[] outgoingEvents) {
    
            // no offset
            if (currentOffset == 0) {
                if (outgoingEvents.Length <= currentRowLimit) {
                    return outgoingEvents;
                }
    
                if (currentRowLimit == 0) {
                    return null;
                }
    
                var limited = new EventBean[currentRowLimit];
                Array.Copy(outgoingEvents, 0, limited, 0, currentRowLimit);
                return limited;
            } else {
                // with offset
                int maxInterested = currentRowLimit + currentOffset;
                if (currentRowLimit == Int32.MaxValue) {
                    maxInterested = Int32.MaxValue;
                }
    
                // more rows then requested
                if (outgoingEvents.Length > maxInterested) {
                    var limited = new EventBean[currentRowLimit];
                    Array.Copy(outgoingEvents, currentOffset, limited, 0, currentRowLimit);
                    return limited;
                }
    
                // less or equal rows to offset
                if (outgoingEvents.Length <= currentOffset) {
                    return null;
                }
    
                int size = outgoingEvents.Length - currentOffset;
                var limited = new EventBean[size];
                Array.Copy(outgoingEvents, currentOffset, limited, 0, size);
                return limited;
            }
        }
    }
} // end of namespace
