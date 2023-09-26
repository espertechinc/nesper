///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.filterspec
{
    public abstract class ExprNodeAdapterBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal readonly ExprEvaluatorContext evaluatorContext;

        internal readonly FilterSpecParamExprNode factory;

        public ExprNodeAdapterBase(
            FilterSpecParamExprNode factory,
            ExprEvaluatorContext evaluatorContext)
        {
            this.factory = factory;
            this.evaluatorContext = evaluatorContext;
        }

        public string StatementName => evaluatorContext.StatementName;

        public int StatementId => evaluatorContext.StatementId;

        public int FilterBoolExprNum => factory.FilterBoolExprId;

        public ExprEvaluatorContext EvaluatorContext => evaluatorContext;

        public int StatementIdBoolExpr => factory.StatementIdBooleanExpr;

        public string Expression => factory.ExprText;

        /// <summary>
        ///     Evaluate the boolean expression given the event as a stream zero event.
        /// </summary>
        /// <param name="theEvent">is the stream zero event (current event)</param>
        /// <returns>boolean result of the expression</returns>
        public abstract bool Evaluate(EventBean theEvent);

        protected bool EvaluatePerStream(EventBean[] eventsPerStream)
        {
            try {
                var result = factory.ExprNode.Evaluate(eventsPerStream, true, evaluatorContext);
                if (result == null) {
                    return false;
                }

                return true.Equals(result);
            }
            catch (EPException) {
                throw;
            }
            catch (Exception ex) {
                var message = "Error evaluating expression '" +
                              factory.ExprText +
                              "' statement '" +
                              StatementName +
                              "': " +
                              ex.Message;
                Log.Error(message, ex);
                throw new EPException(message, ex);
            }
        }

        /// <summary>
        ///     NOTE: Overridden by subclasses as additional information is required for multistream-equals
        /// </summary>
        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (ExprNodeAdapterBase)o;
            return evaluatorContext.StatementId == that.evaluatorContext.StatementId &&
                   evaluatorContext.AgentInstanceId == that.evaluatorContext.AgentInstanceId &&
                   factory.FilterBoolExprId == that.factory.FilterBoolExprId;
        }

        public override int GetHashCode()
        {
            var result = evaluatorContext.StatementId;
            result = 31 * result + evaluatorContext.AgentInstanceId;
            result = 31 * result + factory.FilterBoolExprId;
            return result;
        }
    }
} // end of namespace