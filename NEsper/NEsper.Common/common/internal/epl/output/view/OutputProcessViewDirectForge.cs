///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.output.core;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.output.view
{
    public class OutputProcessViewDirectForge : OutputProcessViewFactoryForge
    {
        private OutputStrategyPostProcessForge outputStrategyPostProcessForge;

        public OutputProcessViewDirectForge(OutputStrategyPostProcessForge outputStrategyPostProcessForge)
        {
            this.outputStrategyPostProcessForge = outputStrategyPostProcessForge;
        }

        public bool IsCodeGenerated {
            get { return false; }
        }

        public void ProvideCodegen(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            CodegenExpressionRef factory = @Ref("factory");
            method.Block
                .DeclareVar(typeof(OutputProcessViewDirectFactory), factory.Ref, NewInstance(typeof(OutputProcessViewDirectFactory)))
                .SetProperty(factory, "PostProcessFactory",
                    outputStrategyPostProcessForge == null ? ConstantNull() : outputStrategyPostProcessForge.Make(method, symbols, classScope))
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

        public void IteratorCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
        }

        public void CollectSchedules(IList<ScheduleHandleCallbackProvider> scheduleHandleCallbackProviders)
        {
        }
    }
} // end of namespace