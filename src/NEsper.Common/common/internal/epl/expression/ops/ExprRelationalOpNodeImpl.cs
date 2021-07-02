///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.client.util;
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
            _relationalOpEnum = relationalOpEnum;
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
            var lhsForge = ChildNodes[0].Forge;
            var rhsForge = ChildNodes[1].Forge;

            var lhsType = ValidateStringOrNumeric(lhsForge);
            var rhsType = ValidateStringOrNumeric(rhsForge);
            
            if ((lhsType != typeof(string)) || (rhsType != typeof(string))) {
                if (!lhsType.IsNumeric()) {
                    throw new ExprValidationException(
                        "Implicit conversion from datatype '" + lhsType.TypeSafeName() + "' to numeric is not allowed");
                }

                if (!rhsType.IsNumeric()) {
                    throw new ExprValidationException(
                        "Implicit conversion from datatype '" + rhsType.TypeSafeName() + "' to numeric is not allowed");
                }
            }

            var coercionType = lhsType.GetCompareToCoercionType(rhsType);
            var computer = _relationalOpEnum.GetComputer(coercionType, lhsType, rhsType);
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

            var other = (ExprRelationalOpNodeImpl) node;
            return other._relationalOpEnum == _relationalOpEnum;
        }

        private Type ValidateStringOrNumeric(ExprForge forge)
        {
            var type = forge.EvaluationType;
            if (type.IsNullTypeSafe()) {
                throw new ExprValidationException("Null-type value is not allow for relational operator");
            }

            if (type == typeof(string)) {
                return typeof(string);
            }

            return ExprNodeUtilityValidate.ValidateReturnsNumeric(forge).GetBoxedType();
        }
    }
} // end of namespace