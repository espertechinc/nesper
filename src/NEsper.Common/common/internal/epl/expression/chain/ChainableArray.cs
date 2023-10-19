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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.epl.expression.core.ExprNodeUtilityQuery; //acceptParams

namespace com.espertech.esper.common.@internal.epl.expression.chain {
    public class ChainableArray : Chainable {
        public ChainableArray (IList<ExprNode> indexExpressions) {
            Indexes = indexExpressions;
        }

        public ChainableArray (
            bool distinct,
            bool optional,
            IList<ExprNode> indexes) : base (distinct, optional) {
            Indexes = indexes;
        }

        public IList<ExprNode> Indexes { get; }

        public override void Accept (ExprNodeVisitor visitor) {
            AcceptParams (visitor, Indexes);
        }

        public override void Accept (ExprNodeVisitorWithParent visitor) {
            AcceptParams (visitor, Indexes);
        }

        public override void Accept (
            ExprNodeVisitorWithParent visitor,
            ExprNode parent) {
            AcceptParams (visitor, Indexes, parent);
        }

        public override void ValidateExpressions (
            ExprNodeOrigin origin,
            ExprValidationContext validationContext) {
            ValidateExpressions (Indexes, origin, validationContext);
        }

        public override void AddParametersTo (ICollection<ExprNode> result) {
            result.AddAll (Indexes);
        }

        public override bool Equals (object o) {
            if (this == o) {
                return true;
            }

            if (o == null || GetType () != o.GetType ()) {
                return false;
            }

            var that = (ChainableArray) o;
            return EqualsChainable (that) && ExprNodeUtilityCompare.DeepEquals (Indexes, that.Indexes);
        }

        public override int GetHashCode () {
            return 0;
        }

        public static ExprNode ValidateSingleIndexExpr (
            IList<ExprNode> indexes,
            Supplier<string> supplier) {
            if (indexes.Count != 1) {
                throw new ExprValidationException (
                    "Incorrect number of index expressions for array operation, expected a single expression returning an integer value but received " +
                    indexes.Count +
                    " expressions for " +
                    supplier.Invoke ());
            }

            var node = indexes[0];
            var evaluationType = node.Forge.EvaluationType;
            if (!evaluationType.IsTypeInt32 ()) {
                throw new ExprValidationException (
                    "Incorrect index expression for array operation, expected an expression returning an integer value but the expression '" +
                    ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe (node) +
                    "' returns '" +
                    evaluationType.CleanName () +
                    "' for " +
                    supplier.Invoke ());
            }

            return node;
        }

        public override IList<ExprNode> ParametersOrEmpty => EmptyList<ExprNode>.Instance;

        public override string RootNameOrEmptyString => "";
    }
} // end of namespace