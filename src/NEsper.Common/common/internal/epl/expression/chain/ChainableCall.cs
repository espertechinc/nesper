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

using static com.espertech.esper.common.@internal.epl.expression.core.ExprNodeUtilityQuery; //acceptParams;

namespace com.espertech.esper.common.@internal.epl.expression.chain
{
    public class ChainableCall : Chainable
    {
        public ChainableCall(
            bool distinct,
            bool optional,
            string name,
            string nameUnescaped,
            IList<ExprNode> parameters) : base(distinct, optional)
        {
            Name = name;
            NameUnescaped = nameUnescaped;
            Parameters = parameters;
        }

        public ChainableCall(
            string name,
            IList<ExprNode> parameters)
        {
            Name = name;
            NameUnescaped = name;
            Parameters = parameters;
        }

        public string Name { get; set; }
        public IList<ExprNode> Parameters { get; set; }
        public string NameUnescaped { get; }

        public override void AddParametersTo(ICollection<ExprNode> result)
        {
            result.AddAll(Parameters);
        }

        public override void Accept(ExprNodeVisitor visitor)
        {
            AcceptParams(visitor, Parameters);
        }

        public override void Accept(ExprNodeVisitorWithParent visitor)
        {
            AcceptParams(visitor, Parameters);
        }

        public override void Accept(
            ExprNodeVisitorWithParent visitor,
            ExprNode parent)
        {
            AcceptParams(visitor, Parameters, parent);
        }

        public override void ValidateExpressions(
            ExprNodeOrigin origin,
            ExprValidationContext validationContext)
        {
            ValidateExpressions(Parameters, origin, validationContext);
        }

        public override string ToString()
        {
            return "ChainableCall{" + "name='" + Name + '\'' + ", parameters=" + Parameters + '}';
        }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (ChainableCall)o;
            return EqualsChainable(that) &&
                   Name.Equals(that.Name) &&
                   ExprNodeUtilityCompare.DeepEquals(Parameters, that.Parameters);
        }

        public override int GetHashCode()
        {
            return Name != null ? Name.GetHashCode() : 0;
        }

        public string RootNameOrEmptyString => Name;

        public IList<ExprNode> ParametersOrEmpty => Parameters;
    }
} // end of namespace