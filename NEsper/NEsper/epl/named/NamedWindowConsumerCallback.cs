///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.client;

namespace com.espertech.esper.epl.named
{
    public interface NamedWindowConsumerCallback : IEnumerable<EventBean>
    {
        void Stopped(NamedWindowConsumerView namedWindowConsumerView);
    }

    public class ProxyNamedWindowConsumerCallback : NamedWindowConsumerCallback
    {
        public Func<IEnumerator<EventBean>> ProcGetEnumerator { get; set; }
        public Action<NamedWindowConsumerView> ProcStopped { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyNamedWindowConsumerCallback"/> class.
        /// </summary>
        public ProxyNamedWindowConsumerCallback()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyNamedWindowConsumerCallback"/> class.
        /// </summary>
        /// <param name="procGetEnumerator">The get enumerator.</param>
        /// <param name="procStopped">The stopped.</param>
        public ProxyNamedWindowConsumerCallback(Func<IEnumerator<EventBean>> procGetEnumerator,
                                                Action<NamedWindowConsumerView> procStopped)
        {
            ProcGetEnumerator = procGetEnumerator;
            ProcStopped = procStopped;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<EventBean> GetEnumerator()
        {
            return ProcGetEnumerator.Invoke();
        }

        /// <summary>
        /// Stoppeds the specified named window consumer view.
        /// </summary>
        /// <param name="namedWindowConsumerView">The named window consumer view.</param>
        public void Stopped(NamedWindowConsumerView namedWindowConsumerView)
        {
            ProcStopped.Invoke(namedWindowConsumerView);
        }
    }
}
