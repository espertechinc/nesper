///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.output.condition
{
    /// <summary>
    ///     Output limit condition that is satisfied when either
    ///     the total number of new events arrived or the total number
    ///     of old events arrived is greater than a preset value.
    /// </summary>
    public sealed class OutputConditionCount : OutputConditionBase,
        OutputCondition
    {
        private const bool DO_OUTPUT = true;
        private const bool FORCE_UPDATE = false;

        private long eventRate;
        private readonly VariableReader variableReader;

        public OutputConditionCount(
            OutputCallback outputCallback,
            long eventRate,
            VariableReader variableReader)
            : base(outputCallback)
        {
            this.eventRate = eventRate;
            this.variableReader = variableReader;
        }

        /// <summary>
        ///     Returns the number of new events.
        /// </summary>
        /// <returns>number of new events</returns>
        public int NewEventsCount { get; private set; }

        /// <summary>
        ///     Returns the number of old events.
        /// </summary>
        /// <returns>number of old events</returns>
        public int OldEventsCount { get; private set; }

        public override void UpdateOutputCondition(
            int newDataCount,
            int oldDataCount)
        {
            if (variableReader != null) {
                var value = variableReader.Value;
                if (value != null) {
                    eventRate = value.AsLong();
                }
            }

            NewEventsCount += newDataCount;
            OldEventsCount += oldDataCount;

            if (IsSatisfied) {
                NewEventsCount = 0;
                OldEventsCount = 0;
                outputCallback.Invoke(DO_OUTPUT, FORCE_UPDATE);
            }
        }

        public override void Terminated()
        {
            outputCallback.Invoke(true, true);
        }

        public override void StopOutputCondition()
        {
            // no action required
        }

        public override string ToString()
        {
            return GetType().Name +
                   " eventRate=" +
                   eventRate;
        }

        private bool IsSatisfied {
            get { return NewEventsCount >= eventRate || OldEventsCount >= eventRate; }
        }
    }
} // end of namespace