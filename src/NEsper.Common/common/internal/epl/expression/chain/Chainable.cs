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

using static com.espertech.esper.common.@internal.epl.expression.core.ExprNodeUtilityValidate; //getValidatedSubtree

namespace com.espertech.esper.common.@internal.epl.expression.chain
{
    public abstract class Chainable
    {
        protected Chainable() : this(false, false)
        {
        }

        protected Chainable(
            bool distinct,
            bool optional)
        {
            IsDistinct = distinct;
            IsOptional = optional;
        }

        public bool IsDistinct { get; }

        public bool IsOptional { get; }

        public abstract void AddParametersTo(ICollection<ExprNode> result);

        public abstract void Accept(ExprNodeVisitor visitor);

        public abstract void Accept(ExprNodeVisitorWithParent visitor);

        public abstract void Accept(
            ExprNodeVisitorWithParent visitor,
            ExprNode parent);

        public abstract string GetRootNameOrEmptyString();

        public abstract IList<ExprNode> GetParametersOrEmpty();

        public abstract void ValidateExpressions(
            ExprNodeOrigin origin,
            ExprValidationContext validationContext);

        public static bool IsPlainPropertyChain(Chainable chainable)
        {
            return chainable is ChainableName && chainable.GetRootNameOrEmptyString().Contains(".");
        }

        public void Validate(
            ExprNodeOrigin origin,
            ExprValidationContext validationContext)
        {
            foreach (var node in GetParametersOrEmpty()) {
                if (node is ExprNamedParameterNode) {
                    throw new ExprValidationException("Named parameters are not allowed");
                }
            }

            ValidateExpressions(origin, validationContext);
        }

        public static IList<Chainable> ChainForDot(Chainable chainable)
        {
            if (!(chainable is ChainableName)) {
                return new List<Chainable>(Collections.SingletonList(chainable));
            }

            var values = chainable.GetRootNameOrEmptyString().Split('.');
            var chain = new List<Chainable>(values.Length + 1);
            foreach (var value in values) {
                chain.Add(new ChainableName(value));
            }

            return chain;
        }

        internal static void ValidateExpressions(
            IList<ExprNode> expressions,
            ExprNodeOrigin origin,
            ExprValidationContext validationContext)
        {
            for (var i = 0; i < expressions.Count; i++) {
                var node = expressions[i];
                var validated = GetValidatedSubtree(origin, node, validationContext);
                if (node != validated) {
                    expressions[i] = validated;
                }
            }
        }

        protected bool EqualsChainable(Chainable that)
        {
            return that.IsDistinct == IsDistinct && that.IsOptional == IsOptional;
        }
    }
} // end of namespace