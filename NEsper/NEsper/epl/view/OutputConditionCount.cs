///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.variable;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.view
{
    /// <summary>
    /// Output limit condition that is satisfied when either the total number of new events arrived or the total number of old events arrived is greater than a preset value.
    /// </summary>
    public sealed class OutputConditionCount
        : OutputConditionBase
        , OutputCondition
    {
        private const bool DO_OUTPUT = true;
        private const bool FORCE_UPDATE = false;

        private long _eventRate;
        private int _newEventsCount;
        private int _oldEventsCount;

        private VariableReader _variableReader;

        public OutputConditionCount(OutputCallback outputCallback, long eventRate, VariableReader variableReader)
            : base(outputCallback)
        {
            _eventRate = eventRate;
            _variableReader = variableReader;
        }

        /// <summary>Returns the number of new events. </summary>
        /// <value>number of new events</value>
        public int NewEventsCount => _newEventsCount;

        /// <summary>Returns the number of old events. </summary>
        /// <value>number of old events</value>
        public int OldEventsCount => _oldEventsCount;

        public override void UpdateOutputCondition(int newDataCount, int oldDataCount)
        {
            if (_variableReader != null)
            {
                var value = _variableReader.Value;
                if (value != null)
                {
                    _eventRate = value.AsLong();
                }
            }

            _newEventsCount += newDataCount;
            _oldEventsCount += oldDataCount;

            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".updateBatchCondition, " +
                        "  newEventsCount==" + _newEventsCount +
                        "  oldEventsCount==" + _oldEventsCount);
            }

            if (IsSatisfied)
            {
                if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
                {
                    Log.Debug(".UpdateOutputCondition() condition satisfied");
                }
                _newEventsCount = 0;
                _oldEventsCount = 0;
                OutputCallback.Invoke(DO_OUTPUT, FORCE_UPDATE);
            }
        }

        public override String ToString()
        {
            return GetType().FullName +
                    " eventRate=" + _eventRate;
        }

        private bool IsSatisfied => (_newEventsCount >= _eventRate) || (_oldEventsCount >= _eventRate);

        public override void Terminated()
        {
            OutputCallback.Invoke(true, true);
        }

        public override void Stop()
        {
            // no action required
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}