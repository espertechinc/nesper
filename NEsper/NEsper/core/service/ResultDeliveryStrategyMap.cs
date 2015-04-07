///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using XLR8.CGLib;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.events;

namespace com.espertech.esper.core.service
{
    using DataMap = IDictionary<String, Object>;

    /// <summary>
    /// A result delivery strategy that uses an "Update" method that accepts a pair of map array.
    /// </summary>
    public class ResultDeliveryStrategyMap : ResultDeliveryStrategy
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly String _statementName;
        private readonly Object _subscriber;
        private readonly FastMethod _fastMethod;
        private readonly String[] _columnNames;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="statementName">Name of the statement.</param>
        /// <param name="subscriber">the object to deliver to</param>
        /// <param name="method">the delivery method</param>
        /// <param name="columnNames">the column names for the map</param>
        public ResultDeliveryStrategyMap(String statementName, Object subscriber, MethodInfo method, String[] columnNames)
        {
            _statementName = statementName;
            _subscriber = subscriber;
            FastClass fastClass = FastClass.Create(subscriber.GetType());
            _fastMethod = fastClass.GetMethod(method);
            _columnNames = columnNames;
        }
    
        public void Execute(UniformPair<EventBean[]> result)
        {
            DataMap[] newData;
            DataMap[] oldData;
    
            if (result == null) {
                newData = null;
                oldData = null;
            }
            else {
                newData = Convert(result.First);
                oldData = Convert(result.Second);
            }
    
            var paramList = new Object[] {newData, oldData};
            try {
                _fastMethod.Invoke(_subscriber, paramList);
            }
            catch (TargetInvocationException e) {
                ResultDeliveryStrategyImpl.Handle(_statementName, Log, e, paramList, _subscriber, _fastMethod);
            }
        }
    
        private DataMap[] Convert(EventBean[] events)
        {
            if ((events == null) || (events.Length == 0))
            {
                return null;
            }

            var result = new DataMap[events.Length];
            var length = 0;
            for (int i = 0; i < result.Length; i++)
            {
                if (events[i] is NaturalEventBean)
                {
                    var natural = (NaturalEventBean) events[i];
                    result[length] = Convert(natural);
                    length++;
                }
            }
    
            if (length == 0)
            {
                return null;
            }
            if (length != events.Length)
            {
                var reduced = new DataMap[length];
                Array.Copy(result, 0, reduced, 0, length);
                result = reduced;
            }
            return result;
        }

        private IDictionary<string, object> Convert(NaturalEventBean natural)
        {
            var map = new Dictionary<String, Object>();
            var columns = natural.Natural;
            for (int i = 0; i < columns.Length; i++)
            {
                map.Put(_columnNames[i], columns[i]);
            }
            return map;
        }
    }
}
