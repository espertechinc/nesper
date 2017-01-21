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

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;

using XLR8.CGLib;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.core;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.core.context.mgr
{
    public class ContextControllerHashedGetterSingleRow : EventPropertyGetter
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private readonly String _statementName;
        private readonly FastMethod _fastMethod;
        private readonly ExprEvaluator[] _evaluators;
        private readonly int _granularity;

        public ContextControllerHashedGetterSingleRow(
            string statementName,
            string functionName,
            Pair<Type, EngineImportSingleRowDesc> func,
            IList<ExprNode> parameters,
            int granularity,
            EngineImportService engineImportService,
            EventType eventType,
            EventAdapterService eventAdapterService,
            int statementId,
            TableService tableService)
        {
            ExprNodeUtilMethodDesc staticMethodDesc = ExprNodeUtility.ResolveMethodAllowWildcardAndStream(
                func.First.Name, null, 
                func.Second.MethodName, 
                parameters,
                engineImportService, 
                eventAdapterService, 
                statementId, true, 
                eventType, 
                new ExprNodeUtilResolveExceptionHandlerDefault(func.Second.MethodName, true),
                func.Second.MethodName,
                tableService);
            _statementName = statementName;
            _evaluators = staticMethodDesc.ChildEvals;
            _granularity = granularity;
            _fastMethod = staticMethodDesc.FastMethod;
        }
    
        public Object Get(EventBean eventBean)
        {
            var events = new[] {eventBean};
            var paramList = new Object[_evaluators.Length];
            for (int i = 0; i < _evaluators.Length; i++) {
                paramList[i] = _evaluators[i].Evaluate(new EvaluateParams(events, true, null));
            }
    
            try
            {
                Object result = _fastMethod.Invoke(null, paramList);
                if (result == null) {
                    return 0;
                }
                int value = result.AsInt();
                if (value >= 0) {
                    return value % _granularity;
                }
                return -value % _granularity;
            }
            catch (TargetInvocationException e)
            {
                var message = TypeHelper.GetMessageInvocationTarget(_statementName, _fastMethod.Target, _fastMethod.DeclaringType.TargetType.FullName, paramList, e);
                Log.Error(message, e.InnerException);
            }
    
            return 0;
        }
    
        public bool IsExistsProperty(EventBean eventBean)
        {
            return false;
        }
    
        public Object GetFragment(EventBean eventBean)
        {
            return null;
        }
    }
}
