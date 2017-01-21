///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.context.util;

namespace com.espertech.esper.epl.view
{
    /// <summary>
    /// An output condition that is satisfied at the first event of either a time-based or count-based batch.
    /// </summary>
    public class OutputConditionFirst : OutputConditionBase, OutputCondition
    {
        private readonly OutputCondition _innerCondition;
        private bool _witnessedFirst;

        public OutputConditionFirst(OutputCallback outputCallback, AgentInstanceContext agentInstanceContext, OutputConditionFactory innerConditionFactory)

            : base(outputCallback)
        {
            OutputCallback localCallback = CreateCallbackToLocal();
            _innerCondition = innerConditionFactory.Make(agentInstanceContext, localCallback);
            _witnessedFirst = false;
        }

        public override void UpdateOutputCondition(int newEventsCount, int oldEventsCount)
        {
            if (!_witnessedFirst)
            {
                _witnessedFirst = true;
                const bool doOutput = true;
                const bool forceUpdate = false;
                OutputCallback.Invoke(doOutput, forceUpdate);
            }
            _innerCondition.UpdateOutputCondition(newEventsCount, oldEventsCount);
        }

        private OutputCallback CreateCallbackToLocal()
        {
            return (doOutput, forceUpdate) => ContinueOutputProcessing(forceUpdate);
        }

        public override void Terminated()
        {
            OutputCallback.Invoke(true, true);
        }

        public override void Stop()
        {
            // no action required
        }
        
        private void ContinueOutputProcessing(bool forceUpdate)
        {
            var doOutput = !_witnessedFirst;
            OutputCallback.Invoke(doOutput, forceUpdate);
            _witnessedFirst = false;
        }
    }
}