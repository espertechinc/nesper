///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.visitor;

namespace com.espertech.esper.common.@internal.epl.expression.assign
{
    public abstract class ExprAssignmentLHS
    {
        public ExprAssignmentLHS(string ident)
        {
            Ident = ident;
        }

        public string Ident { get; }
        public abstract string FullIdentifier { get; }

        public abstract void Validate(
            ExprNodeOrigin origin,
            ExprValidationContext validationContext);

        public abstract void Accept(ExprNodeVisitor visitor);
    }
} // end of namespace