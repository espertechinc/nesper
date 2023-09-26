///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    /// Represents an equals (=) comparator in a filter expressiun tree.
    /// </summary>
    public class ExprEqualsNodeImpl : ExprNodeBase,
        ExprEqualsNode
    {
        private readonly bool _isNotEquals;
        private readonly bool _isIs;
        [NonSerialized] private ExprEqualsNodeForge _forge;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name = "isNotEquals">true if this is a (!=) not equals rather then equals, false if its a '=' equals</param>
        /// <param name = "isIs">true when "is" or "is not" (instead of = or &amp;lt;&amp;gt;)</param>
        public ExprEqualsNodeImpl(
            bool isNotEquals,
            bool isIs)
        {
            this._isNotEquals = isNotEquals;
            this._isIs = isIs;
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
                _forge = new ExprEqualsNodeForgeNC(this);
                return null;
            }

            if (typeOne.Equals(typeTwo) || typeOne.IsAssignableFrom(typeTwo)) {
                _forge = new ExprEqualsNodeForgeNC(this);
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
                _forge = new ExprEqualsNodeForgeNC(this);
            }
            else {
                if (typeOne.IsArray && typeTwo.IsArray) {
                    var typeOneElement = typeOne.GetElementType();
                    var typeTwoElement = typeTwo.GetElementType();
                    // Check to see if we have a "boxed" element trying to compare against an unboxed element.  We can
                    // coerce this with a custom widener.
                    if (typeOneElement.GetBoxedType() == typeTwoElement.GetBoxedType()) {
                        coercionType = typeOneElement.GetBoxedType().MakeArrayType();
                        var coercerLhs = ArrayCoercerFactory.GetCoercer(typeOne, coercionType);
                        var coercerRhs = ArrayCoercerFactory.GetCoercer(typeTwo, coercionType);
                        _forge = new ExprEqualsNodeForgeCoercion(
                            this,
                            coercerLhs,
                            coercerRhs,
                            typeOneElement,
                            typeTwoElement);
                        return null;
                    }
                }

                if (!coercionType.IsTypeNumeric()) {
                    throw new ExprValidationException(
                        "Cannot convert datatype '" +
                        coercionType.CleanName() +
                        "' to a value that fits both type '" +
                        typeOne.CleanName() +
                        "' and type '" +
                        typeTwo.CleanName() +
                        "'");
                }

                var numberCoercerLHS = SimpleNumberCoercerFactory.GetCoercer(typeOne, coercionType);
                var numberCoercerRHS = SimpleNumberCoercerFactory.GetCoercer(typeTwo, coercionType);
                _forge = new ExprEqualsNodeForgeCoercion(this, numberCoercerLHS, numberCoercerRHS, typeOne, typeTwo);
            }

            return null;
        }

        public bool IsConstantResult => false;

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            ChildNodes[0].ToEPL(writer, Precedence, flags);
            if (_isIs) {
                writer.Write(" is ");
                if (_isNotEquals) {
                    writer.Write("not ");
                }
            }
            else {
                if (!_isNotEquals) {
                    writer.Write("=");
                }
                else {
                    writer.Write("!=");
                }
            }

            ChildNodes[1].ToEPL(writer, Precedence, flags);
        }

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.EQUALS;

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprEqualsNode other)) {
                return false;
            }

            return other.IsNotEquals == _isNotEquals;
        }

        public bool IsNotEquals => _isNotEquals;

        public bool IsIs => _isIs;

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

        public IDictionary<string, object> EventType => null;
    }
} // end of namespace