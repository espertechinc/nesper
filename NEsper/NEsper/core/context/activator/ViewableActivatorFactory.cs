///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.property;
using com.espertech.esper.epl.spec;
using com.espertech.esper.filter;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.pattern;

namespace com.espertech.esper.core.context.activator
{
    public interface ViewableActivatorFactory
    {
        ViewableActivator CreateActivatorSimple(
            FilterStreamSpecCompiled filterStreamSpec);

        ViewableActivator CreateFilterProxy(
            EPServicesContext services,
            FilterSpecCompiled filterSpec,
            Attribute[] annotations,
            bool subselect,
            InstrumentationAgent instrumentationAgentSubquery,
            bool isCanIterate);

        ViewableActivator CreateStreamReuseView(
            EPServicesContext services,
            StatementContext statementContext,
            StatementSpecCompiled statementSpec,
            FilterStreamSpecCompiled filterStreamSpec,
            bool isJoin,
            ExprEvaluatorContextStatement evaluatorContextStmt,
            bool filterSubselectSameStream,
            int streamNum,
            bool isCanIterateUnbound);

        ViewableActivator CreatePattern(
            PatternContext patternContext,
            EvalRootFactoryNode rootFactoryNode,
            EventType eventType,
            bool consumingFilters,
            bool suppressSameEventMatches,
            bool discardPartialsOnMatch,
            bool isCanIterateUnbound);

        ViewableActivator CreateNamedWindow(
            NamedWindowProcessor processor,
            IList<ExprNode> filterExpressions,
            PropertyEvaluator optPropertyEvaluator);
    }

    public class ProxyViewableActivatorFactory : ViewableActivatorFactory
    {
        public Func<FilterStreamSpecCompiled, ViewableActivator> ProcCreateActivatorSimple { get; set; }
        
        public ViewableActivator CreateActivatorSimple(
            FilterStreamSpecCompiled filterStreamSpec)
        {
            if (ProcCreateActivatorSimple == null)
                throw new NotSupportedException();

            return ProcCreateActivatorSimple.Invoke(
                filterStreamSpec);
        }

        public Func<EPServicesContext, FilterSpecCompiled, Attribute[], bool, InstrumentationAgent, bool, ViewableActivator>
            ProcCreateActivatorProxy { get; set; }

        public ViewableActivator CreateFilterProxy(
            EPServicesContext services,
            FilterSpecCompiled filterSpec,
            Attribute[] annotations,
            bool subselect,
            InstrumentationAgent instrumentationAgentSubquery,
            bool isCanIterate)
        {
            if (ProcCreateActivatorProxy == null)
                throw new NotSupportedException();

            return ProcCreateActivatorProxy.Invoke(
                services,
                filterSpec,
                annotations,
                subselect,
                instrumentationAgentSubquery,
                isCanIterate);
        }

        public Func<EPServicesContext, StatementContext, StatementSpecCompiled, FilterStreamSpecCompiled, bool, ExprEvaluatorContextStatement, bool, int, bool, ViewableActivator>
            ProcCreateStreamReuseView { get; set; }

        public ViewableActivator CreateStreamReuseView(
            EPServicesContext services,
            StatementContext statementContext,
            StatementSpecCompiled statementSpec,
            FilterStreamSpecCompiled filterStreamSpec,
            bool isJoin,
            ExprEvaluatorContextStatement evaluatorContextStmt,
            bool filterSubselectSameStream,
            int streamNum,
            bool isCanIterateUnbound)
        {
            if (ProcCreateStreamReuseView == null)
                throw new NotSupportedException();

            return ProcCreateStreamReuseView.Invoke(
                services,
                statementContext,
                statementSpec,
                filterStreamSpec,
                isJoin,
                evaluatorContextStmt,
                filterSubselectSameStream,
                streamNum,
                isCanIterateUnbound);
        }

        public Func<PatternContext, EvalRootFactoryNode, EventType, bool, bool, bool, bool, ViewableActivator>
            ProcCreatePattern { get; set; }

        public ViewableActivator CreatePattern(
            PatternContext patternContext,
            EvalRootFactoryNode rootFactoryNode,
            EventType eventType,
            bool consumingFilters,
            bool suppressSameEventMatches,
            bool discardPartialsOnMatch,
            bool isCanIterateUnbound)
        {
            if (ProcCreatePattern == null)
                throw new NotSupportedException();

            return ProcCreatePattern.Invoke(
                patternContext,
                rootFactoryNode,
                eventType,
                consumingFilters,
                suppressSameEventMatches,
                discardPartialsOnMatch,
                isCanIterateUnbound);
        }

        public Func<NamedWindowProcessor, IList<ExprNode>, PropertyEvaluator, ViewableActivator>
            ProcCreateNamedWindow { get; set; }

        public ViewableActivator CreateNamedWindow(
            NamedWindowProcessor processor,
            IList<ExprNode> filterExpressions,
            PropertyEvaluator optPropertyEvaluator)
        {
            if (ProcCreateNamedWindow == null)
                throw new NotSupportedException();

            return ProcCreateNamedWindow.Invoke(
                processor,
                filterExpressions,
                optPropertyEvaluator);
        }
    }
}
