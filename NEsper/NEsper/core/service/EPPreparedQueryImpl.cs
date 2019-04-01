///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.start;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Provides prepared query functionality.
    /// </summary>
    public class EPPreparedQueryImpl : EPOnDemandPreparedQuerySPI
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly EPPreparedExecuteMethod _executeMethod;
        private readonly String _epl;

        /// <summary>Ctor. </summary>
        /// <param name="executeMethod">used at execution time to obtain query results</param>
        /// <param name="epl">is the EPL to execute</param>
        public EPPreparedQueryImpl(EPPreparedExecuteMethod executeMethod, String epl)
        {
            _executeMethod = executeMethod;
            _epl = epl;
        }

        public EPOnDemandQueryResult Execute()
        {
            return ExecuteInternal(null);
        }

        public EPOnDemandQueryResult Execute(ContextPartitionSelector[] contextPartitionSelectors)
        {
            if (contextPartitionSelectors == null)
            {
                throw new ArgumentException("No context partition selectors provided");
            }
            return ExecuteInternal(contextPartitionSelectors);
        }

        private EPOnDemandQueryResult ExecuteInternal(ContextPartitionSelector[] contextPartitionSelectors)
        {
            try
            {
                EPPreparedQueryResult result = _executeMethod.Execute(contextPartitionSelectors);
                return new EPQueryResultImpl(result);
            }
            catch (EPStatementException)
            {
                throw;
            }
            catch (Exception t)
            {
                String message = "Error executing statement: " + t.Message;
                Log.Error("Error executing on-demand statement '" + _epl + "': " + t.Message, t);
                throw new EPStatementException(message, _epl, t);
            }
        }

        public EPPreparedExecuteMethod ExecuteMethod
        {
            get { return _executeMethod; }
        }

        public EventType EventType
        {
            get { return _executeMethod.EventType; }
        }
    }
}
