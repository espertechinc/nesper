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
    /// A result delivery strategy that uses an "Update" method that accepts a underlying 
    /// array for use in wildcard selection.
    /// </summary>
    public class ResultDeliveryStrategyTypeArr : ResultDeliveryStrategy
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly String _statementName;
        private readonly Object _subscriber;
        private readonly FastMethod _fastMethod;
        private readonly Type _componentType;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="statementName">Name of the statement.</param>
        /// <param name="subscriber">is the receiver to method invocations</param>
        /// <param name="method">is the method to deliver to</param>
        public ResultDeliveryStrategyTypeArr(String statementName, Object subscriber, MethodInfo method)
        {
            _statementName = statementName;
            _subscriber = subscriber;
            FastClass fastClass = FastClass.Create(subscriber.GetType());
            _fastMethod = fastClass.GetMethod(method);
            _componentType = method.GetParameters()[0].ParameterType.GetElementType();
        }
    
        public void Execute(UniformPair<EventBean[]> result)
        {
            Object newData;
            Object oldData;
    
            if (result == null) {
                newData = null;
                oldData = null;
            }
            else {
                newData = Convert(result.First);
                oldData = Convert(result.Second);
            }
    
            var paramList = new[] {newData, oldData};
            try {
                _fastMethod.Invoke(_subscriber, paramList);
            }
            catch (TargetInvocationException e) {
                ResultDeliveryStrategyImpl.Handle(_statementName, Log, e, paramList, _subscriber, _fastMethod);
            }
        }
    
        private Object Convert(EventBean[] events)
        {
            if ((events == null) || (events.Length == 0))
            {
                return null;
            }
    
            Array array = Array.CreateInstance(_componentType, events.Length);
            int length = 0;
            for (int i = 0; i < events.Length; i++)
            {
                if (events[i] is NaturalEventBean)
                {
                    var natural = (NaturalEventBean) events[i];
                    array.SetValue(natural.Natural[0], length);
                    length++;
                }
            }
    
            if (length == 0)
            {
                return null;
            }
            if (length != events.Length)
            {
                Array reduced = Array.CreateInstance(_componentType, events.Length);
                Array.Copy(array, 0, reduced, 0, length);
                array = reduced;
            }
            return array;
        }
    }
}
