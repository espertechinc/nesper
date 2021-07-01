///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    /// <summary>
    /// Represents a lesser or greater then (&lt;/&lt;=/&gt;/&gt;=) expression in a filter expression tree.
    /// </summary>
    [Serializable]
    public class ExprRelationalOpNodeImpl : ExprNodeBase,
        ExprRelationalOpNode
    {
        private readonly RelationalOpEnum _relationalOpEnum;

        [NonSerialized] private ExprRelationalOpNodeForge _forge;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="relationalOpEnum">type of compare, ie. lt, gt, le, ge</param>
        public ExprRelationalOpNodeImpl(RelationalOpEnum relationalOpEnum)
        {
            this._relationalOpEnum = relationalOpEnum;
        }

        public ExprEvaluator ExprEvaluator {
            get {
                CheckValidated(_forge);
                return _forge.ExprEvaluator;
            }
        }

        public override ExprForge Forge {
            get {
                CheckValidated(_forge);
                return _forge;
            }
        }

        public bool IsConstantResult {
            get => false;
        }

        /// <summary>
        /// Returns the type of relational op used.
        /// </summary>
        /// <returns>enum with relational op type</returns>
        public RelationalOpEnum RelationalOpEnum {
            get => _relationalOpEnum;
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            // Must have 2 child nodes
            if (ChildNodes.Length != 2) {
                throw new IllegalStateException("Relational op node does not have exactly 2 parameters");
            }

            // Must be either numeric or string
            var typeOne = ChildNodes[0].Forge.EvaluationType.GetBoxedType();
            var typeTwo = ChildNodes[1].Forge.EvaluationType.GetBoxedType();

            if ((typeOne != typeof(string)) || (typeTwo != typeof(string))) {
                if (!typeOne.IsNumeric()) {
                    throw new ExprValidationException(
                        "Implicit conversion from datatype '" +
                        (typeOne == null ? "null" : typeOne.CleanName()) +
                        "' to numeric is not allowed");
                }

                if (!typeTwo.IsNumeric()) {
                    throw new ExprValidationException(
                        "Implicit conversion from datatype '" +
                        (typeTwo == null ? "null" : typeTwo.CleanName()) +
                        "' to numeric is not allowed");
                }
            }

            var coercionType = typeOne.GetCompareToCoercionType(typeTwo);
            var computer = _relationalOpEnum.GetComputer(coercionType, typeOne, typeTwo);
            _forge = new ExprRelationalOpNodeForge(this, computer, coercionType);
            return null;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            ChildNodes[0].ToEPL(writer, Precedence, flags);
            writer.Write(_relationalOpEnum.GetExpressionText());
            ChildNodes[1].ToEPL(writer, Precedence, flags);
        }

        public override ExprPrecedenceEnum Precedence {
            get => ExprPrecedenceEnum.RELATIONAL_BETWEEN_IN;
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprRelationalOpNodeImpl)) {
                return false;
            }

            ExprRelationalOpNodeImpl other = (ExprRelationalOpNodeImpl) node;

            if (other._relationalOpEnum != _relationalOpEnum) {
                return false;
            }

            return true;
        }
    }
} // end of namespace