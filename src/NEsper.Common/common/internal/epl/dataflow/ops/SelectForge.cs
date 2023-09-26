///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.dataflow.annotations;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.aifactory.select;
using com.espertech.esper.common.@internal.epl.agg.rollup;
using com.espertech.esper.common.@internal.epl.annotation;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.epl.dataflow.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.declared.compiletime;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.epl.expression.table;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.dataflow.core.EPDataFlowServiceImpl;

namespace com.espertech.esper.common.@internal.epl.dataflow.ops
{
    public class SelectForge : DataFlowOperatorForge
    {
        private string classNameAIFactoryProvider;
        private string classNameFieldsFactoryProvider;

        private EventType[] eventTypes;

#pragma warning disable 649
        [DataFlowOpParameter] private readonly bool iterate;
#pragma warning restore 649

        private int[] originatingStreamToViewableStream;

#pragma warning disable 649
        [DataFlowOpParameter] private StatementSpecRaw select;
#pragma warning restore 649

        private bool submitEventBean;

        public DataFlowOpForgeInitializeResult InitializeForge(DataFlowOpForgeInitializeContext context)
        {
            if (context.InputPorts.IsEmpty()) {
                throw new ArgumentException("Select operator requires at least one input stream");
            }

            if (context.OutputPorts.Count != 1) {
                throw new ArgumentException(
                    "Select operator requires one output stream but produces " +
                    context.OutputPorts.Count +
                    " streams");
            }

            var portZero = context.OutputPorts[0];
            if (portZero.OptionalDeclaredType != null && !portZero.OptionalDeclaredType.IsUnderlying) {
                submitEventBean = true;
            }

            // determine adapter factories for each type
            var numStreams = context.InputPorts.Count;
            eventTypes = new EventType[numStreams];
            for (var i = 0; i < numStreams; i++) {
                eventTypes[i] = context.InputPorts.Get(i).TypeDesc.EventType;
            }

            // validate
            if (select.InsertIntoDesc != null) {
                throw new ExprValidationException("Insert-into clause is not supported");
            }

            if (select.SelectStreamSelectorEnum != SelectClauseStreamSelectorEnum.ISTREAM_ONLY) {
                throw new ExprValidationException("Selecting remove-stream is not supported");
            }

            var visitor =
                StatementSpecRawWalkerSubselectAndDeclaredDot.WalkSubselectAndDeclaredDotExpr(select);
            var groupByExpressions = GroupByExpressionHelper.GetGroupByRollupExpressions(
                select.GroupByExpressions,
                select.SelectClauseSpec,
                select.WhereClause,
                select.OrderByList,
                null);
            if (!visitor.Subselects.IsEmpty()) {
                throw new ExprValidationException("Subselects are not supported");
            }

            IDictionary<int, FilterStreamSpecRaw> streams = new Dictionary<int, FilterStreamSpecRaw>();
            for (var streamNum = 0; streamNum < select.StreamSpecs.Count; streamNum++) {
                var rawStreamSpec = select.StreamSpecs[streamNum];
                if (!(rawStreamSpec is FilterStreamSpecRaw raw)) {
                    throw new ExprValidationException(
                        "From-clause must contain only streams and cannot contain patterns or other constructs");
                }

                streams.Put(streamNum, raw);
            }

            // compile offered streams
            var streamSpecCompileds = new List<StreamSpecCompiled>();
            originatingStreamToViewableStream = new int[select.StreamSpecs.Count];
            for (var streamNum = 0; streamNum < select.StreamSpecs.Count; streamNum++) {
                var filter = streams.Get(streamNum);
                var inputPort = FindInputPort(filter.RawFilterSpec.EventTypeName, context.InputPorts);
                if (inputPort == null) {
                    throw new ExprValidationException(
                        "Failed to find stream '" +
                        filter.RawFilterSpec.EventTypeName +
                        "' among input ports, input ports are " +
                        GetInputPortNames(context.InputPorts).RenderAny());
                }

                var inputPortValue = inputPort.Value;
                var eventType = inputPortValue.Value.TypeDesc.EventType;
                originatingStreamToViewableStream[inputPortValue.Key] = streamNum;
                var streamAlias = filter.OptionalStreamName;
                var filterSpecCompiled = new FilterSpecCompiled(
                    eventType,
                    streamAlias,
                    FilterSpecPlanForge.EMPTY,
                    null);
                var viewSpecs = select.StreamSpecs[streamNum].ViewSpecs;
                var filterStreamSpecCompiled = new FilterStreamSpecCompiled(
                    filterSpecCompiled,
                    viewSpecs,
                    streamAlias,
                    StreamSpecOptions.DEFAULT);
                streamSpecCompileds.Add(filterStreamSpecCompiled);
            }

            // create compiled statement spec
            var selectClauseCompiled = StatementLifecycleSvcUtil.CompileSelectClause(select.SelectClauseSpec);

            var mergedAnnotations = AnnotationUtil.MergeAnnotations(
                context.StatementRawInfo.Annotations,
                context.OperatorAnnotations);
            mergedAnnotations = AddObjectArrayRepresentation(mergedAnnotations);
            var streamSpecArray = streamSpecCompileds.ToArray();

            // determine if snapshot output is needed
            var outputLimitSpec = select.OutputLimitSpec;
            if (iterate) {
                if (outputLimitSpec != null) {
                    throw new ExprValidationException("Output rate limiting is not supported with 'iterate'");
                }

                outputLimitSpec = new OutputLimitSpec(OutputLimitLimitType.SNAPSHOT, OutputLimitRateType.TERM);
                select.OutputLimitSpec = outputLimitSpec;
            }

            // override the statement spec
            var compiled = new StatementSpecCompiled(
                select,
                streamSpecArray,
                selectClauseCompiled,
                mergedAnnotations,
                groupByExpressions,
                EmptyList<ExprSubselectNode>.Instance,
                EmptyList<ExprDeclaredNode>.Instance,
                EmptyList<ExprTableAccessNode>.Instance);
            var dataflowClassPostfix = context.CodegenEnv.ClassPostfix + "__dfo" + context.OperatorNumber;
            var containerStatement = context.Base.StatementSpec;
            context.Base.StatementSpec = compiled;

            // make forgeable
            var forgeablesResult = StmtForgeMethodSelectUtil.Make(
                context.Container,
                true,
                context.CodegenEnv.Namespace,
                dataflowClassPostfix,
                context.Base,
                context.Services);

            // return the statement spec
            context.Base.StatementSpec = containerStatement;

            var outputEventType = forgeablesResult.EventType;

            var initializeResult = new DataFlowOpForgeInitializeResult();
            initializeResult.TypeDescriptors = new[] { new GraphTypeDesc(false, true, outputEventType) };
            initializeResult.AdditionalForgeables = forgeablesResult.ForgeResult;

            foreach (var forgable in forgeablesResult.ForgeResult.Forgeables) {
                if (forgable.ForgeableType == StmtClassForgeableType.AIFACTORYPROVIDER) {
                    classNameAIFactoryProvider = forgable.ClassName;
                }
                else if (forgable.ForgeableType == StmtClassForgeableType.FIELDS) {
                    classNameFieldsFactoryProvider = forgable.ClassName;
                }
            }

            return initializeResult;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var builder = new SAIFFInitializeBuilder(
                OP_PACKAGE_NAME + ".select.SelectFactory",
                GetType(),
                "select",
                parent,
                symbols,
                classScope);

            return builder
                .EventtypesMayNull(
                    "EventTypes",
                    eventTypes)
                .Constant(
                    "IsSubmitEventBean",
                    submitEventBean)
                .Constant(
                    "IsIterate",
                    iterate)
                .Constant(
                    "OriginatingStreamToViewableStream",
                    originatingStreamToViewableStream)
                .Expression(
                    "FactoryProvider",
                    NewInstanceInner(
                        classNameAIFactoryProvider,
                        symbols.GetAddInitSvc(builder.Method()),
                        NewInstanceInner(classNameFieldsFactoryProvider)
                    ))
                .Build();
        }

        private Attribute[] AddObjectArrayRepresentation(Attribute[] mergedAnnotations)
        {
            IList<Attribute> annotations = new List<Attribute>();
            foreach (var annotation in annotations) {
                if (!(annotation is EventRepresentationAttribute)) {
                    annotations.Add(annotation);
                }
            }

            annotations.Add(new AnnotationEventRepresentation(EventUnderlyingType.OBJECTARRAY));
            return annotations.ToArray();
        }

        private string[] GetInputPortNames(IDictionary<int, DataFlowOpInputPort> inputPorts)
        {
            IList<string> portNames = new List<string>();
            foreach (var entry in inputPorts) {
                if (entry.Value.OptionalAlias != null) {
                    portNames.Add(entry.Value.OptionalAlias);
                    continue;
                }

                if (entry.Value.StreamNames.Count == 1) {
                    portNames.Add(entry.Value.StreamNames.First());
                }
            }

            return portNames.ToArray();
        }

        private KeyValuePair<int, DataFlowOpInputPort>? FindInputPort(
            string eventTypeName,
            IDictionary<int, DataFlowOpInputPort> inputPorts)
        {
            foreach (var entry in inputPorts) {
                if (entry.Value.OptionalAlias != null && entry.Value.OptionalAlias.Equals(eventTypeName)) {
                    return entry;
                }

                if (entry.Value.StreamNames.Count == 1 && entry.Value.StreamNames.First().Equals(eventTypeName)) {
                    return entry;
                }
            }

            return null;
        }
    }
} // end of namespace