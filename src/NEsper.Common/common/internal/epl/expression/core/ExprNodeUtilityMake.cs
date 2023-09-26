///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.expression.etc;
using com.espertech.esper.common.@internal.epl.expression.ops;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;


namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprNodeUtilityMake
    {
        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="sortCriteriaTypes">types</param>
        /// <param name="isSortUsingCollator">flag</param>
        /// <param name="isDescendingValues">flags</param>
        /// <returns>comparator</returns>
        public static IComparer<object> GetComparatorHashableMultiKeys(
            Type[] sortCriteriaTypes,
            bool isSortUsingCollator,
            bool[] isDescendingValues)
        {
            // determine string-type sorting
            var hasStringTypes = false;
            var stringTypes = new bool[sortCriteriaTypes.Length];

            var count = 0;
            for (var i = 0; i < sortCriteriaTypes.Length; i++) {
                if (sortCriteriaTypes[i] == typeof(string)) {
                    hasStringTypes = true;
                    stringTypes[count] = true;
                }

                count++;
            }

            if (sortCriteriaTypes.Length > 1) {
                if (!hasStringTypes || !isSortUsingCollator) {
                    var comparatorMK = new ComparatorHashableMultiKey(isDescendingValues);
                    return new ComparatorHashableMultiKeyCasting(comparatorMK);
                }
                else {
                    var comparatorMk = new ComparatorHashableMultiKeyCollating(isDescendingValues, stringTypes);
                    return new ComparatorHashableMultiKeyCasting(comparatorMk);
                }
            }
            else {
                if (!hasStringTypes || !isSortUsingCollator) {
                    return new ObjectComparator(isDescendingValues[0]);
                }
                else {
                    return new ObjectCollatingComparator(isDescendingValues[0]);
                }
            }
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="sortCriteriaTypes">types</param>
        /// <param name="isSortUsingCollator">flag</param>
        /// <param name="isDescendingValues">flags</param>
        /// <returns>comparator</returns>
        public static IComparer<object> GetComparatorObjectArrayNonHashable(
            Type[] sortCriteriaTypes,
            bool isSortUsingCollator,
            bool[] isDescendingValues)
        {
            // determine string-type sorting
            var hasStringTypes = false;
            var stringTypes = new bool[sortCriteriaTypes.Length];

            var count = 0;
            for (var i = 0; i < sortCriteriaTypes.Length; i++) {
                if (sortCriteriaTypes[i] == typeof(string)) {
                    hasStringTypes = true;
                    stringTypes[count] = true;
                }

                count++;
            }

            if (sortCriteriaTypes.Length > 1) {
                if (!hasStringTypes || !isSortUsingCollator) {
                    var comparatorMK = new ComparatorObjectArray(isDescendingValues);
                    return new ComparatorObjectArrayCasting(comparatorMK);
                }
                else {
                    var comparatorMk = new ComparatorObjectArrayCollating(isDescendingValues, stringTypes);
                    return new ComparatorObjectArrayCasting(comparatorMk);
                }
            }
            else {
                if (!hasStringTypes || !isSortUsingCollator) {
                    return new ObjectComparator(isDescendingValues[0]);
                }
                else {
                    return new ObjectCollatingComparator(isDescendingValues[0]);
                }
            }
        }

        public static ExprForge MakeUnderlyingForge(
            int streamNum,
            Type resultType,
            TableMetaData tableMetadata)
        {
            if (tableMetadata != null) {
                return new ExprEvalUnderlyingEvaluatorTable(streamNum, resultType, tableMetadata);
            }

            return new ExprEvalUnderlyingEvaluator(streamNum, resultType);
        }

        internal static ExprForge[] MakeVarargArrayForges(
            MethodInfo method,
            ExprForge[] childForges)
        {
            var parameters = method.GetParameters();
            var forges = new ExprForge[parameters.Length];
            var parameterType = parameters[^1].ParameterType;
            var varargClass = parameterType.GetElementType();
            var varargClassBoxed = varargClass.GetBoxedType();
            if (parameters.Length > 1) {
                Array.Copy(childForges, 0, forges, 0, forges.Length - 1);
            }

            var varargArrayLength = childForges.Length - parameters.Length + 1;

            // handle passing array along
            if (varargArrayLength == 1) {
                var lastForge = childForges[parameters.Length - 1];
                var lastReturns = lastForge.EvaluationType;
                if (lastReturns != null && lastReturns.IsArray) {
                    forges[parameters.Length - 1] = lastForge;
                    return forges;
                }
            }

            // handle parameter conversion to vararg parameter
            var varargForges = new ExprForge[varargArrayLength];
            var coercers = new Coercer[varargForges.Length];
            var needCoercion = false;
            for (var i = 0; i < varargArrayLength; i++) {
                var childIndex = i + parameters.Length - 1;
                var resultType = childForges[childIndex].EvaluationType;
                varargForges[i] = childForges[childIndex];

                if (resultType == null) {
                    if (!varargClass.IsPrimitive) {
                        continue;
                    }

                    throw new ExprValidationException(
                        "Expression returns null-typed value and varargs does not accept null values");
                }

                var resultTypeClass = resultType;
                if (TypeHelper.IsSubclassOrImplementsInterface(resultTypeClass, varargClass)) {
                    // no need to coerce
                    continue;
                }

                if (resultTypeClass.GetBoxedType() != varargClassBoxed) {
                    needCoercion = true;
                    coercers[i] = SimpleNumberCoercerFactory.GetCoercer(resultTypeClass, varargClassBoxed);
                }
            }

            ExprForge varargForge = new ExprNodeVarargOnlyArrayForge(
                varargForges,
                varargClass,
                needCoercion ? coercers : null);
            forges[parameters.Length - 1] = varargForge;
            return forges;
        }

        public static ExprNode[] AddExpression(
            ExprNode[] expressions,
            ExprNode expression)
        {
            var target = new ExprNode[expressions.Length + 1];
            Array.Copy(expressions, 0, target, 0, expressions.Length);
            target[expressions.Length] = expression;
            return target;
        }

        public static UnsupportedOperationException MakeUnsupportedCompileTime()
        {
            return new UnsupportedOperationException("The operation is not available at compile time");
        }

        public static ExprIdentNode MakeExprIdentNode(
            EventType[] typesPerStream,
            int streamId,
            string property)
        {
            return new ExprIdentNodeImpl(typesPerStream[streamId], property, streamId);
        }

        public static ExprNode ConnectExpressionsByLogicalAndWhenNeeded(ICollection<ExprNode> nodes)
        {
            if (nodes == null || nodes.IsEmpty()) {
                return null;
            }

            if (nodes.Count == 1) {
                return nodes.First();
            }

            return ConnectExpressionsByLogicalAnd(nodes);
        }

        public static ExprNode ConnectExpressionsByLogicalAndWhenNeeded(
            ExprNode left,
            ExprNode right)
        {
            if (left == null && right == null) {
                return null;
            }

            if (left != null && right == null) {
                return left;
            }

            if (left == null) {
                return right;
            }

            ExprAndNode andNode = new ExprAndNodeImpl();
            andNode.AddChildNode(left);
            andNode.AddChildNode(right);
            return andNode;
        }

        public static ExprNode ConnectExpressionsByLogicalOrWhenNeeded(ICollection<ExprNode> nodes)
        {
            if (nodes == null || nodes.IsEmpty()) {
                return null;
            }

            if (nodes.Count == 1) {
                return nodes.First();
            }

            return ConnectExpressionsByLogicalOr(nodes);
        }

        public static ExprNode ConnectExpressionsByLogicalAnd(
            IList<ExprNode> nodes,
            ExprNode optionalAdditionalFilter)
        {
            if (nodes.IsEmpty()) {
                return optionalAdditionalFilter;
            }

            if (optionalAdditionalFilter == null) {
                if (nodes.Count == 1) {
                    return nodes[0];
                }

                return ConnectExpressionsByLogicalAnd(nodes);
            }

            if (nodes.Count == 1) {
                return ConnectExpressionsByLogicalAnd(Arrays.AsList(nodes[0], optionalAdditionalFilter));
            }

            var andNode = ConnectExpressionsByLogicalAnd(nodes);
            andNode.AddChildNode(optionalAdditionalFilter);
            return andNode;
        }

        public static ExprAndNode ConnectExpressionsByLogicalAnd(ICollection<ExprNode> nodes)
        {
            if (nodes.Count < 2) {
                throw new ArgumentException("Invalid empty or 1-element list of nodes");
            }

            ExprAndNode andNode = new ExprAndNodeImpl();
            foreach (var node in nodes) {
                andNode.AddChildNode(node);
            }

            return andNode;
        }

        public static ExprOrNode ConnectExpressionsByLogicalOr(ICollection<ExprNode> nodes)
        {
            if (nodes.Count < 2) {
                throw new ArgumentException("Invalid empty or 1-element list of nodes");
            }

            var orNode = new ExprOrNode();
            foreach (var node in nodes) {
                orNode.AddChildNode(node);
            }

            return orNode;
        }

        public static void SetChildIdentNodesOptionalEvent(ExprNode exprNode)
        {
            var visitor = new ExprNodeIdentifierCollectVisitor();
            exprNode.Accept(visitor);
            foreach (var node in visitor.ExprProperties) {
                node.IsOptionalEvent = true;
            }
        }

        public static string GetSubqueryInfoText(ExprSubselectNode subselect)
        {
            var text = "subquery number " + (subselect.SubselectNumber + 1);
            var streamRaw = subselect.StatementSpecRaw.StreamSpecs[0];
            if (streamRaw is FilterStreamSpecRaw raw) {
                text += " querying " + raw.RawFilterSpec.EventTypeName;
            }

            return text;
        }
    }
} // end of namespace