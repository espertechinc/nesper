///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.runtime.@internal.subscriber
{
    public class ResultDeliveryStrategyMapWStmt : ResultDeliveryStrategyMap
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ResultDeliveryStrategyMapWStmt(
            EPStatement statement,
            object subscriber,
            MethodInfo method,
            string[] columnNames,
            ImportService importService)
            : base(statement, subscriber, method, columnNames, importService)
        {
        }

        public override void Execute(UniformPair<EventBean[]> result)
        {
            IDictionary<string, object>[] newData;
            IDictionary<string, object>[] oldData;

            if (result == null) {
                newData = null;
                oldData = null;
            }
            else {
                newData = Convert(result.First);
                oldData = Convert(result.Second);
            }

            var parameters = new object[] {_statement, newData, oldData};
            try {
                _fastMethod.Invoke(_subscriber, parameters);
            }
            catch (TargetInvocationException e) {
                ResultDeliveryStrategyImpl.Handle(_statement.Name, Log, e, parameters, _subscriber, _fastMethod);
            }
        }
    }
} // end of namespace