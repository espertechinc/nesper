///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;

namespace com.espertech.esper.epl.join.rep
{
	/// <summary>
	/// Node is the structure to hold results of event lookups in joined streams. A node holds
	/// a set of event which are the result of a lookup in a stream's table. A Node can be
	/// linked to its parent node and the event within the parent node, which was the event 
	/// that was used to perform a lookup.
	/// </summary>
	public class Node
	{
	    /// <summary> Returns the stream number of the stream that supplied the event results.</summary>
	    /// <returns> stream number for results
	    /// </returns>
	    public int Stream { get; private set; }

	    /// <summary>
	    /// Gets or sets the parent node, or null if this is a root node.
	    /// </summary>
	    /// <value>The parent.</value>
	    /// <returns> parent node or null for root node
	    /// </returns>
	    public Node Parent { get; set; }

	    /// <summary>
	    /// Gets or sets lookup event.
	    /// </summary>
	    /// <value>The parent event.</value>
	    /// <returns> parent node's event that was used to lookup
	    /// </returns>
	    public EventBean ParentEvent { get; set; }

	    /// <summary>
	    /// Gets or sets the events.
	    /// </summary>
	    /// <value>The events.</value>
        public ICollection<EventBean> Events { get; set; }

	    /// <summary> Ctor.</summary>
		/// <param name="stream">this node stores results for
		/// </param>
		public Node(int stream)
		{
			Stream = stream;
		}
	}
}
