///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.view.internals;

namespace com.espertech.esper.epl.join.@base
{
    /// <summary>
    /// Method for preloading events for a given stream onto the stream's indexes, from a 
    /// buffer already associated with a stream.
    /// </summary>
    public interface JoinPreloadMethod
    {
        /// <summary>
        /// Initialize a stream from the stream buffers data.
        /// </summary>
        /// <param name="stream">to initialize and load indexes</param>
        /// <param name="exprEvaluatorContext">The expr evaluator context.</param>
        void PreloadFromBuffer(int stream, ExprEvaluatorContext exprEvaluatorContext);

        /// <summary>
        /// Initialize the result set process for the purpose of grouping and aggregation from the join result set.
        /// </summary>
        /// <param name="resultSetProcessor">is the grouping and aggregation result processing</param>
        void PreloadAggregation(ResultSetProcessor resultSetProcessor);
    
        /// <summary>Sets the buffee to use. </summary>
        /// <param name="buffer">buffer to use</param>
        /// <param name="i">stream</param>
        void SetBuffer(BufferView buffer, int i);

        bool IsPreloading { get; }
    }
}
