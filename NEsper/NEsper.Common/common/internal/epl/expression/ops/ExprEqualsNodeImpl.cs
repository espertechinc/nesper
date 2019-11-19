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
        [NonSerialized] private ExprEqualsNodeForge forge;

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
                CheckValidated(forge);
                return forge.ExprEvaluator;
            }
        }

        public bool IsConstantResult => false;

        public IDictionary<string, object> EventType => null;

        public override ExprForge Forge {
            get {
                CheckValidated(forge);
                return forge;
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
            var typeOne = lhs.Forge.EvaluationType.GetBoxedType();
            var typeTwo = rhs.Forge.EvaluationType.GetBoxedType();

            // Null constants can be compared for any type
            if (typeOne == null || typeTwo == null) {
                forge = new ExprEqualsNodeForgeNC(this);
                return null;
            }

            if (typeOne.Equals(typeTwo) || typeOne.IsAssignableFrom(typeTwo)) {
                forge = new ExprEqualsNodeForgeNC(this);
                return null;
            }

            // Get the common type such as Bool, String or Double and Long
            Type coercionType;
            try {
                coercionType = typeOne.GetCompareToCoercionType(typeTwo);
            }
            catch (CoercionException) {
                throw new ExprValidationException(
                    "Implicit conversion from datatype '" +
                    typeTwo.CleanName() +
                    "' to '" +
                    typeOne.CleanName() +
                    "' is not allowed");
            }

            // Check if we need to coerce
            if (coercionType == typeOne.GetBoxedType() &&
                coercionType == typeTwo.GetBoxedType()) {
                forge = new ExprEqualsNodeForgeNC(this);
            }
            else {
                if (!coercionType.IsNumeric()) {
                    throw new ExprValidationException(
                        "Cannot convert datatype '" +
                        coercionType.Name +
                        "' to a value that fits both type '" +
                        typeOne.Name +
                        "' and type '" +
                        typeTwo.Name +
                        "'");
                }

                var numberCoercerLHS = SimpleNumberCoercerFactory.GetCoercer(typeOne, coercionType);
                var numberCoercerRHS = SimpleNumberCoercerFactory.GetCoercer(typeTwo, coercionType);
                forge = new ExprEqualsNodeForgeCoercion(this, numberCoercerLHS, numberCoercerRHS);
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

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            ChildNodes[0].ToEPL(writer, Precedence);
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

            ChildNodes[1].ToEPL(writer, Precedence);
        }
    }
} // end of namespace