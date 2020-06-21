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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.intersect;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.view.core.ViewFactoryForgeUtil;

namespace com.espertech.esper.common.@internal.view.union
{
    /// <summary>
    ///     Factory for union-views.
    /// </summary>
    public class UnionViewFactoryForge : ViewFactoryForgeBase,
        DataWindowViewForge
    {
        private readonly IList<ViewFactoryForge> unioned;
        private readonly bool hasAsymetric;

        public IList<ViewFactoryForge> InnerForges => unioned;

        public UnionViewFactoryForge(IList<ViewFactoryForge> unioned)
        {
            this.unioned = unioned;
            if (unioned.IsEmpty()) {
                throw new IllegalStateException("Empty unioned views");
            }

            foreach (var forge in unioned) {
                hasAsymetric |= forge is AsymetricDataWindowViewForge;
            }
        }

        public override string ViewName => IntersectViewFactoryForge.GetViewNameUnionIntersect(false, unioned);

        public override void SetViewParameters(
            IList<ExprNode> parameters,
            ViewForgeEnv viewForgeEnv,
            int streamNumber)
        {
        }

        public override void Attach(
            EventType parentEventType,
            int streamNumber,
            ViewForgeEnv viewForgeEnv)
        {
            eventType = parentEventType;
        }

        internal override Type TypeOfFactory()
        {
            return typeof(UnionViewFactory);
        }

        internal override string FactoryMethod()
        {
            return "Union";
        }

        internal override void Assign(
            CodegenMethod method,
            CodegenExpressionRef factory,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block.SetProperty(factory, "HasAsymetric", Constant(hasAsymetric))
                .SetProperty(
                    factory,
                    "Unioned",
                    LocalMethod(MakeViewFactories(unioned, GetType(), method, classScope, symbols)));
        }

        public override void Accept(ViewForgeVisitor visitor)
        {
            visitor.Visit(this);
            foreach (var forge in unioned) {
                forge.Accept(visitor);
            }
        }
    }
} // end of namespace