using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.aifactory.update;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.context.util
{
    public partial class InternalEventRouterImpl
    {
        internal class IRDescEntry
        {
            internal IRDescEntry(
                InternalEventRouterDesc internalEventRouterDesc,
                InternalRoutePreprocessView outputView,
                StatementContext statementContext,
                bool hasSubselect,
                ExprEvaluator optionalWhereClauseEvaluator)
            {
                InternalEventRouterDesc = internalEventRouterDesc;
                OutputView = outputView;
                StatementContext = statementContext;
                HasSubselect = hasSubselect;
                OptionalWhereClauseEvaluator = optionalWhereClauseEvaluator;
            }

            public InternalEventRouterDesc InternalEventRouterDesc { get; }

            public EventType EventType => InternalEventRouterDesc.EventType;

            public Attribute[] Annotations => InternalEventRouterDesc.Annotations;

            public TypeWidener[] Wideners => InternalEventRouterDesc.Wideners;

            public InternalRoutePreprocessView OutputView { get; }

            public StatementContext StatementContext { get; }

            public bool HasSubselect { get; }

            public ExprEvaluator OptionalWhereClauseEvaluator { get; }

            public InternalEventRouterWriter[] Writers => InternalEventRouterDesc.Writers;
        }
    }
}