///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.attributes;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.exec.@base;

namespace com.espertech.esper.epl.join.@base
{
    /// <summary>
    /// Query strategy for building a join tuple set by using an execution node tree.
    /// </summary>
    [EsperVersion("6.1.1.*")]
    public sealed class ExecNodeQueryStrategy : QueryStrategy
    {
        /// <summary>CTor. </summary>
        /// <param name="forStream">stream the strategy is for</param>
        /// <param name="numStreams">number of streams in total</param>
        /// <param name="execNode">execution node for building join tuple set</param>
        public ExecNodeQueryStrategy(int forStream, int numStreams, ExecNode execNode)
        {
            ForStream = forStream;
            NumStreams = numStreams;
            ExecNode = execNode;
        }

        public void Lookup(EventBean[] lookupEvents, ICollection<MultiKey<EventBean>> joinSet, ExprEvaluatorContext exprEvaluatorContext)
        {
            if ((lookupEvents != null) && (lookupEvents.Length != 0))
            {
                unchecked
                {
                    var results = new List<EventBean[]>();
                    int count = lookupEvents.Length;
                    for (int ii = 0; ii < count; ii++)
                    {
                        var theEvent = lookupEvents[ii];
                        var prototype = new EventBean[NumStreams];

                        // Set up _prototype row
                        prototype[ForStream] = theEvent;

                        // Perform execution
                        ExecNode.Process(theEvent, prototype, results, exprEvaluatorContext);

                        // Convert results into unique set
                        results.ForEach(row => joinSet.Add(new MultiKey<EventBean>(row)));
                        results.Clear();
                    }
                }
            }
        }

        /// <summary>Return stream number this strategy is for. </summary>
        /// <value>stream num</value>
        internal int ForStream { get; private set; }

        /// <summary>Returns the total number of streams. </summary>
        /// <value>number of streams</value>
        internal int NumStreams { get; private set; }

        /// <summary>Returns execution node. </summary>
        /// <value>execution node</value>
        internal ExecNode ExecNode { get; private set; }
    }
}