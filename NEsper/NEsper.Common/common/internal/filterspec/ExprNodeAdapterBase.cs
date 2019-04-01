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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.filterspec
{
    public abstract class ExprNodeAdapterBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        internal readonly FilterSpecParamExprNode factory;
        internal readonly ExprEvaluatorContext evaluatorContext;

        /// <summary>
        /// Evaluate the boolean expression given the event as a stream zero event.
        /// </summary>
        /// <param name="theEvent">is the stream zero event (current event)</param>
        /// <returns>boolean result of the expression</returns>
        public abstract bool Evaluate(EventBean theEvent);

        public ExprNodeAdapterBase(FilterSpecParamExprNode factory, ExprEvaluatorContext evaluatorContext)
        {
            this.factory = factory;
            this.evaluatorContext = evaluatorContext;
        }

        protected bool EvaluatePerStream(EventBean[] eventsPerStream)
        {
            try {
                Boolean result = (Boolean) factory.ExprNode.Evaluate(eventsPerStream, true, this.evaluatorContext);
                if (result == null) {
                    return false;
                }

                return result;
            }
            catch (EPException) {
                throw;
            }
            catch (Exception ex) {
                string message = "Error evaluating expression '" + factory.ExprText + "' statement '" + StatementName +
                                 "': " + ex.Message;
                Log.Error(message, ex);
                throw new EPException(message, ex);
            }
        }

        public string StatementName {
            get => evaluatorContext.StatementName;
        }

        public int StatementId {
            get => evaluatorContext.StatementId;
        }

        public int FilterBoolExprNum {
            get => factory.FilterBoolExprId;
        }

        public ExprEvaluatorContext EvaluatorContext {
            get => evaluatorContext;
        }

        public int StatementIdBoolExpr {
            get => factory.StatementIdBooleanExpr;
        }

        public string Expression {
            get => factory.ExprText;
        }

        /// <summary>
        /// NOTE: Overridden by subclasses as additional information is required for multistream-equals
        /// </summary>
        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;
            ExprNodeAdapterBase that = (ExprNodeAdapterBase) o;
            return evaluatorContext.StatementId == that.evaluatorContext.StatementId &&
                   evaluatorContext.AgentInstanceId == that.evaluatorContext.AgentInstanceId &&
                   factory.FilterBoolExprId == that.factory.FilterBoolExprId;
        }

        public override int GetHashCode()
        {
            int result = evaluatorContext.StatementId;
            result = 31 * result + evaluatorContext.AgentInstanceId;
            result = 31 * result + factory.FilterBoolExprId;
            return result;
        }
    }
} // end of namespace