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
    public class ExprAssignmentLHSIdentWSubprop : ExprAssignmentLHS
    {
        public ExprAssignmentLHSIdentWSubprop(
            string name,
            string subpropertyName) : base(name)
        {
            SubpropertyName = subpropertyName;
        }

        public string SubpropertyName { get; }

        public override string FullIdentifier => Ident + "." + SubpropertyName;

        public override void Validate(
            ExprNodeOrigin origin,
            ExprValidationContext validationContext)
        {
            // specific validation by assignor
        }

        public override void Accept(ExprNodeVisitor visitor)
        {
            // no action
        }
    }
} // end of namespace