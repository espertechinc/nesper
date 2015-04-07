///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.threading;
using com.espertech.esper.epl.db;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.join.pollindex;
using com.espertech.esper.epl.join.table;

namespace com.espertech.esper.view
{
    /// <summary>
    /// Interface for views that poll data based on information from other streams.
    /// </summary>
    public interface HistoricalEventViewable 
        : Viewable
        , ValidatedView
    {
        /// <summary>
        /// Returns true if the parameters expressions to the historical require other stream's data, or 
        /// false if there are no parameters or all parameter expressions are only contants and variables 
        /// without properties of other stream events.
        /// </summary>
        /// <value>
        /// 	indicator whether properties are required for parameter evaluation
        /// </value>
        bool HasRequiredStreams { get; }

        /// <summary>
        /// Returns the a set of stream numbers of all streams that provide property values in any of the 
        /// parameter expressions to the stream.
        /// </summary>
        /// <value>set of stream numbers</value>
        ICollection<int> RequiredStreams { get; }

        /// <summary>
        /// Historical views are expected to provide a thread-local data cache for use in keeping row 
        /// (<seealso cref="com.espertech.esper.client.EventBean"/> references) returned during iteration 
        /// stable, since the concept of a primary key does not exist.
        /// </summary>
        /// <value>The data cache thread local.</value>
        /// <returns>thread-local cache, can be null for any thread to indicate no caching</returns>
        IThreadLocal<DataCache> DataCacheThreadLocal { get; }

        /// <summary>
        /// Poll for stored historical or reference data using events per stream and returing for each 
        /// event-per-stream row a separate list with events representing the poll result.
        /// </summary>
        /// <param name="lookupEventsPerStream">is the events per stream where thefirst dimension is a number of rows (often 1 depending on windows used) and the second dimension is the number of streams participating in a join.</param>
        /// <param name="indexingStrategy">the strategy to use for converting poll results into a indexed table for fast lookup</param>
        /// <param name="exprEvaluatorContext">context for expression evalauation</param>
        /// <returns>
        /// array of lists with one list for each event-per-stream row
        /// </returns>
        EventTable[][] Poll(EventBean[][] lookupEventsPerStream, PollResultIndexingStrategy indexingStrategy, ExprEvaluatorContext exprEvaluatorContext);

        /// <summary>
        /// Stops this instance.
        /// </summary>
        void Stop();
    }
}
