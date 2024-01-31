///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esperio.subscription;

namespace com.espertech.esperio
{
	/// <summary>
	/// An output adapter transforms engine events and
	/// </summary>
	public interface OutputAdapter : Adapter
	{
	    /// <summary>
	    /// Sets the subscriptions for the output adapter.
	    /// </summary>
	    IDictionary<string, Subscription> SubscriptionMap { set; get; }

	    /// <summary>
	    /// Returns a given subscription by it's name, or null if not found
	    /// </summary>
	    /// <param name="subscriptionName">is the subscription</param>
	    /// <returns>subcription or null</returns>
	    Subscription GetSubscription(string subscriptionName);
	}
}