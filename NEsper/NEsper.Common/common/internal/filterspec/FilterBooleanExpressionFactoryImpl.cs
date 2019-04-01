///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.settings;

namespace com.espertech.esper.common.@internal.filterspec
{
    public class FilterBooleanExpressionFactoryImpl : FilterBooleanExpressionFactory
    {
        public static readonly FilterBooleanExpressionFactoryImpl INSTANCE = new FilterBooleanExpressionFactoryImpl();

        public ExprNodeAdapterBase Make(
            FilterSpecParamExprNode node, EventBean[] events, ExprEvaluatorContext exprEvaluatorContext,
            int agentInstanceId, StatementContextFilterEvalEnv filterEvalEnv)
        {
            // handle table evaluator context
            if (node.IsTableAccess) {
                exprEvaluatorContext = new ExprEvaluatorContextWTableAccess(
                    exprEvaluatorContext, filterEvalEnv.TableExprEvaluatorContext);
            }

            ExprNodeAdapterBase adapter;
            if (events == null) {
                // if a subquery is present in a filter stream acquire the agent instance lock
                if (node.IsFilterStreamSubquery) {
                    adapter = GetLockableSingle(
                        node, exprEvaluatorContext, filterEvalEnv.VariableManagementService,
                        filterEvalEnv.ImportServiceRuntime, filterEvalEnv.Annotations, agentInstanceId);
                }
                else if (!node.IsVariable) {
                    // no-variable no-prior event evaluation
                    adapter = new ExprNodeAdapterSSPlain(node, exprEvaluatorContext);
                }
                else {
                    // with-variable no-prior event evaluation
                    adapter = new ExprNodeAdapterSSVariables(
                        node, exprEvaluatorContext, filterEvalEnv.VariableManagementService);
                }
            }
            else {
                // pattern cases
                var variableServiceToUse = node.IsVariable ? filterEvalEnv.VariableManagementService : null;
                if (node.IsFilterStreamSubquery) {
                    adapter = GetLockableMultiStream(
                        node, exprEvaluatorContext, variableServiceToUse, filterEvalEnv.ImportServiceRuntime,
                        events, filterEvalEnv.Annotations, agentInstanceId);
                }
                else {
                    if (node.IsUseLargeThreadingProfile) {
                        adapter = new ExprNodeAdapterMSNoTL(node, exprEvaluatorContext, events, variableServiceToUse);
                    }
                    else {
                        adapter = new ExprNodeAdapterMSPlain(node, exprEvaluatorContext, events, variableServiceToUse);
                    }
                }
            }

            if (!node.IsTableAccess) {
                return adapter;
            }

            // handle table
            return new ExprNodeAdapterWTableAccess(
                node, exprEvaluatorContext, adapter, filterEvalEnv.TableExprEvaluatorContext);
        }

        protected ExprNodeAdapterBase GetLockableSingle(
            FilterSpecParamExprNode factory, ExprEvaluatorContext exprEvaluatorContext,
            VariableManagementService variableService, ImportServiceRuntime importService,
            Attribute[] annotations, int agentInstanceId)
        {
            return new ExprNodeAdapterSSStmtLock(factory, exprEvaluatorContext, variableService);
        }

        protected ExprNodeAdapterBase GetLockableMultiStream(
            FilterSpecParamExprNode factory, ExprEvaluatorContext exprEvaluatorContext,
            VariableManagementService variableServiceToUse, ImportServiceRuntime importService,
            EventBean[] events, Attribute[] annotations, int agentInstanceId)
        {
            return new ExprNodeAdapterMSStmtLock(factory, exprEvaluatorContext, events, variableServiceToUse);
        }
    }
} // end of namespace