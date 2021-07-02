///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    /// <summary>
    ///     Represents an equals (=) comparator in a filter expression tree.
    /// </summary>
    [Serializable]
    public class ExprEqualsNodeImpl : ExprNodeBase,
        ExprEqualsNode
    {
        [NonSerialized] private ExprEqualsNodeForge _forge;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="isNotEquals">true if this is a (!=) not equals rather then equals, false if its a '=' equals</param>
        /// <param name="isIs">true when "is" or "is not" (instead of = or &amp;lt;&amp;gt;)</param>
        public ExprEqualsNodeImpl(
            bool isNotEquals,
            bool isIs)
        {
            IsNotEquals = isNotEquals;
            IsIs = isIs;
        }

        public ExprEvaluator ExprEvaluator {
            get {
                CheckValidated(_forge);
                return _forge.ExprEvaluator;
            }
        }

        public bool IsConstantResult => false;

        public IDictionary<string, object> EventType => null;

        public override ExprForge Forge {
            get {
                CheckValidated(_forge);
                return _forge;
            }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            // Must have 2 child nodes
            if (ChildNodes.Length != 2) {
                throw new ExprValidationException(
                    "Invalid use of equals, expecting left-hand side and right-hand side but received " +
                    ChildNodes.Length +
                    " expressions");
            }

            // Must be the same boxed type returned by expressions under this
            var lhs = ChildNodes[0];
            var rhs = ChildNodes[1];
            var lhsType = lhs.Forge.EvaluationType.GetBoxedType();
            var rhsType = rhs.Forge.EvaluationType.GetBoxedType();

            // Null constants can be compared for any type
            if (lhsType.IsNullTypeSafe() || rhsType.IsNullTypeSafe()) {
                _forge = new ExprEqualsNodeForgeNC(this);
                return null;
            }

            if ((lhsType == rhsType) || (lhsType.IsAssignableFrom(rhsType))) {
                _forge = new ExprEqualsNodeForgeNC(this);
                return null;
            }

            // Get the common type such as Bool, String or Double and Long
            Type coercionType;
            try {
                coercionType = lhsType.GetCompareToCoercionType(rhsType);
            }
            catch (CoercionException) {
                throw new ExprValidationException(
                    "Implicit conversion from datatype '" +
                    rhsType.TypeSafeName() +
                    "' to '" +
                    lhsType.TypeSafeName() +
                    "' is not allowed");
            }

            // Check if we need to coerce
            if (coercionType == lhsType.GetBoxedType() &&
                coercionType == rhsType.GetBoxedType()) {
                _forge = new ExprEqualsNodeForgeNC(this);
            }
            else {
                if (lhsType.IsArray && rhsType.IsArray) {
                    var typeOneElement = lhsType.GetElementType();
                    var typeTwoElement = rhsType.GetElementType();
                    // Check to see if we have a "boxed" element trying to compare against an unboxed element.  We can
                    // coerce this with a custom widener.
                    if (typeOneElement.GetBoxedType() == typeTwoElement.GetBoxedType()) {
                        coercionType = typeOneElement.GetBoxedType().MakeArrayType();
                        var coercerLhs = ArrayCoercerFactory.GetCoercer(lhsType, coercionType);
                        var coercerRhs = ArrayCoercerFactory.GetCoercer(rhsType, coercionType);
                        _forge = new ExprEqualsNodeForgeCoercion(this, coercerLhs, coercerRhs, lhsType, rhsType);
                        return null;
                    }
                }

                if (!coercionType.IsNumeric()) {
                    throw new ExprValidationException(
                        "Cannot convert datatype '" +
                        coercionType.TypeSafeName() +
                        "' to a value that fits both type '" +
                        lhsType.TypeSafeName() +
                        "' and type '" +
                        rhsType.TypeSafeName() +
                        "'");
                }

                var numberCoercerLHS = SimpleNumberCoercerFactory.GetCoercer(lhsType, coercionType);
                var numberCoercerRHS = SimpleNumberCoercerFactory.GetCoercer(rhsType, coercionType);
                _forge = new ExprEqualsNodeForgeCoercion(this, numberCoercerLHS, numberCoercerRHS, lhsType, rhsType);
            }

            return null;
        }

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.EQUALS;

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprEqualsNode)) {
                return false;
            }

            var other = (ExprEqualsNode) node;
            return Equals(other.IsNotEquals, IsNotEquals);
        }

        public bool IsNotEquals { get; }

        public bool IsIs { get; }

        public override void ToPrecedenceFreeEPL(TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            ChildNodes[0].ToEPL(writer, Precedence, flags);
            if (IsIs) {
                writer.Write(" is ");
                if (IsNotEquals) {
                    writer.Write("not ");
                }
            }
            else {
                if (!IsNotEquals) {
                    writer.Write("=");
                }
                else {
                    writer.Write("!=");
                }
            }

            ChildNodes[1].ToEPL(writer, Precedence, flags);
        }
    }
} // end of namespace