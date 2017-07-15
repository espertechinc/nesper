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
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.declexpr;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.events;
using com.espertech.esper.events.arr;
using com.espertech.esper.pattern;

namespace com.espertech.esper.epl.join.hint
{
    public class ExcludePlanHintExprUtil {
    
        internal static readonly ObjectArrayEventType OAEXPRESSIONTYPE;
    
        static ExcludePlanHintExprUtil()
        {
            var properties = new LinkedHashMap<string, Object>();
            properties.Put("from_streamnum", typeof(int));
            properties.Put("to_streamnum", typeof(int));
            properties.Put("from_streamname", typeof(string));
            properties.Put("to_streamname", typeof(string));
            properties.Put("opname", typeof(string));
            properties.Put("exprs", typeof(string[]));
            OAEXPRESSIONTYPE = new ObjectArrayEventType(EventTypeMetadata.CreateAnonymous(typeof(ExcludePlanHintExprUtil).SimpleName, EventTypeMetadata.ApplicationType.OBJECTARR),
                    typeof(ExcludePlanHintExprUtil).SimpleName, 0, null, properties, null, null, null);
        }
    
        public static EventBean ToEvent(int fromStreamnum,
                                        int toStreamnum,
                                        string fromStreamname,
                                        string toStreamname,
                                        string opname,
                                        ExprNode[] expressions) {
            var texts = new string[expressions.Length];
            for (int i = 0; i < expressions.Length; i++) {
                texts[i] = ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(expressions[i]);
            }
            Object[] @event = new Object[]{fromStreamnum, toStreamnum, fromStreamname, toStreamname, opname, texts};
            return new ObjectArrayEventBean(@event, OAEXPRESSIONTYPE);
        }
    
        public static ExprEvaluator ToExpression(string hint, StatementContext statementContext) {
            string toCompile = "select * from java.lang.Object#TimeInMillis(" + hint + ")";
            StatementSpecRaw raw = EPAdministratorHelper.CompileEPL(toCompile, hint, false, null,
                    SelectClauseStreamSelectorEnum.ISTREAM_ONLY, statementContext.EngineImportService,
                    statementContext.VariableService, statementContext.SchedulingService,
                    statementContext.EngineURI, statementContext.ConfigSnapshot,
                    new PatternNodeFactoryImpl(), new ContextManagementServiceImpl(),
                    new ExprDeclaredServiceImpl(), new TableServiceImpl());
            ExprNode expr = raw.StreamSpecs[0].ViewSpecs[0].ObjectParameters[0];
            ExprNode validated = ExprNodeUtility.ValidateSimpleGetSubtree(ExprNodeOrigin.HINT, expr, statementContext, OAEXPRESSIONTYPE, false);
            return Validated.ExprEvaluator;
        }
    }
} // end of namespace
