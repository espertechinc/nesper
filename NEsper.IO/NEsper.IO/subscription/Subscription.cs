using System;

namespace com.espertech.esperio.subscription
{
    /// <summary>
    /// Subscriptions are associated with an output adapter and dictate which events
    /// are sent to a given adapter.
    /// </summary>

    public interface Subscription
    {
        /// <summary>Gets or sets the subscription name.</summary>
        /// <returns>subscription name</returns>
        String SubscriptionName { get; set; }

        /// <summary>Gets the type name of the event type we are looking for.</summary>
        /// <returns>event type name</returns>
        String EventTypeName { get; }

        /// <summary>Gets or sets the output adapter this subscription is associated with.</summary>
        /// <returns>output adapter</returns>
        OutputAdapter Adapter { get; set; }
    }
}