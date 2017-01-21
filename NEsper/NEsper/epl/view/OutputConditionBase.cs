///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.epl.view
{
    public abstract class OutputConditionBase : OutputCondition
    {
        protected readonly OutputCallback OutputCallback;

        protected OutputConditionBase(OutputCallback outputCallback)
        {
            OutputCallback = outputCallback;
        }

        public virtual void Terminated()
        {
            OutputCallback.Invoke(true, true);
        }

        /// <summary>Update the output condition. </summary>
        /// <param name="newEventsCount">number of new events incoming</param>
        /// <param name="oldEventsCount">number of old events incoming</param>
        public abstract void UpdateOutputCondition(int newEventsCount, int oldEventsCount);

        /// <summary>Stops this instance.</summary>
        public abstract void Stop();
    }
}
