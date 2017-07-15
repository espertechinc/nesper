///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.join.exec.@base
{
    public class ExecNodeAllUnidirectionalOuter : ExecNode
    {
        private readonly int _streamNum;
        private readonly int _numStreams;

        public ExecNodeAllUnidirectionalOuter(int streamNum, int numStreams)
        {
            _streamNum = streamNum;
            _numStreams = numStreams;
        }

        public override void Process(
            EventBean lookupEvent,
            EventBean[] prefillPath,
            ICollection<EventBean[]> result,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var events = new EventBean[_numStreams];
            events[_streamNum] = lookupEvent;
            result.Add(events);
        }

        public override void Print(IndentWriter writer)
        {
            writer.WriteLine("ExecNodeNoOp");
        }
    }
} // end of namespace
