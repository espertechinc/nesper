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
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.output.core.OutputProcessViewCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;
using static com.espertech.esper.common.@internal.metrics.instrumentation.InstrumentationCode;

namespace com.espertech.esper.common.@internal.epl.output.core
{
    public class OutputProcessViewDirectSimpleForge : OutputProcessViewFactoryForge
    {
        private readonly OutputStrategyPostProcessForge _postProcess;

        public OutputProcessViewDirectSimpleForge(OutputStrategyPostProcessForge postProcess)
        {
            this._postProcess = postProcess;
        }

        public bool IsCodeGenerated => _postProcess != null;

        public void ProvideCodegen(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            if (_postProcess != null) {
                throw new IllegalStateException("Provide is not required");
            }
            
            method.Block.MethodReturn(PublicConstValue<OutputProcessViewDirectSimpleFactory>("INSTANCE"));
        }

        public void UpdateCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.Apply(Instblock(classScope, "qOutputProcessNonBuffered", REF_NEWDATA, REF_OLDDATA));

            GenerateRSPCall("ProcessViewResult", method, classScope);

            var newOldIsNull = And(
                EqualsNull(ExprDotName(Ref("newOldEvents"), "First")),
                EqualsNull(ExprDotName(Ref("newOldEvents"), "Second")));
            method.Block
                .DeclareVar<bool>("forceOutput", Constant(false))
                .IfCondition(And(EqualsNull(REF_NEWDATA), EqualsNull(REF_OLDDATA)))
                .IfCondition(Or(EqualsNull(Ref("newOldEvents")), newOldIsNull))
                .AssignRef("forceOutput", ConstantTrue());

            method.Block
                .Expression(
                    LocalMethod(
                        _postProcess.PostProcessCodegenMayNullMayForce(classScope, method), Ref("forceOutput"),
                        Ref("newOldEvents")))
                .Apply(Instblock(classScope, "aOutputProcessNonBuffered"));
        }

        public void ProcessCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.Apply(Instblock(classScope, "qOutputProcessNonBufferedJoin", REF_NEWDATA, REF_OLDDATA));

            GenerateRSPCall("ProcessJoinResult", method, classScope);

            method.Block.IfRefNull("newOldEvents")
                .Apply(Instblock(classScope, "aOutputProcessNonBufferedJoin"))
                .BlockReturnNoValue();

            method.Block.Expression(
                LocalMethod(
                    _postProcess.PostProcessCodegenMayNullMayForce(classScope, method), ConstantFalse(),
                    Ref("newOldEvents")));

            method.Block.Apply(Instblock(classScope, "aOutputProcessNonBufferedJoin"));
        }

        public void EnumeratorCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.MethodReturn(
                StaticMethod(
                    typeof(OutputStrategyUtil),
                    "GetEnumerator",
                    Ref(NAME_JOINEXECSTRATEGY),
                    Ref(NAME_RESULTSETPROCESSOR),
                    Ref(NAME_PARENTVIEW),
                    Constant(false),
                    Constant(null)));
        }

        public void CollectSchedules(IList<ScheduleHandleCallbackProvider> scheduleHandleCallbackProviders)
        {
        }

        private void GenerateRSPCall(
            string rspMethod,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block
                .DeclareVar<bool>(
                    "isGenerateSynthetic",
                    ExprDotName(Member("o." + NAME_STATEMENTRESULTSVC), "IsMakeSynthetic"))
                .DeclareVar<bool>(
                    "isGenerateNatural",
                    ExprDotName(Member("o." + NAME_STATEMENTRESULTSVC), "IsMakeNatural"))
                .DeclareVar<UniformPair<EventBean[]>>(
                    "newOldEvents",
                    ExprDotMethod(
                        Ref(NAME_RESULTSETPROCESSOR),
                        rspMethod,
                        REF_NEWDATA,
                        REF_OLDDATA,
                        Ref("isGenerateSynthetic")))
                .IfCondition(And(Not(Ref("isGenerateSynthetic")), Not(Ref("isGenerateNatural"))))
                .BlockReturnNoValue();
        }
    }
} // end of namespace