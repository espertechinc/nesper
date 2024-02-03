///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.declared.compiletime;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.visitor
{
    public class ExprNodeIdentifierAndStreamRefVisitor : ExprNodeVisitor
    {
        private readonly bool isVisitAggregateNodes;
        private readonly bool isVisitDeclaredExprParams;
        private readonly bool isVisitDeclaredExprBody;
        private IList<ExprNodePropOrStreamDesc> refs;
        private bool hasWildcardOrStreamAlias;

        public ExprNodeIdentifierAndStreamRefVisitor(bool isVisitAggregateNodes)
            : this(isVisitAggregateNodes, false, true)
        {
        }

        public ExprNodeIdentifierAndStreamRefVisitor(
            bool isVisitAggregateNodes,
            bool isVisitDeclaredExprParams,
            bool isVisitDeclaredExprBody)
        {
            this.isVisitAggregateNodes = isVisitAggregateNodes;
            this.isVisitDeclaredExprParams = isVisitDeclaredExprParams;
            this.isVisitDeclaredExprBody = isVisitDeclaredExprBody;
        }

        public bool IsVisit(ExprNode exprNode)
        {
            if (exprNode is ExprLambdaGoesNode) {
                return false;
            }

            if (!isVisitDeclaredExprBody && exprNode is ExprDeclaredNode) {
                return false;
            }

            if (isVisitAggregateNodes) {
                return true;
            }

            return !(exprNode is ExprAggregateNode);
        }

        public IList<ExprNodePropOrStreamDesc> Refs {
            get {
                if (refs == null) {
                    return EmptyList<ExprNodePropOrStreamDesc>.Instance;
                }

                return refs;
            }
        }

        public void Visit(ExprNode exprNode)
        {
            if (exprNode is ExprIdentNode identNode) {
                var streamId = identNode.StreamId;
                var propertyName = identNode.ResolvedPropertyName;
                CheckAllocatedRefs();
                refs.Add(new ExprNodePropOrStreamPropDesc(streamId, propertyName));
            }
            else if (exprNode is ExprStreamRefNode streamRefNode) {
                var stream = streamRefNode.StreamReferencedIfAny;
                CheckAllocatedRefs();
                if (stream != null) {
                    refs.Add(new ExprNodePropOrStreamExprDesc(stream.Value, streamRefNode));
                }

                if (streamRefNode is ExprWildcard || streamRefNode is ExprStreamUnderlyingNode) {
                    hasWildcardOrStreamAlias = true;
                }
            }
        }

        public void Reset()
        {
            if (refs != null) {
                refs.Clear();
            }
        }

        public bool IsWalkDeclExprParam => isVisitDeclaredExprParams;

        public bool HasWildcardOrStreamAlias => hasWildcardOrStreamAlias;

        private void CheckAllocatedRefs()
        {
            if (refs == null) {
                refs = new List<ExprNodePropOrStreamDesc>(4);
            }
        }
    }
} // end of namespace