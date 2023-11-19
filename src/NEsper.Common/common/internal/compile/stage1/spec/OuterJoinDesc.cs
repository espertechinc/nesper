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

using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.ops;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>
    ///     Contains the ON-clause criteria in an outer join.
    /// </summary>
    public class OuterJoinDesc
    {
        public static readonly OuterJoinDesc[] EMPTY_OUTERJOIN_ARRAY = Array.Empty<OuterJoinDesc>();

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="outerJoinType">type of the outer join</param>
        /// <param name="optLeftNode">left hand identifier node</param>
        /// <param name="optRightNode">right hand identifier node</param>
        /// <param name="optAddLeftNode">additional optional left hand identifier nodes for the on-clause in a logical-and</param>
        /// <param name="optAddRightNode">additional optional right hand identifier nodes for the on-clause in a logical-and</param>
        public OuterJoinDesc(
            OuterJoinType outerJoinType,
            ExprIdentNode optLeftNode,
            ExprIdentNode optRightNode,
            ExprIdentNode[] optAddLeftNode,
            ExprIdentNode[] optAddRightNode)
        {
            OuterJoinType = outerJoinType;
            OptLeftNode = optLeftNode;
            OptRightNode = optRightNode;
            AdditionalLeftNodes = optAddLeftNode;
            AdditionalRightNodes = optAddRightNode;
        }

        /// <summary>
        ///     Returns the type of outer join (left/right/full).
        /// </summary>
        /// <returns>outer join type</returns>
        public OuterJoinType OuterJoinType { get; }

        /// <summary>
        ///     Returns left hand identifier node.
        /// </summary>
        /// <returns>left hand</returns>
        public ExprIdentNode OptLeftNode { get; }

        /// <summary>
        ///     Returns right hand identifier node.
        /// </summary>
        /// <returns>right hand</returns>
        public ExprIdentNode OptRightNode { get; }

        /// <summary>
        ///     Returns additional properties in the on-clause, if any, that are connected via logical-and
        /// </summary>
        /// <returns>additional properties</returns>
        public ExprIdentNode[] AdditionalLeftNodes { get; }

        /// <summary>
        ///     Returns additional properties in the on-clause, if any, that are connected via logical-and
        /// </summary>
        /// <returns>additional properties</returns>
        public ExprIdentNode[] AdditionalRightNodes { get; }

        public ExprNode MakeExprNode(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            ExprNode representativeNode = new ExprEqualsNodeImpl(false, false);
            representativeNode.AddChildNode(OptLeftNode);
            representativeNode.AddChildNode(OptRightNode);

            if (AdditionalLeftNodes == null) {
                TopValidate(representativeNode, statementRawInfo, compileTimeServices);
                return representativeNode;
            }

            ExprAndNode andNode = new ExprAndNodeImpl();
            TopValidate(representativeNode, statementRawInfo, compileTimeServices);
            andNode.AddChildNode(representativeNode);
            representativeNode = andNode;

            for (var i = 0; i < AdditionalLeftNodes.Length; i++) {
                ExprEqualsNode eqNode = new ExprEqualsNodeImpl(false, false);
                eqNode.AddChildNode(AdditionalLeftNodes[i]);
                eqNode.AddChildNode(AdditionalRightNodes[i]);
                TopValidate(eqNode, statementRawInfo, compileTimeServices);
                andNode.AddChildNode(eqNode);
            }

            TopValidate(andNode, statementRawInfo, compileTimeServices);
            return representativeNode;
        }

        public static bool ConsistsOfAllInnerJoins(OuterJoinDesc[] outerJoinDescList)
        {
            foreach (var desc in outerJoinDescList) {
                if (desc.OuterJoinType != OuterJoinType.INNER) {
                    return false;
                }
            }

            return true;
        }

        public static OuterJoinDesc[] ToArray(ICollection<OuterJoinDesc> expressions)
        {
            if (expressions.IsEmpty()) {
                return EMPTY_OUTERJOIN_ARRAY;
            }

            return expressions.ToArray();
        }

        private void TopValidate(
            ExprNode exprNode,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            try {
                var validationContext =
                    new ExprValidationContextBuilder(null, statementRawInfo, compileTimeServices).Build();
                exprNode.Validate(validationContext);
            }
            catch (ExprValidationException) {
                throw new IllegalStateException("Failed to make representative node for outer join criteria");
            }
        }

        public static bool HasOnClauses(OuterJoinDesc[] outerJoinDescList)
        {
            foreach (var desc in outerJoinDescList) {
                if (desc.OptLeftNode != null) {
                    return true;
                }
            }

            return false;
        }
    }
} // end of namespace