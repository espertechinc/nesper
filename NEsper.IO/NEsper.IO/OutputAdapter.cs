using System;
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
        Subscription GetSubscription(String subscriptionName);
    }
}