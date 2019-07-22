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
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.view.core
{
    public abstract class ViewFactoryForgeBase : ViewFactoryForge
    {
        internal EventType eventType;

        public virtual EventType EventType {
            get => eventType;
            set => eventType = value;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            CodegenSymbolProvider symbols,
            CodegenClassScope classScope)
        {
            return Make(parent, (SAIFFInitializeSymbol) symbols, classScope);
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
                    TypeOfFactory(),
                    factory.Ref,
                    ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Add(EPStatementInitServicesConstants.GETVIEWFACTORYSERVICE)
                        .Add(FactoryMethod()))
                .SetProperty(
                    factory,
                    "EventType",
                    EventTypeUtility.ResolveTypeCodegen(eventType, EPStatementInitServicesConstants.REF));

            Assign(method, factory, symbols, classScope);

            method.Block.MethodReturn(Ref("factory"));
            return LocalMethod(method);
        }

        internal abstract Type TypeOfFactory();

        internal abstract string FactoryMethod();

        internal abstract void Assign(
            CodegenMethod method,
            CodegenExpressionRef factory,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope);

        public abstract void SetViewParameters(
            IList<ExprNode> parameters,
            ViewForgeEnv viewForgeEnv,
            int streamNumber);

        public abstract void Attach(
            EventType parentEventType,
            int streamNumber,
            ViewForgeEnv viewForgeEnv);

        public abstract string ViewName { get; }

        public virtual void Accept(ViewForgeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
} // end of namespace