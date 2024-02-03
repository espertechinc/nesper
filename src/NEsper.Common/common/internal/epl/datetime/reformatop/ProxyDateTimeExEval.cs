///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.datetime.reformatop
{
    public class ProxyDateTimeExEval : DateTimeExEval
    {
        public Func<DateTimeEx, object> ProcEvaluateInternal;

        public object EvaluateInternal(DateTimeEx dateTime)
        {
            return ProcEvaluateInternal?.Invoke(dateTime);
        }

        public Func<CodegenExpression, CodegenExpression> ProcCodegen;

        public CodegenExpression Codegen(CodegenExpression dateTime)
        {
            return ProcCodegen?.Invoke(dateTime);
        }
    }
}