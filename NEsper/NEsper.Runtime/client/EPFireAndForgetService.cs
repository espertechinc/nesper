///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.client.fireandforget;

namespace com.espertech.esper.runtime.client
{
    /// <summary>
    /// Executes fire-and-forget non-continuous on-demand queries against named windows or tables.
    /// <para />Compile queries use the compile-query methods of the compiler.
    /// </summary>
    public interface EPFireAndForgetService
    {
        /// <summary>
        /// Execute a fire-and-forget query.
        /// </summary>
        /// <param name="compiled">is the compiled EPL query to execute</param>
        /// <returns>query result</returns>
        EPFireAndForgetQueryResult ExecuteQuery(EPCompiled compiled);

        /// <summary>
        /// Execute a fire-and-forget query for use with named windows and tables that have a context declared and that may therefore have multiple context partitions,
        /// allows to target context partitions for query execution selectively.
        /// </summary>
        /// <param name="compiled">is the compiled EPL query to execute</param>
        /// <param name="selectors">selects context partitions to consider</param>
        /// <returns>result</returns>
        EPFireAndForgetQueryResult ExecuteQuery(EPCompiled compiled, ContextPartitionSelector[] selectors);

        /// <summary>
        /// Prepare an unparameterized fire-and-forget query before execution and for repeated execution.
        /// </summary>
        /// <param name="compiled">is the compiled EPL query to prepare</param>
        /// <returns>proxy to execute upon, that also provides the event type of the returned results</returns>
        EPFireAndForgetPreparedQuery PrepareQuery(EPCompiled compiled);

        /// <summary>
        /// Prepare a parameterized fire-and-forget query for repeated parameter setting and execution.
        /// Set all values on the returned holder then execute using {@link #executeQuery(EPFireAndForgetPreparedQueryParameterized)}.
        /// </summary>
        /// <param name="compiled">is the compiled EPL query to prepare</param>
        /// <returns>parameter holder upon which to set values</returns>
        EPFireAndForgetPreparedQueryParameterized PrepareQueryWithParameters(EPCompiled compiled);

        /// <summary>
        /// Execute a fire-and-forget parameterized query.
        /// </summary>
        /// <param name="parameterizedQuery">contains the query and parameter values</param>
        /// <returns>query result</returns>
        EPFireAndForgetQueryResult ExecuteQuery(EPFireAndForgetPreparedQueryParameterized parameterizedQuery);

        /// <summary>
        /// Execute a fire-and-forget parameterized query.
        /// </summary>
        /// <param name="parameterizedQuery">contains the query and parameter values</param>
        /// <param name="selectors">selects context partitions to consider</param>
        /// <returns>query result</returns>
        EPFireAndForgetQueryResult ExecuteQuery(EPFireAndForgetPreparedQueryParameterized parameterizedQuery, ContextPartitionSelector[] selectors);
    }
} // end of namespace