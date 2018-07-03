///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.epl.core;

using XLR8.CGLib;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.logging;
using com.espertech.esper.events;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// A result delivery strategy that uses an "Update" method that accepts a pair of object array array.
    /// </summary>
    public class ResultDeliveryStrategyObjectArr : ResultDeliveryStrategy
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal readonly EPStatement _statement;
        internal readonly Object _subscriber;
        internal readonly FastMethod _fastMethod;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="statement">the statement.</param>
        /// <param name="subscriber">is the subscriber to deliver to</param>
        /// <param name="method">the method to invoke</param>
        /// <param name="engineImportService">The engine import service.</param>
        public ResultDeliveryStrategyObjectArr(EPStatement statement, Object subscriber, MethodInfo method, EngineImportService engineImportService)
        {
            _statement = statement;
            _subscriber = subscriber;
            _fastMethod = FastClass.CreateMethod(method);
        }

        public virtual void Execute(UniformPair<EventBean[]> result)
        {
            Object[][] newData;
            Object[][] oldData;

            if (result == null)
            {
                newData = null;
                oldData = null;
            }
            else
            {
                newData = Convert(result.First);
                oldData = Convert(result.Second);
            }

            var paramList = new Object[] { newData, oldData };
            try
            {
                _fastMethod.Invoke(_subscriber, paramList);
            }
            catch (TargetInvocationException e)
            {
                ResultDeliveryStrategyImpl.Handle(_statement.Name, Log, e, paramList, _subscriber, _fastMethod);
            }
        }

        internal Object[][] Convert(EventBean[] events)
        {
            if ((events == null) || (events.Length == 0))
            {
                return null;
            }

            var result = new Object[events.Length][];
            var length = 0;
            for (var i = 0; i < result.Length; i++)
            {
                if (events[i] is NaturalEventBean)
                {
                    var natural = (NaturalEventBean)events[i];
                    result[length] = natural.Natural;
                    length++;
                }
            }

            if (length == 0)
            {
                return null;
            }
            if (length != events.Length)
            {
                var reduced = new Object[length][];
                Array.Copy(result, 0, reduced, 0, length);
                result = reduced;
            }
            return result;
        }
    }
}
