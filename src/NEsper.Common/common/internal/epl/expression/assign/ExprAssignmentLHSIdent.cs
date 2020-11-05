///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.visitor;

namespace com.espertech.esper.common.@internal.epl.expression.assign
{
    public class ExprAssignmentLHSIdent : ExprAssignmentLHS
    {
        public ExprAssignmentLHSIdent(string ident) : base(ident)
        {
        }

        public override string FullIdentifier => Ident;

        public override void Accept(ExprNodeVisitor visitor)
        {
            // no action
        }

        public override void Validate(
            ExprNodeOrigin origin,
            ExprValidationContext validationContext)
        {
            // no action, validated by writer
        }
    }
} // end of namespace