///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.client.soda;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.service.multimatch;
using com.espertech.esper.core.start;
using com.espertech.esper.epl.agg.rollup;
using com.espertech.esper.epl.annotation;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.declexpr;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.expression.ops;
using com.espertech.esper.epl.expression.subquery;
using com.espertech.esper.epl.expression.visitor;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.spec.util;
using com.espertech.esper.events;
using com.espertech.esper.events.arr;
using com.espertech.esper.events.bean;
using com.espertech.esper.events.map;
using com.espertech.esper.filter;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.pattern;
using com.espertech.esper.script;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Provides statement lifecycle services.
    /// </summary>
    public class StatementLifecycleSvcImpl : StatementLifecycleSvc
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>Services context for statement lifecycle management. </summary>
        private readonly EPServicesContext _services;

        /// <summary>Maps of statement id to descriptor. </summary>
        private readonly IDictionary<String, EPStatementDesc> _stmtIdToDescMap;

        /// <summary>Map of statement name to statement. </summary>
        private readonly IDictionary<String, EPStatement> _stmtNameToStmtMap;

        private readonly EPServiceProviderSPI _epServiceProvider;
        private readonly IReaderWriterLock _eventProcessingRwLock;

        private readonly IDictionary<String, String> _stmtNameToIdMap;

        public event EventHandler<StatementLifecycleEvent> LifecycleEvent;

        private readonly ILockable _iLock = LockManager.CreateDefaultLock();

        /// <summary>Ctor. </summary>
        /// <param name="epServiceProvider">is the engine instance to hand to statement-aware listeners</param>
        /// <param name="services">is engine services</param>
        public StatementLifecycleSvcImpl(EPServiceProvider epServiceProvider, EPServicesContext services)
        {
            _services = services;
            _epServiceProvider = (EPServiceProviderSPI)epServiceProvider;

            // lock for starting and stopping statements
            _eventProcessingRwLock = services.EventProcessingRwLock;

            _stmtIdToDescMap = new Dictionary<String, EPStatementDesc>();
            _stmtNameToStmtMap = new Dictionary<String, EPStatement>();
            _stmtNameToIdMap = new LinkedHashMap<String, String>();
        }

        public void Dispose()
        {
            DestroyAllStatements();
        }

        public void Init()
        {
            // called after services are activated, to begin statement loading from store
        }

        public IDictionary<string, EPStatement> StmtNameToStmt
        {
            get { return _stmtNameToStmtMap; }
        }

        public EPStatement CreateAndStart(StatementSpecRaw statementSpec, String expression, bool isPattern, String optStatementName, Object userObject, EPIsolationUnitServices isolationUnitServices, String statementId, EPStatementObjectModel optionalModel)
        {
            using (_iLock.Acquire())
            {
                String assignedStatementId = statementId ?? UuidGenerator.Generate();
                EPStatementDesc desc = CreateStoppedAssignName(statementSpec, expression, isPattern, optStatementName, assignedStatementId, null, userObject, isolationUnitServices, optionalModel);
                Start(assignedStatementId, desc, true, false, false);
                return desc.EpStatement;
            }
        }

        /// <summary>
        /// Creates and starts statement.
        /// </summary>
        /// <param name="statementSpec">defines the statement</param>
        /// <param name="expression">is the EPL</param>
        /// <param name="isPattern">is true for patterns</param>
        /// <param name="optStatementName">is the optional statement name</param>
        /// <param name="statementId">is the statement id</param>
        /// <param name="optAdditionalContext">additional context for use by the statement context</param>
        /// <param name="userObject">the application define user object associated to each statement, if supplied</param>
        /// <param name="isolationUnitServices">isolated service services</param>
        /// <param name="optionalModel">The optional model.</param>
        /// <returns>started statement</returns>
        protected EPStatementDesc CreateStoppedAssignName(StatementSpecRaw statementSpec,
                                                          String expression,
                                                          bool isPattern,
                                                          String optStatementName,
                                                          String statementId,
                                                          IDictionary<String, Object> optAdditionalContext,
                                                          Object userObject,
                                                          EPIsolationUnitServices isolationUnitServices,
                                                          EPStatementObjectModel optionalModel)
        {
            using (_iLock.Acquire())
            {
                var nameProvided = false;
                var statementName = statementId;

                // compile annotations, can produce a null array
                var annotations = AnnotationUtil.CompileAnnotations(statementSpec.Annotations, _services.EngineImportService, expression);

                // find name annotation
                if (optStatementName == null)
                {
                    if (annotations != null && annotations.Length != 0)
                    {
                        foreach (var annotation in annotations)
                        {
                            var nameAttribute = annotation as NameAttribute;
                            if (nameAttribute != null && nameAttribute.Value != null)
                            {
                                optStatementName = nameAttribute.Value;
                            }
                        }
                    }
                }

                // Determine a statement name, i.e. use the id or use/generate one for the name passed in
                if (optStatementName != null)
                {
                    optStatementName = optStatementName.Trim();
                    statementName = GetUniqueStatementName(optStatementName, statementId);
                    nameProvided = true;
                }

                return CreateStopped(statementSpec, annotations, expression, isPattern, statementName, nameProvided, statementId, optAdditionalContext, userObject, isolationUnitServices, false, optionalModel);
            }
        }

        /// <summary>
        /// Create stopped statement.
        /// </summary>
        /// <param name="statementSpec">statement definition</param>
        /// <param name="annotations">The annotations.</param>
        /// <param name="expression">is the expression text</param>
        /// <param name="isPattern">is true for patterns, false for non-patterns</param>
        /// <param name="statementName">is the statement name assigned or given</param>
        /// <param name="nameProvided">true when an explicit statement name is provided</param>
        /// <param name="statementId">is the statement id</param>
        /// <param name="optAdditionalContext">additional context for use by the statement context</param>
        /// <param name="statementUserObject">the application define user object associated to each statement, if supplied</param>
        /// <param name="isolationUnitServices">isolated service services</param>
        /// <param name="isFailed">to start the statement in failed state</param>
        /// <param name="optionalModel">The optional model.</param>
        /// <returns>stopped statement</returns>
        protected EPStatementDesc CreateStopped(StatementSpecRaw statementSpec,
                                                Attribute[] annotations,
                                                String expression,
                                                bool isPattern,
                                                String statementName,
                                                bool nameProvided,
                                                String statementId,
                                                IDictionary<String, Object> optAdditionalContext,
                                                Object statementUserObject,
                                                EPIsolationUnitServices isolationUnitServices,
                                                bool isFailed,
                                                EPStatementObjectModel optionalModel)
        {
            using (_iLock.Acquire())
            {
                EPStatementDesc statementDesc;

                // Hint annotations are often driven by variables
                if (annotations != null)
                {
                    foreach (HintAttribute annotation in annotations.OfType<HintAttribute>())
                    {
                        statementSpec.HasVariables = true;
                        break;
                    }
                }

                // walk subselects, alias expressions, declared expressions, dot-expressions
                ExprNodeSubselectDeclaredDotVisitor visitor;
                try
                {
                    visitor = StatementSpecRawAnalyzer.WalkSubselectAndDeclaredDotExpr(statementSpec);
                }
                catch (ExprValidationException ex)
                {
                    throw new EPStatementException(ex.Message, expression);
                }

                // Determine Subselects for compilation, and lambda-expression shortcut syntax for named windows
                IList<ExprSubselectNode> subselectNodes = visitor.Subselects;
                if (!visitor.ChainedExpressionsDot.IsEmpty())
                {
                    RewriteNamedWindowSubselect(visitor.ChainedExpressionsDot, subselectNodes, _services.NamedWindowService);
                }

                // compile foreign scripts
                ValidateScripts(expression, statementSpec.ScriptExpressions, statementSpec.ExpressionDeclDesc);

                // Determine statement type
                var statementType = StatementMetadataFactoryDefault.GetStatementType(statementSpec, isPattern);

                // Determine stateless statement
                var stateless = DetermineStatelessSelect(statementType, statementSpec, !subselectNodes.IsEmpty(), isPattern);

                // Determine table use
                var writesToTables = StatementLifecycleSvcUtil.IsWritesToTables(statementSpec, _services.TableService);

                // Make context
                StatementContext statementContext = _services.StatementContextFactory.MakeContext(
                    statementId, statementName, expression, statementType, _services, optAdditionalContext, false, annotations, isolationUnitServices, stateless,
                    statementSpec, subselectNodes, writesToTables, statementUserObject);

                StatementSpecCompiled compiledSpec;
                try
                {
                    compiledSpec = Compile(statementSpec, expression, statementContext, false, false, annotations, visitor.Subselects, visitor.DeclaredExpressions, _services);
                }
                catch (EPStatementException ex)
                {
                    _stmtNameToIdMap.Remove(statementName); // Clean out the statement name as it's already assigned
                    throw;
                }

                // For insert-into streams, create a lock taken out as soon as an event is inserted
                // Makes the processing between chained statements more predictable.
                if (statementSpec.InsertIntoDesc != null || statementSpec.OnTriggerDesc is OnTriggerMergeDesc)
                {
                    String insertIntoStreamName;
                    if (statementSpec.InsertIntoDesc != null)
                    {
                        insertIntoStreamName = statementSpec.InsertIntoDesc.EventTypeName;
                    }
                    else
                    {
                        insertIntoStreamName = "merge";
                    }
                    var latchFactoryNameBack = "insert_stream_B_" + insertIntoStreamName + "_" + statementName;
                    var latchFactoryNameFront = "insert_stream_F_" + insertIntoStreamName + "_" + statementName;
                    var msecTimeout = _services.EngineSettingsService.EngineSettings.ThreadingConfig.InsertIntoDispatchTimeout;
                    var locking = _services.EngineSettingsService.EngineSettings.ThreadingConfig.InsertIntoDispatchLocking;
                    var latchFactoryFront = new InsertIntoLatchFactory(latchFactoryNameFront, msecTimeout, locking, _services.TimeSource);
                    var latchFactoryBack = new InsertIntoLatchFactory(latchFactoryNameBack, msecTimeout, locking, _services.TimeSource);
                    statementContext.EpStatementHandle.InsertIntoFrontLatchFactory = latchFactoryFront;
                    statementContext.EpStatementHandle.InsertIntoBackLatchFactory = latchFactoryBack;
                }

                var needDedup = false;
                var streamAnalysis = StatementSpecCompiledAnalyzer.AnalyzeFilters(compiledSpec);
                foreach (FilterSpecCompiled filter in streamAnalysis.Filters) {
                    if (filter.Parameters.Length > 1) {
                        needDedup = true;
                        break;
                    }
                }

                MultiMatchHandler multiMatchHandler;
                var isSubselectPreeval = _services.EngineSettingsService.EngineSettings.ExpressionConfig.IsSelfSubselectPreeval;
                if (!needDedup) {
                    // no dedup
                    if (subselectNodes.IsEmpty()) {
                        multiMatchHandler = MultiMatchHandlerNoSubqueryNoDedup.INSTANCE;
                    }
                    else {
                        if (isSubselectPreeval) {
                            multiMatchHandler = MultiMatchHandlerSubqueryPreevalNoDedup.INSTANCE;
                        }
                        else {
                            multiMatchHandler = MultiMatchHandlerSubqueryPostevalNoDedup.INSTANCE;
                        }
                    }
                }
                else {
                    // with dedup
                    if (subselectNodes.IsEmpty()) {
                        multiMatchHandler = MultiMatchHandlerNoSubqueryWDedup.INSTANCE;
                    }
                    else {
                        multiMatchHandler = new MultiMatchHandlerSubqueryWDedup(isSubselectPreeval);
                    }
                }
                statementContext.EpStatementHandle.MultiMatchHandler = multiMatchHandler;

                // In a join statements if the same event type or it's deep super types are used in the join more then once,
                // then this is a self-join and the statement handle must know to dispatch the results together
                bool canSelfJoin = IsPotentialSelfJoin(compiledSpec) || needDedup;
                statementContext.EpStatementHandle.IsCanSelfJoin = canSelfJoin;

                // add statically typed event type references: those in the from clause; Dynamic (created) types collected by statement context and added on start
                _services.StatementEventTypeRefService.AddReferences(statementName, compiledSpec.EventTypeReferences);

                // add variable references
                _services.StatementVariableRefService.AddReferences(statementName, compiledSpec.VariableReferences, compiledSpec.TableNodes);

                // create metadata
                StatementMetadata statementMetadata = _services.StatementMetadataFactory.Create(new StatementMetadataFactoryContext(statementName, statementId, statementContext, statementSpec, expression, isPattern, optionalModel));

                using (_eventProcessingRwLock.WriteLock.Acquire())
                {
                    try
                    {
                        // create statement - may fail for parser and simple validation errors
                        var preserveDispatchOrder = _services.EngineSettingsService.EngineSettings.ThreadingConfig.IsListenerDispatchPreserveOrder;
                        var isSpinLocks = _services.EngineSettingsService.EngineSettings.ThreadingConfig.ListenerDispatchLocking == ConfigurationEngineDefaults.Threading.Locking.SPIN;
                        var blockingTimeout = _services.EngineSettingsService.EngineSettings.ThreadingConfig.ListenerDispatchTimeout;
                        var timeLastStateChange = _services.SchedulingService.Time;
                        var statement = new EPStatementImpl(
                            _epServiceProvider, statementSpec.ExpressionNoAnnotations, isPattern,
                            _services.DispatchService, this, timeLastStateChange, preserveDispatchOrder, isSpinLocks, blockingTimeout,
                            _services.TimeSource, statementMetadata, statementUserObject, statementContext, isFailed, nameProvided);

                        var isInsertInto = statementSpec.InsertIntoDesc != null;
                        var isDistinct = statementSpec.SelectClauseSpec.IsDistinct;
                        var isForClause = statementSpec.ForClauseSpec != null;
                        statementContext.StatementResultService.SetContext(
                            statement, _epServiceProvider,
                            isInsertInto, isPattern, isDistinct,
                            isForClause, statementContext.EpStatementHandle.MetricsHandle);

                        // create start method
                        var startMethod = EPStatementStartMethodFactory.MakeStartMethod(compiledSpec);

                        statementDesc = new EPStatementDesc(statement, startMethod, statementContext);
                        _stmtIdToDescMap.Put(statementId, statementDesc);
                        _stmtNameToStmtMap.Put(statementName, statement);
                        _stmtNameToIdMap.Put(statementName, statementId);

                        DispatchStatementLifecycleEvent(new StatementLifecycleEvent(statement, StatementLifecycleEvent.LifecycleEventType.CREATE));
                    }
                    catch (Exception)
                    {
                        _stmtIdToDescMap.Remove(statementId);
                        _stmtNameToIdMap.Remove(statementName);
                        _stmtNameToStmtMap.Remove(statementName);
                        throw;
                    }
                }

                return statementDesc;
            }
        }

        /// <summary>
        /// All scripts get compiled/verfied - to ensure they compile (and not just when they are referred to my an expression).
        /// </summary>
        /// <param name="epl"></param>
        /// <param name="scripts"></param>
        /// <param name="expressionDeclDesc"></param>
 
        private void ValidateScripts(String epl, IEnumerable<ExpressionScriptProvided> scripts, ExpressionDeclDesc expressionDeclDesc)
        {
            if (scripts == null)
            {
                return;
            }
            try
            {
                ICollection<NameParameterCountKey> scriptsSet = new HashSet<NameParameterCountKey>();
                foreach (ExpressionScriptProvided script in scripts)
                {
                    ValidateScript(script);

                    var key = new NameParameterCountKey(script.Name, script.ParameterNames.Count);
                    if (scriptsSet.Contains(key))
                    {
                        throw new ExprValidationException("Script name '" + script.Name + "' has already been defined with the same number of parameters");
                    }
                    scriptsSet.Add(key);
                }

                if (expressionDeclDesc != null)
                {
                    foreach (ExpressionDeclItem declItem in expressionDeclDesc.Expressions)
                    {
                        if (scriptsSet.Contains(new NameParameterCountKey(declItem.Name, 0)))
                        {
                            throw new ExprValidationException("Script name '" + declItem.Name + "' overlaps with another expression of the same name");
                        }
                    }
                }
            }
            catch (ExprValidationException ex)
            {
                throw new EPStatementException(ex.Message, ex, epl);
            }
        }

        private void ValidateScript(ExpressionScriptProvided script)
        {
            var dialect = script.OptionalDialect ?? _services.ConfigSnapshot.EngineDefaults.ScriptsConfig.DefaultDialect;
            if (dialect == null)
            {
                throw new ExprValidationException("Failed to determine script dialect for script '" + script.Name + "', please configure a default dialect or provide a dialect explicitly");
            }

            // NOTE: we have to do something here
            _services.ScriptingService.VerifyScript(dialect, script);
            //JSR223Helper.VerifyCompileScript(script, dialect);

            if (!script.ParameterNames.IsEmpty())
            {
                var paramList = new HashSet<String>();
                foreach (String param in script.ParameterNames)
                {
                    if (paramList.Contains(param))
                    {
                        throw new ExprValidationException("Invalid script parameters for script '" + script.Name + "', parameter '" + param + "' is defined more then once");
                    }
                    paramList.Add(param);
                }
            }
        }

        private static bool IsPotentialSelfJoin(StatementSpecCompiled spec)
        {
            // Include create-context as nested contexts that have pattern-initiated sub-contexts may change filters during execution
            if (spec.ContextDesc != null && spec.ContextDesc.ContextDetail is ContextDetailNested)
            {
                return true;
            }

            // if order-by is specified, ans since multiple output rows may produce, ensure dispatch
            if (!spec.OrderByList.IsEmpty())
            {
                return true;
            }

            if (spec.StreamSpecs.OfType<PatternStreamSpecCompiled>().Any())
            {
                return true;
            }

            // not a self join
            if ((spec.StreamSpecs.Length <= 1) && (spec.SubSelectExpressions.IsEmpty()))
            {
                return false;
            }

            // join - determine types joined
            var filteredTypes = new List<EventType>();

            // consider subqueryes
            var optSubselectTypes = PopulateSubqueryTypes(spec.SubSelectExpressions);

            var hasFilterStream = false;
            foreach (EventType type in spec.StreamSpecs.OfType<FilterStreamSpecCompiled>().Select(streamSpec => (streamSpec).FilterSpec.FilterForEventType))
            {
                filteredTypes.Add(type);
                hasFilterStream = true;
            }

            if ((filteredTypes.Count == 1) && (optSubselectTypes.IsEmpty()))
            {
                return false;
            }

            // pattern-only streams are not self-joins
            if (!hasFilterStream)
            {
                return false;
            }

            // is type overlap in filters
            for (int i = 0; i < filteredTypes.Count; i++)
            {
                for (int j = i + 1; j < filteredTypes.Count; j++)
                {
                    var typeOne = filteredTypes[i];
                    var typeTwo = filteredTypes[j];
                    if (typeOne == typeTwo)
                    {
                        return true;
                    }

                    if (typeOne.SuperTypes != null)
                    {
                        if (typeOne.SuperTypes.Any(typeOneSuper => typeOneSuper == typeTwo))
                        {
                            return true;
                        }
                    }
                    if (typeTwo.SuperTypes != null)
                    {
                        if (typeTwo.SuperTypes.Any(typeTwoSuper => typeOne == typeTwoSuper))
                        {
                            return true;
                        }
                    }
                }
            }

            // analyze subselect types
            if (optSubselectTypes.IsNotEmpty())
            {
                foreach (EventType typeOne in filteredTypes)
                {
                    if (optSubselectTypes.Contains(typeOne))
                    {
                        return true;
                    }

                    if (typeOne.SuperTypes != null)
                    {
                        foreach (EventType typeOneSuper in typeOne.SuperTypes)
                        {
                            if (optSubselectTypes.Contains(typeOneSuper))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private static ICollection<EventType> PopulateSubqueryTypes(ExprSubselectNode[] subSelectExpressions)
        {
            ICollection<EventType> set = null;
            foreach (ExprSubselectNode subselect in subSelectExpressions)
            {
                foreach (StreamSpecCompiled streamSpec in subselect.StatementSpecCompiled.StreamSpecs)
                {
                    if (streamSpec is FilterStreamSpecCompiled)
                    {
                        EventType type = ((FilterStreamSpecCompiled)streamSpec).FilterSpec.FilterForEventType;
                        if (set == null)
                        {
                            set = new HashSet<EventType>();
                        }
                        set.Add(type);
                    }
                    else if (streamSpec is PatternStreamSpecCompiled)
                    {
                        EvalNodeAnalysisResult evalNodeAnalysisResult = EvalNodeUtil.RecursiveAnalyzeChildNodes(((PatternStreamSpecCompiled)streamSpec).EvalFactoryNode);
                        IList<EvalFilterFactoryNode> filterNodes = evalNodeAnalysisResult.FilterNodes;
                        foreach (EvalFilterFactoryNode filterNode in filterNodes)
                        {
                            if (set == null)
                            {
                                set = new HashSet<EventType>();
                            }
                            set.Add(filterNode.FilterSpec.FilterForEventType);
                        }
                    }
                }
            }

            return set ?? Collections.GetEmptyList<EventType>();
        }

        public void Start(String statementId)
        {
            using (_iLock.Acquire())
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug(".start Starting statement " + statementId);
                }

                // Acquire a lock for event processing as threads may be in the views used by the statement
                // and that could conflict with the destroy of views
                using (_eventProcessingRwLock.WriteLock.Acquire())
                {
                    var desc = _stmtIdToDescMap.Get(statementId);
                    if (desc == null)
                    {
                        throw new IllegalStateException("Cannot start statement, statement is in destroyed state");
                    }
                    StartInternal(statementId, desc, false, false, false);
                }
            }
        }

        /// <summary>Start the given statement. </summary>
        /// <param name="statementId">is the statement id</param>
        /// <param name="desc">is the cached statement info</param>
        /// <param name="isNewStatement">indicator whether the statement is new or a stop-restart statement</param>
        /// <param name="isRecoveringStatement">if the statement is recovering or new</param>
        /// <param name="isResilient">true if recovering a resilient stmt</param>
        public void Start(String statementId, EPStatementDesc desc, bool isNewStatement, bool isRecoveringStatement, bool isResilient)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug(".start Starting statement " + statementId + " from desc=" + desc);
            }

            // Acquire a lock for event processing as threads may be in the views used by the statement
            // and that could conflict with the destroy of views
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QEngineManagementStmtCompileStart(_services.EngineURI, statementId, desc.EpStatement.Name, desc.EpStatement.Text, _services.SchedulingService.Time); }

            using (_eventProcessingRwLock.WriteLock.Acquire())
            {
                try
                {
                    StartInternal(statementId, desc, isNewStatement, isRecoveringStatement, isResilient);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QaEngineManagementStmtStarted( _services.EngineURI, statementId, desc.EpStatement.Name, desc.EpStatement.Text, _services.SchedulingService.Time); }
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AEngineManagementStmtCompileStart(true, null); }
                }
                catch(Exception ex)
                {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AEngineManagementStmtCompileStart(false, ex.Message); }
                    throw;
                }
            }
        }

        private void StartInternal(String statementId, EPStatementDesc desc, bool isNewStatement, bool isRecoveringStatement, bool isResilient)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug(".startInternal Starting statement " + statementId + " from desc=" + desc);
            }

            if (desc.StartMethod == null)
            {
                throw new IllegalStateException("Statement start method not found for id " + statementId);
            }

            EPStatementSPI statement = desc.EpStatement;
            if (statement.State == EPStatementState.STARTED)
            {
                Log.Debug(".startInternal - Statement already started");
                return;
            }

            EPStatementStartResult startResult;
            try
            {
                startResult = desc.StartMethod.Start(_services, desc.StatementContext, isNewStatement, isRecoveringStatement, isResilient);
            }
            catch (EPStatementException ex)
            {
                HandleRemove(statementId, statement.Name);
                Log.Debug(".start Error starting statement", ex);
                throw;
            }
            catch (ExprValidationException ex)
            {
                HandleRemove(statementId, statement.Name);
                Log.Debug(".start Error starting statement", ex);
                throw new EPStatementException("Error starting statement: " + ex.Message, ex, statement.Text);
            }
            catch (ViewProcessingException ex)
            {
                HandleRemove(statementId, statement.Name);
                Log.Debug(".start Error starting statement", ex);
                throw new EPStatementException("Error starting statement: " + ex.Message, ex, statement.Text);
            }
            catch (Exception ex)
            {
                HandleRemove(statementId, statement.Name);
                Log.Debug(".start Error starting statement", ex);
                throw new EPStatementException("Unexpected exception starting statement: " + ex.Message, ex, statement.Text);
            }

            // hook up
            Viewable parentView = startResult.Viewable;
            desc.StopMethod = startResult.StopMethod;
            desc.DestroyMethod = startResult.DestroyMethod;
            statement.ParentView = parentView;
            long timeLastStateChange = _services.SchedulingService.Time;
            statement.SetCurrentState(EPStatementState.STARTED, timeLastStateChange);

            DispatchStatementLifecycleEvent(new StatementLifecycleEvent(statement, StatementLifecycleEvent.LifecycleEventType.STATECHANGE));
        }

        private void HandleRemove(String statementId, String statementName)
        {
            if (statementId != null)
            {
                _stmtIdToDescMap.Remove(statementId);
            }

            if (statementName != null)
            {
                _stmtNameToIdMap.Remove(statementName);
                _stmtNameToStmtMap.Remove(statementName);
            }

            _services.StatementEventTypeRefService.RemoveReferencesStatement(statementName);
        }

        public void Stop(String statementId)
        {
            using (_iLock.Acquire())
            {
                // Acquire a lock for event processing as threads may be in the views used by the statement
                // and that could conflict with the destroy of views
                try
                {
                    using (_eventProcessingRwLock.WriteLock.Acquire())
                    {
                        EPStatementDesc desc = _stmtIdToDescMap.Get(statementId);
                        if (desc == null)
                        {
                            throw new IllegalStateException("Cannot stop statement, statement is in destroyed state");
                        }

                        EPStatementSPI statement = desc.EpStatement;
                        EPStatementStopMethod stopMethod = desc.StopMethod;
                        if (stopMethod == null)
                        {
                            throw new IllegalStateException("Stop method not found for statement " + statementId);
                        }

                        if (statement.State == EPStatementState.STOPPED)
                        {
                            Log.Debug(".startInternal - Statement already stopped");
                            return;
                        }

                        // fire the statement stop
                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QEngineManagementStmtStop(EPStatementState.STOPPED, _services.EngineURI, statementId, statement.Name, statement.Text, _services.SchedulingService.Time); }
                        desc.StatementContext.StatementStopService.FireStatementStopped();

                        // invoke start-provided stop method
                        stopMethod.Invoke();
                        statement.ParentView = null;
                        desc.StopMethod = null;

                        long timeLastStateChange = _services.SchedulingService.Time;
                        statement.SetCurrentState(EPStatementState.STOPPED, timeLastStateChange);

                        ((EPRuntimeSPI) _epServiceProvider.EPRuntime).ClearCaches();

                        DispatchStatementLifecycleEvent(new StatementLifecycleEvent(statement, StatementLifecycleEvent.LifecycleEventType.STATECHANGE));
                    }
                } finally {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AEngineManagementStmtStop(); }
                }
            }
        }

        public void Destroy(String statementId)
        {
            using (_iLock.Acquire())
            {
                // Acquire a lock for event processing as threads may be in the views used by the statement
                // and that could conflict with the destroy of views
                using (_eventProcessingRwLock.WriteLock.Acquire())
                {
                    EPStatementDesc desc = _stmtIdToDescMap.Get(statementId);
                    if (desc == null)
                    {
                        Log.Debug(".startInternal - Statement already destroyed");
                        return;
                    }

                    try
                    {
                        // fire the statement stop
                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QEngineManagementStmtStop(EPStatementState.DESTROYED, _services.EngineURI, statementId, desc.EpStatement.Name, desc.EpStatement.Text, _services.SchedulingService.Time); }

                        // remove referenced event types
                        _services.StatementEventTypeRefService.RemoveReferencesStatement(desc.EpStatement.Name);

                        // remove referenced variabkes
                        _services.StatementVariableRefService.RemoveReferencesStatement(desc.EpStatement.Name);

                        // remove the named window lock
                        _services.NamedWindowService.RemoveNamedWindowLock(desc.EpStatement.Name);

                        // remove any pattern subexpression counts
                        if (_services.PatternSubexpressionPoolSvc != null)
                        {
                            _services.PatternSubexpressionPoolSvc.RemoveStatement(desc.EpStatement.Name);
                        }

                        EPStatementSPI statement = desc.EpStatement;
                        if (statement.State == EPStatementState.STARTED)
                        {
                            // fire the statement stop
                            desc.StatementContext.StatementStopService.FireStatementStopped();

                            // invoke start-provided stop method
                            EPStatementStopMethod stopMethod = desc.StopMethod;
                            statement.ParentView = null;
                            stopMethod.Invoke();
                        }

                        if (desc.DestroyMethod != null)
                        {
                            desc.DestroyMethod.Invoke();
                        }

                        // finally remove reference to schedulable agent-instance resources (an HA requirements)
                        if (_services.SchedulableAgentInstanceDirectory != null)
                        {
                            _services.SchedulableAgentInstanceDirectory.RemoveStatement(
                                desc.StatementContext.EpStatementHandle.StatementId);
                        }

                        long timeLastStateChange = _services.SchedulingService.Time;
                        statement.SetCurrentState(EPStatementState.DESTROYED, timeLastStateChange);

                        _stmtNameToStmtMap.Remove(statement.Name);
                        _stmtNameToIdMap.Remove(statement.Name);
                        _stmtIdToDescMap.Remove(statementId);

                        if (!_epServiceProvider.IsDestroyed)
                        {
                            ((EPRuntimeSPI) _epServiceProvider.EPRuntime).ClearCaches();
                        }

                        DispatchStatementLifecycleEvent(
                            new StatementLifecycleEvent(
                                statement, StatementLifecycleEvent.LifecycleEventType.STATECHANGE));
                    }
                    finally
                    {
                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AEngineManagementStmtStop(); }
                    }
                }
            }
        }

        public EPStatement GetStatementByName(String name)
        {
            using (_iLock.Acquire())
            {
                return _stmtNameToStmtMap.Get(name);
            }
        }

        public StatementSpecCompiled GetStatementSpec(String statementId)
        {
            using (_iLock.Acquire())
            {
                var desc = _stmtIdToDescMap.Get(statementId);
                if (desc != null)
                {
                    return desc.StartMethod.StatementSpec;
                }
                return null;
            }
        }

        /// <summary>Returns the statement given a statement id. </summary>
        /// <param name="id">is the statement id</param>
        /// <returns>statement</returns>
        public EPStatementSPI GetStatementById(String id)
        {
            var statementDesc = _stmtIdToDescMap.Get(id);
            if (statementDesc == null)
            {
                Log.Warn("Could not locate statement descriptor for statement id '" + id + "'");
                return null;
            }
            return statementDesc.EpStatement;
        }

        public string[] StatementNames
        {
            get
            {
                using (_iLock.Acquire())
                {
                    return _stmtNameToStmtMap.Keys.ToArray();
                }
            }
        }

        public void StartAllStatements()
        {
            using (_iLock.Acquire())
            {
                IEnumerable<string> statementIds = StatementIds;
                foreach (string statementId in statementIds)
                {
                    EPStatement statement = _stmtIdToDescMap.Get(statementId).EpStatement;
                    if (statement.State == EPStatementState.STOPPED)
                    {
                        Start(statementId);
                    }
                }
            }
        }

        public void StopAllStatements()
        {
            using (_iLock.Acquire())
            {
                foreach (string statementId in StatementIds)
                {
                    EPStatement statement = _stmtIdToDescMap.Get(statementId).EpStatement;
                    if (statement.State == EPStatementState.STARTED)
                    {
                        Stop(statementId);
                    }
                }
            }
        }

        public void DestroyAllStatements()
        {
            using (_iLock.Acquire())
            {
                foreach (string statementId in StatementIds)
                {
                    try
                    {
                        Destroy(statementId);
                    }
                    catch (Exception ex)
                    {
                        Log.Warn(string.Format("Error destroying statement:{0}", ex.Message), ex);
                    }
                }
            }
        }

        private IEnumerable<string> StatementIds
        {
            get
            {
                return _stmtNameToIdMap.Values.ToArray();
            }
        }

        private String GetUniqueStatementName(String statementName, String statementId)
        {
            String finalStatementName;

            if (_stmtNameToIdMap.ContainsKey(statementName))
            {
                int count = 0;
                while (true)
                {
                    finalStatementName = statementName + "--" + count;
                    if (!(_stmtNameToIdMap.ContainsKey(finalStatementName)))
                    {
                        break;
                    }
                    if (count > int.MaxValue - 2)
                    {
                        throw new EPException("Failed to establish a unique statement name");
                    }
                    count++;
                }
            }
            else
            {
                finalStatementName = statementName;
            }

            _stmtNameToIdMap.Put(finalStatementName, statementId);
            return finalStatementName;
        }

        public String GetStatementNameById(String id)
        {
            EPStatementDesc desc = _stmtIdToDescMap.Get(id);
            if (desc != null)
            {
                return desc.EpStatement.Name;
            }
            return null;
        }

        public void UpdatedListeners(EPStatement statement, EPStatementListenerSet listeners, bool isRecovery)
        {
            Log.Debug(".UpdatedListeners No action for base implementation");
        }

        /// <summary>
        /// Compiles a statement returning the compile (verified, non-serializable) form of a statement.
        /// </summary>
        /// <param name="spec">is the statement specification</param>
        /// <param name="eplStatement">the statement to compile</param>
        /// <param name="statementContext">the statement services</param>
        /// <param name="isSubquery">is true for subquery compilation or false for statement compile</param>
        /// <param name="isOnDemandQuery">if set to <c>true</c> [is on demand query].</param>
        /// <param name="annotations">statement annotations</param>
        /// <param name="subselectNodes">The subselect nodes.</param>
        /// <param name="declaredNodes">The declared nodes.</param>
        /// <param name="servicesContext">The services context.</param>
        /// <returns>compiled statement</returns>
        /// <throws>EPStatementException if the statement cannot be compiled</throws>
        public static StatementSpecCompiled Compile(StatementSpecRaw spec,
                                                    String eplStatement,
                                                    StatementContext statementContext,
                                                    bool isSubquery,
                                                    bool isOnDemandQuery,
                                                    Attribute[] annotations,
                                                    IList<ExprSubselectNode> subselectNodes,
                                                    IList<ExprDeclaredNode> declaredNodes,
                                                    EPServicesContext servicesContext)
        {
            List<StreamSpecCompiled> compiledStreams;
            ICollection<String> eventTypeReferences = new HashSet<String>();

            // If not using a join and not specifying a data window, make the where-clause, if present, the filter of the stream
            // if selecting using filter spec, and not subquery in where clause
            if ((spec.StreamSpecs.Count == 1) &&
                (spec.StreamSpecs[0] is FilterStreamSpecRaw) &&
                (spec.StreamSpecs[0].ViewSpecs.IsEmpty()) &&
                (spec.FilterRootNode != null) &&
                (spec.OnTriggerDesc == null) &&
                (!isSubquery) &&
                (!isOnDemandQuery) &&
                (spec.TableExpressions == null || spec.TableExpressions.IsEmpty()))
            {
                ExprNode whereClause = spec.FilterRootNode;

                var dotVisitor = new ExprNodeSubselectDeclaredDotVisitor();
                whereClause.Accept(dotVisitor);

                var disqualified = dotVisitor.Subselects.Count > 0;

                if (!disqualified)
                {
                    var viewResourceVisitor = new ExprNodeViewResourceVisitor();
                    whereClause.Accept(viewResourceVisitor);
                    disqualified = viewResourceVisitor.ExprNodes.Count > 0;
                }

                if (!disqualified)
                {
                    // If an alias is provided, find all properties to ensure the alias gets removed
                    String alias = spec.StreamSpecs[0].OptionalStreamName;
                    if (alias != null)
                    {
                        var v = new ExprNodeIdentifierCollectVisitor();
                        whereClause.Accept(v);
                        foreach (ExprIdentNode node in v.ExprProperties)
                        {
                            if (node.StreamOrPropertyName != null && (node.StreamOrPropertyName.Equals(alias)))
                            {
                                node.StreamOrPropertyName = null;
                            }
                        }
                    }

                    spec.FilterExprRootNode = null;
                    var streamSpec = (FilterStreamSpecRaw)spec.StreamSpecs[0];
                    streamSpec.RawFilterSpec.FilterExpressions.Add(whereClause);
                }
            }

            // compile select-clause
            var selectClauseCompiled = StatementLifecycleSvcUtil.CompileSelectClause(spec.SelectClauseSpec);

            // Determine subselects in filter streams, these may need special handling for locking
            var visitor = new ExprNodeSubselectDeclaredDotVisitor();
            StatementLifecycleSvcUtil.WalkStreamSpecs(spec, visitor);
            foreach (ExprSubselectNode subselectNode in visitor.Subselects)
            {
                subselectNode.IsFilterStreamSubselect = true;
            }

            // Determine subselects for compilation, and lambda-expression shortcut syntax for named windows
            visitor.Reset();
            GroupByClauseExpressions groupByRollupExpressions;
 
            try
            {
                StatementLifecycleSvcUtil.WalkStatement(spec, visitor);

                groupByRollupExpressions = GroupByExpressionHelper.GetGroupByRollupExpressions(
                    spec.GroupByExpressions,
                    spec.SelectClauseSpec, 
                    spec.HavingExprRootNode,
                    spec.OrderByList, visitor);

                IList<ExprSubselectNode> subselectsX = visitor.Subselects;
                if (!visitor.ChainedExpressionsDot.IsEmpty())
                {
                    RewriteNamedWindowSubselect(visitor.ChainedExpressionsDot, subselectsX, statementContext.NamedWindowService);
                }
            }
            catch (ExprValidationException ex)
            {
                throw new EPStatementException(ex.Message, eplStatement);
            }

            if (isSubquery && !visitor.Subselects.IsEmpty()) {
                throw new EPStatementException("Invalid nested subquery, subquery-within-subquery is not supported", eplStatement);
            }
            if (isOnDemandQuery && !visitor.Subselects.IsEmpty()) {
                throw new EPStatementException("Subqueries are not a supported feature of on-demand queries", eplStatement);
            }
            foreach (ExprSubselectNode subselectNode in visitor.Subselects) {
                if (!subselectNodes.Contains(subselectNode)) {
                    subselectNodes.Add(subselectNode);
                }
            }

            // Compile subselects found
            int subselectNumber = 0;
            foreach (ExprSubselectNode subselect in subselectNodes)
            {
                StatementSpecRaw raw = subselect.StatementSpecRaw;
                StatementSpecCompiled compiled = Compile(
                    raw, eplStatement, statementContext, true, isOnDemandQuery,
                    new Attribute[0],
                    Collections.GetEmptyList<ExprSubselectNode>(),
                    Collections.GetEmptyList<ExprDeclaredNode>(),
                    servicesContext);
                subselectNumber++;
                subselect.SetStatementSpecCompiled(compiled, subselectNumber);
            }

            // compile each stream used
            try
            {
                compiledStreams = new List<StreamSpecCompiled>();
                int streamNum = 0;
                foreach (StreamSpecRaw rawSpec in spec.StreamSpecs)
                {
                    streamNum++;
                    StreamSpecCompiled compiled = rawSpec.Compile(statementContext, eventTypeReferences, spec.InsertIntoDesc != null, streamNum.AsSingleton(), spec.StreamSpecs.Count > 1, false, spec.OnTriggerDesc != null);
                    compiledStreams.Add(compiled);
                }
            }
            catch (ExprValidationException ex)
            {
                Log.Info("Failed to compile statement: " + ex.Message, ex);
                throw new EPStatementException(ex.Message, eplStatement, ex);
            }
            catch (Exception ex)
            {
                const string text = "Unexpected error compiling statement";
                Log.Error(text, ex);
                throw new EPStatementException(text + ": " + ex.GetType().FullName + ":" + ex.Message, eplStatement, ex);
            }

            // for create window statements, we switch the filter to a new event type
            if (spec.CreateWindowDesc != null)
            {
                try
                {
                    StreamSpecCompiled createWindowTypeSpec = compiledStreams[0];
                    EventType selectFromType;
                    String selectFromTypeName;
                    if (createWindowTypeSpec is FilterStreamSpecCompiled)
                    {
                        var filterStreamSpec = (FilterStreamSpecCompiled)createWindowTypeSpec;
                        selectFromType = filterStreamSpec.FilterSpec.FilterForEventType;
                        selectFromTypeName = filterStreamSpec.FilterSpec.FilterForEventTypeName;

                        if (spec.CreateWindowDesc.IsInsert || spec.CreateWindowDesc.InsertFilter != null)
                        {
                            throw new EPStatementException("A named window by name '" + selectFromTypeName + "' could not be located, use the insert-keyword with an existing named window", eplStatement);
                        }
                    }
                    else
                    {
                        var consumerStreamSpec = (NamedWindowConsumerStreamSpec)createWindowTypeSpec;
                        selectFromType = statementContext.EventAdapterService.GetEventTypeByName(consumerStreamSpec.WindowName);
                        selectFromTypeName = consumerStreamSpec.WindowName;

                        if (spec.CreateWindowDesc.InsertFilter != null)
                        {
                            ExprNode insertIntoFilter = spec.CreateWindowDesc.InsertFilter;
                            String checkMinimal = ExprNodeUtility.IsMinimalExpression(insertIntoFilter);
                            if (checkMinimal != null)
                            {
                                throw new ExprValidationException("Create window where-clause may not have " + checkMinimal);
                            }
                            var streamTypeService = new StreamTypeServiceImpl(selectFromType, selectFromTypeName, true, statementContext.EngineURI);
                            var evaluatorContextStmt = new ExprEvaluatorContextStatement(statementContext, false);
                            var validationContext = new ExprValidationContext(
                                streamTypeService, 
                                statementContext.MethodResolutionService, null,
                                statementContext.SchedulingService, 
                                statementContext.VariableService,
                                statementContext.TableService,
                                evaluatorContextStmt, 
                                statementContext.EventAdapterService,
                                statementContext.StatementName, 
                                statementContext.StatementId,
                                statementContext.Annotations, 
                                statementContext.ContextDescriptor,
                                statementContext.ScriptingService,
                                false,
                                false,
                                false,
                                false,
                                null,
                                false);
                            var insertFilter = ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.CREATEWINDOWFILTER, spec.CreateWindowDesc.InsertFilter, validationContext);
                            spec.CreateWindowDesc.InsertFilter = insertFilter;
                        }

                        // set the window to insert from
                        spec.CreateWindowDesc.InsertFromWindow = consumerStreamSpec.WindowName;
                    }
                    Pair<FilterSpecCompiled, SelectClauseSpecRaw> newFilter = HandleCreateWindow(selectFromType, selectFromTypeName, spec.CreateWindowDesc.Columns, spec, eplStatement, statementContext, servicesContext);
                    eventTypeReferences.Add(((EventTypeSPI)newFilter.First.FilterForEventType).Metadata.PrimaryName);

                    // view must be non-empty list
                    if (spec.CreateWindowDesc.ViewSpecs.IsEmpty())
                    {
                        throw new ExprValidationException(NamedWindowServiceConstants.ERROR_MSG_DATAWINDOWS);
                    }

                    // use the filter specification of the newly created event type and the views for the named window
                    compiledStreams.Clear();
                    var views = spec.CreateWindowDesc.ViewSpecs.ToArray();
                    compiledStreams.Add(new FilterStreamSpecCompiled(newFilter.First, views.ToArray(), null, spec.CreateWindowDesc.StreamSpecOptions));
                    spec.SelectClauseSpec = newFilter.Second;
                }
                catch (ExprValidationException e)
                {
                    throw new EPStatementException(e.Message, eplStatement, e);
                }
            }

            return new StatementSpecCompiled(
                    spec.OnTriggerDesc,
                    spec.CreateWindowDesc,
                    spec.CreateIndexDesc,
                    spec.CreateVariableDesc,
                    spec.CreateTableDesc,
                    spec.CreateSchemaDesc,
                    spec.InsertIntoDesc,
                    spec.SelectStreamSelectorEnum,
                    selectClauseCompiled,
                    compiledStreams.ToArray(),
                    spec.OuterJoinDescList.ToArray(),
                    spec.FilterRootNode,
                    spec.HavingExprRootNode,
                    spec.OutputLimitSpec,
                    spec.OrderByList.ToArray(),
                    subselectNodes.ToArray(),
                    declaredNodes.ToArray(),
                    spec.ReferencedVariables,
                    spec.RowLimitSpec,
                    eventTypeReferences.ToArray(),
                    annotations,
                    spec.UpdateDesc,
                    spec.MatchRecognizeSpec,
                    spec.ForClauseSpec,
                    spec.SqlParameters,
                    spec.CreateContextDesc,
                    spec.OptionalContextName,
                    spec.CreateDataFlowDesc,
                    spec.CreateExpressionDesc,
                    spec.FireAndForgetSpec,
                    groupByRollupExpressions,
                    spec.IntoTableSpec,
                    spec.TableExpressions == null ? null : spec.TableExpressions.ToArray()
                    );
        }

        private static bool DetermineStatelessSelect(StatementType type, StatementSpecRaw spec, bool hasSubselects, bool isPattern)
        {
            if (hasSubselects || isPattern)
            {
                return false;
            }
            if (type != StatementType.SELECT && type != StatementType.INSERT_INTO)
            {
                return false;
            }
            if (spec.StreamSpecs == null || spec.StreamSpecs.Count > 1 || spec.StreamSpecs.IsEmpty())
            {
                return false;
            }
            StreamSpecRaw singleStream = spec.StreamSpecs[0];
            if (!(singleStream is FilterStreamSpecRaw) && !(singleStream is NamedWindowConsumerStreamSpec))
            {
                return false;
            }
            if (singleStream.ViewSpecs != null && singleStream.ViewSpecs.Length > 0)
            {
                return false;
            }
            if (spec.OutputLimitSpec != null)
            {
                return false;
            }
            if (spec.MatchRecognizeSpec != null)
            {
                return false;
            }

            var expressions = StatementSpecRawAnalyzer.CollectExpressionsShallow(spec);
            if (expressions.IsEmpty())
            {
                return true;
            }

            var visitor = new ExprNodeSummaryVisitor();
            foreach (ExprNode expr in expressions)
            {
                if (expr == null)
                {
                    continue;
                }
                expr.Accept(visitor);
            }

            return !visitor.HasAggregation && !visitor.HasPreviousPrior && !visitor.HasSubselect;
        }

        private static void RewriteNamedWindowSubselect(IList<ExprDotNode> chainedExpressionsDot, IList<ExprSubselectNode> subselects, NamedWindowService service)
        {
            foreach (ExprDotNode dotNode in chainedExpressionsDot)
            {
                String proposedWindow = dotNode.ChainSpec[0].Name;
                if (!service.IsNamedWindow(proposedWindow))
                {
                    continue;
                }

                // build spec for subselect
                var raw = new StatementSpecRaw(SelectClauseStreamSelectorEnum.ISTREAM_ONLY);
                var filter = new FilterSpecRaw(proposedWindow, Collections.GetEmptyList<ExprNode>(), null);
                raw.StreamSpecs.Add(new FilterStreamSpecRaw(filter, new ViewSpec[0], proposedWindow, new StreamSpecOptions()));

                ExprChainedSpec firstChain = dotNode.ChainSpec.Pluck(0);
                if (!firstChain.Parameters.IsEmpty())
                {
                    if (firstChain.Parameters.Count == 1)
                    {
                        raw.FilterExprRootNode = firstChain.Parameters[0];
                    }
                    else
                    {
                        ExprAndNode andNode = new ExprAndNodeImpl();
                        foreach (ExprNode node in firstChain.Parameters)
                        {
                            andNode.AddChildNode(node);
                        }
                        raw.FilterExprRootNode = andNode;
                    }
                }

                // activate subselect
                ExprSubselectNode subselect = new ExprSubselectRowNode(raw);
                subselects.Add(subselect);
                dotNode.ChildNodes = new []{subselect};
            }
        }

        /// <summary>Compile a select clause allowing subselects. </summary>
        /// <param name="spec">to compile</param>
        /// <returns>select clause compiled</returns>
        /// <throws>ExprValidationException when validation fails</throws>
        public static SelectClauseSpecCompiled CompileSelectAllowSubselect(SelectClauseSpecRaw spec)
        {
            // Look for expressions with sub-selects in select expression list and filter expression
            // Recursively compile the statement within the statement.
            var visitor = new ExprNodeSubselectDeclaredDotVisitor();
            var selectElements = new List<SelectClauseElementCompiled>();
            foreach (SelectClauseElementRaw raw in spec.SelectExprList)
            {
                if (raw is SelectClauseExprRawSpec)
                {
                    var rawExpr = (SelectClauseExprRawSpec)raw;
                    rawExpr.SelectExpression.Accept(visitor);
                    selectElements.Add(new SelectClauseExprCompiledSpec(rawExpr.SelectExpression, rawExpr.OptionalAsName, rawExpr.OptionalAsName, rawExpr.IsEvents));
                }
                else if (raw is SelectClauseStreamRawSpec)
                {
                    var rawExpr = (SelectClauseStreamRawSpec)raw;
                    selectElements.Add(new SelectClauseStreamCompiledSpec(rawExpr.StreamName, rawExpr.OptionalAsName));
                }
                else if (raw is SelectClauseElementWildcard)
                {
                    var wildcard = (SelectClauseElementWildcard)raw;
                    selectElements.Add(wildcard);
                }
                else
                {
                    throw new IllegalStateException("Unexpected select clause element class : " + raw.GetType().FullName);
                }
            }

            return new SelectClauseSpecCompiled(selectElements.ToArray(), spec.IsDistinct);
        }

        // The create window command:
        //      create window windowName[.window_view_list] as [select properties from] type
        //
        // This section expected s single FilterStreamSpecCompiled representing the selected type.
        // It creates a new event type representing the window type and a sets the type selected on the filter stream spec.
        private static Pair<FilterSpecCompiled, SelectClauseSpecRaw> HandleCreateWindow(
            EventType selectFromType,
            String selectFromTypeName,
            IList<ColumnDesc> columns,
            StatementSpecRaw spec,
            String eplStatement,
            StatementContext statementContext,
            EPServicesContext servicesContext)
        {
            String typeName = spec.CreateWindowDesc.WindowName;
            EventType targetType;

            // Validate the select expressions which consists of properties only
            var evaluatorContextStmt = new ExprEvaluatorContextStatement(statementContext, false);
            var select = CompileLimitedSelect(
                spec.SelectClauseSpec, eplStatement,
                selectFromType,
                selectFromTypeName,
                statementContext.EngineURI,
                evaluatorContextStmt,
                statementContext.MethodResolutionService,
                statementContext.EventAdapterService,
                statementContext.ScriptingService,
                statementContext.StatementName,
                statementContext.StatementId,
                statementContext.Annotations);

            // Create Map or Wrapper event type from the select clause of the window.
            // If no columns selected, simply create a wrapper type
            // Build a list of properties
            var newSelectClauseSpecRaw = new SelectClauseSpecRaw();
            IDictionary<String, Object> properties;
            bool hasProperties = false;
            if ((columns != null) && (columns.IsNotEmpty()))
            {
                properties = EventTypeUtility.BuildType(columns, statementContext.EventAdapterService, null, statementContext.MethodResolutionService.EngineImportService);
                hasProperties = true;
            }
            else
            {
                properties = new LinkedHashMap<String, Object>();
                foreach (NamedWindowSelectedProps selectElement in select)
                {
                    if (selectElement.FragmentType != null)
                    {
                        properties.Put(selectElement.AssignedName, selectElement.FragmentType);
                    }
                    else
                    {
                        properties.Put(selectElement.AssignedName, selectElement.SelectExpressionType);
                    }

                    // Add any properties to the new select clause for use by consumers to the statement itself
                    newSelectClauseSpecRaw.Add(new SelectClauseExprRawSpec(new ExprIdentNodeImpl(selectElement.AssignedName), null, false));
                    hasProperties = true;
                }
            }

            // Create Map or Wrapper event type from the select clause of the window.
            // If no columns selected, simply create a wrapper type
            bool isOnlyWildcard = spec.SelectClauseSpec.IsOnlyWildcard;
            bool isWildcard = spec.SelectClauseSpec.IsUsingWildcard;
            if (statementContext.ValueAddEventService.IsRevisionTypeName(selectFromTypeName))
            {
                targetType = statementContext.ValueAddEventService.CreateRevisionType(typeName, selectFromTypeName, statementContext.StatementStopService, statementContext.EventAdapterService, servicesContext.EventTypeIdGenerator);
            }
            else if (isWildcard && !isOnlyWildcard)
            {
                targetType = statementContext.EventAdapterService.AddWrapperType(typeName, selectFromType, properties, true, false);
            }
            else
            {
                // Some columns selected, use the types of the columns
                if (hasProperties && !isOnlyWildcard)
                {
                    var compiledProperties = EventTypeUtility.CompileMapTypeProperties(properties, statementContext.EventAdapterService);
                    var mapType = EventRepresentationUtil.IsMap(statementContext.Annotations, servicesContext.ConfigSnapshot, AssignedType.NONE);
                    if (mapType)
                    {
                        targetType = statementContext.EventAdapterService.AddNestableMapType(typeName, compiledProperties, null, false, false, false, true, false);
                    }
                    else
                    {
                        targetType = statementContext.EventAdapterService.AddNestableObjectArrayType(typeName, compiledProperties, null, false, false, false, true, false, false, null);
                    }
                }
                else
                {
                    // No columns selected, no wildcard, use the type as is or as a wrapped type
                    if (selectFromType is ObjectArrayEventType)
                    {
                        var objectArrayEventType = (ObjectArrayEventType) selectFromType;
                        targetType = statementContext.EventAdapterService.AddNestableObjectArrayType(typeName, objectArrayEventType.Types, null, false, false, false, true, false, false, null);
                    }
                    else if (selectFromType is MapEventType)
                    {
                        var mapType = (MapEventType)selectFromType;
                        targetType = statementContext.EventAdapterService.AddNestableMapType(typeName, mapType.Types, null, false, false, false, true, false);
                    }
                    else if (selectFromType is BeanEventType)
                    {
                        var beanType = (BeanEventType)selectFromType;
                        targetType = statementContext.EventAdapterService.AddBeanTypeByName(typeName, beanType.UnderlyingType, true);
                    }
                    else
                    {
                        var addOnTypes = new Dictionary<String, Object>();
                        targetType = statementContext.EventAdapterService.AddWrapperType(typeName, selectFromType, addOnTypes, true, false);
                    }
                }
            }

            var filter = new FilterSpecCompiled(targetType, typeName, new List<FilterSpecParam>[0], null);
            return new Pair<FilterSpecCompiled, SelectClauseSpecRaw>(filter, newSelectClauseSpecRaw);
        }

        /// <summary>
        /// Compiles the limited select.
        /// </summary>
        /// <param name="spec">The spec.</param>
        /// <param name="eplStatement">The epl statement.</param>
        /// <param name="singleType">Type of the single.</param>
        /// <param name="selectFromTypeName">Name of the select from type.</param>
        /// <param name="engineURI">The engine URI.</param>
        /// <param name="exprEvaluatorContext">The expr evaluator context.</param>
        /// <param name="methodResolutionService">The method resolution service.</param>
        /// <param name="eventAdapterService">The event adapter service.</param>
        /// <param name="scriptingService">The scripting service.</param>
        /// <param name="statementName">Name of the statement.</param>
        /// <param name="statementId">The statement id.</param>
        /// <param name="annotations">The annotations.</param>
        /// <returns></returns>
        private static IEnumerable<NamedWindowSelectedProps> CompileLimitedSelect(
            SelectClauseSpecRaw spec,
            String eplStatement,
            EventType singleType,
            String selectFromTypeName,
            String engineURI,
            ExprEvaluatorContext exprEvaluatorContext,
            MethodResolutionService methodResolutionService,
            EventAdapterService eventAdapterService,
            ScriptingService scriptingService,
            String statementName,
            String statementId,
            Attribute[] annotations)
        {
            var selectProps = new List<NamedWindowSelectedProps>();
            var streams = new StreamTypeServiceImpl(new[] { singleType }, new[] { "stream_0" }, new[] { false }, engineURI, false);

            var validationContext = new ExprValidationContext(
                streams, methodResolutionService, null, null, null, null, exprEvaluatorContext, eventAdapterService,
                statementName, statementId, annotations, null, scriptingService, false, false, false, false, null, false);
            foreach (SelectClauseElementRaw raw in spec.SelectExprList)
            {
                if (!(raw is SelectClauseExprRawSpec))
                {
                    continue;
                }
                var exprSpec = (SelectClauseExprRawSpec)raw;
                ExprNode validatedExpression;
                try
                {
                    validatedExpression = ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.SELECT, exprSpec.SelectExpression, validationContext);
                }
                catch (ExprValidationException e)
                {
                    throw new EPStatementException(e.Message, e, eplStatement);
                }

                // determine an element name if none assigned
                var asName = exprSpec.OptionalAsName ?? ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(validatedExpression);

                // check for fragments
                EventType fragmentType = null;
                if ((validatedExpression is ExprIdentNode) && (!(singleType is NativeEventType)))
                {
                    var identNode = (ExprIdentNode)validatedExpression;
                    var fragmentEventType = singleType.GetFragmentType(identNode.FullUnresolvedName);
                    if ((fragmentEventType != null) && (!fragmentEventType.IsNative))
                    {
                        fragmentType = fragmentEventType.FragmentType;
                    }
                }

                var validatedElement = new NamedWindowSelectedProps(validatedExpression.ExprEvaluator.ReturnType, asName, fragmentType);
                selectProps.Add(validatedElement);
            }

            return selectProps;
        }

        public void DispatchStatementLifecycleEvent(StatementLifecycleEvent theEvent)
        {
            if (LifecycleEvent != null)
            {
                LifecycleEvent(this, theEvent);
            }
        }

        /// <summary>
        /// Statement information.
        /// </summary>
        public class EPStatementDesc
        {
            /// <summary>
            /// Ctor.
            /// </summary>
            /// <param name="epStatement">the statement</param>
            /// <param name="startMethod">the start method</param>
            /// <param name="statementContext">statement context</param>
            public EPStatementDesc(EPStatementSPI epStatement, EPStatementStartMethod startMethod, StatementContext statementContext)
            {
                EpStatement = epStatement;
                StartMethod = startMethod;
                StatementContext = statementContext;
            }

            /// <summary>Returns the statement. </summary>
            /// <value>statement.</value>
            public EPStatementSPI EpStatement { get; private set; }

            /// <summary>Returns the start method. </summary>
            /// <value>start method</value>
            public EPStatementStartMethod StartMethod { get; private set; }

            /// <summary>Returns the stop method. </summary>
            /// <value>stop method</value>
            public EPStatementStopMethod StopMethod { get; set; }

            /// <summary>Returns the statement context. </summary>
            /// <value>statement context</value>
            public StatementContext StatementContext { get; private set; }

            /// <summary>Return destroy method. </summary>
            /// <value>method.</value>
            public EPStatementDestroyMethod DestroyMethod { get; set; }
        }
    }
}