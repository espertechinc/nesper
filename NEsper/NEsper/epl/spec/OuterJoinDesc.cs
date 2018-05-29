///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.ops;
using com.espertech.esper.type;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.spec
{
    /// <summary>
    /// Contains the ON-clause criteria in an outer join.
    /// </summary>
    [Serializable]
    public class OuterJoinDesc : MetaDefItem
    {
        public static readonly OuterJoinDesc[] EMPTY_OUTERJOIN_ARRAY = new OuterJoinDesc[0];

        /// <summary>Ctor. </summary>
        /// <param name="outerJoinType">type of the outer join</param>
        /// <param name="optLeftNode">left hand identifier node</param>
        /// <param name="optRightNode">right hand identifier node</param>
        /// <param name="optAddLeftNode">additional optional left hand identifier nodes for the on-clause in a logical-and</param>
        /// <param name="optAddRightNode">additional optional right hand identifier nodes for the on-clause in a logical-and</param>
        public OuterJoinDesc(OuterJoinType outerJoinType, ExprIdentNode optLeftNode, ExprIdentNode optRightNode, ExprIdentNode[] optAddLeftNode, ExprIdentNode[] optAddRightNode)
        {
            OuterJoinType = outerJoinType;
            OptLeftNode = optLeftNode;
            OptRightNode = optRightNode;
            AdditionalLeftNodes = optAddLeftNode;
            AdditionalRightNodes = optAddRightNode;
        }

        /// <summary>Returns the type of outer join (left/right/full). </summary>
        /// <value>outer join type</value>
        public OuterJoinType OuterJoinType { get; private set; }

        /// <summary>Returns left hand identifier node. </summary>
        /// <value>left hand</value>
        public ExprIdentNode OptLeftNode { get; private set; }

        /// <summary>Returns right hand identifier node. </summary>
        /// <value>right hand</value>
        public ExprIdentNode OptRightNode { get; private set; }

        /// <summary>Returns additional properties in the on-clause, if any, that are connected via logical-and </summary>
        /// <value>additional properties</value>
        public ExprIdentNode[] AdditionalLeftNodes { get; private set; }

        /// <summary>Returns additional properties in the on-clause, if any, that are connected via logical-and </summary>
        /// <value>additional properties</value>
        public ExprIdentNode[] AdditionalRightNodes { get; private set; }

        /// <summary>Make an expression node that represents the outer join criteria as specified in the on-clause. </summary>
        /// <param name="exprEvaluatorContext">context for expression evalauation</param>
        /// <returns>expression node for outer join criteria</returns>
        public ExprNode MakeExprNode(ExprEvaluatorContext exprEvaluatorContext)
        {
            ExprNode representativeNode = new ExprEqualsNodeImpl(false, false);
            representativeNode.AddChildNode(OptLeftNode);
            representativeNode.AddChildNode(OptRightNode);
    
            if (AdditionalLeftNodes == null)
            {
                TopValidate(representativeNode, exprEvaluatorContext);
                return representativeNode;
            }
    
            ExprAndNode andNode = new ExprAndNodeImpl();
            TopValidate(representativeNode, exprEvaluatorContext);
            andNode.AddChildNode(representativeNode);
            representativeNode = andNode;
    
            for (int i = 0; i < AdditionalLeftNodes.Length; i++)
            {
                ExprEqualsNode eqNode = new ExprEqualsNodeImpl(false, false);
                eqNode.AddChildNode(AdditionalLeftNodes[i]);
                eqNode.AddChildNode(AdditionalRightNodes[i]);
                TopValidate(eqNode, exprEvaluatorContext);
                andNode.AddChildNode(eqNode);
            }
    
            TopValidate(andNode, exprEvaluatorContext);
            return representativeNode;
        }
    
        public static bool ConsistsOfAllInnerJoins(OuterJoinDesc[] outerJoinDescList)
        {
            foreach (OuterJoinDesc desc in outerJoinDescList)
            {
                if (desc.OuterJoinType != OuterJoinType.INNER)
                {
                    return false;
                }
            }
            return true;
        }
    
        public static OuterJoinDesc[] ToArray(ICollection<OuterJoinDesc> expressions)
        {
            if (expressions.IsEmpty())
            {
                return EMPTY_OUTERJOIN_ARRAY;
            }
            return expressions.ToArray();
        }
    
        private void TopValidate(ExprNode exprNode, ExprEvaluatorContext exprEvaluatorContext)
        {
            try {
                var container = exprEvaluatorContext != null
                    ? FallbackContainer.GetInstance(exprEvaluatorContext.Container)
                    : FallbackContainer.GetInstance();
                                
                var validationContext = new ExprValidationContext(
                    container,
                    null, null, null,
                    null, null, null, 
                    null, exprEvaluatorContext, null,
                    null, -1, null,
                    null, null, 
                    false, false, false, false, null, false);
                exprNode.Validate(validationContext);
            }
            catch (ExprValidationException)
            {
                throw new IllegalStateException("Failed to make representative node for outer join criteria");
            }
        }

        public static bool HasOnClauses(OuterJoinDesc[] outerJoinDescList)
        {
            foreach (OuterJoinDesc desc in outerJoinDescList)
            {
                if (desc.OptLeftNode != null)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
