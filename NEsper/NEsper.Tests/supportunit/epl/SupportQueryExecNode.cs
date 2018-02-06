///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.client;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.exec.@base;
using com.espertech.esper.util;

namespace com.espertech.esper.supportunit.epl
{
    public class SupportQueryExecNode : ExecNode
    {
        public SupportQueryExecNode(String id)
        {
            Id = id;
        }

        public string Id { get; private set; }

        public EventBean[] LastPrefillPath { get; private set; }

        public override void Process(EventBean lookupEvent, EventBean[] prefillPath, ICollection<EventBean[]> result, ExprEvaluatorContext exprEvaluatorContext)
        {
            LastPrefillPath = prefillPath;
        }

        public override void Print(IndentWriter writer)
        {
            writer.WriteLine("SupportQueryExecNode");
        }
    }
}