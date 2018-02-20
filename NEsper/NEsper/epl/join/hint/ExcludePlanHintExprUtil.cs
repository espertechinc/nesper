///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.core.service;
using com.espertech.esper.core.support;
using com.espertech.esper.epl.declexpr;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.events;
using com.espertech.esper.events.arr;
using com.espertech.esper.pattern;

namespace com.espertech.esper.epl.join.hint
{
    public class ExcludePlanHintExprUtil
    {
        internal static readonly ObjectArrayEventType OAEXPRESSIONTYPE;
    
        static ExcludePlanHintExprUtil()
        {
            var properties = new Dictionary<string, Object>();
            properties.Put("from_streamnum", typeof(int));
            properties.Put("to_streamnum", typeof(int));
            properties.Put("from_streamname", typeof(string));
            properties.Put("to_streamname", typeof(string));
            properties.Put("opname", typeof(string));
            properties.Put("exprs", typeof(string[]));
            OAEXPRESSIONTYPE =
                new ObjectArrayEventType(
                    EventTypeMetadata.CreateAnonymous(typeof (ExcludePlanHintExprUtil).Name, ApplicationType.OBJECTARR),
                    typeof (ExcludePlanHintExprUtil).Name, 0, SupportEventAdapterService.Service, properties, null, null, null);
        }
    
        public static EventBean ToEvent(int fromStreamnum,
                                        int toStreamnum,
                                        string fromStreamname,
                                        string toStreamname,
                                        string opname,
                                        ExprNode[] expressions) {
            var texts = new string[expressions.Length];
            for (var i = 0; i < expressions.Length; i++) {
                texts[i] = ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(expressions[i]);
            }
            var @event = new Object[]{fromStreamnum, toStreamnum, fromStreamname, toStreamname, opname, texts};
            return new ObjectArrayEventBean(@event, OAEXPRESSIONTYPE);
        }
    
        public static ExprEvaluator ToExpression(
            string hint,
            StatementContext statementContext)
        {
            var toCompile = "select * from com.esper.espertech.compat.DateTimeOffsetHelper#TimeInMillis(" + hint + ")";
            var raw = EPAdministratorHelper.CompileEPL(
                statementContext.Container,
                toCompile, hint, false, null,
                SelectClauseStreamSelectorEnum.ISTREAM_ONLY, statementContext.EngineImportService,
                statementContext.VariableService,
                statementContext.SchedulingService,
                statementContext.EngineURI,
                statementContext.ConfigSnapshot,
                new PatternNodeFactoryImpl(),
                new ContextManagementServiceImpl(),
                new ExprDeclaredServiceImpl(statementContext.EngineServices.LockManager),
                new TableServiceImpl(statementContext.RWLockManager, statementContext.ThreadLocalManager)
            );
            var expr = raw.StreamSpecs[0].ViewSpecs[0].ObjectParameters[0];
            var validated = ExprNodeUtility.ValidateSimpleGetSubtree(ExprNodeOrigin.HINT, expr, statementContext, OAEXPRESSIONTYPE, false);
            return validated.ExprEvaluator;
        }
    }
} // end of namespace
