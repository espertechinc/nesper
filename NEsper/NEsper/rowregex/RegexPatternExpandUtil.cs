///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

namespace com.espertech.esper.rowregex
{
    public class RegexPatternExpandUtil
    {
        private static readonly RowRegexExprNodeCopierAtom ATOM_HANDLER = new RowRegexExprNodeCopierAtom();
        //private static readonly RowRegexExprNodeCopierNested NESTED_HANDLER = new RowRegexExprNodeCopierNested();

        public static RowRegexExprNode Expand(
            IContainer container, RowRegexExprNode pattern)
        {
            var visitor = new RowRegexExprNodeVisitorRepeat();
            pattern.Accept(visitor);
            var newParentNode = pattern;

            // expand permutes
            var permutes = visitor.Permutes;
            permutes.SortInPlace((o1, o2) =>
            {
                if (o1.Level > o2.Level)
                {
                    return -1;
                }
                return o1.Level == o2.Level ? 0 : 1;
            });

            foreach (var permute in permutes)
            {
                var alteration = ExpandPermute(container, permute.Permute);
                RowRegexExprNode optionalNewParent = Replace(permute.OptionalParent, permute.Permute, Collections.SingletonList<RowRegexExprNode>(alteration));
                if (optionalNewParent != null)
                {
                    newParentNode = optionalNewParent;
                }
            }

            // expand atoms
            var atomPairs = visitor.Atoms;
            foreach (var pair in atomPairs)
            {
                var atom = pair.First;
                var expandedRepeat = ExpandRepeat(atom, atom.OptionalRepeat, atom.NFAType, ATOM_HANDLER);
                var optionalNewParent = Replace(pair.Second, pair.First, expandedRepeat);
                if (optionalNewParent != null)
                {
                    newParentNode = optionalNewParent;
                }
            }

            // expand nested
            var nestedPairs = visitor.Nesteds;
            nestedPairs.SortInPlace((o1, o2) =>
            {
                if (o1.Level > o2.Level)
                {
                    return -1;
                }
                return o1.Level == o2.Level ? 0 : 1;
            });

            var nestedHandler = new RowRegexExprNodeCopierNested(container);
            foreach (var pair in nestedPairs)
            {
                var nested = pair.Nested;
                var expandedRepeat = ExpandRepeat(nested, nested.OptionalRepeat, nested.NFAType, nestedHandler);
                var optionalNewParent = Replace(pair.OptionalParent, pair.Nested, expandedRepeat);
                if (optionalNewParent != null)
                {
                    newParentNode = optionalNewParent;
                }
            }

            return newParentNode;
        }

        private static RowRegexExprNodeAlteration ExpandPermute(
            IContainer container, RowRegexExprNodePermute permute)
        {
            var e = PermutationEnumerator.Create(permute.ChildNodes.Count);
            var parent = new RowRegexExprNodeAlteration();
            foreach (int[] indexes in e)
            {
                var concat = new RowRegexExprNodeConcatenation();
                parent.AddChildNode(concat);
                for (var i = 0; i < indexes.Length; i++)
                {
                    RowRegexExprNode toCopy = permute.ChildNodes[indexes[i]];
                    var copy = CheckedCopy(container, toCopy);
                    concat.AddChildNode(copy);
                }
            }
            return parent;
        }

        private static RowRegexExprNode Replace(RowRegexExprNode optionalParent, RowRegexExprNode originalNode, IList<RowRegexExprNode> expandedRepeat)
        {
            if (optionalParent == null)
            {
                var newParentNode = new RowRegexExprNodeConcatenation();
                newParentNode.ChildNodes.AddAll(expandedRepeat);
                return newParentNode;
            }

            // for nested nodes, use a concatenation instead
            if (optionalParent is RowRegexExprNodeNested ||
                    optionalParent is RowRegexExprNodeAlteration)
            {
                var concatenation = new RowRegexExprNodeConcatenation();
                concatenation.ChildNodes.AddAll(expandedRepeat);
                optionalParent.ReplaceChildNode(originalNode, Collections.SingletonList<RowRegexExprNode>(concatenation));
            }
            // concatenations are simply changed
            else
            {
                optionalParent.ReplaceChildNode(originalNode, expandedRepeat);
            }

            return null;
        }

        private static IList<RowRegexExprNode> ExpandRepeat(
            RowRegexExprNode node,
            RowRegexExprRepeatDesc repeat,
            RegexNFATypeEnum type,
            RowRegexExprNodeCopier copier)
        {
            var evaluateParams = new EvaluateParams(null, true, null);
            // handle single-bounds (no ranges)
            IList<RowRegexExprNode> repeated = new List<RowRegexExprNode>();
            if (repeat.Single != null)
            {
                ValidateExpression(repeat.Single);
                int numRepeated = repeat.Single.ExprEvaluator.Evaluate(evaluateParams).AsInt();
                ValidateRange(numRepeated, 1, int.MaxValue);
                for (var i = 0; i < numRepeated; i++)
                {
                    var copy = copier.Copy(node, type);
                    repeated.Add(copy);
                }
                return repeated;
            }

            // evaluate bounds
            int? lower = null;
            int? upper = null;
            if (repeat.Lower != null)
            {
                ValidateExpression(repeat.Lower);
                lower = (int?)repeat.Lower.ExprEvaluator.Evaluate(evaluateParams);
            }
            if (repeat.Upper != null)
            {
                ValidateExpression(repeat.Upper);
                upper = (int?)repeat.Upper.ExprEvaluator.Evaluate(evaluateParams);
            }

            // handle range
            if (lower != null && upper != null)
            {
                ValidateRange(lower.Value, 1, int.MaxValue);
                ValidateRange(upper.Value, 1, int.MaxValue);
                ValidateRange(lower.Value, 1, upper.Value);
                for (var i = 0; i < lower; i++)
                {
                    var copy = copier.Copy(node, type);
                    repeated.Add(copy);
                }
                for (int i = lower.Value; i < upper; i++)
                {
                    // make type optional
                    var newType = type;
                    if (type == RegexNFATypeEnum.SINGLE)
                    {
                        newType = RegexNFATypeEnum.ONE_OPTIONAL;
                    }
                    else if (type == RegexNFATypeEnum.ONE_TO_MANY)
                    {
                        newType = RegexNFATypeEnum.ZERO_TO_MANY;
                    }
                    else if (type == RegexNFATypeEnum.ONE_TO_MANY_RELUCTANT)
                    {
                        newType = RegexNFATypeEnum.ZERO_TO_MANY_RELUCTANT;
                    }
                    var copy = copier.Copy(node, newType);
                    repeated.Add(copy);
                }
                return repeated;
            }

            // handle lower-bounds only
            if (upper == null)
            {
                ValidateRange(lower.Value, 1, int.MaxValue);
                for (var i = 0; i < lower; i++)
                {
                    var copyInner = copier.Copy(node, type);
                    repeated.Add(copyInner);
                }
                // make type optional
                var newType = type;
                if (type == RegexNFATypeEnum.SINGLE)
                {
                    newType = RegexNFATypeEnum.ZERO_TO_MANY;
                }
                else if (type == RegexNFATypeEnum.ONE_OPTIONAL)
                {
                    newType = RegexNFATypeEnum.ZERO_TO_MANY;
                }
                else if (type == RegexNFATypeEnum.ONE_OPTIONAL_RELUCTANT)
                {
                    newType = RegexNFATypeEnum.ZERO_TO_MANY_RELUCTANT;
                }
                else if (type == RegexNFATypeEnum.ONE_TO_MANY)
                {
                    newType = RegexNFATypeEnum.ZERO_TO_MANY;
                }
                else if (type == RegexNFATypeEnum.ONE_TO_MANY_RELUCTANT)
                {
                    newType = RegexNFATypeEnum.ZERO_TO_MANY_RELUCTANT;
                }
                var copy = copier.Copy(node, newType);
                repeated.Add(copy);
                return repeated;
            }

            // handle upper-bounds only
            ValidateRange(upper.Value, 1, int.MaxValue);
            for (var i = 0; i < upper; i++)
            {
                // make type optional
                var newType = type;
                if (type == RegexNFATypeEnum.SINGLE)
                {
                    newType = RegexNFATypeEnum.ONE_OPTIONAL;
                }
                else if (type == RegexNFATypeEnum.ONE_TO_MANY)
                {
                    newType = RegexNFATypeEnum.ZERO_TO_MANY;
                }
                else if (type == RegexNFATypeEnum.ONE_TO_MANY_RELUCTANT)
                {
                    newType = RegexNFATypeEnum.ZERO_TO_MANY_RELUCTANT;
                }
                var copy = copier.Copy(node, newType);
                repeated.Add(copy);
            }
            return repeated;
        }

        private static RowRegexExprNode CheckedCopy(IContainer container, RowRegexExprNode inner)
        {
            try
            {
                return (RowRegexExprNode)SerializableObjectCopier.Copy(container, inner);
            }
            catch (Exception e)
            {
                throw new EPException("Failed to repeat nested match-recognize: " + e.Message, e);
            }
        }

        private static void ValidateRange(int value, int min, int maxValue)
        {
            if (value < min || value > maxValue)
            {
                var message = "Invalid pattern quantifier value " + value + ", expecting a minimum of " + min;
                if (maxValue != int.MaxValue)
                {
                    message += " and maximum of " + maxValue;
                }
                throw new ExprValidationException(message);
            }
        }

        private static void ValidateExpression(ExprNode repeat)
        {
            ExprNodeUtility.ValidatePlainExpression(ExprNodeOrigin.MATCHRECOGPATTERN, repeat);
            if (!repeat.IsConstantResult)
            {
                throw new ExprValidationException(GetPatternQuantifierExpressionText(repeat) + " must return a constant value");
            }
            if (repeat.ExprEvaluator.ReturnType.IsNotInt32()) {
                throw new ExprValidationException(GetPatternQuantifierExpressionText(repeat) + " must return an integer-type value");
            }
        }

        private interface RowRegexExprNodeCopier
        {
            RowRegexExprNode Copy(RowRegexExprNode nodeToCopy, RegexNFATypeEnum newType);
        }

        private class RowRegexExprNodeCopierAtom : RowRegexExprNodeCopier
        {
            public RowRegexExprNode Copy(
                RowRegexExprNode nodeToCopy, 
                RegexNFATypeEnum newType)
            {
                var atom = (RowRegexExprNodeAtom)nodeToCopy;
                return new RowRegexExprNodeAtom(atom.Tag, newType, null);
            }
        }

        private class RowRegexExprNodeCopierNested : RowRegexExprNodeCopier
        {
            private readonly IContainer _container;

            public RowRegexExprNodeCopierNested(IContainer container)
            {
                _container = container;
            }

            public RowRegexExprNode Copy(
                RowRegexExprNode nodeToCopy, 
                RegexNFATypeEnum newType)
            {
                var nested = (RowRegexExprNodeNested)nodeToCopy;
                var nestedCopy = new RowRegexExprNodeNested(newType, null);
                foreach (var inner in nested.ChildNodes)
                {
                    var innerCopy = CheckedCopy(_container, inner);
                    nestedCopy.AddChildNode(innerCopy);
                }
                return nestedCopy;
            }
        }

        private static String GetPatternQuantifierExpressionText(ExprNode exprNode)
        {
            return "pattern quantifier '" + ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(exprNode) + "'";
        }
    }
} // end of namespace
