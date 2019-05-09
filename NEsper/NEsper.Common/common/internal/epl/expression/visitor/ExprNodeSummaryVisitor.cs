///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.prev;
using com.espertech.esper.common.@internal.epl.expression.prior;
using com.espertech.esper.common.@internal.epl.expression.subquery;

namespace com.espertech.esper.common.@internal.epl.expression.visitor
{
    /// <summary>
    ///     Visitor for compiling usage informaton of special expressions within an expression tree.
    /// </summary>
    public class ExprNodeSummaryVisitor : ExprNodeVisitor
    {
        /// <summary>
        ///     Returns true if the expression is a plain-value expression, without any of the following:
        ///     properties, aggregation, subselect, stream select, previous or prior
        /// </summary>
        /// <value>true for plain</value>
        public bool IsPlain => !(HasProperties | HasAggregation | HasSubselect | HasStreamSelect | HasPreviousPrior);

        public bool HasProperties { get; private set; }

        public bool HasAggregation { get; private set; }

        public bool HasSubselect { get; private set; }

        public bool HasStreamSelect { get; private set; }

        public bool HasPreviousPrior { get; private set; }

        public bool IsVisit(ExprNode exprNode)
        {
            return true;
        }

        public void Visit(ExprNode exprNode)
        {
            if (exprNode is ExprIdentNode) {
                HasProperties = true;
            }
            else if (exprNode is ExprSubselectNode) {
                HasSubselect = true;
            }
            else if (exprNode is ExprAggregateNode) {
                HasAggregation = true;
            }
            else if (exprNode is ExprStreamUnderlyingNode) {
                HasStreamSelect = true;
            }
            else if (exprNode is ExprPriorNode || exprNode is ExprPreviousNode) {
                HasPreviousPrior = true;
            }
        }

        /// <summary>
        ///     Returns a message if the expression contains special-instruction expressions.
        /// </summary>
        /// <value>message</value>
        public string Message {
            get {
                if (HasProperties) {
                    return "event properties";
                }

                if (HasAggregation) {
                    return "aggregation functions";
                }

                if (HasSubselect) {
                    return "sub-selects";
                }

                if (HasStreamSelect) {
                    return "stream selects or event instance methods";
                }

                if (HasPreviousPrior) {
                    return "previous or prior functions";
                }

                return null;
            }
        }
    }
} // end of namespace