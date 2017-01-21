///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.enummethod.dot;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.visitor
{
    public class ExprNodeIdentifierAndStreamRefVisitor : ExprNodeVisitor
    {
        private readonly bool _isVisitAggregateNodes;
        private IList<ExprNodePropOrStreamDesc> _refs;

        public ExprNodeIdentifierAndStreamRefVisitor(bool isVisitAggregateNodes)
        {
            _isVisitAggregateNodes = isVisitAggregateNodes;
        }

        public bool IsVisit(ExprNode exprNode)
        {
            if (exprNode is ExprLambdaGoesNode)
            {
                return false;
            }
            if (_isVisitAggregateNodes)
            {
                return true;
            }
            return (!(exprNode is ExprAggregateNode));
        }

        public IList<ExprNodePropOrStreamDesc> GetRefs()
        {
            if (_refs == null)
            {
                return Collections.GetEmptyList<ExprNodePropOrStreamDesc>();
            }
            return _refs;
        }

        public void Visit(ExprNode exprNode)
        {
            if (exprNode is ExprIdentNode)
            {
                var identNode = (ExprIdentNode) exprNode;

                var streamId = identNode.StreamId;
                var propertyName = identNode.ResolvedPropertyName;
                CheckAllocatedRefs();
                _refs.Add(new ExprNodePropOrStreamPropDesc(streamId, propertyName));
            }
            else if (exprNode is ExprStreamRefNode)
            {
                var streamRefNode = (ExprStreamRefNode) exprNode;
                var stream = streamRefNode.StreamReferencedIfAny;
                CheckAllocatedRefs();
                if (stream != null)
                {
                    _refs.Add(new ExprNodePropOrStreamExprDesc(stream.Value, streamRefNode));
                }
            }
        }

        public void Reset()
        {
            if (_refs != null)
            {
                _refs.Clear();
            }
        }

        private void CheckAllocatedRefs()
        {
            if (_refs == null)
            {
                _refs = new List<ExprNodePropOrStreamDesc>(4);
            }
        }
    }
} // end of namespace
