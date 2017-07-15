///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.enummethod.dot;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.visitor
{
    public class ExprNodeIdentifierAndStreamRefVisitor : ExprNodeVisitor {
        private readonly bool isVisitAggregateNodes;
        private List<ExprNodePropOrStreamDesc> refs;
    
        public ExprNodeIdentifierAndStreamRefVisitor(bool isVisitAggregateNodes) {
            this.isVisitAggregateNodes = isVisitAggregateNodes;
        }
    
        public bool IsVisit(ExprNode exprNode) {
            if (exprNode is ExprLambdaGoesNode) {
                return false;
            }
            if (isVisitAggregateNodes) {
                return true;
            }
            return !(exprNode is ExprAggregateNode);
        }
    
        public List<ExprNodePropOrStreamDesc> GetRefs() {
            if (refs == null) {
                return Collections.EmptyList();
            }
            return refs;
        }
    
        public void Visit(ExprNode exprNode) {
            if (exprNode is ExprIdentNode) {
                ExprIdentNode identNode = (ExprIdentNode) exprNode;
    
                int streamId = identNode.StreamId;
                string propertyName = identNode.ResolvedPropertyName;
                CheckAllocatedRefs();
                refs.Add(new ExprNodePropOrStreamPropDesc(streamId, propertyName));
            } else if (exprNode is ExprStreamRefNode) {
                ExprStreamRefNode streamRefNode = (ExprStreamRefNode) exprNode;
                int? stream = streamRefNode.StreamReferencedIfAny;
                CheckAllocatedRefs();
                if (stream != null) {
                    refs.Add(new ExprNodePropOrStreamExprDesc(stream, streamRefNode));
                }
            }
        }
    
        public void Reset() {
            if (refs != null) {
                refs.Clear();
            }
        }
    
        private void CheckAllocatedRefs() {
            if (refs == null) {
                refs = new List<ExprNodePropOrStreamDesc>(4);
            }
        }
    }
} // end of namespace
