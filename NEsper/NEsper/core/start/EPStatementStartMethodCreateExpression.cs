///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.factory;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.spec;
using com.espertech.esper.view;

namespace com.espertech.esper.core.start
{
    /// <summary>
    /// Starts and provides the stop method for EPL statements.
    /// </summary>
    public class EPStatementStartMethodCreateExpression : EPStatementStartMethodBase
    {
        public EPStatementStartMethodCreateExpression(StatementSpecCompiled statementSpec)
            : base(statementSpec)
        {
        }

        public override EPStatementStartResult StartInternal(
            EPServicesContext services,
            StatementContext statementContext,
            bool isNewStatement,
            bool isRecoveringStatement,
            bool isRecoveringResilient)
        {
            var expressionName =
                services.ExprDeclaredService.AddExpressionOrScript(StatementSpec.CreateExpressionDesc);

            // define output event type
            var typeName = "EventType_Expression_" + expressionName;
            var resultType = services.EventAdapterService.CreateAnonymousMapType(
                typeName, Collections.GetEmptyMap<String, Object>(), true);

            var stopMethod = new ProxyEPStatementStopMethod(
                () =>
                {
                    // no action
                });

            var destroyMethod = new ProxyEPStatementDestroyMethod(
                () => services.ExprDeclaredService.DestroyedExpression(StatementSpec.CreateExpressionDesc));

            Viewable resultView = new ZeroDepthStreamNoIterate(resultType);
            statementContext.StatementAgentInstanceFactory = new StatementAgentInstanceFactoryNoAgentInstance(resultView);
            return new EPStatementStartResult(resultView, stopMethod, destroyMethod);
        }
    }
}
