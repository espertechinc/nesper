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
        internal readonly IList<ViewFactoryForge> intersected;
        internal int batchViewIndex = -1;
        internal bool hasAsymetric;

        public IntersectViewFactoryForge(IList<ViewFactoryForge> intersected)
        {
            this.intersected = intersected;
            if (intersected.IsEmpty())
            {
                throw new IllegalStateException("Empty intersected forges");
            }

            int batchCount = 0;
            for (int i = 0; i < intersected.Count; i++)
            {
                ViewFactoryForge forge = intersected[i];
                hasAsymetric |= forge is AsymetricDataWindowViewForge;
                if (forge is DataWindowBatchingViewForge)
                {
                    batchCount++;
                    batchViewIndex = i;
                }
            }

            if (batchCount > 1)
            {
                throw new ViewProcessingException("Cannot combined multiple batch data windows into an intersection");
            }
        }

        public override void SetViewParameters(IList<ExprNode> parameters, ViewForgeEnv viewForgeEnv, int streamNumber)
        {
        }

        public override void Attach(EventType parentEventType, int streamNumber, ViewForgeEnv viewForgeEnv)
        {
            this.eventType = parentEventType;
        }

        internal override Type TypeOfFactory()
        {
            return typeof(IntersectViewFactory);
        }

        internal override string FactoryMethod()
        {
            return "intersect";
        }

        internal override void Assign(
            CodegenMethod method, CodegenExpressionRef factory, SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block
                .ExprDotMethod(factory, "setBatchViewIndex", Constant(batchViewIndex))
                .ExprDotMethod(factory, "setHasAsymetric", Constant(hasAsymetric))
                .ExprDotMethod(
                    factory, "setIntersecteds", LocalMethod(
                        MakeViewFactories(intersected, this.GetType(), method, classScope, symbols)));
        }

        public override string ViewName
        {
            get => GetViewNameUnionIntersect(true, intersected);
        }

        public override void Accept(ViewForgeVisitor visitor)
        {
            visitor.Visit(this);
            foreach (ViewFactoryForge forge in intersected)
            {
                forge.Accept(visitor);
            }
        }

        public static string GetViewNameUnionIntersect(bool intersect, ICollection<ViewFactoryForge> forges)
        {
            StringBuilder buf = new StringBuilder();
            buf.Append(intersect ? "Intersection" : "Union");

            if (forges == null)
            {
                return buf.ToString();
            }

            buf.Append(" of ");
            string delimiter = "";
            foreach (ViewFactoryForge forge in forges)
            {
                buf.Append(delimiter);
                buf.Append(forge.ViewName);
                delimiter = ",";
            }

            return buf.ToString();
        }

        public ISet<string> UniquenessCandidatePropertyNames {
            get {
                foreach (ViewFactoryForge forge in intersected) {
                    if (forge is DataWindowViewForgeUniqueCandidate) {
                        DataWindowViewForgeUniqueCandidate unique = (DataWindowViewForgeUniqueCandidate) forge;
                        ISet<string> props = unique.UniquenessCandidatePropertyNames;
                        if (props != null) {
                            return props;
                        }
                    }
                }

                return null;
            }
        }
    }
} // end of namespace