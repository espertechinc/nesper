///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.statement.helper;

using static com.espertech.esper.common.@internal.epl.expression.core.ExprNodeUtilityValidate; //getValidatedSubtree;

namespace com.espertech.esper.common.@internal.epl.expression.assign
{
    public class ExprAssignmentStraight : ExprAssignment
    {
        public ExprAssignmentStraight(
            ExprNode originalExpression,
            ExprAssignmentLHS lhs,
            ExprNode rhs) : base(originalExpression)
        {
            Lhs = lhs;
            Rhs = rhs;
        }

        public ExprAssignmentLHS Lhs { get; }

        public ExprNode Rhs { get; private set; }

        public override void Validate(
            ExprNodeOrigin origin,
            ExprValidationContext validationContext)
        {
            Rhs = GetValidatedSubtree(origin, Rhs, validationContext);
            Lhs.Validate(origin, validationContext);
            EPStatementStartMethodHelperValidate.ValidateNoAggregations(Rhs, ValidationAggMsg);
        }

        public override void Accept(ExprNodeVisitor visitor)
        {
            Lhs.Accept(visitor);
            Rhs.Accept(visitor);
        }
    }
} // end of namespace