///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.chain
{
    public class ChainableName : Chainable
    {
        public ChainableName(
            bool distinct,
            bool optional,
            string name)
            : base(distinct, optional)
        {
            Name = name;
            NameUnescaped = name;
        }

        public ChainableName(
            bool distinct,
            bool optional,
            string name,
            string nameUnescaped)
            : base(distinct, optional)
        {
            Name = name;
            NameUnescaped = nameUnescaped;
        }

        public ChainableName(string name)
        {
            Name = name;
            NameUnescaped = name;
        }

        public string Name { get; }

        public string NameUnescaped { get; }

        public override void AddParametersTo(ICollection<ExprNode> result)
        {
            // no parameters
        }

        public override void Accept(ExprNodeVisitor visitor)
        {
            // no parameters
        }

        public override void Accept(ExprNodeVisitorWithParent visitor)
        {
            // no parameters
        }

        public override void Accept(
            ExprNodeVisitorWithParent visitor,
            ExprNode parent)
        {
            // no parameters
        }

        public override string GetRootNameOrEmptyString()
        {
            return Name;
        }

        public override IList<ExprNode> GetParametersOrEmpty()
        {
            return EmptyList<ExprNode>.Instance;
        }

        public override void ValidateExpressions(
            ExprNodeOrigin origin,
            ExprValidationContext validationContext)
        {
            // no action
        }

        public override string ToString()
        {
            return $"ChainableName{{name='{Name}{'\''}{'}'}";
        }

        protected bool Equals(ChainableName other)
        {
            return Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((ChainableName) obj);
        }

        public override int GetHashCode()
        {
            return Name != null ? Name.GetHashCode() : 0;
        }
    }
} // end of namespace