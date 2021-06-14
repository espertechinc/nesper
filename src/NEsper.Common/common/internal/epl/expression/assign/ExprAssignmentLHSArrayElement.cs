///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.chain;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.statement.helper;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.assign
{
    public class ExprAssignmentLHSArrayElement : ExprAssignmentLHS
    {
        private IList<ExprNode> _indexExpressions;

        public ExprAssignmentLHSArrayElement(
            string ident,
            IList<ExprNode> indexExpressions) : base(ident)
        {
            _indexExpressions = indexExpressions;
        }

        public override string FullIdentifier => Ident;

        public ExprNode IndexExpression => _indexExpressions[0];

        public override void Accept(ExprNodeVisitor visitor)
        {
            foreach (var node in _indexExpressions) {
                node.Accept(visitor);
            }
        }

        public override void Validate(
            ExprNodeOrigin origin,
            ExprValidationContext validationContext)
        {
            var index = _indexExpressions[0];
            index = ExprNodeUtilityValidate.GetValidatedSubtree(origin, index, validationContext);
            _indexExpressions = Collections.SingletonList(index);
            ChainableArray.ValidateSingleIndexExpr(_indexExpressions, () => "expression '" + Ident + "'");
            EPStatementStartMethodHelperValidate.ValidateNoAggregations(index, ExprAssignment.ValidationAggMsg);
        }
    }
} // end of namespace