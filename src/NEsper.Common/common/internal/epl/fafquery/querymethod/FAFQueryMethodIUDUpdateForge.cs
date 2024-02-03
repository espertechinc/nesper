///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage1;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.fafquery.processor;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.epl.updatehelper;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
    /// <summary>
    /// Starts and provides the stop method for EPL statements.
    /// </summary>
    public class FAFQueryMethodIUDUpdateForge : FAFQueryMethodIUDBaseForge
    {
        public const string INITIAL_VALUE_STREAM_NAME = "initial";

        private EventBeanUpdateHelperForge updateHelper;

        public FAFQueryMethodIUDUpdateForge(
            StatementSpecCompiled spec,
            Compilable compilable,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
            : base(spec, compilable, statementRawInfo, services)

        {
        }

        protected override void InitExec(
            string aliasName,
            StatementSpecCompiled spec,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            var assignmentTypeService = new StreamTypeServiceImpl(
                new EventType[] {
                    processor.EventTypeRSPInputEvents, null,
                    processor.EventTypeRSPInputEvents
                },
                new string[] { aliasName, "", INITIAL_VALUE_STREAM_NAME },
                new bool[] { true, true, true },
                true,
                false);
            assignmentTypeService.IsStreamZeroUnambigous = true;
            var validationContext =
                new ExprValidationContextBuilder(assignmentTypeService, statementRawInfo, services)
                    .WithAllowBindingConsumption(true)
                    .Build();

            // validate update expressions
            var updateSpec = (FireAndForgetSpecUpdate)spec.Raw.FireAndForgetSpec;
            try {
                foreach (var assignment in updateSpec.Assignments) {
                    ExprNodeUtilityValidate.ValidateAssignment(
                        false,
                        ExprNodeOrigin.UPDATEASSIGN,
                        assignment,
                        validationContext);
                }
            }
            catch (ExprValidationException e) {
                throw new EPException(e.Message, e);
            }

            // make updater
            try {
                var copyOnWrite = processor is FireAndForgetProcessorNamedWindowForge;
                updateHelper = EventBeanUpdateHelperForgeFactory.Make(
                    processor.ProcessorName,
                    (EventTypeSPI)processor.EventTypeRSPInputEvents,
                    updateSpec.Assignments,
                    aliasName,
                    null,
                    copyOnWrite,
                    statementRawInfo.StatementName,
                    services.EventTypeAvroHandler);
            }
            catch (ExprValidationException e) {
                throw new EPException(e.Message, e);
            }
        }

        protected override Type TypeOfMethod()
        {
            return typeof(FAFQueryMethodIUDUpdate);
        }

        protected override void MakeInlineSpecificSetter(
            CodegenExpressionRef queryMethod,
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block.SetProperty(
                queryMethod,
                "OptionalWhereClause",
                whereClause == null
                    ? ConstantNull()
                    : ExprNodeUtilityCodegen.CodegenEvaluator(whereClause.Forge, method, GetType(), classScope));
            if (processor is FireAndForgetProcessorNamedWindowForge) {
                method.Block.SetProperty(
                    queryMethod,
                    "UpdateHelperNamedWindow",
                    updateHelper.MakeWCopy(method, classScope));
            }
            else {
                var table = (FireAndForgetProcessorTableForge)processor;
                method.Block
                    .SetProperty(queryMethod, "UpdateHelperTable", updateHelper.MakeNoCopy(method, classScope))
                    .SetProperty(
                        queryMethod,
                        "Table",
                        TableDeployTimeResolver.MakeResolveTable(table.Table, symbols.GetAddInitSvc(method)));
            }
        }
    }
} // end of namespace