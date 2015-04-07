///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;

namespace com.espertech.esper.epl.view
{
    public class OutputProcessViewDirectPostProcess : OutputProcessViewDirect
    {
        private readonly OutputStrategyPostProcess _postProcessor;

        public OutputProcessViewDirectPostProcess(ResultSetProcessor resultSetProcessor, OutputProcessViewDirectFactory parent, OutputStrategyPostProcess postProcessor)
            : base(resultSetProcessor, parent)
        {
            _postProcessor = postProcessor;
        }

        protected override void PostProcess(bool force, UniformPair<EventBean[]> newOldEvents, UpdateDispatchView childView)
        {
            _postProcessor.Output(force, newOldEvents, childView);
        }
    }
}