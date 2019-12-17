///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.output.condition
{
    public abstract class OutputConditionBase : OutputCondition
    {
        protected internal readonly OutputCallback outputCallback;

        protected OutputConditionBase(OutputCallback outputCallback)
        {
            this.outputCallback = outputCallback;
        }

        public virtual void Terminated()
        {
            outputCallback.Invoke(true, true);
        }

        public abstract void UpdateOutputCondition(
            int newEventsCount,
            int oldEventsCount);

        public abstract void StopOutputCondition();
    }
} // end of namespace