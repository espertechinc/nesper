///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.epl.util;

namespace com.espertech.esper.common.@internal.epl.index.advanced.index.service
{
    public class EventAdvancedIndexProvisionRuntime
    {
        public string[] IndexExpressionTexts { get; set; }

        public string[] IndexProperties { get; set; }

        public EventAdvancedIndexFactory Factory { get; set; }

        public ExprEvaluator[] ParameterEvaluators { get; set; }

        public string[] ParameterExpressionTexts { get; set; }

        public string IndexTypeName { get; set; }

        public EventAdvancedIndexConfigStatement ConfigStatement { get; set; }

        public ExprNode[] IndexExpressionsOpt { get; set; }

        public ExprNode[] ParameterExpressionsOpt { get; set; }

        public bool IsIndexExpressionsAllProps { get; set; }

        public EventAdvancedIndexProvisionCompileTime ToCompileTime(
            EventType eventTypeIndexed, StatementRawInfo statementRawInfo, StatementCompileTimeServices services)
        {
            ExprNode[] indexedExpr;
            if (IndexExpressionsOpt != null)
            {
                indexedExpr = IndexExpressionsOpt;
            }
            else
            {
                if (IsIndexExpressionsAllProps)
                {
                    indexedExpr = new ExprNode[IndexProperties.Length];
                    for (var i = 0; i < IndexProperties.Length; i++)
                    {
                        indexedExpr[i] = new ExprIdentNodeImpl(eventTypeIndexed, IndexProperties[i], 0);
                    }
                }
                else
                {
                    indexedExpr = new ExprNode[IndexProperties.Length];
                    for (var i = 0; i < IndexProperties.Length; i++)
                    {
                        indexedExpr[i] = services.CompilerServices.CompileExpression(IndexExpressionTexts[i], services);
                        indexedExpr[i] = EPLValidationUtil.ValidateSimpleGetSubtree(
                            ExprNodeOrigin.CREATEINDEXCOLUMN, indexedExpr[i], eventTypeIndexed, false, statementRawInfo,
                            services);
                    }
                }
            }

            var desc = new AdvancedIndexDescWExpr(IndexTypeName, indexedExpr);

            ExprNode[] parameters;
            if (ParameterExpressionsOpt != null)
            {
                parameters = ParameterExpressionsOpt;
            }
            else
            {
                parameters = new ExprNode[ParameterExpressionTexts.Length];
                for (var i = 0; i < ParameterExpressionTexts.Length; i++)
                {
                    parameters[i] = services.CompilerServices.CompileExpression(ParameterExpressionTexts[i], services);
                }
            }

            return new EventAdvancedIndexProvisionCompileTime(
                desc, parameters, Factory.Forge, Factory.ToConfigStatement(indexedExpr));
        }
    }
} // end of namespace