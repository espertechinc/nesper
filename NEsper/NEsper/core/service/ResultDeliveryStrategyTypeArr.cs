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
    /// A result delivery strategy that uses an "Update" method that accepts a underlying 
    /// array for use in wildcard selection.
    /// </summary>
    public class ResultDeliveryStrategyTypeArr : ResultDeliveryStrategy
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal readonly EPStatement _statement;
        internal readonly Object _subscriber;
        internal readonly FastMethod _fastMethod;
        internal readonly Type _componentType;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="statement">the statement.</param>
        /// <param name="subscriber">is the receiver to method invocations</param>
        /// <param name="method">is the method to deliver to</param>
        /// <param name="componentType">Type of the component.</param>
        /// <param name="engineImportService">The engine import service.</param>
        public ResultDeliveryStrategyTypeArr(EPStatement statement, Object subscriber, MethodInfo method, Type componentType, EngineImportService engineImportService)
        {
            _statement = statement;
            _subscriber = subscriber;
            _fastMethod = FastClass.CreateMethod(method);
            _componentType = componentType;
        }

        public virtual void Execute(UniformPair<EventBean[]> result)
        {
            Object newData;
            Object oldData;

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

            var paramList = new[] { newData, oldData };
            try
            {
                _fastMethod.Invoke(_subscriber, paramList);
            }
            catch (TargetInvocationException e)
            {
                ResultDeliveryStrategyImpl.Handle(_statement.Name, Log, e, paramList, _subscriber, _fastMethod);
            }
        }

        internal Object Convert(EventBean[] events)
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
                    var natural = (NaturalEventBean)events[i];
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
