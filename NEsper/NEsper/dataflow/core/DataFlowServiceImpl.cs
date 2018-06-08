///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.client.dataflow;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.dataflow.annotations;
using com.espertech.esper.dataflow.interfaces;
using com.espertech.esper.dataflow.runnables;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.epl.annotation;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.events;
using com.espertech.esper.events.arr;
using com.espertech.esper.util;

namespace com.espertech.esper.dataflow.core
{
    public class DataFlowServiceImpl : DataFlowService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const String EVENT_WRAPPED_TYPE = "eventbean";

        private readonly IDictionary<String, DataFlowServiceEntry> _graphs = new Dictionary<String, DataFlowServiceEntry>();
        private readonly IDictionary<String, EPDataFlowInstance> _instances = new Dictionary<String, EPDataFlowInstance>();

        private readonly EPServiceProvider _epService;
        private readonly DataFlowConfigurationStateService _configurationState;

        private readonly ILockable _iLock;

        public DataFlowServiceImpl(
            EPServiceProvider epService, 
            DataFlowConfigurationStateService configurationState,
            ILockManager lockManager)
        {
            _epService = epService;
            _configurationState = configurationState;
            _iLock = lockManager.CreateLock(MethodBase.GetCurrentMethod().DeclaringType);
        }

        public EPDataFlowDescriptor GetDataFlow(String dataFlowName)
        {
            using (_iLock.Acquire())
            {
                var entry = _graphs.Get(dataFlowName);
                if (entry == null)
                {
                    return null;
                }

                return new EPDataFlowDescriptor(dataFlowName, entry.State, entry.DataFlowDesc.StatementContext.StatementName);
            }
        }

        public String[] GetDataFlows()
        {
            using (_iLock.Acquire())
            {
                return _graphs.Keys.ToArray();
            }
        }

        public void AddStartGraph(CreateDataFlowDesc desc, StatementContext statementContext, EPServicesContext servicesContext, AgentInstanceContext agentInstanceContext, bool newStatement)
        {
            using (_iLock.Acquire())
            {
                CompileTimeValidate(desc);

                var existing = _graphs.Get(desc.GraphName);
                if (existing != null && (existing.State == EPStatementState.STARTED || newStatement))
                {
                    throw new ExprValidationException(
                        "Data flow by name '" + desc.GraphName + "' has already been declared");
                }
                if (existing != null)
                {
                    existing.State = EPStatementState.STARTED;
                    return;
                }

                // compile annotations
                var operatorAnnotations = new Dictionary<GraphOperatorSpec, Attribute[]>();
                foreach (GraphOperatorSpec spec in desc.Operators)
                {
                    Attribute[] operatorAnnotation = AnnotationUtil.CompileAnnotations(
                        spec.Annotations, servicesContext.EngineImportService, null);
                    operatorAnnotations.Put(spec, operatorAnnotation);
                }

                var stmtDesc = new DataFlowStmtDesc(
                    desc, statementContext, servicesContext, agentInstanceContext, operatorAnnotations);
                _graphs.Put(desc.GraphName, new DataFlowServiceEntry(stmtDesc, EPStatementState.STARTED));
            }
        }

        public void StopGraph(String graphName)
        {
            using (_iLock.Acquire())
            {
                var existing = _graphs.Get(graphName);
                if (existing != null && existing.State == EPStatementState.STARTED)
                {
                    existing.State = EPStatementState.STOPPED;
                }
            }
        }

        public void RemoveGraph(String graphName)
        {
            using (_iLock.Acquire())
            {
                _graphs.Remove(graphName);
            }
        }

        public EPDataFlowInstance Instantiate(String dataFlowName)
        {
            return Instantiate(dataFlowName, null);
        }

        public EPDataFlowInstance Instantiate(String dataFlowName, EPDataFlowInstantiationOptions options)
        {
            using (_iLock.Acquire())
            {
                var serviceDesc = _graphs.Get(dataFlowName);
                if (serviceDesc == null)
                {
                    throw new EPDataFlowInstantiationException(
                        "Data flow by name '" + dataFlowName + "' has not been defined");
                }
                if (serviceDesc.State != EPStatementState.STARTED)
                {
                    throw new EPDataFlowInstantiationException(
                        "Data flow by name '" + dataFlowName + "' is currently in STOPPED statement state");
                }
                DataFlowStmtDesc stmtDesc = serviceDesc.DataFlowDesc;
                try {
                    return InstantiateInternal(
                        dataFlowName, options, stmtDesc.GraphDesc, stmtDesc.StatementContext, stmtDesc.ServicesContext,
                        stmtDesc.AgentInstanceContext, stmtDesc.OperatorAnnotations);
                }
                catch (Exception ex)
                {
                    String message = "Failed to instantiate data flow '" + dataFlowName + "': " + ex.Message;
                    Log.Debug(message, ex);
                    throw new EPDataFlowInstantiationException(message, ex);
                }
            }
        }

        public void Dispose()
        {
            using (_iLock.Acquire())
            {
                _graphs.Clear();
            }
        }

        public void SaveConfiguration(String dataflowConfigName, String dataFlowName, EPDataFlowInstantiationOptions options)
        {
            using (_iLock.Acquire())
            {
                var dataFlow = _graphs.Get(dataFlowName);
                if (dataFlow == null)
                {
                    String message = "Failed to locate data flow '" + dataFlowName + "'";
                    throw new EPDataFlowNotFoundException(message);
                }
                if (_configurationState.Exists(dataflowConfigName))
                {
                    String message = "Data flow saved configuration by name '" + dataflowConfigName + "' already exists";
                    throw new EPDataFlowAlreadyExistsException(message);
                }
                _configurationState.Add(new EPDataFlowSavedConfiguration(dataflowConfigName, dataFlowName, options));
            }
        }

        public string[] SavedConfigurations
        {
            get
            {
                using (_iLock.Acquire())
                {
                    return _configurationState.SavedConfigNames;
                }
            }
        }

        public EPDataFlowSavedConfiguration GetSavedConfiguration(String configurationName)
        {
            using (_iLock.Acquire())
            {
                return _configurationState.GetSavedConfig(configurationName);
            }
        }

        public EPDataFlowInstance InstantiateSavedConfiguration(String configurationName)
        {
            using (_iLock.Acquire())
            {
                var savedConfiguration = _configurationState.GetSavedConfig(configurationName);
                if (savedConfiguration == null)
                {
                    throw new EPDataFlowInstantiationException(
                        "Dataflow saved configuration '" + configurationName + "' could not be found");
                }
                var options = savedConfiguration.Options;
                if (options == null)
                {
                    options = new EPDataFlowInstantiationOptions();
                    options.DataFlowInstanceId(configurationName);
                }
                return Instantiate(savedConfiguration.DataflowName, options);
            }
        }

        public bool RemoveSavedConfiguration(String configurationName)
        {
            using (_iLock.Acquire())
            {
                return _configurationState.RemovePrototype(configurationName) != null;
            }
        }

        public void SaveInstance(String instanceName, EPDataFlowInstance instance)
        {
            using (_iLock.Acquire())
            {
                if (_instances.ContainsKey(instanceName))
                {
                    throw new EPDataFlowAlreadyExistsException(
                        "Data flow instance name '" + instanceName + "' already saved");
                }
                _instances.Put(instanceName, instance);
            }
        }

        public string[] SavedInstances
        {
            get
            {
                using (_iLock.Acquire())
                {
                    ICollection<String> instanceids = _instances.Keys;
                    return instanceids.ToArray();
                }
            }
        }

        public EPDataFlowInstance GetSavedInstance(String instanceName)
        {
            using (_iLock.Acquire())
            {
                return _instances.Get(instanceName);
            }
        }

        public bool RemoveSavedInstance(String instanceName)
        {
            using (_iLock.Acquire())
            {
                return _instances.Remove(instanceName); // != null;
            }
        }

        private EPDataFlowInstance InstantiateInternal(
            String dataFlowName,
            EPDataFlowInstantiationOptions options,
            CreateDataFlowDesc desc,
            StatementContext statementContext,
            EPServicesContext servicesContext,
            AgentInstanceContext agentInstanceContext,
            IDictionary<GraphOperatorSpec, Attribute[]> operatorAnnotations)
        {
            if (options == null)
            {
                options = new EPDataFlowInstantiationOptions();
            }

            //
            // Building a model.
            //

            // resolve types
            IDictionary<String, EventType> declaredTypes = ResolveTypes(desc, statementContext, servicesContext);

            // resolve operator classes
            IDictionary<int, OperatorMetadataDescriptor> operatorMetadata = ResolveMetadata(desc, options, servicesContext.EngineImportService, operatorAnnotations);

            // build dependency graph:  operator -> [input_providing_op, input_providing_op]
            IDictionary<int, OperatorDependencyEntry> operatorDependencies = AnalyzeDependencies(desc);

            // determine build order of operators
            ICollection<int> operatorBuildOrder = AnalyzeBuildOrder(operatorDependencies);

            // assure variables
            servicesContext.VariableService.SetLocalVersion();

            // instantiate operators
            IDictionary<int, object> operators = InstantiateOperators(operatorMetadata, desc, options, statementContext);

            // Build graph that references port numbers (port number is simply the method offset number or to-be-generated slot in the list)
            var runtimeEventSender = (EPRuntimeEventSender)_epService.EPRuntime;
            var operatorChannels = DetermineChannels(dataFlowName, operatorBuildOrder, operatorDependencies, operators, declaredTypes, operatorMetadata, options, servicesContext.EventAdapterService, servicesContext.EngineImportService, statementContext, servicesContext, agentInstanceContext, runtimeEventSender);
            if (Log.IsDebugEnabled)
            {
                Log.Debug("For flow '" + dataFlowName + "' channels are: " + LogicalChannelUtil.PrintChannels(operatorChannels));
            }

            //
            // Build the realization.
            //

            // Determine binding of each channel to input methods (ports)
            var operatorChannelBindings = new List<LogicalChannelBinding>();
            foreach (LogicalChannel channel in operatorChannels)
            {
                Type targetClass = operators.Get(channel.ConsumingOpNum).GetType();
                LogicalChannelBindingMethodDesc consumingMethod = FindMatchingMethod(channel.ConsumingOpPrettyPrint, targetClass, channel, false);
                LogicalChannelBindingMethodDesc onSignalMethod = null;
                if (channel.OutputPort.HasPunctuation)
                {
                    onSignalMethod = FindMatchingMethod(channel.ConsumingOpPrettyPrint, targetClass, channel, true);
                }
                operatorChannelBindings.Add(new LogicalChannelBinding(channel, consumingMethod, onSignalMethod));
            }

            // Obtain realization
            var dataFlowSignalManager = new DataFlowSignalManager();
            var startDesc = RealizationFactoryInterface.Realize(
                dataFlowName, operators, operatorMetadata, operatorBuildOrder, operatorChannelBindings,
                dataFlowSignalManager, options, servicesContext, statementContext);

            // For each GraphSource add runnable
            var sourceRunnables = new List<GraphSourceRunnable>();
            var audit = AuditEnum.DATAFLOW_SOURCE.GetAudit(statementContext.Annotations) != null;
            foreach (var operatorEntry in operators)
            {
                if (!(operatorEntry.Value is DataFlowSourceOperator))
                {
                    continue;
                }
                var meta = operatorMetadata.Get(operatorEntry.Key);
                var graphSource = (DataFlowSourceOperator)operatorEntry.Value;
                var runnable = new GraphSourceRunnable(statementContext.EngineURI, statementContext.StatementName, graphSource, dataFlowName, meta.OperatorName, operatorEntry.Key, meta.OperatorPrettyPrint, options.GetExceptionHandler(), audit);
                sourceRunnables.Add(runnable);

                dataFlowSignalManager.AddSignalListener(operatorEntry.Key, runnable);
            }

            bool auditStates = AuditEnum.DATAFLOW_TRANSITION.GetAudit(statementContext.Annotations) != null;
            return new EPDataFlowInstanceImpl(
                servicesContext.EngineURI, statementContext.StatementName, auditStates, dataFlowName,
                options.GetDataFlowInstanceUserObject(), options.GetDataFlowInstanceId(), EPDataFlowState.INSTANTIATED,
                sourceRunnables, operators, operatorBuildOrder, startDesc.StatisticsProvider, options.ParametersURIs,
                statementContext.EngineImportService);
        }

        private static IDictionary<String, EventType> ResolveTypes(
            CreateDataFlowDesc desc,
            StatementContext statementContext,
            EPServicesContext servicesContext)
        {
            var types = new Dictionary<String, EventType>();
            foreach (CreateSchemaDesc spec in desc.Schemas)
            {
                EventType eventType = EventTypeUtility.CreateNonVariantType(
                    true, spec, statementContext.Annotations, statementContext.ConfigSnapshot,
                    statementContext.EventAdapterService, servicesContext.EngineImportService);
                types.Put(spec.SchemaName, eventType);
            }
            return types;
        }

        private IDictionary<int, object> InstantiateOperators(
            IDictionary<int, OperatorMetadataDescriptor> operatorClasses,
            CreateDataFlowDesc desc,
            EPDataFlowInstantiationOptions options,
            StatementContext statementContext)
        {
            var operators = new Dictionary<int, object>();
            var exprValidationContext = ExprNodeUtility.GetExprValidationContextStatementOnly(statementContext);

            foreach (var operatorEntry in operatorClasses) {
                var @operator = InstantiateOperator(desc.GraphName, operatorEntry.Key, operatorEntry.Value, desc.Operators[operatorEntry.Key], options, exprValidationContext);
                operators.Put(operatorEntry.Key, @operator);
            }

            return operators;
        }

        private Object InstantiateOperator(
            String dataFlowName,
            int operatorNum,
            OperatorMetadataDescriptor desc,
            GraphOperatorSpec graphOperator,
            EPDataFlowInstantiationOptions options,
            ExprValidationContext exprValidationContext)
        {
            var operatorObject = desc.OptionalOperatorObject;
            if (operatorObject == null)
            {
                var clazz = desc.OperatorFactoryClass ?? desc.OperatorClass;

                // use non-factory class if provided
                try
                {
                    operatorObject = _epService.Container.CreateInstance<object>(clazz);
                }
                catch (Exception e)
                {
                    throw new ExprValidationException("Failed to instantiate: " + e.Message);
                }
            }

            // inject properties
            var configs = graphOperator.Detail == null ? Collections.EmptyDataMap : graphOperator.Detail.Configs;
            InjectObjectProperties(dataFlowName, graphOperator.OperatorName, operatorNum, configs, operatorObject, options.GetParameterProvider(), options.ParametersURIs, exprValidationContext);

            if (operatorObject is DataFlowOperatorFactory)
            {
                try
                {
                    operatorObject = ((DataFlowOperatorFactory)operatorObject).Create();
                }
                catch (Exception ex)
                {
                    throw new ExprValidationException("Failed to obtain operator '" + desc.OperatorName + "', encountered an exception raised by factory class " + operatorObject.GetType().Name + ": " + ex.Message, ex);
                }
            }

            return operatorObject;
        }

        private static void InjectObjectProperties(
            String dataFlowName,
            String operatorName,
            int operatorNum,
            IDictionary<string, object> configs,
            object instance,
            EPDataFlowOperatorParameterProvider optionalParameterProvider,
            IDictionary<string, object> optionalParameterURIs,
            ExprValidationContext exprValidationContext)
        {
            // determine if there is a property holder which holds all properties
            var propertyHolderFields = TypeHelper.FindAnnotatedFields(instance.GetType(), typeof(DataFlowOpPropertyHolderAttribute));
            if (propertyHolderFields.Count > 1)
            {
                throw new ArgumentException("May apply " + typeof(DataFlowOpPropertyHolderAttribute).Name + " annotation only to a single field");
            }

            // determine which class to write properties to
            Object propertyInstance;
            if (propertyHolderFields.IsEmpty())
            {
                propertyInstance = instance;
            }
            else
            {
                var propertyHolderClass = propertyHolderFields.First().FieldType;
                try
                {
                    propertyInstance = Activator.CreateInstance(propertyHolderClass);
                }
                catch (Exception e)
                {
                    throw new ExprValidationException("Failed to instantiate '" + propertyHolderClass + "': " + e.Message, e);
                }
            }

            // populate either the instance itself or the property-holder
            PopulateUtil.PopulateObject(
                operatorName, operatorNum, dataFlowName, configs, propertyInstance, ExprNodeOrigin.DATAFLOW,
                exprValidationContext, optionalParameterProvider, optionalParameterURIs);

            // set holder
            if (propertyHolderFields.IsNotEmpty())
            {
                var field = propertyHolderFields.FirstOrDefault();
                try
                {
                    field.SetValue(instance, propertyInstance);
                }
                catch (Exception e)
                {
                    throw new ExprValidationException("Failed to set field '" + field.Name + "': " + e.Message, e);
                }
            }
        }

        private IList<LogicalChannel> DetermineChannels(String dataflowName,
                                                        ICollection<int> operatorBuildOrder,
                                                        IDictionary<int, OperatorDependencyEntry> operatorDependencies,
                                                        IDictionary<int, object> operators,
                                                        IDictionary<String, EventType> types,
                                                        IDictionary<int, OperatorMetadataDescriptor> operatorMetadata,
                                                        EPDataFlowInstantiationOptions options,
                                                        EventAdapterService eventAdapterService,
                                                        EngineImportService engineImportService,
                                                        StatementContext statementContext,
                                                        EPServicesContext servicesContext,
                                                        AgentInstanceContext agentInstanceContext,
                                                        EPRuntimeEventSender runtimeEventSender)
        {
            // This is a multi-step process.
            //
            // Step 1: find all the operators that have explicit output ports and determine the type of such
            var declaredOutputPorts = new Dictionary<int, IList<LogicalChannelProducingPortDeclared>>();
            foreach (int operatorNum in operatorBuildOrder)
            {
                var metadata = operatorMetadata.Get(operatorNum);
                var @operator = operators.Get(operatorNum);

                var annotationPorts = DetermineAnnotatedOutputPorts(operatorNum, @operator, metadata, engineImportService, eventAdapterService);
                var graphDeclaredPorts = DetermineGraphDeclaredOutputPorts(@operator, operatorNum, metadata, types, servicesContext);

                var allDeclaredPorts = new List<LogicalChannelProducingPortDeclared>();
                allDeclaredPorts.AddAll(annotationPorts);
                allDeclaredPorts.AddAll(graphDeclaredPorts);

                declaredOutputPorts.Put(operatorNum, allDeclaredPorts);
            }

            // Step 2: determine for each operator the output ports: some are determined via "prepare" and some can be implicit
            // since they may not be declared or can be punctuation.
            // Therefore we need to meet ends: on one end the declared types, on the other the implied and dynamically-determined types based on input.
            // We do this in operator build order.
            var compiledOutputPorts = new Dictionary<int, IList<LogicalChannelProducingPortCompiled>>();
            foreach (int myOpNum in operatorBuildOrder)
            {

                GraphOperatorSpec operatorSpec = operatorMetadata.Get(myOpNum).OperatorSpec;
                Object @operator = operators.Get(myOpNum);
                OperatorMetadataDescriptor metadata = operatorMetadata.Get(myOpNum);

                // Handle incoming first: if the operator has incoming ports, each of such should already have type information
                // Compile type information, call method, obtain output types.
                var incomingDependentOpNums = operatorDependencies.Get(myOpNum).Incoming;
                var typesPerOutput = DetermineOutputForInput(dataflowName, myOpNum, @operator, metadata, operatorSpec, declaredOutputPorts, compiledOutputPorts, types, incomingDependentOpNums, options, statementContext, servicesContext, agentInstanceContext, runtimeEventSender);

                // Handle outgoing second:
                //   If there is outgoing declared, use that.
                //   If output types have been determined based on input, use that.
                //   else error
                var outgoingPorts = DetermineOutgoingPorts(myOpNum, @operator, operatorSpec, metadata, compiledOutputPorts, declaredOutputPorts, typesPerOutput, incomingDependentOpNums);
                compiledOutputPorts.Put(myOpNum, outgoingPorts);
            }

            // Step 3: normalization and connecting input ports with output ports (logically, no methods yet)
            var channels = new List<LogicalChannel>();
            var channelId = 0;
            foreach (int myOpNum in operatorBuildOrder)
            {
                var dependencies = operatorDependencies.Get(myOpNum);
                var inputNames = operatorMetadata.Get(myOpNum).OperatorSpec.Input.StreamNamesAndAliases;
                var descriptor = operatorMetadata.Get(myOpNum);

                // handle each (a,b,c AS d)
                int streamNum = -1;
                foreach (GraphOperatorInputNamesAlias inputName in inputNames)
                {
                    streamNum++;

                    // get producers
                    var producingPorts = LogicalChannelUtil.GetOutputPortByStreamName(dependencies.Incoming, inputName.InputStreamNames, compiledOutputPorts);
                    if (producingPorts.Count < inputName.InputStreamNames.Length)
                    {
                        throw new IllegalStateException("Failed to find producing ports");
                    }

                    // determine type compatibility
                    if (producingPorts.Count > 1)
                    {
                        LogicalChannelProducingPortCompiled first = producingPorts[0];
                        for (int i = 1; i < producingPorts.Count; i++)
                        {
                            LogicalChannelProducingPortCompiled other = producingPorts[i];
                            CompareTypeInfo(descriptor.OperatorName, first.StreamName, first.GraphTypeDesc, other.StreamName, other.GraphTypeDesc);
                        }
                    }

                    String optionalAlias = inputName.OptionalAsName;

                    // handle each stream name
                    foreach (String streamName in inputName.InputStreamNames)
                    {
                        foreach (LogicalChannelProducingPortCompiled port in producingPorts)
                        {
                            if (port.StreamName == streamName)
                            {
                                var channel = new LogicalChannel(channelId++, descriptor.OperatorName, myOpNum, streamNum, streamName, optionalAlias, descriptor.OperatorPrettyPrint, port);
                                channels.Add(channel);
                            }
                        }
                    }
                }
            }

            return channels;
        }

        private static void CompareTypeInfo(String operatorName, String firstName, GraphTypeDesc firstType, String otherName, GraphTypeDesc otherType)
        {
            if (firstType.EventType != null && otherType.EventType != null && !firstType.EventType.Equals(otherType.EventType))
            {
                throw new ExprValidationException("For operator '" + operatorName + "' stream '" + firstName + "'" +
                        " typed '" + firstType.EventType.Name + "'" +
                        " is not the same type as stream '" + otherName + "'" +
                        " typed '" + otherType.EventType.Name + "'");
            }
            if (firstType.IsWildcard != otherType.IsWildcard)
            {
                throw new ExprValidationException("For operator '" + operatorName + "' streams '" + firstName + "'" +
                        " and '" + otherName + "' have differing wildcard type information");
            }
            if (firstType.IsUnderlying != otherType.IsUnderlying)
            {
                throw new ExprValidationException("For operator '" + operatorName + "' streams '" + firstName + "'" +
                        " and '" + otherName + "' have differing underlying information");
            }
        }

        private IList<LogicalChannelProducingPortCompiled> DetermineOutgoingPorts(int myOpNum,
                                                                                  Object @operator,
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
            var result = new List<LogicalChannelProducingPortCompiled>();

            // we go port-by-port: what was declared, what types were determined
            var types = new Dictionary<String, GraphTypeDesc>();
            for (int port = 0; port < numPorts; port++)
            {
                String portStreamName = operatorSpec.Output.Items[port].StreamName;

                // find declaration, if any
                LogicalChannelProducingPortDeclared foundDeclared = null;
                IList<LogicalChannelProducingPortDeclared> declaredList = declaredOutputPorts.Get(myOpNum);
                foreach (LogicalChannelProducingPortDeclared declared in declaredList)
                {
                    if (declared.StreamNumber == port)
                    {
                        if (foundDeclared != null)
                        {
                            throw new ExprValidationException("Found a declaration twice for port " + port);
                        }
                        foundDeclared = declared;
                    }
                }

                if (foundDeclared == null && (typesPerOutput == null || typesPerOutput.Length <= port || typesPerOutput[port] == null))
                {
                    throw new ExprValidationException("Operator neither declares an output type nor provided by the operator itself in a 'prepare' method");
                }
                if (foundDeclared != null && typesPerOutput != null && typesPerOutput.Length > port && typesPerOutput[port] != null)
                {
                    throw new ExprValidationException("Operator both declares an output type and provided a type in the 'prepare' method");
                }

                // punctuation determined by input
                bool hasPunctuationSignal = (foundDeclared != null ? foundDeclared.HasPunctuation : false) || DetermineReceivesPunctuation(incomingDependentOpNums, operatorSpec.Input, compiledOutputPorts);

                GraphTypeDesc compiledType;
                if (foundDeclared != null)
                {
                    compiledType = foundDeclared.TypeDesc;
                }
                else
                {
                    compiledType = typesPerOutput[port];
                }

                var compiled = new LogicalChannelProducingPortCompiled(myOpNum, metadata.OperatorPrettyPrint, portStreamName, port, compiledType, hasPunctuationSignal);
                result.Add(compiled);

                // check type compatibility
                GraphTypeDesc existingType = types.Get(portStreamName);
                types.Put(portStreamName, compiledType);
                if (existingType != null)
                {
                    CompareTypeInfo(operatorSpec.OperatorName, portStreamName, existingType, portStreamName, compiledType);
                }
            }

            return result;
        }

        private static bool DetermineReceivesPunctuation(ICollection<int> incomingDependentOpNums, GraphOperatorInput input, IDictionary<int, IList<LogicalChannelProducingPortCompiled>> compiledOutputPorts)
        {
            foreach (GraphOperatorInputNamesAlias inputItem in input.StreamNamesAndAliases)
            {
                var list = LogicalChannelUtil.GetOutputPortByStreamName(incomingDependentOpNums, inputItem.InputStreamNames, compiledOutputPorts);
                if (list.Any(port => port.HasPunctuation))
                {
                    return true;
                }
            }

            return false;
        }

        private GraphTypeDesc[] DetermineOutputForInput(String dataFlowName,
                                                        int myOpNum,
                                                        Object @operator,
                                                        OperatorMetadataDescriptor meta,
                                                        GraphOperatorSpec operatorSpec,
                                                        IDictionary<int, IList<LogicalChannelProducingPortDeclared>> declaredOutputPorts,
                                                        IDictionary<int, IList<LogicalChannelProducingPortCompiled>> compiledOutputPorts,
                                                        IDictionary<String, EventType> types,
                                                        ICollection<int> incomingDependentOpNums,
                                                        EPDataFlowInstantiationOptions options,
                                                        StatementContext statementContext,
                                                        EPServicesContext servicesContext,
                                                        AgentInstanceContext agentInstanceContext,
                                                        EPRuntimeEventSender runtimeEventSender)
        {
            if (!(@operator is DataFlowOpLifecycle))
            {
                return null;
            }

            // determine input ports to build up the input port metadata
            var numDeclared = operatorSpec.Input.StreamNamesAndAliases.Count;
            var inputPorts = new LinkedHashMap<int, DataFlowOpInputPort>();
            for (int inputPortNum = 0; inputPortNum < numDeclared; inputPortNum++)
            {
                var inputItem = operatorSpec.Input.StreamNamesAndAliases[inputPortNum];
                var producingPorts = LogicalChannelUtil.GetOutputPortByStreamName(incomingDependentOpNums, inputItem.InputStreamNames, compiledOutputPorts);

                DataFlowOpInputPort port;
                if (producingPorts.IsEmpty())
                { // this can be when the operator itself is the incoming port, i.e. feedback loop
                    var declareds = declaredOutputPorts.Get(myOpNum);
                    if (declareds == null || declareds.IsEmpty())
                    {
                        throw new ExprValidationException("Failed validation for operator '" + operatorSpec.OperatorName + "': No output ports declared");
                    }
                    LogicalChannelProducingPortDeclared foundDeclared = null;
                    foreach (LogicalChannelProducingPortDeclared declared in declareds)
                    {
                        if (inputItem.InputStreamNames.Contains(declared.StreamName))
                        {
                            foundDeclared = declared;
                            break;
                        }
                    }
                    if (foundDeclared == null)
                    {
                        throw new ExprValidationException("Failed validation for operator '" + operatorSpec.OperatorName + "': Failed to find output port declared");
                    }
                    port = new DataFlowOpInputPort(foundDeclared.TypeDesc, new HashSet<String>(inputItem.InputStreamNames), inputItem.OptionalAsName, false);
                }
                else
                {
                    port = new DataFlowOpInputPort(
                        new GraphTypeDesc(false, false, producingPorts[0].GraphTypeDesc.EventType),
                        new HashSet<String>(inputItem.InputStreamNames),
                        inputItem.OptionalAsName,
                        producingPorts[0].HasPunctuation);
                }
                inputPorts.Put(inputPortNum, port);
            }

            // determine output ports to build up the output port metadata
            IDictionary<int, DataFlowOpOutputPort> outputPorts = GetDeclaredOutputPorts(operatorSpec, types, servicesContext);

            // determine event sender
            EPRuntimeEventSender dfRuntimeEventSender = runtimeEventSender;
            if (options.SurrogateEventSender != null)
            {
                dfRuntimeEventSender = options.SurrogateEventSender;
            }

            var preparable = (DataFlowOpLifecycle)@operator;
            var context = new DataFlowOpInitializateContext(dataFlowName, options.GetDataFlowInstanceId(), options.GetDataFlowInstanceUserObject(), inputPorts, outputPorts, statementContext, servicesContext, agentInstanceContext, dfRuntimeEventSender, _epService, meta.OperatorAnnotations);

            DataFlowOpInitializeResult prepareResult;
            try
            {
                prepareResult = preparable.Initialize(context);
            }
            catch (ExprValidationException e)
            {
                throw new ExprValidationException("Failed validation for operator '" + operatorSpec.OperatorName + "': " + e.Message, e);
            }
            catch (Exception e)
            {
                throw new ExprValidationException("Failed initialization for operator '" + operatorSpec.OperatorName + "': " + e.Message, e);
            }

            if (prepareResult == null)
            {
                return null;
            }
            return prepareResult.TypeDescriptors;
        }

        private static IList<LogicalChannelProducingPortDeclared> DetermineAnnotatedOutputPorts(int producingOpNum, object @operator, OperatorMetadataDescriptor descriptor, EngineImportService engineImportService, EventAdapterService eventAdapterService)
        {
            var ports = new List<LogicalChannelProducingPortDeclared>();

            // See if any @OutputTypes annotations exists
            var unwrapAttributes = @operator.GetType().UnwrapAttributes();
            var outputTypeAttributes = TypeHelper.GetAnnotations<OutputTypeAttribute>(
                unwrapAttributes).Cast<OutputTypeAttribute>();
            var outputTypeGroups = outputTypeAttributes.GroupBy(
                attribute => attribute.Port,
                attribute => attribute);

            foreach (var outputTypeGroup in outputTypeGroups)
            {
                // create local event type for the declared type
                var propertiesRaw = new LinkedHashMap<string, object>();
                var outputTypeArr = outputTypeGroup.ToArray();
                var outputTypePort = outputTypeGroup.Key;

                foreach (var outputType in outputTypeArr)
                {
                    Type clazz;
                    if ((outputType.Type != null) && (outputType.Type != typeof(OutputTypeAttribute)))
                    {
                        clazz = outputType.Type;
                    }
                    else
                    {
                        var typeName = outputType.TypeName;
                        clazz = TypeHelper.GetTypeForSimpleName(typeName);
                        if (clazz == null)
                        {
                            try
                            {
                                clazz = engineImportService.ResolveType(typeName, false);
                            }
                            catch (EngineImportException)
                            {
                                throw new EPException("Failed to resolve type '" + typeName + "'");
                            }
                        }
                    }
                    propertiesRaw.Put(outputType.Name, clazz);
                }

                var propertiesCompiled = EventTypeUtility.CompileMapTypeProperties(propertiesRaw, eventAdapterService);
                var eventType = eventAdapterService.CreateAnonymousObjectArrayType("TYPE_" + @operator.GetType(), propertiesCompiled);

                // determine output stream name, which must be provided
                var declaredOutput = descriptor.OperatorSpec.Output.Items;
                if (declaredOutput.IsEmpty())
                {
                    throw new ExprValidationException("No output stream declared");
                }
                if (declaredOutput.Count < outputTypePort)
                {
                    throw new ExprValidationException("No output stream declared for this port");
                }

                var streamName = declaredOutput[outputTypePort].StreamName;
                var isDeclaredPunctuated = TypeHelper.IsAnnotationListed(
                    typeof(DataFlowOpProvideSignalAttribute), unwrapAttributes);
                var port = new LogicalChannelProducingPortDeclared(
                    producingOpNum, descriptor.OperatorPrettyPrint, streamName, outputTypePort,
                    new GraphTypeDesc(false, false, eventType), isDeclaredPunctuated);
                ports.Add(port);
            }

            return ports;
        }

        private static IList<LogicalChannelProducingPortDeclared> DetermineGraphDeclaredOutputPorts(Object @operator, int producingOpNum, OperatorMetadataDescriptor metadata, IDictionary<String, EventType> types, EPServicesContext servicesContext)
        {
            var ports = new List<LogicalChannelProducingPortDeclared>();

            int portNumber = 0;
            foreach (GraphOperatorOutputItem outputItem in metadata.OperatorSpec.Output.Items)
            {
                if (outputItem.TypeInfo.Count > 1)
                {
                    throw new ExprValidationException("Multiple parameter types are not supported");
                }

                if (!outputItem.TypeInfo.IsEmpty())
                {
                    GraphTypeDesc typeDesc = DetermineTypeOutputPort(outputItem.TypeInfo[0], types, servicesContext);
                    bool isDeclaredPunctuated = TypeHelper.IsAnnotationListed(typeof(DataFlowOpProvideSignalAttribute), @operator.GetType().UnwrapAttributes());
                    ports.Add(new LogicalChannelProducingPortDeclared(producingOpNum, metadata.OperatorPrettyPrint, outputItem.StreamName, portNumber, typeDesc, isDeclaredPunctuated));
                }
                portNumber++;
            }

            return ports;
        }

        private static IDictionary<int, OperatorDependencyEntry> AnalyzeDependencies(CreateDataFlowDesc graphDesc)
        {
            var logicalOpDependencies = new Dictionary<int, OperatorDependencyEntry>();
            for (int i = 0; i < graphDesc.Operators.Count; i++)
            {
                OperatorDependencyEntry entry = new OperatorDependencyEntry();
                logicalOpDependencies.Put(i, entry);
            }
            for (int consumingOpNum = 0; consumingOpNum < graphDesc.Operators.Count; consumingOpNum++)
            {
                OperatorDependencyEntry entry = logicalOpDependencies.Get(consumingOpNum);
                GraphOperatorSpec op = graphDesc.Operators[consumingOpNum];

                // for each input item
                foreach (GraphOperatorInputNamesAlias input in op.Input.StreamNamesAndAliases)
                {

                    // for each stream name listed
                    foreach (String inputStreamName in input.InputStreamNames)
                    {
                        // find all operators providing such input stream
                        bool found = false;

                        // for each operator
                        for (int providerOpNum = 0; providerOpNum < graphDesc.Operators.Count; providerOpNum++)
                        {
                            GraphOperatorSpec from = graphDesc.Operators[providerOpNum];
                            foreach (GraphOperatorOutputItem outputItem in from.Output.Items)
                            {
                                if (outputItem.StreamName.Equals(inputStreamName))
                                {
                                    found = true;
                                    entry.AddIncoming(providerOpNum);
                                    logicalOpDependencies.Get(providerOpNum).AddOutgoing(consumingOpNum);
                                }
                            }
                        }

                        if (!found)
                        {
                            throw new ExprValidationException("Input stream '" + inputStreamName + "' consumed by operator '" + op.OperatorName + "' could not be found");
                        }
                    }
                }
            }
            return logicalOpDependencies;
        }

        private IDictionary<int, OperatorMetadataDescriptor> ResolveMetadata(CreateDataFlowDesc graphDesc,
                                                                             EPDataFlowInstantiationOptions options,
                                                                             EngineImportService engineImportService,
                                                                             IDictionary<GraphOperatorSpec, Attribute[]> operatorAnnotations)
        {
            IDictionary<int, OperatorMetadataDescriptor> operatorClasses = new Dictionary<int, OperatorMetadataDescriptor>();
            for (int i = 0; i < graphDesc.Operators.Count; i++)
            {
                var operatorSpec = graphDesc.Operators[i];
                var operatorPrettyPrint = ToPrettyPrint(i, operatorSpec);
                var operatorAnnotation = operatorAnnotations.Get(operatorSpec);

                // see if the operator is already provided by options
                OperatorMetadataDescriptor descriptor;
                if (options.GetOperatorProvider() != null)
                {
                    var @operator = options.GetOperatorProvider().Provide(new EPDataFlowOperatorProviderContext(graphDesc.GraphName, operatorSpec.OperatorName, operatorSpec));
                    if (@operator != null)
                    {
                        descriptor = new OperatorMetadataDescriptor(operatorSpec, i, @operator.GetType(), null, @operator, operatorPrettyPrint, operatorAnnotation);
                        operatorClasses.Put(i, descriptor);
                        continue;
                    }
                }

                // try to find factory class with factory annotation
                Type factoryClass = null;
                try
                {
                    factoryClass = engineImportService.ResolveType(StringExtensions.Capitalize(operatorSpec.OperatorName + "Factory"), false);
                }
                catch (EngineImportException)
                {
                }

                // if the factory : the interface use that
                if (factoryClass != null && factoryClass.IsImplementsInterface(typeof(DataFlowOperatorFactory)))
                {
                    descriptor = new OperatorMetadataDescriptor(operatorSpec, i, null, factoryClass, null, operatorPrettyPrint, operatorAnnotation);
                    operatorClasses.Put(i, descriptor);
                    continue;
                }

                // resolve by class name
                Type clazz;
                try
                {
                    clazz = engineImportService.ResolveType(StringExtensions.Capitalize(operatorSpec.OperatorName), false);
                }
                catch (EngineImportException e)
                {
                    throw new ExprValidationException("Failed to resolve operator '" + operatorSpec.OperatorName + "': " + e.Message, e);
                }

                if (!TypeHelper.IsImplementsInterface(clazz, typeof(DataFlowSourceOperator)) &&
                    !TypeHelper.IsAnnotationListed(typeof(DataFlowOperatorAttribute), clazz.UnwrapAttributes()))
                {
                    throw new ExprValidationException(
                        "Failed to resolve operator '" + operatorSpec.OperatorName + "', operator class " + clazz.FullName +
                        " does not declare the " + typeof(DataFlowOperatorAttribute).Name +
                        " annotation or implement the " + typeof(DataFlowSourceOperator).Name + " interface");
                }

                descriptor = new OperatorMetadataDescriptor(operatorSpec, i, clazz, null, null, operatorPrettyPrint, operatorAnnotation);
                operatorClasses.Put(i, descriptor);
            }
            return operatorClasses;
        }

        private String ToPrettyPrint(int operatorNum, GraphOperatorSpec spec)
        {
            var writer = new StringWriter();
            writer.Write(spec.OperatorName);
            writer.Write("#");
            writer.Write(Convert.ToString(operatorNum));

            writer.Write("(");
            String delimiter = "";
            foreach (GraphOperatorInputNamesAlias inputItem in spec.Input.StreamNamesAndAliases)
            {
                writer.Write(delimiter);
                ToPrettyPrintInput(inputItem, writer);
                if (inputItem.OptionalAsName != null)
                {
                    writer.Write(" as ");
                    writer.Write(inputItem.OptionalAsName);
                }
                delimiter = ", ";
            }
            writer.Write(")");

            if (spec.Output.Items.IsEmpty())
            {
                return writer.ToString();
            }
            writer.Write(" -> ");

            delimiter = "";
            foreach (GraphOperatorOutputItem outputItem in spec.Output.Items)
            {
                writer.Write(delimiter);
                writer.Write(outputItem.StreamName);
                WriteTypes(outputItem.TypeInfo, writer);
                delimiter = ",";
            }

            return writer.ToString();
        }

        private static void ToPrettyPrintInput(GraphOperatorInputNamesAlias inputItem, TextWriter writer)
        {
            if (inputItem.InputStreamNames.Length == 1)
            {
                writer.Write(inputItem.InputStreamNames[0]);
            }
            else
            {
                writer.Write("(");
                String delimiterNames = "";
                foreach (String name in inputItem.InputStreamNames)
                {
                    writer.Write(delimiterNames);
                    writer.Write(name);
                    delimiterNames = ",";
                }
                writer.Write(")");
            }
        }

        private void WriteTypes(ICollection<GraphOperatorOutputItemType> types, TextWriter writer)
        {
            if (types.IsEmpty())
            {
                return;
            }

            writer.Write("<");
            String typeDelimiter = "";
            foreach (GraphOperatorOutputItemType type in types)
            {
                writer.Write(typeDelimiter);
                WriteType(type, writer);
                typeDelimiter = ",";
            }
            writer.Write(">");
        }

        private void WriteType(GraphOperatorOutputItemType type, TextWriter writer)
        {
            if (type.IsWildcard)
            {
                writer.Write('?');
                return;
            }
            writer.Write(type.TypeOrClassname);
            WriteTypes(type.TypeParameters, writer);
        }

        private static ICollection<int> AnalyzeBuildOrder(IDictionary<int, OperatorDependencyEntry> operators)
        {
            var graph = new DependencyGraph(operators.Count, true);
            foreach (var entry in operators)
            {
                var myOpNum = entry.Key;
                var incomings = entry.Value.Incoming;
                foreach (int incoming in incomings)
                {
                    graph.AddDependency(myOpNum, incoming);
                }
            }

            ICollection<int> topDownSet = new SortedSet<int>();
            while (topDownSet.Count < operators.Count)
            {
                // secondary sort according to the order of listing
                ICollection<int> rootNodes = new SortedSet<int>(
                    new ProxyComparer<int>((o1, o2) => -1 * o1.CompareTo(o2)));
                rootNodes.AddAll(graph.GetRootNodes(topDownSet));

                if (rootNodes.IsEmpty()) // circular dependency could cause this
                {
                    for (int i = 0; i < operators.Count; i++)
                    {
                        if (!topDownSet.Contains(i))
                        {
                            rootNodes.Add(i);
                            break;
                        }
                    }
                }

                topDownSet.AddAll(rootNodes);
            }

            return topDownSet;
        }

        private static LogicalChannelBindingMethodDesc FindMatchingMethod(String operatorName, Type target, LogicalChannel channelDesc, bool isPunctuation)
        {
            if (isPunctuation)
            {
                return target.GetMethods()
                    .Where(m => m.Name == "OnSignal")
                    .Select(method => new LogicalChannelBindingMethodDesc(method, LogicalChannelBindingTypePassAlong.INSTANCE))
                    .FirstOrDefault();
            }

            LogicalChannelProducingPortCompiled outputPort = channelDesc.OutputPort;

            Type[] expectedIndividual;
            Type expectedUnderlying;
            EventType expectedUnderlyingType;
            GraphTypeDesc typeDesc = outputPort.GraphTypeDesc;

            if (typeDesc.IsWildcard)
            {
                expectedIndividual = new Type[0];
                expectedUnderlying = null;
                expectedUnderlyingType = null;
            }
            else
            {
                expectedIndividual = new Type[typeDesc.EventType.PropertyNames.Length];
                int i = 0;
                foreach (EventPropertyDescriptor descriptor in typeDesc.EventType.PropertyDescriptors)
                {
                    expectedIndividual[i] = descriptor.PropertyType;
                    i++;
                }
                expectedUnderlying = typeDesc.EventType.UnderlyingType;
                expectedUnderlyingType = typeDesc.EventType;
            }

            String channelSpecificMethodName = null;
            if (channelDesc.ConsumingOptStreamAliasName != null)
            {
                channelSpecificMethodName = "On" + channelDesc.ConsumingOptStreamAliasName;
            }

            foreach (var method in target.GetMethods())
            {
                bool eligible = method.Name.Equals("OnInput");
                if (!eligible && (method.Name == channelSpecificMethodName))
                {
                    eligible = true;
                }

                if (!eligible)
                {
                    continue;
                }

                // handle Object[]
                var paramTypes = method.GetParameterTypes();
                var numParams = paramTypes.Length;

                if (expectedUnderlying != null)
                {
                    if (numParams == 1 && TypeHelper.IsSubclassOrImplementsInterface(paramTypes[0], expectedUnderlying))
                    {
                        return new LogicalChannelBindingMethodDesc(method, LogicalChannelBindingTypePassAlong.INSTANCE);
                    }
                    if (numParams == 2 && paramTypes[0].IsInt32() && TypeHelper.IsSubclassOrImplementsInterface(paramTypes[1], expectedUnderlying))
                    {
                        return new LogicalChannelBindingMethodDesc(method, new LogicalChannelBindingTypePassAlongWStream(channelDesc.ConsumingOpStreamNum));
                    }
                }

                if (numParams == 1 && (paramTypes[0] == typeof(Object) || (paramTypes[0] == typeof(object[]) && method.IsVarArgs())))
                {
                    return new LogicalChannelBindingMethodDesc(method, LogicalChannelBindingTypePassAlong.INSTANCE);
                }
                if (numParams == 2 && paramTypes[0] == typeof(int) && (paramTypes[1] == typeof(Object) || (paramTypes[1] == typeof(object[]) && method.IsVarArgs())))
                {
                    return new LogicalChannelBindingMethodDesc(method, new LogicalChannelBindingTypePassAlongWStream(channelDesc.ConsumingOpStreamNum));
                }

                // if exposing a method that exactly matches each property type in order, use that, i.e. "onInut(String p0, int p1)"
                if (expectedUnderlyingType is ObjectArrayEventType && TypeHelper.IsSignatureCompatible(expectedIndividual, paramTypes))
                {
                    return new LogicalChannelBindingMethodDesc(method, LogicalChannelBindingTypeUnwind.INSTANCE);
                }
            }

            var choices = new LinkedHashSet<String>();
            choices.Add(typeof(Object).Name);
            choices.Add("Object[]");
            if (expectedUnderlying != null)
            {
                choices.Add(expectedUnderlying.Name);
            }
            throw new ExprValidationException("Failed to find OnInput method on for operator '" + operatorName + "' class " +
                    target.FullName + ", expected an OnInput method that takes any of {" + CollectionUtil.ToString(choices) + "}");
        }

        private static IDictionary<int, DataFlowOpOutputPort> GetDeclaredOutputPorts(GraphOperatorSpec operatorSpec, IDictionary<String, EventType> types, EPServicesContext servicesContext)
        {
            IDictionary<int, DataFlowOpOutputPort> outputPorts = new LinkedHashMap<int, DataFlowOpOutputPort>();
            for (int outputPortNum = 0; outputPortNum < operatorSpec.Output.Items.Count; outputPortNum++)
            {
                GraphOperatorOutputItem outputItem = operatorSpec.Output.Items[outputPortNum];
                GraphTypeDesc typeDesc = null;
                if (!outputItem.TypeInfo.IsEmpty())
                {
                    typeDesc = DetermineTypeOutputPort(outputItem.TypeInfo[0], types, servicesContext);
                }
                outputPorts.Put(outputPortNum, new DataFlowOpOutputPort(outputItem.StreamName, typeDesc));
            }

            return outputPorts;
        }

        private static GraphTypeDesc DetermineTypeOutputPort(GraphOperatorOutputItemType outType, IDictionary<String, EventType> types, EPServicesContext servicesContext)
        {
            EventType eventType = null;
            bool isWildcard = false;
            bool isUnderlying = true;

            String typeOrClassname = outType.TypeOrClassname;
            if (typeOrClassname != null && (typeOrClassname.ToLower() == EVENT_WRAPPED_TYPE))
            {
                isUnderlying = false;
                if (!outType.TypeParameters.IsEmpty() && !outType.TypeParameters[0].IsWildcard)
                {
                    String typeName = outType.TypeParameters[0].TypeOrClassname;
                    eventType = ResolveType(typeName, types, servicesContext);
                }
                else
                {
                    isWildcard = true;
                }
            }
            else if (typeOrClassname != null)
            {
                eventType = ResolveType(typeOrClassname, types, servicesContext);
            }
            else
            {
                isWildcard = true;
            }
            return new GraphTypeDesc(isWildcard, isUnderlying, eventType);
        }

        private static EventType ResolveType(String typeOrClassname, IDictionary<String, EventType> types, EPServicesContext servicesContext)
        {
            var eventType = types.Get(typeOrClassname) ?? servicesContext.EventAdapterService.GetEventTypeByName(typeOrClassname);
            if (eventType == null)
            {
                throw new ExprValidationException("Failed to find event type '" + typeOrClassname + "'");
            }
            return eventType;
        }

        private static void CompileTimeValidate(CreateDataFlowDesc desc)
        {
            foreach (var spec in desc.Operators)
            {
                foreach (var @out in spec.Output.Items)
                {
                    if (@out.TypeInfo.Count > 1)
                    {
                        throw new ExprValidationException("Failed to validate operator '" + spec.OperatorName + "': Multiple output types for a single stream '" + @out.StreamName + "' are not supported");
                    }
                }
            }

            var schemaNames = new HashSet<String>();
            foreach (var schema in desc.Schemas)
            {
                if (schemaNames.Contains(schema.SchemaName))
                {
                    throw new ExprValidationException("Schema name '" + schema.SchemaName + "' is declared more then once");
                }
                schemaNames.Add(schema.SchemaName);
            }
        }
    }
}
