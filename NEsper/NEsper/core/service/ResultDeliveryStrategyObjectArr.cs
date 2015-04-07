///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
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

        private readonly String _statementName;
        private readonly Object _subscriber;
        private readonly FastMethod _fastMethod;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="statementName">Name of the statement.</param>
        /// <param name="subscriber">is the subscriber to deliver to</param>
        /// <param name="method">the method to invoke</param>
        public ResultDeliveryStrategyObjectArr(String statementName, Object subscriber, MethodInfo method)
        {
            _statementName = statementName;
            _subscriber = subscriber;
            FastClass fastClass = FastClass.Create(subscriber.GetType());
            _fastMethod = fastClass.GetMethod(method);
        }
    
        public void Execute(UniformPair<EventBean[]> result)
        {
            Object[][] newData;
            Object[][] oldData;
    
            if (result == null) {
                newData = null;
                oldData = null;
            }
            else {
                newData = Convert(result.First);
                oldData = Convert(result.Second);
            }
    
            Object[] paramList = new Object[] {newData, oldData};
            try {
                _fastMethod.Invoke(_subscriber, paramList);
            }
            catch (TargetInvocationException e) {
                ResultDeliveryStrategyImpl.Handle(_statementName, Log, e, paramList, _subscriber, _fastMethod);
            }
        }
    
        private Object[][] Convert(EventBean[] events)
        {
            if ((events == null) || (events.Length == 0))
            {
                return null;
            }
    
            Object[][] result = new Object[events.Length][];
            int length = 0;
            for (int i = 0; i < result.Length; i++)
            {
                if (events[i] is NaturalEventBean)
                {
                    NaturalEventBean natural = (NaturalEventBean) events[i];
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
                Object[][] reduced = new Object[length][];
                Array.Copy(result, 0, reduced, 0, length);
                result = reduced;
            }
            return result;
        }
    }
}
