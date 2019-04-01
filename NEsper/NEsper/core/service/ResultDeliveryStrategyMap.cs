///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.epl.core;

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

        internal readonly EPStatement _statement;
        internal readonly Object _subscriber;
        internal readonly FastMethod _fastMethod;
        internal readonly String[] _columnNames;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="statement">The statement.</param>
        /// <param name="subscriber">the object to deliver to</param>
        /// <param name="method">the delivery method</param>
        /// <param name="columnNames">the column names for the map</param>
        /// <param name="engineImportService">The engine import service.</param>
        public ResultDeliveryStrategyMap(EPStatement statement, object subscriber, MethodInfo method, string[] columnNames, EngineImportService engineImportService)
        {
            _statement = statement;
            _subscriber = subscriber;
            _fastMethod = FastClass.CreateMethod(method);
            _columnNames = columnNames;
        }

        public virtual void Execute(UniformPair<EventBean[]> result)
        {
            DataMap[] newData;
            DataMap[] oldData;

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

        internal DataMap[] Convert(EventBean[] events)
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
                    var natural = (NaturalEventBean)events[i];
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
