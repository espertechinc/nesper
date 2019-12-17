///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.context.aifactory.update
{
    public class InternalEventRouterDesc
    {
        public TypeWidener[] Wideners { get; set; }

        public EventType EventType { get; set; }

        public Attribute[] Annotations { get; set; }

        public ExprEvaluator OptionalWhereClauseEval { get; set; }

        public string[] Properties { get; set; }

        public ExprEvaluator[] Assignments { get; set; }
    }
} // end of namespace