///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.adapter
{
    /// <summary>An output adapter transforms engine events and </summary>
    public interface OutputAdapter : Adapter
    {
        /// <summary>Returns the subscriptions. </summary>
        /// <returns>map of name and subscription</returns>
        IDictionary<string, Subscription> SubscriptionMap { get; set; }

        /// <summary>Returns a given subscription by it's name, or null if not found </summary>
        /// <param name="subscriptionName">is the subscription</param>
        /// <returns>subcription or null</returns>
        Subscription GetSubscription(String subscriptionName);
    }
}
