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
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.statemgmtsettings;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.view.core
{
    public abstract class ViewFactoryForgeBase : ViewFactoryForge
    {
        internal EventType eventType;
        internal StateMgmtSetting stateMgmtSettings = StateMgmtSettingDefault.INSTANCE;

        public abstract Type TypeOfFactory();

        public abstract string FactoryMethod();

        public abstract AppliesTo AppliesTo();

        #region AUTO_ABSTRACT

        public abstract void SetViewParameters(
            IList<ExprNode> parameters,
            ViewForgeEnv viewForgeEnv,
            int streamNumber);

        public abstract string ViewName { get; }

        public virtual IList<StmtClassForgeableFactory> InitAdditionalForgeables(ViewForgeEnv viewForgeEnv)
        {
            return EmptyList<StmtClassForgeableFactory>.Instance;
        }

        public virtual IList<ViewFactoryForge> InnerForges => EmptyList<ViewFactoryForge>.Instance;

        #endregion
        
        public abstract void AttachValidate(
            EventType parentEventType,
            int streamNumber,
            ViewForgeEnv viewForgeEnv,
            bool grouped);

        internal abstract void Assign(
            CodegenMethod method,
            CodegenExpressionRef factory,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope);

        public virtual EventType EventType {
            get => eventType;
            set => eventType = value;
        }

        public virtual void Attach(
            EventType parentEventType,
            int streamNumber,
            ViewForgeEnv viewForgeEnv,
            bool grouped)
        {
            AttachValidate(parentEventType, streamNumber, viewForgeEnv, grouped);
            stateMgmtSettings = viewForgeEnv.StateMgmtSettingsProvider.GetView(
                viewForgeEnv.StatementRawInfo,
                streamNumber,
                viewForgeEnv.IsSubquery,
                grouped,
                AppliesTo());
        }

        public virtual void Accept(ViewForgeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public virtual CodegenExpression Make(
            CodegenMethodScope parent,
            CodegenSymbolProvider symbols,
            CodegenClassScope classScope)
        {
            if (!(symbols is SAIFFInitializeSymbol initializeSymbol)) {
                throw new ArgumentException("invalid value", nameof(symbols));
            }
            
            if (eventType == null) {
                throw new IllegalStateException("Event type is unassigned");
            }

            var method = parent.MakeChild(typeof(ViewFactory), GetType(), classScope);
            var factory = Ref("factory");
            method.Block
                .DeclareVar(
                    TypeOfFactory(),
                    factory.Ref,
                    ExprDotMethodChain(initializeSymbol.GetAddInitSvc(method))
                        .Get(EPStatementInitServicesConstants.VIEWFACTORYSERVICE)
                        .Add(FactoryMethod(), stateMgmtSettings.ToExpression()))
                .SetProperty(
                    factory,
                    "EventType",
                    EventTypeUtility.ResolveTypeCodegen(eventType, EPStatementInitServicesConstants.REF));

            Assign(method, factory, initializeSymbol, classScope);

            method.Block.MethodReturn(Ref("factory"));
            return LocalMethod(method);
        }
    }
} // end of namespace