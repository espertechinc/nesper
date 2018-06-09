///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.metric;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Interface for a statement-level service for coordinating the insert/remove stream generation, 
    /// native deliver to subscribers and the presence/absence of listener or subscribers to a statement.
    /// </summary>
    public interface StatementResultService
    {
        /// <summary>
        /// For initialization of the service to provide statement context.
        /// </summary>
        /// <param name="epStatement">the statement</param>
        /// <param name="epServiceProvider">the engine instance</param>
        /// <param name="isInsertInto">true if this is insert into</param>
        /// <param name="isPattern">true if this is a pattern statement</param>
        /// <param name="isDistinct">true if using distinct</param>
        /// <param name="isForClause">if set to <c>true</c> [is for clause].</param>
        /// <param name="statementMetricHandle">handle for metrics reporting</param>
        void SetContext(EPStatementSPI epStatement,
                        EPServiceProviderSPI epServiceProvider,
                        bool isInsertInto,
                        bool isPattern,
                        bool isDistinct,
                        bool isForClause,
                        StatementMetricHandle statementMetricHandle);

        /// <summary>
        /// For initialize of the service providing select clause column types and names.
        /// </summary>
        /// <param name="selectClauseTypes">types of columns in the select clause</param>
        /// <param name="selectClauseColumnNames">column names</param>
        /// <param name="forClauseDelivery">if set to <c>true</c> [for clause delivery].</param>
        /// <param name="groupDeliveryExpressions">The group delivery expressions.</param>
        /// <param name="exprEvaluatorContext">The expr evaluator context.</param>
        void SetSelectClause(Type[] selectClauseTypes,
                             String[] selectClauseColumnNames,
                             Boolean forClauseDelivery,
                             ExprEvaluator[] groupDeliveryExpressions,
                             ExprEvaluatorContext exprEvaluatorContext);

        /// <summary>Returns true to indicate that synthetic events should be produced, for use in select expression processing. </summary>
        /// <value>true to produce synthetic events</value>
        bool IsMakeSynthetic { get; }

        /// <summary>Returns true to indicate that natural events should be produced, for use in select expression processing. </summary>
        /// <value>true to produce natural (object[] column) events</value>
        bool IsMakeNatural { get; }

        /// <summary>Dispatch the remaining results, if any, to listeners as the statement is about to be stopped. </summary>
        void DispatchOnStop();

        /// <summary>
        /// Indicate a change in Update listener.
        /// </summary>
        /// <param name="updateListeners">is the new listeners and subscriber</param>
        /// <param name="isRecovery">if set to <c>true</c> [is recovery].</param>
        void SetUpdateListeners(EPStatementListenerSet updateListeners, bool isRecovery);
    
        /// <summary>Stores for dispatching the statement results. </summary>
        /// <param name="results">is the insert and remove stream data</param>
        void Indicate(UniformPair<EventBean[]> results);
    
        /// <summary>Execution of result indication. </summary>
        void Execute();

        /// <summary>
        /// Gets the name of the statement.
        /// </summary>
        /// <value>The name of the statement.</value>
        string StatementName { get; }

        /// <summary>
        /// Gets the statement id.
        /// </summary>
        /// <value>The statement id.</value>
        int StatementId { get; }

        /// <summary>
        /// Gets the statement listener set.
        /// </summary>
        /// <value>The statement listener set.</value>
        EPStatementListenerSet StatementListenerSet { get; }
    }
}
