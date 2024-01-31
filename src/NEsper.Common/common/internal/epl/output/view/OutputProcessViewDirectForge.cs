///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.output.core;
using com.espertech.esper.common.@internal.schedule;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.output.view
{
    public class OutputProcessViewDirectForge : OutputProcessViewFactoryForge
    {
        private readonly OutputStrategyPostProcessForge _outputStrategyPostProcessForge;

        public OutputProcessViewDirectForge(OutputStrategyPostProcessForge outputStrategyPostProcessForge)
        {
            _outputStrategyPostProcessForge = outputStrategyPostProcessForge;
        }

        public bool IsCodeGenerated => false;

        public bool IsDirectAndSimple => false;

        public void ProvideCodegen(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var factory = Ref("factory");
            method.Block
                .DeclareVarNewInstance<OutputProcessViewDirectFactory>(factory.Ref)
                .SetProperty(
                    factory,
                    "PostProcessFactory",
                    _outputStrategyPostProcessForge == null
                        ? ConstantNull()
                        : _outputStrategyPostProcessForge.Make(method, symbols, classScope))
                .MethodReturn(factory);
        }

        public void UpdateCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
        }

        public void ProcessCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
        }

        public void EnumeratorCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
        }

        public void CollectSchedules(IList<ScheduleHandleTracked> scheduleHandleCallbackProviders)
        {
        }
    }
} // end of namespace