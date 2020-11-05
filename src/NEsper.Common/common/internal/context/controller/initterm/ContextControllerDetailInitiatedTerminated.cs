///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.context.controller.condition;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.context.controller.initterm
{
    public class ContextControllerDetailInitiatedTerminated : ContextControllerDetail
    {
        public ContextConditionDescriptor StartCondition { get; set; }

        public ContextConditionDescriptor EndCondition { get; set; }

        public bool IsOverlapping { get; set; }

        public ExprEvaluator DistinctEval { get; set; }

        public Type[] DistinctTypes { get; set; }
        
        public DataInputOutputSerde DistinctSerde { get; set; }
    }
} // end of namespace