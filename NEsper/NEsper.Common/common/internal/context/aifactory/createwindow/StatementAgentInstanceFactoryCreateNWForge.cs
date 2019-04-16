///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.activator;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.aifactory.createwindow
{
    public class StatementAgentInstanceFactoryCreateNWForge
    {
        private readonly ViewableActivatorFilterForge activator;
        private readonly string namedWindowName;
        private readonly IList<ViewFactoryForge> views;
        private readonly NamedWindowMetaData insertFromNamedWindow;
        private readonly ExprNode insertFromFilter;
        private readonly EventType asEventType;
        private readonly string resultSetProcessorProviderClassName;

        public StatementAgentInstanceFactoryCreateNWForge(
            ViewableActivatorFilterForge activator,
            string namedWindowName,
            IList<ViewFactoryForge> views,
            NamedWindowMetaData insertFromNamedWindow,
            ExprNode insertFromFilter,
            EventType asEventType,
            string resultSetProcessorProviderClassName)
        {
            this.activator = activator;
            this.namedWindowName = namedWindowName;
            this.views = views;
            this.insertFromNamedWindow = insertFromNamedWindow;
            this.insertFromFilter = insertFromFilter;
            this.asEventType = asEventType;
            this.resultSetProcessorProviderClassName = resultSetProcessorProviderClassName;
        }

        public CodegenMethod InitializeCodegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            CodegenMethod method = parent.MakeChild(typeof(StatementAgentInstanceFactoryCreateNW), this.GetType(), classScope);
            method.Block
                .DeclareVar(typeof(StatementAgentInstanceFactoryCreateNW), "saiff", NewInstance(typeof(StatementAgentInstanceFactoryCreateNW)));

            method.Block
                .ExprDotMethod(@Ref("saiff"), "setActivator", activator.MakeCodegen(method, symbols, classScope))
                .ExprDotMethod(@Ref("saiff"), "setNamedWindowName", Constant(namedWindowName))
                .ExprDotMethod(
                    @Ref("saiff"), "setViewFactories", ViewFactoryForgeUtil.CodegenForgesWInit(views, 0, null, method, symbols, classScope))
                .ExprDotMethod(
                    @Ref("saiff"), "setInsertFromNamedWindow",
                    insertFromNamedWindow == null
                        ? ConstantNull()
                        : NamedWindowDeployTimeResolver.MakeResolveNamedWindow(insertFromNamedWindow, symbols.GetAddInitSvc(method)))
                .ExprDotMethod(
                    @Ref("saiff"), "setInsertFromFilter",
                    insertFromFilter == null
                        ? ConstantNull()
                        : ExprNodeUtilityCodegen.CodegenEvaluator(insertFromFilter.Forge, method, this.GetType(), classScope))
                .ExprDotMethod(
                    @Ref("saiff"), "setAsEventType",
                    asEventType == null ? ConstantNull() : EventTypeUtility.ResolveTypeCodegen(asEventType, EPStatementInitServicesConstants.REF))
                .ExprDotMethod(
                    @Ref("saiff"), "setResultSetProcessorFactoryProvider",
                    CodegenExpressionBuilder.NewInstance(resultSetProcessorProviderClassName, symbols.GetAddInitSvc(method)))
                .ExprDotMethod(symbols.GetAddInitSvc(method), "addReadyCallback", @Ref("saiff"));

            method.Block.MethodReturn(@Ref("saiff"));
            return method;
        }
    }
} // end of namespace