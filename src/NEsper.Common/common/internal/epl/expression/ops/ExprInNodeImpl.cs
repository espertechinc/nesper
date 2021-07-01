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
using System.Linq;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    /// <summary>
    /// Represents the in-clause (set check) function in an expression tree.
    /// </summary>
    [Serializable]
    public class ExprInNodeImpl : ExprNodeBase,
        ExprInNode
    {
        private readonly bool _isNotIn;

        [NonSerialized] private ExprInNodeForge _forge;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="isNotIn">is true for "not in" and false for "in"</param>
        public ExprInNodeImpl(bool isNotIn)
        {
            this._isNotIn = isNotIn;
        }

        public ExprEvaluator ExprEvaluator {
            get {
                CheckValidated(_forge);
                return _forge.ExprEvaluator;
            }
        }

        public override ExprForge Forge {
            get => _forge;
        }

        /// <summary>
        /// Returns true for not-in, false for regular in
        /// </summary>
        /// <returns>false for "val in (a,b,c)" or true for "val not in (a,b,c)"</returns>
        public bool IsNotIn {
            get => _isNotIn;
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            ValidateWithoutContext();
            return null;
        }

        public void ValidateWithoutContext()
        {
            if (ChildNodes.Length < 2) {
                throw new ExprValidationException("The IN operator requires at least 2 child expressions");
            }

            // Must be the same boxed type returned by expressions under this
            Type typeOne = Boxing.GetBoxedType(ChildNodes[0].Forge.EvaluationType);

            // collections, array or map not supported
            if ((typeOne.IsArray) ||
                (typeOne.IsGenericCollection()) ||
                (typeOne.IsGenericDictionary())) {
                throw new ExprValidationException(
                    "Collection or array comparison is not allowed for the IN, ANY, SOME or ALL keywords");
            }

            IList<Type> comparedTypes = new List<Type>();
            comparedTypes.Add(typeOne);
            bool hasCollectionOrArray = false;
            for (int i = 0; i < ChildNodes.Length - 1; i++) {
                Type propType = ChildNodes[i + 1].Forge.EvaluationType;
                if (propType == null) {
                    continue;
                }

                if (propType.IsArray) {
                    hasCollectionOrArray = true;
                    if (propType.GetElementType() != typeof(object)) {
                        comparedTypes.Add(propType.GetElementType());
                    }
                }
                else if (propType.IsGenericCollection()) {
                    hasCollectionOrArray = true;
                }
                else if (propType.IsGenericDictionary()) {
                    hasCollectionOrArray = true;
                }
                else {
                    comparedTypes.Add(propType);
                }
            }

            // Determine common denominator type
            Type coercionType;
            try {
                coercionType = TypeHelper.GetCommonCoercionType(comparedTypes.ToArray());
            }
            catch (CoercionException ex) {
                throw new ExprValidationException("Implicit conversion not allowed: " + ex.Message);
            }

            // Check if we need to coerce
            bool mustCoerce = false;
            Coercer coercer = null;
            if (TypeHelper.IsNumeric(coercionType)) {
                foreach (Type compareType in comparedTypes) {
                    if (coercionType != Boxing.GetBoxedType(compareType)) {
                        mustCoerce = true;
                    }
                }

                if (mustCoerce) {
                    coercer = SimpleNumberCoercerFactory.GetCoercer(null, Boxing.GetBoxedType(coercionType));
                }
            }

            _forge = new ExprInNodeForge(this, mustCoerce, coercer, coercionType, hasCollectionOrArray);
        }

        public bool IsConstantResult {
            get => false;
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprInNodeImpl)) {
                return false;
            }

            ExprInNodeImpl other = (ExprInNodeImpl) node;
            return other._isNotIn == _isNotIn;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            string delimiter = "";
            ChildNodes[0].ToEPL(writer, Precedence, flags);
            if (_isNotIn) {
                writer.Write(" not in (");
            }
            else {
                writer.Write(" in (");
            }

            for (int ii = 1 ; ii < ChildNodes.Length; ii++) {
                var inSetValueExpr = ChildNodes[ii];
                writer.Write(delimiter);
                inSetValueExpr.ToEPL(writer, Precedence, flags);
                delimiter = ",";
            }

            writer.Write(')');
        }

        public override ExprPrecedenceEnum Precedence {
            get => ExprPrecedenceEnum.RELATIONAL_BETWEEN_IN;
        }
    }
} // end of namespace