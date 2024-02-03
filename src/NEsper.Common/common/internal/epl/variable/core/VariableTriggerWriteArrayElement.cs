///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.variable.core
{
    public class VariableTriggerWriteArrayElement : VariableTriggerWrite
    {
        public string VariableName { get; set; }

        public ExprEvaluator IndexExpression { get; set; }

        public TypeWidener TypeWidener { get; set; }
    }
} // end of namespace