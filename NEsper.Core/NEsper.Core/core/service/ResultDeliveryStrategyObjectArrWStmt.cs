///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.core;
using com.espertech.esper.events;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// A result delivery strategy that uses an "update" method that accepts a pair of object array array.
    /// </summary>
    public class ResultDeliveryStrategyObjectArrWStmt : ResultDeliveryStrategyObjectArr
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ResultDeliveryStrategyObjectArrWStmt(EPStatement statement, object subscriber, MethodInfo method, EngineImportService engineImportService)
            : base(statement, subscriber, method, engineImportService)
        {
        }

        public override void Execute(UniformPair<EventBean[]> result)
        {
            object[][] newData;
            object[][] oldData;

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

            var parameters = new object[] { _statement, newData, oldData };
            try
            {
                _method.Invoke(_subscriber, parameters);
            }
            catch (TargetInvocationException e)
            {
                ResultDeliveryStrategyImpl.Handle(_statement.Name, Log, e, parameters, _subscriber, _method);
            }
        }
    }
} // end of namespace
