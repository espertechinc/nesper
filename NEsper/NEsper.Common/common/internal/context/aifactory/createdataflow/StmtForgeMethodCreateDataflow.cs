///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.annotations;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.annotation;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.epl.dataflow.ops;
using com.espertech.esper.common.@internal.epl.dataflow.realize;
using com.espertech.esper.common.@internal.epl.dataflow.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.@select.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.context.aifactory.createdataflow
{
    public class StmtForgeMethodCreateDataflow : StmtForgeMethod
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string EVENT_WRAPPED_TYPE = "eventbean";

        private readonly StatementBaseInfo @base;

        public StmtForgeMethodCreateDataflow(StatementBaseInfo @base)
        {
            this.@base = @base;
        }

        public StmtForgeMethodResult Make(
            string packageName,
            string classPostfix,
            StatementCompileTimeServices services)
        {
            var statementSpec = @base.StatementSpec;

            var createDataFlowDesc = statementSpec.Raw.CreateDataFlowDesc;
            services.DataFlowCompileTimeRegistry.NewDataFlow(createDataFlowDesc.GraphName);

            string eventTypeName = services.EventTypeNameGeneratorStatement.AnonymousTypeName;
            var metadata = new EventTypeMetadata(
                eventTypeName, @base.ModuleName, EventTypeTypeClass.STATEMENTOUT, EventTypeApplicationType.MAP,
                NameAccessModifier.TRANSIENT, EventTypeBusModifier.NONBUS, false, EventTypeIdPair.Unassigned());
            EventType eventType = BaseNestableEventUtil.MakeMapTypeCompileTime(
                metadata, Collections.GetEmptyMap<string, object>(), null, null, null, null,
                services.BeanEventTypeFactoryPrivate,
                services.EventTypeCompileTimeResolver);
            services.EventTypeCompileTimeRegistry.NewType(eventType);

            var statementFieldsClassName =
                CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(StatementFields), classPostfix);
            var codegenEnv = new DataFlowOpForgeCodegenEnv(packageName, classPostfix);

            var dataflowForge = BuildForge(createDataFlowDesc, codegenEnv, @base, services);

            var packageScope = new CodegenNamespaceScope(
                packageName, statementFieldsClassName, services.IsInstrumented);
            var aiFactoryProviderClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(
                typeof(StatementAIFactoryProvider), classPostfix);
            var forge =
                new StatementAgentInstanceFactoryCreateDataflowForge(eventType, dataflowForge);
            var aiFactoryForgable =
                new StmtClassForgableAIFactoryProviderCreateDataflow(aiFactoryProviderClassName, packageScope, forge);

            var selectSubscriberDescriptor = new SelectSubscriberDescriptor();
            var informationals = StatementInformationalsUtil.GetInformationals(
                @base,
                Collections.GetEmptyList<FilterSpecCompiled>(),
                Collections.GetEmptyList<ScheduleHandleCallbackProvider>(),
                Collections.GetEmptyList<NamedWindowConsumerStreamSpec>(),
                false,
                selectSubscriberDescriptor, packageScope, services);
            var statementProviderClassName =
                CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(StatementProvider), classPostfix);
            var stmtProvider = new StmtClassForgableStmtProvider(
                aiFactoryProviderClassName, statementProviderClassName, informationals, packageScope);

            IList<StmtClassForgable> forgables = new List<StmtClassForgable>();
            forgables.Add(aiFactoryForgable);
            forgables.Add(stmtProvider);
            forgables.Add(new StmtClassForgableStmtFields(statementFieldsClassName, packageScope, 0));

            // compiled filter spec list
            IList<FilterSpecCompiled> filterSpecCompileds = new List<FilterSpecCompiled>();
            foreach (KeyValuePair<int, DataFlowOperatorForge> entry in dataflowForge.OperatorFactories) {
                if (entry.Value is EventBusSourceForge) {
                    var eventBusSource = (EventBusSourceForge) entry.Value;
                    filterSpecCompileds.Add(eventBusSource.FilterSpecCompiled);
                }
            }

            var filterBooleanExpr = FilterSpecCompiled.MakeExprNodeList(
                filterSpecCompileds, Collections.GetEmptyList<FilterSpecParamExprNodeForge>());
            IList<NamedWindowConsumerStreamSpec> namedWindowConsumers = new List<NamedWindowConsumerStreamSpec>();
            IList<ScheduleHandleCallbackProvider> scheduleds = new List<ScheduleHandleCallbackProvider>();

            // add additional forgeables
            foreach (StmtForgeMethodResult additional in dataflowForge.AdditionalForgables) {
                foreach (var v in Enumerable.Reverse(additional.Forgables)) {
                    forgables.Insert(0, v);
                }
                scheduleds.AddAll(additional.Scheduleds);
            }

            return new StmtForgeMethodResult(
                forgables, filterSpecCompileds, scheduleds, namedWindowConsumers, filterBooleanExpr);
        }

        private static DataflowDescForge BuildForge(
            CreateDataFlowDesc desc,
            DataFlowOpForgeCodegenEnv codegenEnv,
            StatementBaseInfo @base,
            StatementCompileTimeServices services)
        {
            // basic validation
            Validate(desc);

            // compile operator annotations
            IDictionary<object, Attribute[]> operatorAnnotations = new Dictionary<object, Attribute[]>();
            var count = 0;
            foreach (var spec in desc.Operators) {
                Attribute[] operatorAnnotation;
                try {
                    operatorAnnotation = AnnotationUtil.CompileAnnotations(
                        spec.Annotations, services.ImportServiceCompileTime, null);
                }
                catch (StatementSpecCompileException e) {
                    throw new ExprValidationException("Invalid annotation: " + e.Message, e);
                }

                // Using the 'count' helps facilitate the build order based lookups, but this is really quite
                // problematic and messy.  Esper is using type erasure in a way that is highly frowned upon.
                operatorAnnotations.Put(count, operatorAnnotation);
                count++;
            }

            // resolve types
            var declaredTypes = ResolveTypes(desc, @base, services);

            // resolve operator classes
            var operatorMetadata = ResolveMetadata(
                desc, operatorAnnotations, @base, services);

            // build dependency graph:  operator -> [input_providing_op, input_providing_op]
            var operatorDependencies = AnalyzeDependencies(desc);

            // determine build order of operators
            var operatorBuildOrder = AnalyzeBuildOrder(operatorDependencies);

            // instantiate operator forges
            var operatorForges = InstantiateOperatorForges(
                operatorDependencies, operatorMetadata, operatorAnnotations, declaredTypes, desc, @base, services);

            // Build graph that references port numbers (port number is simply the method offset number or to-be-generated slot in the list)
            var initForgesResult = DetermineChannelsInitForges(
                operatorForges, operatorBuildOrder, operatorAnnotations, operatorDependencies, operatorMetadata,
                declaredTypes, desc, codegenEnv, @base, services);
            if (Log.IsDebugEnabled) {
                Log.Debug(
                    "For flow '" + desc.GraphName + "' channels are: " +
                    LogicalChannelUtil.PrintChannels(initForgesResult.LogicalChannels));
            }

            return new DataflowDescForge(
                desc.GraphName, declaredTypes, operatorMetadata, operatorBuildOrder,
                operatorForges, initForgesResult.LogicalChannels, initForgesResult.AdditionalForgables);
        }

        private static InitForgesResult DetermineChannelsInitForges(
            IDictionary<int, DataFlowOperatorForge> operatorForges,
            ISet<int> operatorBuildOrder,
            IDictionary<object, Attribute[]> operatorAnnotations,
            IDictionary<int, OperatorDependencyEntry> operatorDependencies,
            IDictionary<int, OperatorMetadataDescriptor> operatorMetadata,
            IDictionary<string, EventType> declaredTypes,
            CreateDataFlowDesc desc,
            DataFlowOpForgeCodegenEnv codegenEnv,
            StatementBaseInfo @base,
            StatementCompileTimeServices services)
        {
            var container = services.Container;

            // Step 1: find all the operators that have explicit output ports and determine the type of such
            IDictionary<int, IList<LogicalChannelProducingPortDeclared>> declaredOutputPorts =
                new Dictionary<int, IList<LogicalChannelProducingPortDeclared>>();
            foreach (var operatorNum in operatorBuildOrder) {
                var metadata = operatorMetadata.Get(operatorNum);
                var operatorForge = operatorForges.Get(operatorNum);
                var operatorSpec = desc.Operators[operatorNum];
                var annotationPorts = DetermineAnnotatedOutputPorts(
                    operatorNum, operatorForge, operatorSpec, metadata, @base, services);
                var graphDeclaredPorts = DetermineGraphDeclaredOutputPorts(
                    operatorNum, operatorForge, operatorSpec, metadata, declaredTypes, services);

                IList<LogicalChannelProducingPortDeclared> allDeclaredPorts =
                    new List<LogicalChannelProducingPortDeclared>();
                allDeclaredPorts.AddAll(annotationPorts);
                allDeclaredPorts.AddAll(graphDeclaredPorts);

                declaredOutputPorts.Put(operatorNum, allDeclaredPorts);
            }

            // Step 2: determine for each operator the output ports: some are determined via "prepare" and some can be implicit
            // since they may not be declared or can be punctuation.
            // Therefore we need to meet ends: on one end the declared types, on the other the implied and dynamically-determined
            // types based on input.
            //
            // We do this in operator build order.
            IDictionary<int, IList<LogicalChannelProducingPortCompiled>> compiledOutputPorts =
                new Dictionary<int, IList<LogicalChannelProducingPortCompiled>>();
            IList<StmtForgeMethodResult> additionalForgables = new List<StmtForgeMethodResult>();
            foreach (var operatorNum in operatorBuildOrder) {
                var metadata = operatorMetadata.Get(operatorNum);
                var operatorForge = operatorForges.Get(operatorNum);
                var operatorSpec = desc.Operators[operatorNum];
                var operatorAnno = operatorAnnotations.Get(operatorNum);

                // Handle incoming first: if the operator has incoming ports, each of such should already have type information
                // Compile type information, call method, obtain output types.
                var incomingDependentOpNums = operatorDependencies[operatorNum].Incoming;
                var initializeResult = InitializeOperatorForge(
                    container, operatorNum, operatorForge, operatorAnno, metadata, operatorSpec,
                    declaredOutputPorts, compiledOutputPorts, declaredTypes, incomingDependentOpNums,
                    desc, codegenEnv, @base, services);

                GraphTypeDesc[] typesPerOutput = null;
                if (initializeResult != null) {
                    typesPerOutput = initializeResult.TypeDescriptors;
                    if (initializeResult.AdditionalForgables != null) {
                        additionalForgables.Add(initializeResult.AdditionalForgables);
                    }
                }

                // Handle outgoing second:
                //   If there is outgoing declared, use that.
                //   If output types have been determined based on input, use that.
                //   else error
                var outgoingPorts = DetermineOutgoingPorts(
                    operatorNum,
                    operatorSpec,
                    metadata,
                    compiledOutputPorts,
                    declaredOutputPorts,
                    typesPerOutput,
                    incomingDependentOpNums);
                compiledOutputPorts.Put(operatorNum, outgoingPorts);
            }

            // Step 3: normalization and connecting input ports with output ports (logically, no methods yet)
            IList<LogicalChannel> channels = new List<LogicalChannel>();
            var channelId = 0;
            foreach (var operatorNum in operatorBuildOrder) {
                var dependencies = operatorDependencies.Get(operatorNum);
                var operatorSpec = desc.Operators[operatorNum];
                var inputNames = operatorSpec.Input.StreamNamesAndAliases;
                var descriptor = operatorMetadata.Get(operatorNum);

                // handle each (a,b,c AS d)
                var streamNum = -1;
                foreach (var inputName in inputNames) {
                    streamNum++;

                    // get producers
                    IList<LogicalChannelProducingPortCompiled> producingPorts =
                        LogicalChannelUtil.GetOutputPortByStreamName(
                            dependencies.Incoming, inputName.InputStreamNames, compiledOutputPorts);
                    if (producingPorts.Count < inputName.InputStreamNames.Length) {
                        throw new IllegalStateException("Failed to find producing ports");
                    }

                    // determine type compatibility
                    if (producingPorts.Count > 1) {
                        var first = producingPorts[0];
                        for (var i = 1; i < producingPorts.Count; i++) {
                            var other = producingPorts[i];
                            CompareTypeInfo(
                                descriptor.OperatorName, first.StreamName, first.GraphTypeDesc, other.StreamName,
                                other.GraphTypeDesc);
                        }
                    }

                    var optionalAlias = inputName.OptionalAsName;

                    // handle each stream name
                    foreach (var streamName in inputName.InputStreamNames) {
                        foreach (var port in producingPorts) {
                            if (port.StreamName.Equals(streamName)) {
                                var channel = new LogicalChannel(
                                    channelId++, descriptor.OperatorName, operatorNum, streamNum, streamName,
                                    optionalAlias, descriptor.OperatorPrettyPrint, port);
                                channels.Add(channel);
                            }
                        }
                    }
                }
            }

            return new InitForgesResult(channels, additionalForgables);
        }

        private static IDictionary<int, DataFlowOperatorForge> InstantiateOperatorForges(
            IDictionary<int, OperatorDependencyEntry> operatorDependencies,
            IDictionary<int, OperatorMetadataDescriptor> operatorMetadata,
            IDictionary<object, Attribute[]> operatorAnnotations,
            IDictionary<string, EventType> declaredTypes,
            CreateDataFlowDesc createDataFlowDesc,
            StatementBaseInfo @base,
            StatementCompileTimeServices services)
        {
            IDictionary<int, DataFlowOperatorForge> forges = new Dictionary<int, DataFlowOperatorForge>();
            foreach (var entry in operatorMetadata) {
                var forge = InstantiateOperatorForge(
                    createDataFlowDesc, entry.Key, entry.Value, @base, services);
                forges.Put(entry.Key, forge);
            }

            return forges;
        }

        private static DataFlowOperatorForge InstantiateOperatorForge(
            CreateDataFlowDesc createDataFlowDesc,
            int operatorNum,
            OperatorMetadataDescriptor desc,
            StatementBaseInfo @base,
            StatementCompileTimeServices services)
        {
            var operatorSpec = createDataFlowDesc.Operators[operatorNum];
            var dataflowName = createDataFlowDesc.GraphName;
            Type clazz = desc.ForgeClass;

            // use non-factory class if provided
            object forgeObject;
            try {
                forgeObject = TypeHelper.Instantiate(clazz);
            }
            catch (Exception e) {
                throw new ExprValidationException("Failed to instantiate: " + e.Message);
            }

            // inject properties
            var exprValidationContext = new ExprValidationContextBuilder(
                    new StreamTypeServiceImpl(false), @base.StatementRawInfo, services)
                .Build();
            var configs = operatorSpec.Detail == null
                ? Collections.GetEmptyMap<string, object>()
                : operatorSpec.Detail.Configs;
            InjectObjectProperties(
                dataflowName, operatorSpec.OperatorName, operatorNum, configs, forgeObject, null, null,
                exprValidationContext);

            if (!(forgeObject is DataFlowOperatorForge)) {
                throw new ExprValidationException(
                    "Operator object '" + forgeObject.GetType().Name + "' does not implement the '" +
                    typeof(DataFlowOperatorForge) + "' interface ");
            }

            return (DataFlowOperatorForge) forgeObject;
        }

        private static IDictionary<int, DataFlowOpInputPort> GetInputPorts(
            int operatorNumber,
            GraphOperatorSpec operatorSpec,
            ISet<int> incomingDependentOpNums,
            IDictionary<int, IList<LogicalChannelProducingPortDeclared>> declaredOutputPorts,
            IDictionary<int, IList<LogicalChannelProducingPortCompiled>> compiledOutputPorts)
        {
            // determine input ports to build up the input port metadata
            var numDeclared = operatorSpec.Input.StreamNamesAndAliases.Count;
            IDictionary<int, DataFlowOpInputPort> inputPorts = new LinkedHashMap<int, DataFlowOpInputPort>();
            for (var inputPortNum = 0; inputPortNum < numDeclared; inputPortNum++) {
                var inputItem = operatorSpec.Input.StreamNamesAndAliases[inputPortNum];
                IList<LogicalChannelProducingPortCompiled> producingPorts =
                    LogicalChannelUtil.GetOutputPortByStreamName(
                        incomingDependentOpNums, inputItem.InputStreamNames, compiledOutputPorts);

                DataFlowOpInputPort port;
                if (producingPorts.IsEmpty()) {
                    // this can be when the operator itself is the incoming port, i.e. feedback loop
                    var declareds = declaredOutputPorts.Get(operatorNumber);
                    if (declareds == null || declareds.IsEmpty()) {
                        throw new ExprValidationException(
                            "Failed validation for operator '" + operatorSpec.OperatorName +
                            "': No output ports declared");
                    }

                    LogicalChannelProducingPortDeclared foundDeclared = null;
                    foreach (var declared in declareds) {
                        if (inputItem.InputStreamNames.Contains(declared.StreamName)) {
                            foundDeclared = declared;
                            break;
                        }
                    }

                    if (foundDeclared == null) {
                        throw new ExprValidationException(
                            "Failed validation for operator '" + operatorSpec.OperatorName +
                            "': Failed to find output port declared");
                    }

                    port = new DataFlowOpInputPort(
                        foundDeclared.TypeDesc, new HashSet<string>(inputItem.InputStreamNames),
                        inputItem.OptionalAsName, false);
                }
                else {
                    port = new DataFlowOpInputPort(
                        new GraphTypeDesc(false, false, producingPorts[0].GraphTypeDesc.EventType),
                        new HashSet<string>(inputItem.InputStreamNames), inputItem.OptionalAsName,
                        producingPorts[0].HasPunctuation);
                }

                inputPorts.Put(inputPortNum, port);
            }

            return inputPorts;
        }

        private static IDictionary<int, OperatorMetadataDescriptor> ResolveMetadata(
            CreateDataFlowDesc desc,
            IDictionary<object, Attribute[]> operatorAnnotations,
            StatementBaseInfo @base,
            StatementCompileTimeServices services)
        {
            IDictionary<int, OperatorMetadataDescriptor> operatorClasses =
                new Dictionary<int, OperatorMetadataDescriptor>();
            for (var i = 0; i < desc.Operators.Count; i++) {
                var operatorSpec = desc.Operators[i];
                var numOutputPorts = operatorSpec.Output.Items.Count;
                var operatorName = operatorSpec.OperatorName;
                var operatorPrettyPrint = ToPrettyPrint(i, operatorSpec);
                var operatorAnnotation = operatorAnnotations.Get(operatorSpec);

                Type forgeClass = null;
                try {
                    var forgeClassName = operatorSpec.OperatorName + "Forge";
                    forgeClass = services.ImportServiceCompileTime.ResolveClass(forgeClassName, false);
                }
                catch (ImportException e) {
                    try {
                        var forgeClassName = operatorSpec.OperatorName;
                        forgeClass = services.ImportServiceCompileTime.ResolveClass(forgeClassName, false);
                    }
                    catch (ImportException) {
                        // expected
                    }

                    if (forgeClass == null) {
                        throw new ExprValidationException(
                            "Failed to resolve forge class for operator '" + operatorSpec.OperatorName + "': " +
                            e.Message, e);
                    }
                }

                // if the factory implements the interface use that
                if (!TypeHelper.IsImplementsInterface(forgeClass, typeof(DataFlowOperatorForge))) {
                    throw new ExprValidationException(
                        "Forge class for operator '" + operatorSpec.OperatorName + "' does not implement interface '" +
                        typeof(DataFlowOperatorForge).Name + "' (class '" + forgeClass.Name + "')");
                }

                var descriptor = new OperatorMetadataDescriptor(
                    forgeClass, operatorPrettyPrint, operatorAnnotation, numOutputPorts, operatorName);
                operatorClasses.Put(i, descriptor);
            }

            return operatorClasses;
        }

        private static IDictionary<string, EventType> ResolveTypes(
            CreateDataFlowDesc desc,
            StatementBaseInfo @base,
            StatementCompileTimeServices services)
        {
            IDictionary<string, EventType> types = new Dictionary<string, EventType>();
            foreach (var spec in desc.Schemas) {
                var eventType = EventTypeUtility.CreateNonVariantType(true, spec, @base, services);
                types.Put(spec.SchemaName, eventType);
            }

            return types;
        }

        private static void Validate(CreateDataFlowDesc desc)
        {
            foreach (var spec in desc.Operators) {
                foreach (var @out in spec.Output.Items) {
                    if (@out.TypeInfo.Count > 1) {
                        throw new ExprValidationException(
                            "Failed to validate operator '" + spec.OperatorName +
                            "': Multiple output types for a single stream '" + @out.StreamName + "' are not supported");
                    }
                }
            }

            ISet<string> schemaNames = new HashSet<string>();
            foreach (var schema in desc.Schemas) {
                if (schemaNames.Contains(schema.SchemaName)) {
                    throw new ExprValidationException(
                        "Schema name '" + schema.SchemaName + "' is declared more then once");
                }

                schemaNames.Add(schema.SchemaName);
            }
        }

        private static string ToPrettyPrint(
            int operatorNum,
            GraphOperatorSpec spec)
        {
            var writer = new StringWriter();
            writer.Write(spec.OperatorName);
            writer.Write("#");
            writer.Write(Convert.ToString(operatorNum));

            writer.Write("(");
            var delimiter = "";
            foreach (var inputItem in spec.Input.StreamNamesAndAliases) {
                writer.Write(delimiter);
                ToPrettyPrintInput(inputItem, writer);
                if (inputItem.OptionalAsName != null) {
                    writer.Write(" as ");
                    writer.Write(inputItem.OptionalAsName);
                }

                delimiter = ", ";
            }

            writer.Write(")");

            if (spec.Output.Items.IsEmpty()) {
                return writer.ToString();
            }

            writer.Write(" -> ");

            delimiter = "";
            foreach (var outputItem in spec.Output.Items) {
                writer.Write(delimiter);
                writer.Write(outputItem.StreamName);
                WriteTypes(outputItem.TypeInfo, writer);
                delimiter = ",";
            }

            return writer.ToString();
        }

        private static void ToPrettyPrintInput(
            GraphOperatorInputNamesAlias inputItem,
            TextWriter writer)
        {
            if (inputItem.InputStreamNames.Length == 1) {
                writer.Write(inputItem.InputStreamNames[0]);
            }
            else {
                writer.Write("(");
                var delimiterNames = "";
                foreach (var name in inputItem.InputStreamNames) {
                    writer.Write(delimiterNames);
                    writer.Write(name);
                    delimiterNames = ",";
                }

                writer.Write(")");
            }
        }

        private static void WriteTypes(
            IList<GraphOperatorOutputItemType> types,
            TextWriter writer)
        {
            if (types.IsEmpty()) {
                return;
            }

            writer.Write("<");
            var typeDelimiter = "";
            foreach (var type in types) {
                writer.Write(typeDelimiter);
                WriteType(type, writer);
                typeDelimiter = ",";
            }

            writer.Write(">");
        }

        private static void WriteType(
            GraphOperatorOutputItemType type,
            TextWriter writer)
        {
            if (type.IsWildcard) {
                writer.Write('?');
                return;
            }

            writer.Write(type.TypeOrClassname);
            WriteTypes(type.TypeParameters, writer);
        }

        private static IDictionary<int, OperatorDependencyEntry> AnalyzeDependencies(CreateDataFlowDesc graphDesc)
        {
            IDictionary<int, OperatorDependencyEntry> logicalOpDependencies =
                new Dictionary<int, OperatorDependencyEntry>();
            for (var i = 0; i < graphDesc.Operators.Count; i++) {
                var entry = new OperatorDependencyEntry();
                logicalOpDependencies.Put(i, entry);
            }

            for (var consumingOpNum = 0; consumingOpNum < graphDesc.Operators.Count; consumingOpNum++) {
                var entry = logicalOpDependencies.Get(consumingOpNum);
                var op = graphDesc.Operators[consumingOpNum];

                // for each input item
                foreach (var input in op.Input.StreamNamesAndAliases) {
                    // for each stream name listed
                    foreach (var inputStreamName in input.InputStreamNames) {
                        // find all operators providing such input stream
                        var found = false;

                        // for each operator
                        for (var providerOpNum = 0; providerOpNum < graphDesc.Operators.Count; providerOpNum++) {
                            var from = graphDesc.Operators[providerOpNum];

                            foreach (var outputItem in from.Output.Items) {
                                if (outputItem.StreamName.Equals(inputStreamName)) {
                                    found = true;
                                    entry.AddIncoming(providerOpNum);
                                    logicalOpDependencies.Get(providerOpNum).AddOutgoing(consumingOpNum);
                                }
                            }
                        }

                        if (!found) {
                            throw new ExprValidationException(
                                "Input stream '" + inputStreamName + "' consumed by operator '" + op.OperatorName +
                                "' could not be found");
                        }
                    }
                }
            }

            return logicalOpDependencies;
        }

        private static ISet<int> AnalyzeBuildOrder(IDictionary<int, OperatorDependencyEntry> operators)
        {
            var graph = new DependencyGraph(operators.Count, true);
            foreach (var entry in operators) {
                var myOpNum = entry.Key;
                foreach (var incoming in entry.Value.Incoming) {
                    if (myOpNum != incoming) {
                        graph.AddDependency(myOpNum, incoming);
                    }
                }
            }

            var topDownSet = new LinkedHashSet<int>();
            while (topDownSet.Count < operators.Count) {
                // secondary sort according to the order of listing
                ISet<int> rootNodes = new SortedSet<int>(
                    new ProxyComparer<int>(
                        (
                                o1,
                                o2) => -1 * o1.CompareTo(o2)));

                rootNodes.AddAll(graph.GetRootNodes(topDownSet));

                if (rootNodes.IsEmpty()) { // circular dependency could cause this
                    for (var i = 0; i < operators.Count; i++) {
                        if (!topDownSet.Contains(i)) {
                            rootNodes.Add(i);
                            break;
                        }
                    }
                }

                topDownSet.AddAll(rootNodes);
            }

            // invert the output
            var inverted = new LinkedHashSet<int>();
            var arr = topDownSet.ToArray();
            for (var i = arr.Length - 1; i >= 0; i--) {
                inverted.Add(arr[i]);
            }

            return inverted;
        }

        private static void InjectObjectProperties(
            string dataFlowName,
            string operatorName,
            int operatorNum,
            IDictionary<string, object> configs,
            object instance,
            EPDataFlowOperatorParameterProvider optionalParameterProvider,
            IDictionary<string, object> optionalParameterURIs,
            ExprValidationContext exprValidationContext)
        {
            // determine if there is a property holder which holds all properties
            ICollection<FieldInfo> propertyHolderFields = TypeHelper.FindAnnotatedFields(
                instance.GetType(), typeof(DataFlowOpPropertyHolderAttribute));
            if (propertyHolderFields.Count > 1) {
                throw new ArgumentException(
                    "May apply " + typeof(DataFlowOpPropertyHolderAttribute).Name + " annotation only to a single field");
            }

            // determine which class to write properties to
            object propertyInstance;
            if (propertyHolderFields.IsEmpty()) {
                propertyInstance = instance;
            }
            else {
                Type propertyHolderClass = propertyHolderFields.First().FieldType;
                try {
                    propertyInstance = TypeHelper.Instantiate(propertyHolderClass);
                }
                catch (Exception e) {
                    throw new ExprValidationException(
                        "Failed to instantiate '" + propertyHolderClass + "': " + e.Message, e);
                }
            }

            // populate either the instance itself or the property-holder
            PopulateUtil.PopulateObject(
                operatorName, operatorNum, dataFlowName, configs, propertyInstance, ExprNodeOrigin.DATAFLOW,
                exprValidationContext, optionalParameterProvider, optionalParameterURIs);

            // set holder
            if (!propertyHolderFields.IsEmpty()) {
                var field = propertyHolderFields.First();
                try {
                    field.SetValue(instance, propertyInstance);
                }
                catch (Exception e) {
                    throw new ExprValidationException("Failed to set field '" + field.Name + "': " + e.Message, e);
                }
            }
        }

        private static IList<LogicalChannelProducingPortDeclared> DetermineAnnotatedOutputPorts(
            int operatorNumber,
            DataFlowOperatorForge forge,
            GraphOperatorSpec operatorSpec,
            OperatorMetadataDescriptor descriptor,
            StatementBaseInfo @base,
            StatementCompileTimeServices services)
        {
            IList<LogicalChannelProducingPortDeclared> ports = new List<LogicalChannelProducingPortDeclared>();

            // See if any @OutputTypes annotations exists
            var annotations = TypeHelper.GetAnnotations(
                typeof(OutputTypeAttribute),
                forge.GetType().GetCustomAttributes().UnwrapIntoArray<Attribute>());

            foreach (var annotation in annotations) {
                OutputTypesAttribute outputTypes = (OutputTypesAttribute) annotation;

                // create local event type for the declared type
                IDictionary<string, object> propertiesRaw = new LinkedHashMap<string, object>();
                OutputTypeAttribute[] outputTypeArr = outputTypes.Value;
                foreach (OutputTypeAttribute outputType in outputTypeArr) {
                    Type clazz;
                    if ((outputType.Type != null) && (outputType.Type != typeof(OutputTypeAttribute))) {
                        clazz = outputType.Type;
                    }
                    else {
                        string typeName = outputType.TypeName;
                        clazz = TypeHelper.GetTypeForSimpleName(
                            typeName, services.ImportServiceCompileTime.ClassForNameProvider);
                        if (clazz == null) {
                            try {
                                clazz = services.ImportServiceCompileTime.ResolveClass(typeName, false);
                            }
                            catch (ImportException) {
                                throw new EPRuntimeException("Failed to resolve type '" + typeName + "'");
                            }
                        }
                    }

                    propertiesRaw.Put(outputType.Name, clazz);
                }

                var propertiesCompiled = EventTypeUtility.CompileMapTypeProperties(
                    propertiesRaw, services.EventTypeCompileTimeResolver);
                var eventTypeName =
                    services.EventTypeNameGeneratorStatement.GetDataflowOperatorTypeName(operatorNumber);
                var metadata = new EventTypeMetadata(
                    eventTypeName, @base.ModuleName, EventTypeTypeClass.DBDERIVED, EventTypeApplicationType.OBJECTARR,
                    NameAccessModifier.TRANSIENT, EventTypeBusModifier.NONBUS, false, EventTypeIdPair.Unassigned());
                EventType eventType = BaseNestableEventUtil.MakeOATypeCompileTime(
                    metadata, propertiesCompiled, null, null, null, null, services.BeanEventTypeFactoryPrivate,
                    services.EventTypeCompileTimeResolver);
                services.EventTypeCompileTimeRegistry.NewType(eventType);

                // determine output stream name, which must be provided
                var declaredOutput = operatorSpec.Output.Items;
                if (declaredOutput.IsEmpty()) {
                    throw new ExprValidationException("No output stream declared");
                }

                if (declaredOutput.Count < outputTypes.PortNumber) {
                    throw new ExprValidationException("No output stream declared for this port");
                }

                var streamName = declaredOutput[outputTypes.PortNumber].StreamName;

                var isDeclaredPunctuated = TypeHelper.IsAnnotationListed(
                    typeof(DataFlowOpProvideSignalAttribute),
                    forge.GetType().GetCustomAttributes().UnwrapIntoArray<Attribute>());
                var port = new LogicalChannelProducingPortDeclared(
                    operatorNumber, descriptor.OperatorPrettyPrint, streamName, outputTypes.PortNumber,
                    new GraphTypeDesc(false, false, eventType), isDeclaredPunctuated);
                ports.Add(port);
            }

            return ports;
        }

        private static IList<LogicalChannelProducingPortDeclared> DetermineGraphDeclaredOutputPorts(
            int producingOpNum,
            DataFlowOperatorForge operatorForge,
            GraphOperatorSpec operatorSpec,
            OperatorMetadataDescriptor metadata,
            IDictionary<string, EventType> types,
            StatementCompileTimeServices services)
        {
            IList<LogicalChannelProducingPortDeclared> ports = new List<LogicalChannelProducingPortDeclared>();

            var portNumber = 0;
            foreach (var outputItem in operatorSpec.Output.Items) {
                if (outputItem.TypeInfo.Count > 1) {
                    throw new ExprValidationException("Multiple parameter types are not supported");
                }

                if (!outputItem.TypeInfo.IsEmpty()) {
                    var typeDesc = DetermineTypeOutputPort(outputItem.TypeInfo[0], types, services);
                    var operatorForgeClass = operatorForge.GetType();
                    var isDeclaredPunctuated = TypeHelper.IsAnnotationListed(
                        typeof(DataFlowOpProvideSignalAttribute),
                        operatorForgeClass.GetCustomAttributes().UnwrapIntoArray<Attribute>());
                    ports.Add(
                        new LogicalChannelProducingPortDeclared(
                            producingOpNum, metadata.OperatorPrettyPrint, outputItem.StreamName, portNumber, typeDesc,
                            isDeclaredPunctuated));
                }

                portNumber++;
            }

            return ports;
        }

        private static GraphTypeDesc DetermineTypeOutputPort(
            GraphOperatorOutputItemType outType,
            IDictionary<string, EventType> types,
            StatementCompileTimeServices services)
        {
            EventType eventType = null;
            var isWildcard = false;
            var isUnderlying = true;

            var typeOrClassname = outType.TypeOrClassname;
            if (typeOrClassname != null && typeOrClassname.ToLowerInvariant().Equals(EVENT_WRAPPED_TYPE)) {
                isUnderlying = false;
                if (!outType.TypeParameters.IsEmpty() && !outType.TypeParameters[0].IsWildcard) {
                    var typeName = outType.TypeParameters[0].TypeOrClassname;
                    eventType = ResolveType(typeName, types, services);
                }
                else {
                    isWildcard = true;
                }
            }
            else if (typeOrClassname != null) {
                eventType = ResolveType(typeOrClassname, types, services);
            }
            else {
                isWildcard = true;
            }

            return new GraphTypeDesc(isWildcard, isUnderlying, eventType);
        }

        private static EventType ResolveType(
            string typeOrClassname,
            IDictionary<string, EventType> types,
            StatementCompileTimeServices services)
        {
            var eventType = types.Get(typeOrClassname);
            if (eventType == null) {
                eventType = services.EventTypeCompileTimeResolver.GetTypeByName(typeOrClassname);
            }

            if (eventType == null) {
                throw new ExprValidationException("Failed to find event type '" + typeOrClassname + "'");
            }

            return eventType;
        }

        private static DataFlowOpForgeInitializeResult InitializeOperatorForge(
            IContainer container,
            int operatorNumber,
            DataFlowOperatorForge forge,
            Attribute[] operatorAnnotations,
            OperatorMetadataDescriptor meta,
            GraphOperatorSpec operatorSpec,
            IDictionary<int, IList<LogicalChannelProducingPortDeclared>> declaredOutputPorts,
            IDictionary<int, IList<LogicalChannelProducingPortCompiled>> compiledOutputPorts,
            IDictionary<string, EventType> types,
            ICollection<int> incomingDependentOpNums,
            CreateDataFlowDesc desc,
            DataFlowOpForgeCodegenEnv codegenEnv,
            StatementBaseInfo @base,
            StatementCompileTimeServices services)
        {
            // determine input ports to build up the input port metadata
            var numDeclared = operatorSpec.Input.StreamNamesAndAliases.Count;
            IDictionary<int, DataFlowOpInputPort> inputPorts = new LinkedHashMap<int, DataFlowOpInputPort>();
            for (var inputPortNum = 0; inputPortNum < numDeclared; inputPortNum++) {
                GraphOperatorInputNamesAlias inputItem = operatorSpec.Input.StreamNamesAndAliases[inputPortNum];
                IList<LogicalChannelProducingPortCompiled> producingPorts =
                    LogicalChannelUtil.GetOutputPortByStreamName(
                        incomingDependentOpNums, inputItem.InputStreamNames, compiledOutputPorts);

                DataFlowOpInputPort port;
                if (producingPorts.IsEmpty()) {
                    // this can be when the operator itself is the incoming port, i.e. feedback loop
                    var declareds = declaredOutputPorts.Get(operatorNumber);
                    if (declareds == null || declareds.IsEmpty()) {
                        throw new ExprValidationException(
                            "Failed validation for operator '" + operatorSpec.OperatorName +
                            "': No output ports declared");
                    }

                    LogicalChannelProducingPortDeclared foundDeclared = null;
                    foreach (var declared in declareds) {
                        if (inputItem.InputStreamNames.Contains(declared.StreamName)) {
                            foundDeclared = declared;
                            break;
                        }
                    }

                    if (foundDeclared == null) {
                        throw new ExprValidationException(
                            "Failed validation for operator '" + operatorSpec.OperatorName +
                            "': Failed to find output port for input port " + inputPortNum);
                    }

                    port = new DataFlowOpInputPort(
                        foundDeclared.TypeDesc, new HashSet<string>(inputItem.InputStreamNames),
                        inputItem.OptionalAsName, false);
                }
                else {
                    port = new DataFlowOpInputPort(
                        new GraphTypeDesc(false, false, producingPorts[0].GraphTypeDesc.EventType),
                        new HashSet<string>(inputItem.InputStreamNames), inputItem.OptionalAsName,
                        producingPorts[0].HasPunctuation);
                }

                inputPorts.Put(inputPortNum, port);
            }

            // determine output ports to build up the output port metadata
            var outputPorts = GetDeclaredOutputPorts(operatorSpec, types, services);

            DataFlowOpForgeInitializeResult initializeResult;
            try {
                var context = new DataFlowOpForgeInitializeContext(
                    container,
                    desc.GraphName,
                    operatorNumber,
                    operatorAnnotations,
                    operatorSpec,
                    inputPorts,
                    outputPorts,
                    codegenEnv, 
                    @base,
                    services);
                initializeResult = forge.InitializeForge(context);
            }
            catch (EPException) {
                throw;
            }
            catch (Exception t) {
                throw new ExprValidationException(
                    "Failed to obtain operator '" + operatorSpec.OperatorName + "': " + t.Message, t);
            }

            return initializeResult;
        }

        private static IList<LogicalChannelProducingPortCompiled> DetermineOutgoingPorts(
            int myOpNum,
            GraphOperatorSpec operatorSpec,
            OperatorMetadataDescriptor metadata,
            IDictionary<int, IList<LogicalChannelProducingPortCompiled>> compiledOutputPorts,
            IDictionary<int, IList<LogicalChannelProducingPortDeclared>> declaredOutputPorts,
            GraphTypeDesc[] typesPerOutput,
            ICollection<int> incomingDependentOpNums)
        {
            // Either
            //  (A) the port is explicitly declared via @OutputTypes
            //  (B) the port is declared via "=> ABC<type>"
            //  (C) the port is implicit since there is only one input port and the operator is a functor

            var numPorts = operatorSpec.Output.Items.Count;
            IList<LogicalChannelProducingPortCompiled> result = new List<LogicalChannelProducingPortCompiled>();

            // we go port-by-port: what was declared, what types were determined
            IDictionary<string, GraphTypeDesc> types = new Dictionary<string, GraphTypeDesc>();
            for (var port = 0; port < numPorts; port++) {
                var portStreamName = operatorSpec.Output.Items[port].StreamName;

                // find declaration, if any
                LogicalChannelProducingPortDeclared foundDeclared = null;
                var declaredList = declaredOutputPorts.Get(myOpNum);
                foreach (var declared in declaredList) {
                    if (declared.StreamNumber == port) {
                        if (foundDeclared != null) {
                            throw new ExprValidationException("Found a declaration twice for port " + port);
                        }

                        foundDeclared = declared;
                    }
                }

                if (foundDeclared == null && (typesPerOutput == null || typesPerOutput.Length <= port ||
                                              typesPerOutput[port] == null)) {
                    throw new ExprValidationException(
                        "Operator neither declares an output type nor provided by the operator itself in a 'prepare' method");
                }

                if (foundDeclared != null && typesPerOutput != null && typesPerOutput.Length > port &&
                    typesPerOutput[port] != null) {
                    throw new ExprValidationException(
                        "Operator both declares an output type and provided a type in the 'prepare' method");
                }

                // punctuation determined by input
                var hasPunctuationSignal = (foundDeclared != null ? foundDeclared.HasPunctuation : false) ||
                                           DetermineReceivesPunctuation(
                                               incomingDependentOpNums,
                                               operatorSpec.Input,
                                               compiledOutputPorts);

                GraphTypeDesc compiledType;
                if (foundDeclared != null) {
                    compiledType = foundDeclared.TypeDesc;
                }
                else {
                    compiledType = typesPerOutput[port];
                }

                var compiled = new LogicalChannelProducingPortCompiled(
                    myOpNum, metadata.OperatorPrettyPrint, portStreamName, port, compiledType, hasPunctuationSignal);
                result.Add(compiled);

                // check type compatibility
                var existingType = types.Get(portStreamName);
                types.Put(portStreamName, compiledType);
                if (existingType != null) {
                    CompareTypeInfo(
                        operatorSpec.OperatorName, portStreamName, existingType, portStreamName, compiledType);
                }
            }

            return result;
        }

        private static bool DetermineReceivesPunctuation(
            ICollection<int> incomingDependentOpNums,
            GraphOperatorInput input,
            IDictionary<int, IList<LogicalChannelProducingPortCompiled>> compiledOutputPorts)
        {
            foreach (var inputItem in input.StreamNamesAndAliases) {
                IList<LogicalChannelProducingPortCompiled> list = LogicalChannelUtil.GetOutputPortByStreamName(
                    incomingDependentOpNums, inputItem.InputStreamNames, compiledOutputPorts);
                foreach (var port in list) {
                    if (port.HasPunctuation) {
                        return true;
                    }
                }
            }

            return false;
        }

        private static void CompareTypeInfo(
            string operatorName,
            string firstName,
            GraphTypeDesc firstType,
            string otherName,
            GraphTypeDesc otherType)
        {
            if (firstType.EventType != null && otherType.EventType != null &&
                !firstType.EventType.Equals(otherType.EventType)) {
                throw new ExprValidationException(
                    "For operator '" + operatorName + "' stream '" + firstName + "'" +
                    " typed '" + firstType.EventType.Name + "'" +
                    " is not the same type as stream '" + otherName + "'" +
                    " typed '" + otherType.EventType.Name + "'");
            }

            if (firstType.IsWildcard != otherType.IsWildcard) {
                throw new ExprValidationException(
                    "For operator '" + operatorName + "' streams '" + firstName + "'" +
                    " and '" + otherName + "' have differing wildcard type information");
            }

            if (firstType.IsUnderlying != otherType.IsUnderlying) {
                throw new ExprValidationException(
                    "For operator '" + operatorName + "' streams '" + firstName + "'" +
                    " and '" + otherName + "' have differing underlying information");
            }
        }

        private static IDictionary<int, DataFlowOpOutputPort> GetDeclaredOutputPorts(
            GraphOperatorSpec operatorSpec,
            IDictionary<string, EventType> types,
            StatementCompileTimeServices services)
        {
            IDictionary<int, DataFlowOpOutputPort> outputPorts = new LinkedHashMap<int, DataFlowOpOutputPort>();
            for (var outputPortNum = 0; outputPortNum < operatorSpec.Output.Items.Count; outputPortNum++) {
                var outputItem = operatorSpec.Output.Items[outputPortNum];

                GraphTypeDesc typeDesc = null;
                if (!outputItem.TypeInfo.IsEmpty()) {
                    typeDesc = DetermineTypeOutputPort(outputItem.TypeInfo[0], types, services);
                }

                outputPorts.Put(outputPortNum, new DataFlowOpOutputPort(outputItem.StreamName, typeDesc));
            }

            return outputPorts;
        }

        private class InitForgesResult
        {
            public InitForgesResult(
                IList<LogicalChannel> logicalChannels,
                IList<StmtForgeMethodResult> additionalForgables)
            {
                LogicalChannels = logicalChannels;
                AdditionalForgables = additionalForgables;
            }

            public IList<LogicalChannel> LogicalChannels { get; }
            public IList<StmtForgeMethodResult> AdditionalForgables { get; }
        }
    }
} // end of namespace