///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.variable.core
{
    public class VariableTriggerSetDesc
    {
        public VariableTriggerSetDesc(
            string variableName,
            ExprEvaluator evaluator)
        {
            VariableName = variableName;
            Evaluator = evaluator;
        }

        public string VariableName { get; }

        public ExprEvaluator Evaluator { get; }
    }
} // end of namespace