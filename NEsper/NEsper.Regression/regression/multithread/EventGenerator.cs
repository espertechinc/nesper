///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.multithread;

namespace com.espertech.esper.regression.multithread
{
    public class EventGenerator
    {
        public static GeneratorIteratorCallback DEFAULT_SUPPORTEBEAN_CB = 
            numEvent => new SupportBean(Convert.ToString(numEvent), numEvent);

        /// <summary>
        /// Makes the events.
        /// </summary>
        /// <param name="maxNumEvents">The max num events.</param>
        /// <param name="callback">The callback.</param>
        /// <returns></returns>
        public static IEnumerator<object> MakeEvents(int maxNumEvents, GeneratorIteratorCallback callback)
        {
            for(int ii = 0 ; ii < maxNumEvents; ii++)
            {
                yield return callback.Invoke(ii);
            }
        }

        /// <summary>
        /// Makes the events.
        /// </summary>
        /// <param name="maxNumEvents">The max num events.</param>
        /// <returns></returns>
        public static IEnumerator<object> MakeEvents(int maxNumEvents)
        {
            return MakeEvents(maxNumEvents, DEFAULT_SUPPORTEBEAN_CB);
        }
    }
}
