///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    public sealed class OutputConditionPolledCount : OutputConditionPolled
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private long _eventRate;
        private int _newEventsCount;
        private int _oldEventsCount;
        private readonly VariableReader _variableReader;
        private bool _isFirst = true;

        /// <summary>Constructor. </summary>
        /// <param name="eventRate">is the number of old or new events thatmust arrive in order for the condition to be satisfied </param>
        /// <param name="variableReader">is for reading the variable value, if a variable was supplied, else null</param>
        public OutputConditionPolledCount(int eventRate, VariableReader variableReader)
        {
            if ((eventRate < 1) && (variableReader == null))
            {
                throw new ArgumentException("Limiting output by event count requires an event count of at least 1 or a variable name");
            }
            _eventRate = eventRate;
            _variableReader = variableReader;
            _newEventsCount = eventRate;
            _oldEventsCount = eventRate;
        }

        public bool UpdateOutputCondition(int newDataCount, int oldDataCount)
        {
            if (_variableReader != null)
            {
                Object value = _variableReader.Value;
                if (value != null) {
                    _eventRate = value.AsLong();
                }
            }

            _newEventsCount += newDataCount;
            _oldEventsCount += oldDataCount;

            if (IsSatisfied() || _isFirst)
            {
                if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
                {
                    Log.Debug(".UpdateOutputCondition() condition satisfied");
                }
                _isFirst = false;
                _newEventsCount = 0;
                _oldEventsCount = 0;
                return true;
            }
            return false;
        }

        public override String ToString()
        {
            return string.Format("{0} eventRate={1}", GetType().Name, _eventRate);
        }

        private bool IsSatisfied()
        {
            return (_newEventsCount >= _eventRate) || (_oldEventsCount >= _eventRate);
        }
    }
}
