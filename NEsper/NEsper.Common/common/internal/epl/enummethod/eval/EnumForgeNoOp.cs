///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumForgeNoOp : EnumForgeBase,
        EnumEval
    {
        public EnumForgeNoOp(int streamCountIncoming) : base(streamCountIncoming)
        {
        }

        public override EnumEval EnumEvaluator => this;

        public object EvaluateEnumMethod(
            EventBean[] eventsLambda, 
            ICollection<object> target, 
            bool isNewData, 
            ExprEvaluatorContext context)
        {
            return target;
        }


        public override CodegenExpression Codegen(
            EnumForgeCodegenParams premade, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope)
        {
            return premade.Enumcoll;
        }
    }
} // end of namespace