///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.strategy
{
	/// <summary>
	/// Interface for an execution node that looks up events and builds a result set contributing to an overall
	/// join result set.
	/// </summary>
	public abstract class ExecNode {
	    /// <summary>
	    /// Process single event using the prefill events to compile lookup results.
	    /// </summary>
	    /// <param name="lookupEvent">event to look up for or query for</param>
	    /// <param name="prefillPath">set of events currently in the example tuple to serveas a prototype for result rows.
	    /// </param>
	    /// <param name="result">is the list of tuples to add a result row to</param>
	    /// <param name="exprEvaluatorContext">context for expression evaluation</param>
	    public abstract void Process(EventBean lookupEvent, EventBean[] prefillPath, ICollection<EventBean[]> result, ExprEvaluatorContext exprEvaluatorContext);

	    /// <summary>
	    /// Output the execution strategy.
	    /// </summary>
	    /// <param name="writer">to output to</param>
	    public abstract void Print(IndentWriter writer);

	    /// <summary>
	    /// Print in readable format the execution strategy.
	    /// </summary>
	    /// <param name="execNode">execution node to print</param>
	    /// <returns>readable text with execution nodes constructed for actual streams</returns>
	    public static string Print(ExecNode execNode) {
	        StringWriter buf = new StringWriter();
	        PrintWriter printer = new PrintWriter(buf);
	        IndentWriter indentWriter = new IndentWriter(printer, 4, 2);
	        execNode.Print(indentWriter);

	        return buf.ToString();
	    }

	}
} // end of namespace