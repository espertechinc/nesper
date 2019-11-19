///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.collection;
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
            bool hasStringTypes = false;
            bool[] stringTypes = new bool[sortCriteriaTypes.Length];

            int count = 0;
            for (int i = 0; i < sortCriteriaTypes.Length; i++) {
                if (sortCriteriaTypes[i] == typeof(string)) {
                    hasStringTypes = true;
                    stringTypes[count] = true;
                }

                count++;
            }

            if (sortCriteriaTypes.Length > 1) {
                if ((!hasStringTypes) || (!isSortUsingCollator)) {
                    ComparatorHashableMultiKey comparatorMK = new ComparatorHashableMultiKey(isDescendingValues);
                    return new ComparatorHashableMultiKeyCasting(comparatorMK);
                }
                else {
                    ComparatorHashableMultiKeyCollating comparatorMk =
                        new ComparatorHashableMultiKeyCollating(isDescendingValues, stringTypes);
                    return new ComparatorHashableMultiKeyCasting(comparatorMk);
                }
            }
            else {
                if ((!hasStringTypes) || (!isSortUsingCollator)) {
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
            bool hasStringTypes = false;
            bool[] stringTypes = new bool[sortCriteriaTypes.Length];

            int count = 0;
            for (int i = 0; i < sortCriteriaTypes.Length; i++) {
                if (sortCriteriaTypes[i] == typeof(string)) {
                    hasStringTypes = true;
                    stringTypes[count] = true;
                }

                count++;
            }

            if (sortCriteriaTypes.Length > 1) {
                if ((!hasStringTypes) || (!isSortUsingCollator)) {
                    ComparatorObjectArray comparatorMK = new ComparatorObjectArray(isDescendingValues);
                    return new ComparatorObjectArrayCasting(comparatorMK);
                }
                else {
                    ComparatorObjectArrayCollating comparatorMk =
                        new ComparatorObjectArrayCollating(isDescendingValues, stringTypes);
                    return new ComparatorObjectArrayCasting(comparatorMk);
                }
            }
            else {
                if ((!hasStringTypes) || (!isSortUsingCollator)) {
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

        internal static Pair<ExprForge[], ExprEvaluator[]> MakeVarargArrayEval(
            MethodInfo method,
            ExprForge[] childForges)
        {
            var methodParameterTypes = method.GetParameterTypes();
            ExprEvaluator[] evals = new ExprEvaluator[methodParameterTypes.Length];
            ExprForge[] forges = new ExprForge[methodParameterTypes.Length];
            Type varargClass = methodParameterTypes[methodParameterTypes.Length - 1].GetElementType();
            Type varargClassBoxed = varargClass.GetBoxedType();
            if (methodParameterTypes.Length > 1) {
                Array.Copy(childForges, 0, forges, 0, forges.Length - 1);
            }

            int varargArrayLength = childForges.Length - methodParameterTypes.Length + 1;

            // handle passing array along
            if (varargArrayLength == 1) {
                ExprForge lastForge = childForges[methodParameterTypes.Length - 1];
                Type lastReturns = lastForge.EvaluationType;
                if (lastReturns != null && lastReturns.IsArray) {
                    forges[methodParameterTypes.Length - 1] = lastForge;
                    return new Pair<ExprForge[], ExprEvaluator[]>(forges, evals);
                }
            }

            // handle parameter conversion to vararg parameter
            ExprForge[] varargForges = new ExprForge[varargArrayLength];
            SimpleNumberCoercer[] coercers = new SimpleNumberCoercer[varargForges.Length];
            bool needCoercion = false;
            for (int i = 0; i < varargArrayLength; i++) {
                int childIndex = i + methodParameterTypes.Length - 1;
                Type resultType = childForges[childIndex].EvaluationType;
                varargForges[i] = childForges[childIndex];

                if (resultType == null && varargClass.CanBeNull()) {
                    continue;
                }

                if (TypeHelper.IsSubclassOrImplementsInterface(resultType, varargClass)) {
                    // no need to coerce
                    continue;
                }

                if (resultType.GetBoxedType() != varargClassBoxed) {
                    needCoercion = true;
                    coercers[i] = SimpleNumberCoercerFactory.GetCoercer(resultType, varargClassBoxed);
                }
            }

            ExprForge varargForge = new ExprNodeVarargOnlyArrayForge(
                varargForges,
                varargClass,
                needCoercion ? coercers : null);
            forges[methodParameterTypes.Length - 1] = varargForge;
            evals[methodParameterTypes.Length - 1] = varargForge.ExprEvaluator;
            return new Pair<ExprForge[], ExprEvaluator[]>(forges, evals);
        }

        public static ExprNode[] AddExpression(
            ExprNode[] expressions,
            ExprNode expression)
        {
            ExprNode[] target = new ExprNode[expressions.Length + 1];
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
                return ConnectExpressionsByLogicalAnd(Collections.List(nodes[0], optionalAdditionalFilter));
            }

            ExprAndNode andNode = ConnectExpressionsByLogicalAnd(nodes);
            andNode.AddChildNode(optionalAdditionalFilter);
            return andNode;
        }

        public static ExprAndNode ConnectExpressionsByLogicalAnd(ICollection<ExprNode> nodes)
        {
            if (nodes.Count < 2) {
                throw new ArgumentException("Invalid empty or 1-element list of nodes");
            }

            ExprAndNode andNode = new ExprAndNodeImpl();
            foreach (ExprNode node in nodes) {
                andNode.AddChildNode(node);
            }

            return andNode;
        }

        public static void SetChildIdentNodesOptionalEvent(ExprNode exprNode)
        {
            ExprNodeIdentifierCollectVisitor visitor = new ExprNodeIdentifierCollectVisitor();
            exprNode.Accept(visitor);
            foreach (ExprIdentNode node in visitor.ExprProperties) {
                node.IsOptionalEvent = true;
            }
        }

        public static string GetSubqueryInfoText(ExprSubselectNode subselect)
        {
            string text = "subquery number " + (subselect.SubselectNumber + 1);
            StreamSpecRaw streamRaw = subselect.StatementSpecRaw.StreamSpecs[0];
            if (streamRaw is FilterStreamSpecRaw) {
                text += " querying " + ((FilterStreamSpecRaw) streamRaw).RawFilterSpec.EventTypeName;
            }

            return text;
        }
    }
} // end of namespace