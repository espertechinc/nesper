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
using com.espertech.esper.common.client.dataflow.annotations;
using com.espertech.esper.common.client.dataflow.util;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.epl.dataflow.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.etc;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.epl.dataflow.core.EPDataFlowServiceImpl;

namespace com.espertech.esper.common.@internal.epl.dataflow.ops
{
    public class BeaconSourceForge : DataFlowOperatorForge
    {
        private static readonly IList<string> PARAMETER_PROPERTIES = Collections.List(
            "interval",
            "iterations",
            "initialDelay");

        private readonly IDictionary<string, ExprNode> allProperties = new LinkedHashMap<string, ExprNode>();
        private ExprForge[] evaluatorForges;
        private EventBeanManufacturerForge eventBeanManufacturer;

        [DataFlowOpParameter] private ExprNode initialDelay;

        [DataFlowOpParameter] private ExprNode interval;

        [DataFlowOpParameter] private ExprNode iterations;

        private EventType outputEventType;

        private bool produceEventBean;

        public DataFlowOpForgeInitializeResult InitializeForge(DataFlowOpForgeInitializeContext context)
        {
            iterations = DataFlowParameterValidation.Validate("iterations", iterations, typeof(object), context);
            initialDelay = DataFlowParameterValidation.Validate("initialDelay", initialDelay, typeof(object), context);
            interval = DataFlowParameterValidation.Validate("interval", interval, typeof(object), context);

            if (context.OutputPorts.Count != 1) {
                throw new ArgumentException(
                    "BeaconSource operator requires one output stream but produces " +
                    context.OutputPorts.Count +
                    " streams");
            }

            var port = context.OutputPorts[0];

            // Check if a type is declared
            if (port.OptionalDeclaredType == null || port.OptionalDeclaredType.EventType == null) {
                return InitializeTypeUndeclared(context);
            }

            return InitializeTypeDeclared(port, context);
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            return new SAIFFInitializeBuilder(
                    OP_PACKAGE_NAME + ".beaconsource.BeaconSourceFactory",
                    GetType(),
                    "factory",
                    parent,
                    symbols,
                    classScope)
                .Exprnode("Iterations", iterations)
                .Exprnode("InitialDelay", initialDelay)
                .Exprnode("Interval", interval)
                .Constant("IsProduceEventBean", produceEventBean)
                .Eventtype("OutputEventType", outputEventType)
                .Forges("PropertyEvaluators", evaluatorForges)
                .Manufacturer("Manufacturer", eventBeanManufacturer)
                .Build();
        }

        [DataFlowOpParameter(IsAll = true)]
        public void SetProperty(
            string name,
            ExprNode value)
        {
            allProperties.Put(name, value);
        }

        private DataFlowOpForgeInitializeResult InitializeTypeDeclared(
            DataFlowOpOutputPort port,
            DataFlowOpForgeInitializeContext context)
        {
            produceEventBean = port.OptionalDeclaredType != null && !port.OptionalDeclaredType.IsUnderlying;

            // compile properties to populate
            outputEventType = port.OptionalDeclaredType.EventType;
            var props = allProperties.Keys;
            props.RemoveAll(PARAMETER_PROPERTIES);
            var writables = SetupProperties(props.ToArray(), outputEventType);
            try {
                eventBeanManufacturer = EventTypeUtility.GetManufacturer(
                    outputEventType,
                    writables,
                    context.Services.ImportServiceCompileTime,
                    false,
                    context.Services.EventTypeAvroHandler);
            }
            catch (EventBeanManufactureException e) {
                throw new ExprValidationException(
                    "Cannot manufacture event for the provided type '" + outputEventType.Name + "': " + e.Message,
                    e);
            }

            var index = 0;
            evaluatorForges = new ExprForge[writables.Length];
            var typeWidenerCustomizer =
                context.Services.EventTypeAvroHandler.GetTypeWidenerCustomizer(outputEventType);
            foreach (var writable in writables) {
                object providedProperty = allProperties.Get(writable.PropertyName);
                var exprNode = (ExprNode) providedProperty;
                var validated = EPLValidationUtil.ValidateSimpleGetSubtree(
                    ExprNodeOrigin.DATAFLOWBEACON,
                    exprNode,
                    null,
                    false,
                    context.Base.StatementRawInfo,
                    context.Services);
                TypeWidenerSPI widener;
                try {
                    widener = TypeWidenerFactory.GetCheckPropertyAssignType(
                        ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(validated),
                        validated.Forge.EvaluationType,
                        writable.PropertyType,
                        writable.PropertyName,
                        false,
                        typeWidenerCustomizer,
                        context.Base.StatementName);
                }
                catch (TypeWidenerException e) {
                    throw new ExprValidationException("Failed for property '" + writable.PropertyName + "'", e);
                }

                if (widener != null) {
                    evaluatorForges[index] = new ExprEvalWithTypeWidener(widener, validated, writable.PropertyType);
                }
                else {
                    evaluatorForges[index] = validated.Forge;
                }

                index++;
            }

            return null;
        }

        private DataFlowOpForgeInitializeResult InitializeTypeUndeclared(DataFlowOpForgeInitializeContext context)
        {
            // No type has been declared, we can create one
            var types = new LinkedHashMap<string, object>();
            var props = allProperties.Keys;
            props.RemoveAll(PARAMETER_PROPERTIES);

            var count = 0;
            evaluatorForges = new ExprForge[props.Count];
            foreach (var propertyName in props) {
                var exprNode = allProperties.Get(propertyName);
                var validated = EPLValidationUtil.ValidateSimpleGetSubtree(
                    ExprNodeOrigin.DATAFLOWBEACON,
                    exprNode,
                    null,
                    false,
                    context.StatementRawInfo,
                    context.Services);
                types.Put(propertyName, validated.Forge.EvaluationType);
                evaluatorForges[count] = validated.Forge;
                count++;
            }

            var eventTypeName =
                context.Services.EventTypeNameGeneratorStatement.GetDataflowOperatorTypeName(context.OperatorNumber);
            var metadata = new EventTypeMetadata(
                eventTypeName,
                context.Base.ModuleName,
                EventTypeTypeClass.DBDERIVED,
                EventTypeApplicationType.OBJECTARR,
                NameAccessModifier.TRANSIENT,
                EventTypeBusModifier.NONBUS,
                false,
                EventTypeIdPair.Unassigned());
            outputEventType = BaseNestableEventUtil.MakeOATypeCompileTime(
                metadata,
                types,
                null,
                null,
                null,
                null,
                context.Services.BeanEventTypeFactoryPrivate,
                context.Services.EventTypeCompileTimeResolver);
            context.Services.EventTypeCompileTimeRegistry.NewType(outputEventType);

            return new DataFlowOpForgeInitializeResult(new[] {new GraphTypeDesc(false, true, outputEventType)});
        }

        private static WriteablePropertyDescriptor[] SetupProperties(
            string[] propertyNamesOffered,
            EventType outputEventType)
        {
            var writeables = EventTypeUtility.GetWriteableProperties(outputEventType, false, false);
            var writablesList = new List<WriteablePropertyDescriptor>();

            for (var i = 0; i < propertyNamesOffered.Length; i++) {
                var propertyName = propertyNamesOffered[i];
                var writable = EventTypeUtility.FindWritable(propertyName, writeables);
                if (writable == null) {
                    throw new ExprValidationException(
                        "Failed to find writable property '" +
                        propertyName +
                        "' for event type '" +
                        outputEventType.Name +
                        "'");
                }

                writablesList.Add(writable);
            }

            return writablesList.ToArray();
        }
    }
} // end of namespace