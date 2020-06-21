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
    ///     Represents an equals-for-group (= ANY/ALL/SOME (expression list)) comparator in a expression tree.
    /// </summary>
    [Serializable]
    public class ExprEqualsAllAnyNode : ExprNodeBase
    {
        [NonSerialized] private ExprEqualsAllAnyNodeForge _forge;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="isNotEquals">true if this is a (!=) not equals rather then equals, false if its a '=' equals</param>
        /// <param name="isAll">true if all, false for any</param>
        public ExprEqualsAllAnyNode(
            bool isNotEquals,
            bool isAll)
        {
            IsNot = isNotEquals;
            IsAll = isAll;
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

        /// <summary>
        ///     Returns true if this is a NOT EQUALS node, false if this is a EQUALS node.
        /// </summary>
        /// <returns>true for !=, false for =</returns>
        public bool IsNot { get; }

        /// <summary>
        ///     True if all.
        /// </summary>
        /// <returns>all-flag</returns>
        public bool IsAll { get; }

        public bool IsConstantResult => false;

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.EQUALS;

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            // Must have 2 child nodes
            if (ChildNodes.Length < 1) {
                throw new IllegalStateException("Equals group node does not have 1 or more parameters");
            }

            // Must be the same boxed type returned by expressions under this
            Type typeOne = ChildNodes[0].Forge.EvaluationType.GetBoxedType();

            // collections, array or map not supported
            if (typeOne.IsArray || typeOne.IsGenericCollection() || typeOne.IsGenericStringDictionary()) {
                throw new ExprValidationException(
                    "Collection or array comparison is not allowed for the IN, ANY, SOME or ALL keywords");
            }

            IList<Type> comparedTypes = new List<Type>();
            comparedTypes.Add(typeOne);
            var hasCollectionOrArray = false;
            for (var i = 0; i < ChildNodes.Length - 1; i++) {
                var propType = ChildNodes[i + 1].Forge.EvaluationType;
                if (propType == null) {
                    // no action
                }
                else if (propType.IsArray) {
                    hasCollectionOrArray = true;
                    if (propType.GetElementType() != typeof(object)) {
                        comparedTypes.Add(propType.GetElementType());
                    }
                }
                else if (propType.IsGenericCollection()) {
                    hasCollectionOrArray = true;
                }
                else if (propType.IsGenericStringDictionary()) {
                    hasCollectionOrArray = true;
                }
                else {
                    comparedTypes.Add(propType);
                }
            }

            // Determine common denominator type
            Type coercionTypeBoxed;
            try {
                coercionTypeBoxed = TypeHelper.GetCommonCoercionType(comparedTypes.ToArray());
            }
            catch (CoercionException ex) {
                throw new ExprValidationException("Implicit conversion not allowed: " + ex.Message);
            }

            // Check if we need to coerce
            var mustCoerce = false;
            Coercer coercer = null;
            if (coercionTypeBoxed.IsNumeric()) {
                foreach (var compareType in comparedTypes) {
                    if (coercionTypeBoxed != compareType.GetBoxedType()) {
                        mustCoerce = true;
                    }
                }

                if (mustCoerce) {
                    coercer = SimpleNumberCoercerFactory.GetCoercer(null, coercionTypeBoxed.GetBoxedType());
                }
            }

            _forge = new ExprEqualsAllAnyNodeForge(this, mustCoerce, coercer, coercionTypeBoxed, hasCollectionOrArray);
            return null;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            ChildNodes[0].ToEPL(writer, Precedence, flags);
            if (IsAll) {
                if (IsNot) {
                    writer.Write("!=all");
                }
                else {
                    writer.Write("=all");
                }
            }
            else {
                if (IsNot) {
                    writer.Write("!=any");
                }
                else {
                    writer.Write("=any");
                }
            }

            writer.Write("(");

            var delimiter = "";
            for (var i = 0; i < ChildNodes.Length - 1; i++) {
                writer.Write(delimiter);
                ChildNodes[i + 1].ToEPL(writer, Precedence, flags);
                delimiter = ",";
            }

            writer.Write(")");
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprEqualsAllAnyNode)) {
                return false;
            }

            var other = (ExprEqualsAllAnyNode) node;
            return other.IsNot == IsNot && other.IsAll == IsAll;
        }
    }
} // end of namespace