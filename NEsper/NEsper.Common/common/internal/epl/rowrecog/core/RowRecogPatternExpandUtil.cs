///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.rowrecog.expr;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.rowrecog.core
{
    public class RowRecogPatternExpandUtil
    {
        private static readonly RowRegexExprNodeCopierAtom ATOM_HANDLER = new RowRegexExprNodeCopierAtom();
        private static readonly RowRegexExprNodeCopierNested NESTED_HANDLER = new RowRegexExprNodeCopierNested();

        public static RowRecogExprNode Expand(RowRecogExprNode pattern)
        {
            var visitor = new RowRecogExprNodeVisitorRepeat();
            pattern.Accept(visitor);
            var newParentNode = pattern;

            // expand permutes
            IList<RowRegexPermuteDesc> permutes = visitor.Permutes;
            Collections.Sort(
                permutes, new ProxyComparator<RowRecogExprNodeVisitorRepeat.RowRegexPermuteDesc> {
                    ProcCompare = (o1, o2) => {
                        if (o1.Level > o2.Level) {
                            return -1;
                        }

                        return o1.Level == o2.Level ? 0 : 1;
                    }
                });
            foreach (RowRecogExprNodeVisitorRepeat.RowRegexPermuteDesc permute in permutes) {
                var alteration = ExpandPermute(permute.Permute);
                var optionalNewParent = Replace(
                    permute.OptionalParent, permute.Permute, Collections.SingletonList<RowRecogExprNode>(alteration));
                if (optionalNewParent != null) {
                    newParentNode = optionalNewParent;
                }
            }

            // expand atoms
            IList<Pair<RowRecogExprNodeAtom, RowRecogExprNode>> atomPairs = visitor.Atoms;
            foreach (var pair in atomPairs) {
                var atom = pair.First;
                var expandedRepeat = ExpandRepeat(atom, atom.OptionalRepeat, atom.Type, ATOM_HANDLER);
                var optionalNewParent = Replace(pair.Second, pair.First, expandedRepeat);
                if (optionalNewParent != null) {
                    newParentNode = optionalNewParent;
                }
            }

            // expand nested
            IList<RowRegexNestedDesc> nestedPairs = visitor.Nesteds;
            Collections.Sort(
                nestedPairs, new ProxyComparator<RowRecogExprNodeVisitorRepeat.RowRegexNestedDesc> {
                    ProcCompare = (o1, o2) => {
                        if (o1.Level > o2.Level) {
                            return -1;
                        }

                        return o1.Level == o2.Level ? 0 : 1;
                    }
                });
            foreach (RowRecogExprNodeVisitorRepeat.RowRegexNestedDesc pair in nestedPairs) {
                RowRecogExprNodeNested nested = pair.Nested;
                var expandedRepeat = ExpandRepeat(nested, nested.OptionalRepeat, nested.Type, NESTED_HANDLER);
                var optionalNewParent = Replace(pair.OptionalParent, pair.Nested, expandedRepeat);
                if (optionalNewParent != null) {
                    newParentNode = optionalNewParent;
                }
            }

            return newParentNode;
        }

        private static RowRecogExprNodeAlteration ExpandPermute(RowRecogExprNodePermute permute)
        {
            var e = new PermutationEnumeration(permute.ChildNodes.Count);
            var parent = new RowRecogExprNodeAlteration();
            while (e.HasMoreElements) {
                int[] indexes = e.NextElement();
                var concat = new RowRecogExprNodeConcatenation();
                parent.AddChildNode(concat);
                for (var i = 0; i < indexes.Length; i++) {
                    RowRecogExprNode toCopy = permute.ChildNodes.Get(indexes[i]);
                    var copy = CheckedCopy(toCopy);
                    concat.AddChildNode(copy);
                }
            }

            return parent;
        }

        private static RowRecogExprNode Replace(
            RowRecogExprNode optionalParent, RowRecogExprNode originalNode, IList<RowRecogExprNode> expandedRepeat)
        {
            if (optionalParent == null) {
                var newParentNode = new RowRecogExprNodeConcatenation();
                newParentNode.ChildNodes.AddAll(expandedRepeat);
                return newParentNode;
            }

            // for nested nodes, use a concatenation instead
            if (optionalParent is RowRecogExprNodeNested ||
                optionalParent is RowRecogExprNodeAlteration) {
                var concatenation = new RowRecogExprNodeConcatenation();
                concatenation.ChildNodes.AddAll(expandedRepeat);
                optionalParent.ReplaceChildNode(
                    originalNode, Collections.SingletonList<RowRecogExprNode>(concatenation));
            }
            else {
                // concatenations are simply changed
                optionalParent.ReplaceChildNode(originalNode, expandedRepeat);
            }

            return null;
        }

        private static IList<RowRecogExprNode> ExpandRepeat(
            RowRecogExprNode node,
            RowRecogExprRepeatDesc repeat,
            RowRecogNFATypeEnum type,
            RowRegexExprNodeCopier copier)
        {
            // handle single-bounds (no ranges)
            IList<RowRecogExprNode> repeated = new List<RowRecogExprNode>();
            if (repeat.Single != null) {
                ValidateExpression(repeat.Single);
                var numRepeated = (int) repeat.Single.Forge.ExprEvaluator.Evaluate(null, true, null);
                ValidateRange(numRepeated, 1, int.MaxValue);
                for (var i = 0; i < numRepeated; i++) {
                    var copy = copier.Copy(node, type);
                    repeated.Add(copy);
                }

                return repeated;
            }

            // evaluate bounds
            int? lower = null;
            int? upper = null;
            if (repeat.Lower != null) {
                ValidateExpression(repeat.Lower);
                lower = (int?) repeat.Lower.Forge.ExprEvaluator.Evaluate(null, true, null);
            }

            if (repeat.Upper != null) {
                ValidateExpression(repeat.Upper);
                upper = (int?) repeat.Upper.Forge.ExprEvaluator.Evaluate(null, true, null);
            }

            // handle range
            if (lower != null && upper != null) {
                ValidateRange(lower, 1, int.MaxValue);
                ValidateRange(upper, 1, int.MaxValue);
                ValidateRange(lower, 1, upper);
                for (var i = 0; i < lower; i++) {
                    var copy = copier.Copy(node, type);
                    repeated.Add(copy);
                }

                for (int i = lower; i < upper; i++) {
                    // makeInline type optional
                    var newType = type;
                    if (type == RowRecogNFATypeEnum.SINGLE) {
                        newType = RowRecogNFATypeEnum.ONE_OPTIONAL;
                    }
                    else if (type == RowRecogNFATypeEnum.ONE_TO_MANY) {
                        newType = RowRecogNFATypeEnum.ZERO_TO_MANY;
                    }
                    else if (type == RowRecogNFATypeEnum.ONE_TO_MANY_RELUCTANT) {
                        newType = RowRecogNFATypeEnum.ZERO_TO_MANY_RELUCTANT;
                    }

                    var copy = copier.Copy(node, newType);
                    repeated.Add(copy);
                }

                return repeated;
            }

            // handle lower-bounds only
            if (upper == null) {
                ValidateRange(lower, 1, int.MaxValue);
                for (var i = 0; i < lower; i++) {
                    var copy = copier.Copy(node, type);
                    repeated.Add(copy);
                }

                // makeInline type optional
                var newType = type;
                if (type == RowRecogNFATypeEnum.SINGLE) {
                    newType = RowRecogNFATypeEnum.ZERO_TO_MANY;
                }
                else if (type == RowRecogNFATypeEnum.ONE_OPTIONAL) {
                    newType = RowRecogNFATypeEnum.ZERO_TO_MANY;
                }
                else if (type == RowRecogNFATypeEnum.ONE_OPTIONAL_RELUCTANT) {
                    newType = RowRecogNFATypeEnum.ZERO_TO_MANY_RELUCTANT;
                }
                else if (type == RowRecogNFATypeEnum.ONE_TO_MANY) {
                    newType = RowRecogNFATypeEnum.ZERO_TO_MANY;
                }
                else if (type == RowRecogNFATypeEnum.ONE_TO_MANY_RELUCTANT) {
                    newType = RowRecogNFATypeEnum.ZERO_TO_MANY_RELUCTANT;
                }

                var copy = copier.Copy(node, newType);
                repeated.Add(copy);
                return repeated;
            }

            // handle upper-bounds only
            ValidateRange(upper, 1, int.MaxValue);
            for (var i = 0; i < upper; i++) {
                // makeInline type optional
                var newType = type;
                if (type == RowRecogNFATypeEnum.SINGLE) {
                    newType = RowRecogNFATypeEnum.ONE_OPTIONAL;
                }
                else if (type == RowRecogNFATypeEnum.ONE_TO_MANY) {
                    newType = RowRecogNFATypeEnum.ZERO_TO_MANY;
                }
                else if (type == RowRecogNFATypeEnum.ONE_TO_MANY_RELUCTANT) {
                    newType = RowRecogNFATypeEnum.ZERO_TO_MANY_RELUCTANT;
                }

                var copy = copier.Copy(node, newType);
                repeated.Add(copy);
            }

            return repeated;
        }

        private static RowRecogExprNode CheckedCopy(RowRecogExprNode inner)
        {
            try {
                return SerializableObjectCopier.Copy(inner);
            }
            catch (Exception e) {
                throw new EPException("Failed to repeat nested match-recognize: " + e.Message, e);
            }
        }

        private static void ValidateRange(int value, int min, int maxValue)
        {
            if (value < min || value > maxValue) {
                var message = "Invalid pattern quantifier value " + value + ", expecting a minimum of " + min;
                if (maxValue != int.MaxValue) {
                    message += " and maximum of " + maxValue;
                }

                throw new ExprValidationException(message);
            }
        }

        private static void ValidateExpression(ExprNode repeat)
        {
            ExprNodeUtilityValidate.ValidatePlainExpression(ExprNodeOrigin.MATCHRECOGPATTERN, repeat);

            if (!(repeat is ExprConstantNode)) {
                throw new ExprValidationException(
                    GetPatternQuantifierExpressionText(repeat) + " must return a constant value");
            }

            if (repeat.Forge.EvaluationType.GetBoxedType() != typeof(int)) {
                throw new ExprValidationException(
                    GetPatternQuantifierExpressionText(repeat) + " must return an integer-type value");
            }
        }

        private static string GetPatternQuantifierExpressionText(ExprNode exprNode)
        {
            return "Pattern quantifier '" + ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(exprNode) + "'";
        }

        private interface RowRegexExprNodeCopier
        {
            RowRecogExprNode Copy(RowRecogExprNode nodeToCopy, RowRecogNFATypeEnum newType);
        }

        public class RowRegexExprNodeCopierAtom : RowRegexExprNodeCopier
        {
            public RowRecogExprNode Copy(RowRecogExprNode nodeToCopy, RowRecogNFATypeEnum newType)
            {
                var atom = (RowRecogExprNodeAtom) nodeToCopy;
                return new RowRecogExprNodeAtom(atom.Tag, newType, null);
            }
        }

        public class RowRegexExprNodeCopierNested : RowRegexExprNodeCopier
        {
            public RowRecogExprNode Copy(RowRecogExprNode nodeToCopy, RowRecogNFATypeEnum newType)
            {
                var nested = (RowRecogExprNodeNested) nodeToCopy;
                var nestedCopy = new RowRecogExprNodeNested(newType, null);
                foreach (RowRecogExprNode inner in nested.ChildNodes) {
                    var innerCopy = CheckedCopy(inner);
                    nestedCopy.AddChildNode(innerCopy);
                }

                return nestedCopy;
            }
        }
    }
} // end of namespace