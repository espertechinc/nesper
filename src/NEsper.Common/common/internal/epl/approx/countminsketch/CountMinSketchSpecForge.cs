///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.approx.countminsketch
{
    public class CountMinSketchSpecForge
    {
        public CountMinSketchSpecForge(
            CountMinSketchSpecHashes hashesSpec,
            int? topkSpec,
            CountMinSketchAgentForge agent)
        {
            HashesSpec = hashesSpec;
            TopkSpec = topkSpec;
            Agent = agent;
        }

        public CountMinSketchSpecHashes HashesSpec { get; }

        public int? TopkSpec { get; set; }

        public CountMinSketchAgentForge Agent { get; set; }

        public CodegenExpression CodegenMake(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(CountMinSketchSpec), GetType(), classScope);
            method.Block
                .DeclareVar<CountMinSketchSpec>("spec", NewInstance<CountMinSketchSpec>())
                .SetProperty(Ref("spec"), "HashesSpec", HashesSpec.CodegenMake(method, classScope))
                .SetProperty(Ref("spec"), "TopkSpec", Constant(TopkSpec))
                .SetProperty(Ref("spec"), "Agent", Agent.CodegenMake(method, classScope))
                .MethodReturn(Ref("spec"));
            return LocalMethod(method);
        }
    }
} // end of namespace