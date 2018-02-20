///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.variable;

namespace com.espertech.esper.filter
{
    public class FilterBooleanExpressionFactoryImpl : FilterBooleanExpressionFactory {
    
        public ExprNodeAdapterBase Make(
            FilterSpecParamExprNode node, 
            EventBean[] events,
            ExprEvaluatorContext exprEvaluatorContext, 
            StatementContext statementContext,
            int agentInstanceId) {
    
            int filterSpecId = node.FilterSpecId;
            int filterSpecParamPathNum = node.FilterSpecParamPathNum;
            ExprNode exprNode = node.ExprNode;
            VariableService variableService = node.VariableService;
    
            // handle table evaluator context
            if (node.HasTableAccess) {
                exprEvaluatorContext = new ExprEvaluatorContextWTableAccess(exprEvaluatorContext, node.TableService);
            }
    
            // non-pattern case
            ExprNodeAdapterBase adapter;
            if (events == null) {
    
                // if a subquery is present in a filter stream acquire the agent instance lock
                if (node.HasFilterStreamSubquery) {
                    adapter = GetLockableSingle(filterSpecId, filterSpecParamPathNum, exprNode, exprEvaluatorContext, variableService, statementContext, agentInstanceId);
                } else if (!node.HasVariable) {
                    // no-variable no-prior event evaluation
                    adapter = new ExprNodeAdapterBase(filterSpecId, filterSpecParamPathNum, exprNode, exprEvaluatorContext);
                } else {
                    // with-variable no-prior event evaluation
                    adapter = new ExprNodeAdapterBaseVariables(filterSpecId, filterSpecParamPathNum, exprNode, exprEvaluatorContext, variableService);
                }
            } else {
                // pattern cases
                VariableService variableServiceToUse = node.HasVariable ? variableService : null;
                if (node.UseLargeThreadingProfile) {
                    // no-threadlocal evaluation
                    // if a subquery is present in a pattern filter acquire the agent instance lock
                    if (node.HasFilterStreamSubquery) {
                        adapter = GetLockableMultiStreamNoTL(filterSpecId, filterSpecParamPathNum, exprNode, exprEvaluatorContext, variableServiceToUse, events);
                    } else {
                        adapter = new ExprNodeAdapterMultiStreamNoTL(
                            filterSpecId,
                            filterSpecParamPathNum,
                            exprNode, 
                            exprEvaluatorContext, 
                            variableServiceToUse,
                            events,
                            statementContext.ThreadLocalManager);
                    }
                } else {
                    if (node.HasFilterStreamSubquery) {
                        adapter = GetLockableMultiStream(filterSpecId, filterSpecParamPathNum, exprNode, exprEvaluatorContext, variableServiceToUse, events, statementContext.ThreadLocalManager);
                    } else {
                        // evaluation with threadlocal cache
                        adapter = new ExprNodeAdapterMultiStream(
                            filterSpecId,
                            filterSpecParamPathNum,
                            exprNode,
                            exprEvaluatorContext,
                            variableServiceToUse,
                            events,
                            statementContext.ThreadLocalManager);
                    }
                }
            }
    
            if (!node.HasTableAccess) {
                return adapter;
            }
    
            // handle table
            return new ExprNodeAdapterBaseWTableAccess(filterSpecId, filterSpecParamPathNum, exprNode, exprEvaluatorContext, adapter, node.TableService);
        }
    
        protected ExprNodeAdapterBase GetLockableSingle(int filterSpecId, int filterSpecParamPathNum, ExprNode exprNode, ExprEvaluatorContext exprEvaluatorContext, VariableService variableService, StatementContext statementContext, int agentInstanceId) {
            return new ExprNodeAdapterBaseStmtLock(filterSpecId, filterSpecParamPathNum, exprNode, exprEvaluatorContext, variableService);
        }
    
        protected ExprNodeAdapterBase GetLockableMultiStreamNoTL(int filterSpecId, int filterSpecParamPathNum, ExprNode exprNode, ExprEvaluatorContext exprEvaluatorContext, VariableService variableServiceToUse, EventBean[] events) {
            return new ExprNodeAdapterMultiStreamNoTLStmtLock(
                filterSpecId, 
                filterSpecParamPathNum, 
                exprNode, 
                exprEvaluatorContext, 
                variableServiceToUse, 
                events,
                exprEvaluatorContext.Container.Resolve<IThreadLocalManager>());
        }
    
        protected ExprNodeAdapterBase GetLockableMultiStream(int filterSpecId, int filterSpecParamPathNum, ExprNode exprNode, ExprEvaluatorContext exprEvaluatorContext, VariableService variableServiceToUse, EventBean[] events, IThreadLocalManager threadLocalManager) {
            return new ExprNodeAdapterMultiStreamStmtLock(filterSpecId, filterSpecParamPathNum, exprNode, exprEvaluatorContext, variableServiceToUse, events, threadLocalManager);
        }
    }
} // end of namespace
