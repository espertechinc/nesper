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
using com.espertech.esper.common.@internal.fabric;
using com.espertech.esper.common.@internal.statemgmtsettings;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.view.core
{
    public abstract class ViewFactoryForgeBase : ViewFactoryForge
    {
        internal EventType eventType;

        public StateMgmtSetting StateMgmtSettings { get; private set; } = StateMgmtSettingDefault.INSTANCE;

        public virtual EventType EventType {
            get => eventType;
            set => eventType = value;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            CodegenSymbolProvider symbols,
            CodegenClassScope classScope)
        {
            return Make(parent, (SAIFFInitializeSymbol)symbols, classScope);
        }

        public abstract void SetViewParameters(
            IList<ExprNode> parameters,
            ViewForgeEnv viewForgeEnv,
            int streamNumber);

        public virtual void Attach(
            EventType parentEventType,
            ViewForgeEnv viewForgeEnv)
        {
            AttachValidate(parentEventType, viewForgeEnv);
        }

        public virtual void AssignStateMgmtSettings(
            FabricCharge fabricCharge,
            ViewForgeEnv viewForgeEnv,
            int[] grouping)
        {
            StateMgmtSettings = viewForgeEnv.StateMgmtSettingsProvider.View(
                fabricCharge,
                grouping,
                viewForgeEnv,
                this);
        }

        public abstract string ViewName { get; }

        public abstract T Accept<T>(ViewFactoryForgeVisitor<T> visitor);

        public virtual void Accept(ViewForgeVisitor visitor)
        {
            visitor.Visit(this);
        }
        
        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            if (eventType == null) {
                throw new IllegalStateException("Event type is unassigned");
            }

            var method = parent.MakeChild(typeof(ViewFactory), GetType(), classScope);
            var factory = Ref("factory");
            method.Block
                .DeclareVar(
                    TypeOfFactory,
                    factory.Ref,
                    ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Get(EPStatementInitServicesConstants.VIEWFACTORYSERVICE)
                        .Add(FactoryMethod, StateMgmtSettings.ToExpression()))
                .SetProperty(
                    factory,
                    "EventType",
                    EventTypeUtility.ResolveTypeCodegen(eventType, EPStatementInitServicesConstants.REF));

            Assign(method, factory, symbols, classScope);

            method.Block.MethodReturn(Ref("factory"));
            return LocalMethod(method);
        }

        public virtual IList<StmtClassForgeableFactory> InitAdditionalForgeables(ViewForgeEnv viewForgeEnv)
        {
            return EmptyList<StmtClassForgeableFactory>.Instance;
        }

        public virtual IList<ViewFactoryForge> InnerForges => EmptyList<ViewFactoryForge>.Instance;

        internal abstract Type TypeOfFactory { get; }

        internal abstract string FactoryMethod { get; }

        internal abstract void Assign(
            CodegenMethod method,
            CodegenExpressionRef factory,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope);

        public abstract AppliesTo AppliesTo();

        public abstract void AttachValidate(
            EventType parentEventType,
            ViewForgeEnv viewForgeEnv);
    }
} // end of namespace