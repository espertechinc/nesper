///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.client
{
    public class EPSubscriber
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EPSubscriber"/> class.
        /// </summary>
        public EPSubscriber()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EPSubscriber"/> class.
        /// </summary>
        /// <param name="subscriber">The subscriber.</param>
        public EPSubscriber(object subscriber)
        {
            Subscriber = subscriber;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EPSubscriber"/> class.
        /// </summary>
        /// <param name="subscriber">The subscriber.</param>
        /// <param name="subscriberMethod">The subscriber method.</param>
        public EPSubscriber(object subscriber, string subscriberMethod)
        {
            Subscriber = subscriber;
            SubscriberMethod = subscriberMethod;
        }

        /// <summary>
        /// Gets or sets the subscriber instance.
        /// </summary>
        public object Subscriber { get; set; }

        /// <summary>
        /// Gets or sets the subscriber method.
        /// </summary>
        public string SubscriberMethod { get; set; }
    }
}
