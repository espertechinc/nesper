///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.view.core.ViewFactoryForgeUtil;

namespace com.espertech.esper.common.@internal.view.intersect
{
    /// <summary>
    /// Factory for union-views.
    /// </summary>
    public class IntersectViewFactoryForge : ViewFactoryForgeBase,
        DataWindowViewForge,
        DataWindowViewForgeUniqueCandidate
    {
        private readonly IList<ViewFactoryForge> intersected;
        private int batchViewIndex = -1;
        private bool hasAsymetric;

        public IntersectViewFactoryForge(IList<ViewFactoryForge> intersected)
        {
            this.intersected = intersected;
            if (intersected.IsEmpty()) {
                throw new IllegalStateException("Empty intersected forges");
            }

            var batchCount = 0;
            for (var i = 0; i < intersected.Count; i++) {
                var forge = intersected[i];
                hasAsymetric |= forge is AsymetricDataWindowViewForge;
                if (forge is DataWindowBatchingViewForge) {
                    batchCount++;
                    batchViewIndex = i;
                }
            }

            if (batchCount > 1) {
                throw new ViewProcessingException("Cannot combined multiple batch data windows into an intersection");
            }
        }

        public override void SetViewParameters(
            IList<ExprNode> parameters,
            ViewForgeEnv viewForgeEnv,
            int streamNumber)
        {
        }

        public override void AttachValidate(
            EventType parentEventType,
            ViewForgeEnv viewForgeEnv)
        {
            eventType = parentEventType;
        }

        internal override Type TypeOfFactory => typeof(IntersectViewFactory);

        internal override string FactoryMethod => "Intersect";

        internal override void Assign(
            CodegenMethod method,
            CodegenExpressionRef factory,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block
                .SetProperty(factory, "BatchViewIndex", Constant(batchViewIndex))
                .SetProperty(factory, "HasAsymetric", Constant(hasAsymetric))
                .SetProperty(
                    factory,
                    "Intersecteds",
                    LocalMethod(
                        MakeViewFactories(intersected, GetType(), method, classScope, symbols)));
        }

        public override string ViewName => GetViewNameUnionIntersect(true, intersected);

        public override void Accept(ViewForgeVisitor visitor)
        {
            visitor.Visit(this);
            foreach (var forge in intersected) {
                forge.Accept(visitor);
            }
        }

        public static string GetViewNameUnionIntersect(
            bool intersect,
            ICollection<ViewFactoryForge> forges)
        {
            var buf = new StringBuilder();
            buf.Append(intersect ? "Intersection" : "Union");

            if (forges == null) {
                return buf.ToString();
            }

            buf.Append(" of ");
            var delimiter = "";
            foreach (var forge in forges) {
                buf.Append(delimiter);
                buf.Append(forge.ViewName);
                delimiter = ",";
            }

            return buf.ToString();
        }

        public ISet<string> UniquenessCandidatePropertyNames {
            get {
                foreach (var forge in intersected) {
                    if (forge is DataWindowViewForgeUniqueCandidate unique) {
                        var props = unique.UniquenessCandidatePropertyNames;
                        if (props != null) {
                            return props;
                        }
                    }
                }

                return null;
            }
        }

        public override IList<ViewFactoryForge> InnerForges => intersected;

        public override AppliesTo AppliesTo()
        {
            return client.annotation.AppliesTo.WINDOW_INTERSECT;
        }

        public override T Accept<T>(ViewFactoryForgeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
} // end of namespace