///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.client.hook;
using com.espertech.esper.client.soda;
using com.espertech.esper.client.util;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.service.multimatch;
using com.espertech.esper.core.start;
using com.espertech.esper.epl.agg.rollup;
using com.espertech.esper.epl.annotation;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.declexpr;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.expression.ops;
using com.espertech.esper.epl.expression.subquery;
using com.espertech.esper.epl.expression.table;
using com.espertech.esper.epl.expression.visitor;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.spec.util;
using com.espertech.esper.events;
using com.espertech.esper.events.arr;
using com.espertech.esper.events.avro;
using com.espertech.esper.events.bean;
using com.espertech.esper.events.map;
using com.espertech.esper.filter;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.pattern;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Provides statement lifecycle services.
    /// </summary>
    public class StatementLifecycleSvcImpl : StatementLifecycleSvc
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Services context for statement lifecycle management.
        /// </summary>
        internal readonly EPServicesContext Services;

        /// <summary>
        /// Maps of statement id to descriptor.
        /// </summary>
        internal readonly IDictionary<int, EPStatementDesc> StmtIdToDescMap;

        /// <summary>
        /// Map of statement name to statement.
        /// </summary>
        internal readonly IDictionary<string, EPStatement> StmtNameToStmtMap;

        private readonly EPServiceProviderSPI _epServiceProvider;
        private readonly IReaderWriterLock _eventProcessingRWLock;

        private readonly IDictionary<string, int?> _stmtNameToIdMap;

        public event EventHandler<StatementLifecycleEvent> LifecycleEvent;

        private int _lastStatementId;

        private readonly ILockable _lock;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="epServiceProvider">is the engine instance to hand to statement-aware listeners</param>
        /// <param name="services">is engine services</param>
        public StatementLifecycleSvcImpl(EPServiceProvider epServiceProvider, EPServicesContext services)
        {
            Services = services;
            _lock = services.LockManager.CreateLock(MethodBase.GetCurrentMethod().DeclaringType);
            _epServiceProvider = (EPServiceProviderSPI)epServiceProvider;

            // lock for starting and stopping statements
            _eventProcessingRWLock = services.EventProcessingRWLock;

            StmtIdToDescMap = new Dictionary<int, EPStatementDesc>();
            StmtNameToStmtMap = new Dictionary<string, EPStatement>();
            _stmtNameToIdMap = new LinkedHashMap<string, int?>();
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
            get { return StmtNameToStmtMap; }
        }

        public EPStatement CreateAndStart(
            StatementSpecRaw statementSpec,
            string expression,
            bool isPattern,
            string optStatementName,
            object userObject,
            EPIsolationUnitServices isolationUnitServices,
            int? optionalStatementId,
            EPStatementObjectModel optionalModel)
        {
            using (_lock.Acquire())
            {
                var assignedStatementId = optionalStatementId;
                if (assignedStatementId == null)
                {
                    do
                    {
                        _lastStatementId++;
                        assignedStatementId = _lastStatementId;
                    } while (StmtIdToDescMap.ContainsKey(assignedStatementId.Value));
                }

                var desc = CreateStoppedAssignName(
                    statementSpec, expression, isPattern, optStatementName, assignedStatementId.Value, null, userObject,
                    isolationUnitServices, optionalModel);
                Start(assignedStatementId.Value, desc, true, false, false);
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
        /// <returns>
        /// started statement
        /// </returns>
        protected internal EPStatementDesc CreateStoppedAssignName(
            StatementSpecRaw statementSpec,
            string expression,
            bool isPattern,
            string optStatementName,
            int statementId,
            IDictionary<string, object> optAdditionalContext,
            object userObject,
            EPIsolationUnitServices isolationUnitServices,
            EPStatementObjectModel optionalModel)
        {
            using (_lock.Acquire())
            {
                var nameProvided = false;
                var statementName = "stmt_" + statementId;

                // compile annotations, can produce a null array
                var annotations = AnnotationUtil.CompileAnnotations(statementSpec.Annotations, Services.EngineImportService, expression);

                // find name annotation
                if (optStatementName == null)
                {
                    if (annotations != null && annotations.Length != 0)
                    {
                        foreach (var annotation in annotations)
                        {
                            if (annotation is NameAttribute)
                            {
                                var name = (NameAttribute)annotation;
                                if (name.Value != null)
                                {
                                    optStatementName = name.Value;
                                }
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

                if (statementSpec.FireAndForgetSpec != null)
                {
                    throw new EPStatementException("Provided EPL expression is an on-demand query expression (not a continuous query), please use the runtime executeQuery API instead", expression);
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
        /// <returns>
        /// stopped statement
        /// </returns>
        /// <exception cref="EPStatementException"></exception>
        protected internal EPStatementDesc CreateStopped(
            StatementSpecRaw statementSpec,
            Attribute[] annotations,
            string expression,
            bool isPattern,
            string statementName,
            bool nameProvided,
            int statementId,
            IDictionary<string, object> optAdditionalContext,
            object statementUserObject,
            EPIsolationUnitServices isolationUnitServices,
            bool isFailed,
            EPStatementObjectModel optionalModel)
        {
            using (_lock.Acquire())
            {
                EPStatementDesc statementDesc;
                EPStatementStartMethod startMethod;

                // Hint annotations are often driven by variables
                if (annotations != null)
                {
                    foreach (var annotation in annotations)
                    {
                        if (annotation is HintAttribute)
                        {
                            statementSpec.HasVariables = true;
                        }
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

                // Determine table access nodes
                var tableAccessNodes = DetermineTableAccessNodes(statementSpec.TableExpressions, visitor);

                if (statementSpec.TableExpressions != null)
                {
                    tableAccessNodes.AddAll(statementSpec.TableExpressions);
                }
                if (visitor.DeclaredExpressions != null)
                {
                    var tableAccessVisitor = new ExprNodeTableAccessVisitor(tableAccessNodes);
                    foreach (var declared in visitor.DeclaredExpressions)
                    {
                        declared.Body.Accept(tableAccessVisitor);
                    }
                }
                foreach (var subselectNode in visitor.Subselects)
                {
                    if (subselectNode.StatementSpecRaw.TableExpressions != null)
                    {
                        tableAccessNodes.AddAll(subselectNode.StatementSpecRaw.TableExpressions);
                    }
                }

                // Determine Subselects for compilation, and lambda-expression shortcut syntax for named windows
                var subselectNodes = visitor.Subselects;
                if (!visitor.ChainedExpressionsDot.IsEmpty())
                {
                    RewriteNamedWindowSubselect(visitor.ChainedExpressionsDot, subselectNodes, Services.NamedWindowMgmtService);
                }

                // compile foreign scripts
                ValidateScripts(expression, statementSpec.ScriptExpressions, statementSpec.ExpressionDeclDesc);

                // Determine statement type
                var statementType = StatementMetadataFactoryDefault.GetStatementType(statementSpec, isPattern);

                // Determine stateless statement
                var stateless = DetermineStatelessSelect(statementType, statementSpec, !subselectNodes.IsEmpty(), isPattern);

                // Determine table use
                var writesToTables = StatementLifecycleSvcUtil.IsWritesToTables(statementSpec, Services.TableService);

                // Make context
                var statementContext = Services.StatementContextFactory.MakeContext(statementId, statementName, expression, statementType, Services, optAdditionalContext, false, annotations, isolationUnitServices, stateless, statementSpec, subselectNodes, writesToTables, statementUserObject);

                StatementSpecCompiled compiledSpec;
                try
                {
                    compiledSpec = Compile(statementSpec, expression, statementContext, false, false, annotations, visitor.Subselects, visitor.DeclaredExpressions, tableAccessNodes, Services);
                }
                catch (Exception)
                {
                    HandleRemove(statementId, statementName);
                    throw;
                }

                // We keep a reference of the compiled spec as part of the statement context
                statementContext.StatementSpecCompiled = compiledSpec;

                // For insert-into streams, create a lock taken out as soon as an event is inserted
                // Makes the processing between chained statements more predictable.
                if (statementSpec.InsertIntoDesc != null || statementSpec.OnTriggerDesc is OnTriggerMergeDesc)
                {
                    string insertIntoStreamName;
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
                    var msecTimeout = Services.EngineSettingsService.EngineSettings.Threading.InsertIntoDispatchTimeout;
                    var locking = Services.EngineSettingsService.EngineSettings.Threading.InsertIntoDispatchLocking;
                    var latchFactoryFront = new InsertIntoLatchFactory(latchFactoryNameFront, stateless, msecTimeout, locking, Services.TimeSource);
                    var latchFactoryBack = new InsertIntoLatchFactory(latchFactoryNameBack, stateless, msecTimeout, locking, Services.TimeSource);
                    statementContext.EpStatementHandle.InsertIntoFrontLatchFactory = latchFactoryFront;
                    statementContext.EpStatementHandle.InsertIntoBackLatchFactory = latchFactoryBack;
                }

                // determine overall filters, assign the filter spec index to filter boolean expressions
                var needDedup = false;
                var streamAnalysis = StatementSpecCompiledAnalyzer.AnalyzeFilters(compiledSpec);
                FilterSpecCompiled[] filterSpecAll = streamAnalysis.Filters.ToArray();
                NamedWindowConsumerStreamSpec[] namedWindowConsumersAll = streamAnalysis.NamedWindowConsumers.ToArray();
                compiledSpec.FilterSpecsOverall = filterSpecAll;
                compiledSpec.NamedWindowConsumersAll = namedWindowConsumersAll;
                foreach (var filter in filterSpecAll)
                {
                    if (filter.Parameters.Length > 1)
                    {
                        needDedup = true;
                    }
                    StatementLifecycleSvcUtil.AssignFilterSpecIds(filter, filterSpecAll);
                    RegisterNonPropertyGetters(filter, statementName, Services.FilterNonPropertyRegisteryService);
                }

                MultiMatchHandler multiMatchHandler;
                var isSubselectPreeval = Services.EngineSettingsService.EngineSettings.Expression.IsSelfSubselectPreeval;
                if (!needDedup)
                {
                    // no dedup
                    if (subselectNodes.IsEmpty())
                    {
                        multiMatchHandler = Services.MultiMatchHandlerFactory.MakeNoDedupNoSubq();
                    }
                    else
                    {
                        if (isSubselectPreeval)
                        {
                            multiMatchHandler = Services.MultiMatchHandlerFactory.MakeNoDedupSubselectPreval();
                        }
                        else
                        {
                            multiMatchHandler = Services.MultiMatchHandlerFactory.MakeNoDedupSubselectPosteval();
                        }
                    }
                }
                else
                {
                    // with dedup
                    if (subselectNodes.IsEmpty())
                    {
                        multiMatchHandler = Services.MultiMatchHandlerFactory.MakeDedupNoSubq();
                    }
                    else
                    {
                        multiMatchHandler = Services.MultiMatchHandlerFactory.MakeDedupSubq(isSubselectPreeval);
                    }
                }
                statementContext.EpStatementHandle.MultiMatchHandler = multiMatchHandler;

                // In a join statements if the same event type or it's deep super types are used in the join more then once,
                // then this is a self-join and the statement handle must know to dispatch the results together
                var canSelfJoin = IsPotentialSelfJoin(compiledSpec) || needDedup;
                statementContext.EpStatementHandle.IsCanSelfJoin = canSelfJoin;

                // add statically typed event type references: those in the from clause; Dynamic (created) types collected by statement context and added on start
                Services.StatementEventTypeRefService.AddReferences(statementName, compiledSpec.EventTypeReferences);

                // add variable references
                Services.StatementVariableRefService.AddReferences(statementName, compiledSpec.VariableReferences, compiledSpec.TableNodes);

                // create metadata
                var statementMetadata =
                    Services.StatementMetadataFactory.Create(
                        new StatementMetadataFactoryContext(
                            statementName, statementId, statementContext, statementSpec, expression, isPattern,
                            optionalModel));

                using (_eventProcessingRWLock.AcquireWriteLock())
                {
                    try
                    {
                        // create statement - may fail for parser and simple validation errors
                        var preserveDispatchOrder = Services.EngineSettingsService.EngineSettings.Threading.IsListenerDispatchPreserveOrder
                                && !stateless;
                        var isSpinLocks = Services.EngineSettingsService.EngineSettings.Threading.ListenerDispatchLocking == ConfigurationEngineDefaults.ThreadingConfig.Locking.SPIN;
                        var blockingTimeout = Services.EngineSettingsService.EngineSettings.Threading.ListenerDispatchTimeout;
                        var timeLastStateChange = Services.SchedulingService.Time;
                        var statement = Services.EpStatementFactory.Make(
                            statementSpec.ExpressionNoAnnotations, isPattern,
                            Services.DispatchService, this, timeLastStateChange, preserveDispatchOrder, isSpinLocks, blockingTimeout,
                            Services.TimeSource, statementMetadata, statementUserObject, statementContext, isFailed, nameProvided);
                        statementContext.Statement = statement;

                        var isInsertInto = statementSpec.InsertIntoDesc != null;
                        var isDistinct = statementSpec.SelectClauseSpec.IsDistinct;
                        var isForClause = statementSpec.ForClauseSpec != null;
                        statementContext.StatementResultService.SetContext(statement, _epServiceProvider,
                                isInsertInto, isPattern, isDistinct, isForClause, statementContext.EpStatementHandle.MetricsHandle);

                        // create start method
                        startMethod = EPStatementStartMethodFactory.MakeStartMethod(compiledSpec);

                        statementDesc = new EPStatementDesc(statement, startMethod, statementContext);
                        StmtIdToDescMap.Put(statementId, statementDesc);
                        StmtNameToStmtMap.Put(statementName, statement);
                        _stmtNameToIdMap.Put(statementName, statementId);

                        DispatchStatementLifecycleEvent(new StatementLifecycleEvent(statement, StatementLifecycleEvent.LifecycleEventType.CREATE));
                    }
                    catch (Exception)
                    {
                        StmtIdToDescMap.Remove(statementId);
                        _stmtNameToIdMap.Remove(statementName);
                        StmtNameToStmtMap.Remove(statementName);
                        throw;
                    }
                }

                return statementDesc;
            }
        }

        private ISet<ExprTableAccessNode> DetermineTableAccessNodes(IEnumerable<ExprTableAccessNode> statementDirectTableAccess, ExprNodeSubselectDeclaredDotVisitor visitor)
        {
            ISet<ExprTableAccessNode> tableAccessNodes = new HashSet<ExprTableAccessNode>();
            if (statementDirectTableAccess != null)
            {
                tableAccessNodes.AddAll(statementDirectTableAccess);
            }
            // include all declared expression usages
            var tableAccessVisitor = new ExprNodeTableAccessVisitor(tableAccessNodes);
            foreach (var declared in visitor.DeclaredExpressions)
            {
                declared.Body.Accept(tableAccessVisitor);
            }
            // include all subqueries (and their declared expressions)
            // This is nested as declared expressions can have more subqueries, however all subqueries are in this list.
            foreach (var subselectNode in visitor.Subselects)
            {
                if (subselectNode.StatementSpecRaw.TableExpressions != null)
                {
                    tableAccessNodes.AddAll(subselectNode.StatementSpecRaw.TableExpressions);
                }
            }
            return tableAccessNodes;
        }

        // All scripts get compiled/verfied - to ensure they compile (and not just when they are referred to my an expression).
        private void ValidateScripts(string epl, IList<ExpressionScriptProvided> scripts, ExpressionDeclDesc expressionDeclDesc)
        {
            if (scripts == null)
            {
                return;
            }
            try
            {
                ISet<NameParameterCountKey> scriptsSet = new HashSet<NameParameterCountKey>();
                foreach (var script in scripts)
                {
                    ValidateScript(script);

                    var key = new NameParameterCountKey(script.Name, script.ParameterNames.Count);
                    if (scriptsSet.Contains(key))
                    {
                        throw new ExprValidationException(string.Format("Script name '{0}' has already been defined with the same number of parameters", script.Name));
                    }
                    scriptsSet.Add(key);
                }

                if (expressionDeclDesc != null)
                {
                    foreach (var declItem in expressionDeclDesc.Expressions)
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
            var dialect = script.OptionalDialect ?? Services.ConfigSnapshot.EngineDefaults.Scripts.DefaultDialect;
            if (dialect == null)
            {
                throw new ExprValidationException(
                    string.Format("Failed to determine script dialect for script '{0}', please configure a default dialect or provide a dialect explicitly", script.Name));
            }

            // NOTE: we have to do something here
            Services.ScriptingService.VerifyScript(dialect, script);
            //JSR223Helper.VerifyCompileScript(script, dialect);

            if (!script.ParameterNames.IsEmpty())
            {
                var parameters = new HashSet<string>();
                foreach (var param in script.ParameterNames)
                {
                    if (parameters.Contains(param))
                    {
                        throw new ExprValidationException(
                            string.Format(
                                "Invalid script parameters for script '{0}', parameter '{1}' is defined more then once",
                                script.Name, param));
                    }
                    parameters.Add(param);
                }
            }
        }

        private bool IsPotentialSelfJoin(StatementSpecCompiled spec)
        {
            // Include create-context as nested contexts that have pattern-initiated sub-contexts may change filters during execution
            if (spec.ContextDesc != null && spec.ContextDesc.ContextDetail is ContextDetailNested)
            {
                return true;
            }

            // if order-by is specified, ans since multiple output rows may produce, ensure dispatch
            if (spec.OrderByList.Length > 0)
            {
                return true;
            }

            if (spec.StreamSpecs.OfType<PatternStreamSpecCompiled>().Any())
            {
                return true;
            }

            // not a self join
            if ((spec.StreamSpecs.Length <= 1) && (spec.SubSelectExpressions.Length == 0))
            {
                return false;
            }

            // join - determine types joined
            IList<EventType> filteredTypes = new List<EventType>();

            // consider subqueryes
            var optSubselectTypes = PopulateSubqueryTypes(spec.SubSelectExpressions);

            var hasFilterStream = false;
            foreach (var streamSpec in spec.StreamSpecs)
            {
                if (streamSpec is FilterStreamSpecCompiled)
                {
                    var type = ((FilterStreamSpecCompiled)streamSpec).FilterSpec.FilterForEventType;
                    filteredTypes.Add(type);
                    hasFilterStream = true;
                }
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
            for (var i = 0; i < filteredTypes.Count; i++)
            {
                for (var j = i + 1; j < filteredTypes.Count; j++)
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
            if (!optSubselectTypes.IsEmpty())
            {
                foreach (var typeOne in filteredTypes)
                {
                    if (optSubselectTypes.Contains(typeOne))
                    {
                        return true;
                    }

                    if (typeOne.SuperTypes != null)
                    {
                        if (typeOne.SuperTypes.Any(optSubselectTypes.Contains))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private ISet<EventType> PopulateSubqueryTypes(ExprSubselectNode[] subSelectExpressions)
        {
            ISet<EventType> set = null;
            foreach (var subselect in subSelectExpressions)
            {
                foreach (var streamSpec in subselect.StatementSpecCompiled.StreamSpecs)
                {
                    if (streamSpec is FilterStreamSpecCompiled)
                    {
                        var type = ((FilterStreamSpecCompiled)streamSpec).FilterSpec.FilterForEventType;
                        if (set == null)
                        {
                            set = new HashSet<EventType>();
                        }
                        set.Add(type);
                    }
                    else if (streamSpec is PatternStreamSpecCompiled)
                    {
                        var evalNodeAnalysisResult = EvalNodeUtil.RecursiveAnalyzeChildNodes(((PatternStreamSpecCompiled)streamSpec).EvalFactoryNode);
                        var filterNodes = evalNodeAnalysisResult.FilterNodes;
                        foreach (var filterNode in filterNodes)
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
            if (set == null)
            {
                return Collections.GetEmptySet<EventType>();
            }
            return set;
        }

        public void Start(int statementId)
        {
            using (_lock.Acquire())
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug(".start Starting statement " + statementId);
                }

                // Acquire a lock for event processing as threads may be in the views used by the statement
                // and that could conflict with the destroy of views
                using (_eventProcessingRWLock.AcquireWriteLock())
                {
                    var desc = StmtIdToDescMap.Get(statementId);
                    if (desc == null)
                    {
                        throw new IllegalStateException("Cannot start statement, statement is in destroyed state");
                    }
                    StartInternal(statementId, desc, false, false, false);
                }
            }
        }

        /// <summary>
        /// Start the given statement.
        /// </summary>
        /// <param name="statementId">is the statement id</param>
        /// <param name="desc">is the cached statement info</param>
        /// <param name="isNewStatement">indicator whether the statement is new or a stop-restart statement</param>
        /// <param name="isRecoveringStatement">if the statement is recovering or new</param>
        /// <param name="isResilient">true if recovering a resilient stmt</param>
        public void Start(int statementId, EPStatementDesc desc, bool isNewStatement, bool isRecoveringStatement, bool isResilient)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug(".start Starting statement " + statementId + " from desc=" + desc);
            }

            // Acquire a lock for event processing as threads may be in the views used by the statement
            // and that could conflict with the destroy of views
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().QEngineManagementStmtCompileStart(
                    Services.EngineURI, statementId, desc.EpStatement.Name, desc.EpStatement.Text, Services.SchedulingService.Time);
            }

            using (_eventProcessingRWLock.AcquireWriteLock())
            {
                try
                {
                    StartInternal(statementId, desc, isNewStatement, isRecoveringStatement, isResilient);
                    if (InstrumentationHelper.ENABLED)
                    {
                        InstrumentationHelper.Get().QaEngineManagementStmtStarted(
                            Services.EngineURI, statementId, desc.EpStatement.Name, desc.EpStatement.Text, Services.SchedulingService.Time);
                    }

                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AEngineManagementStmtCompileStart(true, null); }
                }
                catch (Exception ex)
                {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AEngineManagementStmtCompileStart(false, ex.Message); }
                    throw;
                }
            }
        }

        private void StartInternal(int statementId, EPStatementDesc desc, bool isNewStatement, bool isRecoveringStatement, bool isResilient)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug(".startInternal Starting statement " + statementId + " from desc=" + desc);
            }

            if (desc.StartMethod == null)
            {
                throw new IllegalStateException("Statement start method not found for id " + statementId);
            }

            var statement = desc.EpStatement;
            if (statement.State == EPStatementState.STARTED)
            {
                Log.Debug(".startInternal - Statement already started");
                return;
            }

            EPStatementStartResult startResult;
            try
            {
                // start logically
                startResult = desc.StartMethod.Start(Services, desc.StatementContext, isNewStatement, isRecoveringStatement, isResilient);

                // start named window consumers
                Services.NamedWindowConsumerMgmtService.Start(desc.StatementContext.StatementName);
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
            var parentView = startResult.Viewable;
            desc.StopMethod = startResult.StopMethod;
            desc.DestroyMethod = startResult.DestroyMethod;
            statement.ParentView = parentView;
            var timeLastStateChange = Services.SchedulingService.Time;
            statement.SetCurrentState(EPStatementState.STARTED, timeLastStateChange);

            DispatchStatementLifecycleEvent(new StatementLifecycleEvent(statement, StatementLifecycleEvent.LifecycleEventType.STATECHANGE));
        }

        private void HandleRemove(int statementId, string statementName)
        {
            StmtIdToDescMap.Remove(statementId);
            _stmtNameToIdMap.Remove(statementName);
            StmtNameToStmtMap.Remove(statementName);
            Services.StatementEventTypeRefService.RemoveReferencesStatement(statementName);
            Services.StatementVariableRefService.RemoveReferencesStatement(statementName);
            Services.FilterNonPropertyRegisteryService.RemoveReferencesStatement(statementName);
            Services.NamedWindowConsumerMgmtService.RemoveReferences(statementName);
        }

        public void Stop(int statementId)
        {
            using (_lock.Acquire())
            {
                // Acquire a lock for event processing as threads may be in the views used by the statement
                // and that could conflict with the destroy of views
                try
                {
                    using (_eventProcessingRWLock.AcquireWriteLock())
                    {
                        var desc = StmtIdToDescMap.Get(statementId);
                        if (desc == null)
                        {
                            throw new IllegalStateException("Cannot stop statement, statement is in destroyed state");
                        }

                        var statement = desc.EpStatement;
                        var stopMethod = desc.StopMethod;
                        if (stopMethod == null)
                        {
                            throw new IllegalStateException("Stop method not found for statement " + statementId);
                        }

                        if (statement.State == EPStatementState.STOPPED)
                        {
                            Log.Debug(".startInternal - Statement already stopped");
                            return;
                        }

                        // stop named window consumers
                        Services.NamedWindowConsumerMgmtService.Stop(desc.StatementContext.StatementName);

                        // fire the statement stop
                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QEngineManagementStmtStop(EPStatementState.STOPPED, Services.EngineURI, statementId, statement.Name, statement.Text, Services.SchedulingService.Time); }

                        desc.StatementContext.StatementStopService.FireStatementStopped();

                        // invoke start-provided stop method
                        stopMethod.Stop();
                        statement.ParentView = null;
                        desc.StopMethod = null;

                        var timeLastStateChange = Services.SchedulingService.Time;
                        statement.SetCurrentState(EPStatementState.STOPPED, timeLastStateChange);

                        ((EPRuntimeSPI)_epServiceProvider.EPRuntime).ClearCaches();

                        DispatchStatementLifecycleEvent(new StatementLifecycleEvent(statement, StatementLifecycleEvent.LifecycleEventType.STATECHANGE));
                    }
                }
                finally
                {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AEngineManagementStmtStop(); }
                }
            }
        }

        public void Dispose(int statementId)
        {
            using (_lock.Acquire())
            {
                // Acquire a lock for event processing as threads may be in the views used by the statement
                // and that could conflict with the destroy of views
                using (_eventProcessingRWLock.AcquireWriteLock())
                {
                    var desc = StmtIdToDescMap.Get(statementId);
                    if (desc == null)
                    {
                        Log.Debug(".destroy - Statement already destroyed");
                        return;
                    }
                    DestroyInternal(desc);
                }
            }
        }

        public EPStatement GetStatementByName(string name)
        {
            using (_lock.Acquire())
            {
                return StmtNameToStmtMap.Get(name);
            }
        }

        public StatementSpecCompiled GetStatementSpec(int statementId)
        {
            using (_lock.Acquire())
            {
                var desc = StmtIdToDescMap.Get(statementId);
                if (desc != null)
                {
                    return desc.StartMethod.StatementSpec;
                }
                return null;
            }
        }

        /// <summary>
        /// Returns the statement given a statement id.
        /// </summary>
        /// <param name="statementId">is the statement id</param>
        /// <returns>statement</returns>
        public EPStatementSPI GetStatementById(int statementId)
        {
            var statementDesc = StmtIdToDescMap.Get(statementId);
            if (statementDesc == null)
            {
                Log.Warn("Could not locate statement descriptor for statement id '" + statementId + "'");
                return null;
            }
            return statementDesc.EpStatement;
        }

        public StatementContext GetStatementContextById(int statementId)
        {
            var statementDesc = StmtIdToDescMap.Get(statementId);
            if (statementDesc == null)
            {
                return null;
            }
            return statementDesc.EpStatement.StatementContext;
        }

        public string[] StatementNames
        {
            get
            {
                using (_lock.Acquire())
                {
                    var statements = new string[StmtNameToStmtMap.Count];
                    var count = 0;
                    foreach (string key in StmtNameToStmtMap.Keys)
                    {
                        statements[count++] = key;
                    }
                    return statements;
                }
            }
        }

        public void StartAllStatements()
        {
            using (_lock.Acquire())
            {
                int[] statementIds = StatementIds;
                for (var i = 0; i < statementIds.Length; i++)
                {
                    EPStatement statement = StmtIdToDescMap.Get(statementIds[i]).EpStatement;
                    if (statement.State == EPStatementState.STOPPED)
                    {
                        Start(statementIds[i]);
                    }
                }
            }
        }

        public void StopAllStatements()
        {
            using (_lock.Acquire())
            {
                int[] statementIds = StatementIds;
                for (var i = 0; i < statementIds.Length; i++)
                {
                    EPStatement statement = StmtIdToDescMap.Get(statementIds[i]).EpStatement;
                    if (statement.State == EPStatementState.STARTED)
                    {
                        Stop(statementIds[i]);
                    }
                }
            }
        }

        public void DestroyAllStatements()
        {
            using (_lock.Acquire())
            {
                // Acquire a lock for event processing as threads may be in the views used by the statement
                // and that could conflict with the destroy of views
                using (_eventProcessingRWLock.AcquireWriteLock())
                {
                    int[] statementIds = StatementIds;
                    foreach (var statementId in statementIds)
                    {
                        var desc = StmtIdToDescMap.Get(statementId);
                        if (desc == null)
                        {
                            continue;
                        }

                        try
                        {
                            DestroyInternal(desc);
                        }
                        catch (Exception ex)
                        {
                            Services.ExceptionHandlingService.HandleException(
                                ex, desc.EpStatement.Name, desc.EpStatement.Text, ExceptionHandlerExceptionType.STOP, null);
                        }
                    }
                }
            }
        }

        private int[] StatementIds
        {
            get
            {
                var statementIds = new int[_stmtNameToIdMap.Count];
                var count = 0;
                foreach (int id in _stmtNameToIdMap.Values)
                {
                    statementIds[count++] = id;
                }
                return statementIds;
            }
        }

        private string GetUniqueStatementName(string statementName, int statementId)
        {
            string finalStatementName;

            if (_stmtNameToIdMap.ContainsKey(statementName))
            {
                var count = 0;
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

        public string GetStatementNameById(int statementId)
        {
            var desc = StmtIdToDescMap.Get(statementId);
            if (desc != null)
            {
                return desc.EpStatement.Name;
            }
            return null;
        }

        public void UpdatedListeners(EPStatement statement, EPStatementListenerSet listeners, bool isRecovery)
        {
            Log.Debug(".updatedListeners No action for base implementation");
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
        /// <param name="tableAccessNodes">The table access nodes.</param>
        /// <param name="servicesContext">The services context.</param>
        /// <returns>
        /// compiled statement
        /// </returns>
        /// <throws>EPStatementException if the statement cannot be compiled</throws>
        internal static StatementSpecCompiled Compile(
            StatementSpecRaw spec,
            string eplStatement,
            StatementContext statementContext,
            bool isSubquery,
            bool isOnDemandQuery,
            Attribute[] annotations,
            IList<ExprSubselectNode> subselectNodes,
            IList<ExprDeclaredNode> declaredNodes,
            ICollection<ExprTableAccessNode> tableAccessNodes,
            EPServicesContext servicesContext)
        {
            IList<StreamSpecCompiled> compiledStreams;
            ISet<string> eventTypeReferences = new HashSet<string>();

            // If not using a join and not specifying a data window, make the where-clause, if present, the filter of the stream
            // if selecting using filter spec, and not subquery in where clause
            if ((spec.StreamSpecs.Count == 1) &&
                (spec.StreamSpecs[0] is FilterStreamSpecRaw) &&
                (spec.StreamSpecs[0].ViewSpecs.IsEmpty()) &&
                (spec.FilterRootNode != null) &&
                (spec.OnTriggerDesc == null) &&
                (!isSubquery) &&
                (!isOnDemandQuery) &&
                (tableAccessNodes == null || tableAccessNodes.IsEmpty()))
            {
                var whereClause = spec.FilterRootNode;

                var dotVisitor = new ExprNodeSubselectDeclaredDotVisitor();
                whereClause.Accept(dotVisitor);

                var disqualified = dotVisitor.Subselects.Count > 0 || HintEnum.DISABLE_WHEREEXPR_MOVETO_FILTER.GetHint(annotations) != null;

                if (!disqualified)
                {
                    var viewResourceVisitor = new ExprNodeViewResourceVisitor();
                    whereClause.Accept(viewResourceVisitor);
                    disqualified = viewResourceVisitor.ExprNodes.Count > 0;
                }

                if (!disqualified)
                {
                    // If an alias is provided, find all properties to ensure the alias gets removed
                    string alias = spec.StreamSpecs[0].OptionalStreamName;
                    if (alias != null)
                    {
                        var v = new ExprNodeIdentifierCollectVisitor();
                        whereClause.Accept(v);
                        foreach (var node in v.ExprProperties)
                        {
                            if (node.StreamOrPropertyName != null && (node.StreamOrPropertyName == alias))
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
            foreach (var subselectNode in visitor.Subselects)
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
                    servicesContext.Container,
                    spec.GroupByExpressions,
                    spec.SelectClauseSpec, 
                    spec.HavingExprRootNode, 
                    spec.OrderByList,
                    visitor);

                var subselects = visitor.Subselects;
                if (!visitor.ChainedExpressionsDot.IsEmpty())
                {
                    RewriteNamedWindowSubselect(visitor.ChainedExpressionsDot, subselects, statementContext.NamedWindowMgmtService);
                }
            }
            catch (ExprValidationException ex)
            {
                throw new EPStatementException(ex.Message, eplStatement);
            }

            if (isSubquery && !visitor.Subselects.IsEmpty())
            {
                throw new EPStatementException("Invalid nested subquery, subquery-within-subquery is not supported", eplStatement);
            }
            if (isOnDemandQuery && !visitor.Subselects.IsEmpty())
            {
                throw new EPStatementException("Subqueries are not a supported feature of on-demand queries", eplStatement);
            }
            foreach (var subselectNode in visitor.Subselects)
            {
                if (!subselectNodes.Contains(subselectNode))
                {
                    subselectNodes.Add(subselectNode);
                }
            }

            // Compile subselects found
            var subselectNumber = 0;
            foreach (var subselect in subselectNodes)
            {
                var raw = subselect.StatementSpecRaw;
                StatementSpecCompiled compiled = Compile(
                    raw, eplStatement, statementContext, true, isOnDemandQuery, new Attribute[0],
                    Collections.GetEmptyList<ExprSubselectNode>(),
                    Collections.GetEmptyList<ExprDeclaredNode>(), raw.TableExpressions, servicesContext);
                subselectNumber++;
                subselect.SetStatementSpecCompiled(compiled, subselectNumber);
            }

            // compile each stream used
            try
            {
                compiledStreams = new List<StreamSpecCompiled>(spec.StreamSpecs.Count);
                var streamNum = 0;
                foreach (var rawSpec in spec.StreamSpecs)
                {
                    streamNum++;
                    var compiled = rawSpec.Compile(
                        statementContext, eventTypeReferences, spec.InsertIntoDesc != null,
                        Collections.SingletonList(streamNum), spec.StreamSpecs.Count > 1, false, spec.OnTriggerDesc != null,
                        rawSpec.OptionalStreamName);
                    compiledStreams.Add(compiled);
                }
            }
            catch (ExprValidationException ex)
            {
                Log.Info("Failed to compile statement: " + ex.Message, ex);
                if (ex.Message == null)
                {
                    throw new EPStatementException("Unexpected exception compiling statement, please consult the log file and report the exception", eplStatement, ex);
                }
                else
                {
                    throw new EPStatementException(ex.Message, eplStatement, ex);
                }
            }
            catch (Exception ex)
            {
                const string text = "Unexpected error compiling statement";
                Log.Error(text, ex);
                throw new EPStatementException(text + ": " + ex.GetType().Name + ":" + ex.Message, eplStatement, ex);
            }

            // for create window statements, we switch the filter to a new event type
            if (spec.CreateWindowDesc != null)
            {
                try
                {
                    StreamSpecCompiled createWindowTypeSpec = compiledStreams[0];
                    EventType selectFromType;
                    string selectFromTypeName;
                    if (createWindowTypeSpec is FilterStreamSpecCompiled)
                    {
                        var filterStreamSpec = (FilterStreamSpecCompiled)createWindowTypeSpec;
                        selectFromType = filterStreamSpec.FilterSpec.FilterForEventType;
                        selectFromTypeName = filterStreamSpec.FilterSpec.FilterForEventTypeName;

                        if (spec.CreateWindowDesc.IsInsert || spec.CreateWindowDesc.InsertFilter != null)
                        {
                            throw new EPStatementException(string.Format("A named window by name '{0}' could not be located, use the insert-keyword with an existing named window", selectFromTypeName), eplStatement);
                        }
                    }
                    else
                    {
                        var consumerStreamSpec = (NamedWindowConsumerStreamSpec)createWindowTypeSpec;
                        selectFromType = statementContext.EventAdapterService.GetEventTypeByName(consumerStreamSpec.WindowName);
                        selectFromTypeName = consumerStreamSpec.WindowName;

                        if (spec.CreateWindowDesc.InsertFilter != null)
                        {
                            var insertIntoFilter = spec.CreateWindowDesc.InsertFilter;
                            var checkMinimal = ExprNodeUtility.IsMinimalExpression(insertIntoFilter);
                            if (checkMinimal != null)
                            {
                                throw new ExprValidationException("Create window where-clause may not have " + checkMinimal);
                            }
                            StreamTypeService streamTypeService = new StreamTypeServiceImpl(selectFromType, selectFromTypeName, true, statementContext.EngineURI);
                            var evaluatorContextStmt = new ExprEvaluatorContextStatement(statementContext, false);
                            var validationContext = new ExprValidationContext(
                                statementContext.Container,
                                streamTypeService,
                                statementContext.EngineImportService,
                                statementContext.StatementExtensionServicesContext, null,
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
                    var newFilter = HandleCreateWindow(selectFromType, selectFromTypeName, spec.CreateWindowDesc.Columns, spec, eplStatement, statementContext, servicesContext);
                    eventTypeReferences.Add(((EventTypeSPI)newFilter.First.FilterForEventType).Metadata.PrimaryName);

                    // view must be non-empty list
                    if (spec.CreateWindowDesc.ViewSpecs.IsEmpty())
                    {
                        throw new ExprValidationException(NamedWindowMgmtServiceConstants.ERROR_MSG_DATAWINDOWS);
                    }

                    // use the filter specification of the newly created event type and the views for the named window
                    compiledStreams.Clear();
                    var views = spec.CreateWindowDesc.ViewSpecs.ToArray();
                    compiledStreams.Add(new FilterStreamSpecCompiled(newFilter.First, views, null, spec.CreateWindowDesc.StreamSpecOptions));
                    spec.SelectClauseSpec = newFilter.Second;
                }
                catch (ExprValidationException e)
                {
                    throw new EPStatementException(e.Message, eplStatement);
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
                    OrderByItem.ToArray(spec.OrderByList),
                    ExprSubselectNode.ToArray(subselectNodes),
                    ExprNodeUtility.ToArray(declaredNodes),
                    spec.ScriptExpressions.MaterializeArray(),
                    spec.ReferencedVariables,
                    spec.RowLimitSpec,
                    CollectionUtil.ToArray(eventTypeReferences),
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
                    tableAccessNodes.ToArrayOrNull());
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
            foreach (var expr in expressions.Where(e => e != null))
            {
                expr.Accept(visitor);
            }

            return !visitor.HasAggregation && !visitor.HasPreviousPrior && !visitor.HasSubselect;
        }

        private static void RewriteNamedWindowSubselect(
            IList<ExprDotNode> chainedExpressionsDot,
            IList<ExprSubselectNode> subselects,
            NamedWindowMgmtService service)
        {
            foreach (var dotNode in chainedExpressionsDot)
            {
                string proposedWindow = dotNode.ChainSpec[0].Name;
                if (!service.IsNamedWindow(proposedWindow))
                {
                    continue;
                }

                // build spec for subselect
                var raw = new StatementSpecRaw(SelectClauseStreamSelectorEnum.ISTREAM_ONLY);
                var filter = new FilterSpecRaw(proposedWindow, Collections.GetEmptyList<ExprNode>(), null);
                raw.StreamSpecs.Add(new FilterStreamSpecRaw(filter, ViewSpec.EMPTY_VIEWSPEC_ARRAY, proposedWindow, StreamSpecOptions.DEFAULT));

                var firstChain = dotNode.ChainSpec.DeleteAt(0);
                if (!firstChain.Parameters.IsEmpty())
                {
                    if (firstChain.Parameters.Count == 1)
                    {
                        raw.FilterExprRootNode = firstChain.Parameters[0];
                    }
                    else
                    {
                        ExprAndNode andNode = new ExprAndNodeImpl();
                        foreach (var node in firstChain.Parameters)
                        {
                            andNode.AddChildNode(node);
                        }
                        raw.FilterExprRootNode = andNode;
                    }
                }

                // activate subselect
                ExprSubselectNode subselect = new ExprSubselectRowNode(raw);
                subselects.Add(subselect);
                dotNode.ChildNodes = new[] { subselect };
            }
        }

        /// <summary>
        /// Compile a select clause allowing subselects.
        /// </summary>
        /// <param name="spec">to compile</param>
        /// <returns>select clause compiled</returns>
        /// <throws>ExprValidationException when validation fails</throws>
        public static SelectClauseSpecCompiled CompileSelectAllowSubselect(SelectClauseSpecRaw spec)
        {
            // Look for expressions with sub-selects in select expression list and filter expression
            // Recursively compile the statement within the statement.
            var visitor = new ExprNodeSubselectDeclaredDotVisitor();
            IList<SelectClauseElementCompiled> selectElements = new List<SelectClauseElementCompiled>();
            foreach (var raw in spec.SelectExprList)
            {
                if (raw is SelectClauseExprRawSpec)
                {
                    var rawExpr = (SelectClauseExprRawSpec)raw;
                    rawExpr.SelectExpression.Accept(visitor);
                    selectElements.Add(
                        new SelectClauseExprCompiledSpec(
                            rawExpr.SelectExpression, rawExpr.OptionalAsName, rawExpr.OptionalAsName, rawExpr.IsEvents));
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
                    throw new IllegalStateException("Unexpected select clause element class : " + raw.GetType().Name);
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
            string selectFromTypeName,
            IList<ColumnDesc> columns,
            StatementSpecRaw spec,
            string eplStatement,
            StatementContext statementContext,
            EPServicesContext servicesContext)
        {
            var typeName = spec.CreateWindowDesc.WindowName;
            EventType targetType;

            // determine that the window name is not already in use as an event type name
            var existingType = servicesContext.EventAdapterService.GetEventTypeByName(typeName);
            if (existingType != null && ((EventTypeSPI)existingType).Metadata.TypeClass != TypeClass.NAMED_WINDOW)
            {
                throw new ExprValidationException("Error starting statement: An event type or schema by name '" + typeName + "' already exists");
            }

            // Validate the select expressions which consists of properties only
            var evaluatorContextStmt = new ExprEvaluatorContextStatement(statementContext, false);
            var select = CompileLimitedSelect(
                statementContext.Container,
                spec.SelectClauseSpec, 
                eplStatement,
                selectFromType,
                selectFromTypeName,
                statementContext.EngineURI,
                evaluatorContextStmt, 
                statementContext.EngineImportService, 
                statementContext.EventAdapterService, 
                statementContext.StatementName, 
                statementContext.StatementId, 
                statementContext.Annotations, 
                statementContext.StatementExtensionServicesContext);

            // Create Map or Wrapper event type from the select clause of the window.
            // If no columns selected, simply create a wrapper type
            // Build a list of properties
            var newSelectClauseSpecRaw = new SelectClauseSpecRaw();
            IDictionary<string, object> properties;
            var hasProperties = false;
            if ((columns != null) && (!columns.IsEmpty()))
            {
                properties = EventTypeUtility.BuildType(
                    columns, statementContext.EventAdapterService, null,
                    statementContext.EngineImportService);
                hasProperties = true;
            }
            else
            {
                properties = new LinkedHashMap<string, object>();
                foreach (var selectElement in select)
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
                    newSelectClauseSpecRaw.Add(
                        new SelectClauseExprRawSpec(new ExprIdentNodeImpl(selectElement.AssignedName), null, false));
                    hasProperties = true;
                }
            }

            // Create Map or Wrapper event type from the select clause of the window.
            // If no columns selected, simply create a wrapper type
            var isOnlyWildcard = spec.SelectClauseSpec.IsOnlyWildcard;
            var isWildcard = spec.SelectClauseSpec.IsUsingWildcard;
            if (statementContext.ValueAddEventService.IsRevisionTypeName(selectFromTypeName))
            {
                targetType = statementContext.ValueAddEventService.CreateRevisionType(
                    typeName, selectFromTypeName, statementContext.StatementStopService,
                    statementContext.EventAdapterService, servicesContext.EventTypeIdGenerator);
            }
            else if (isWildcard && !isOnlyWildcard)
            {
                targetType = statementContext.EventAdapterService.AddWrapperType(
                    typeName, selectFromType, properties, true, false);
            }
            else
            {
                // Some columns selected, use the types of the columns
                if (hasProperties && !isOnlyWildcard)
                {
                    var compiledProperties = EventTypeUtility.CompileMapTypeProperties(
                        properties, statementContext.EventAdapterService);
                    var representation = EventRepresentationUtil.GetRepresentation(
                        statementContext.Annotations, servicesContext.ConfigSnapshot, AssignedType.NONE);
                    if (representation == EventUnderlyingType.MAP)
                    {
                        targetType = statementContext.EventAdapterService.AddNestableMapType(
                            typeName, compiledProperties, null, false, false, false, true, false);
                    }
                    else if (representation == EventUnderlyingType.OBJECTARRAY)
                    {
                        targetType = statementContext.EventAdapterService.AddNestableObjectArrayType(
                            typeName, compiledProperties, null, false, false, false, true, false, false, null);
                    }
                    else if (representation == EventUnderlyingType.AVRO)
                    {
                        targetType = statementContext.EventAdapterService.AddAvroType(
                            typeName, compiledProperties, false, false, false, true, false, statementContext.Annotations,
                            null, statementContext.StatementName, statementContext.EngineURI);
                    }
                    else
                    {
                        throw new IllegalStateException("Unrecognized representation " + representation);
                    }
                }
                else
                {
                    // No columns selected, no wildcard, use the type as is or as a wrapped type
                    if (selectFromType is ObjectArrayEventType)
                    {
                        var objectArrayEventType = (ObjectArrayEventType)selectFromType;
                        targetType = statementContext.EventAdapterService.AddNestableObjectArrayType(
                            typeName, objectArrayEventType.Types, null, false, false, false, true, false, false, null);
                    }
                    else if (selectFromType is AvroSchemaEventType)
                    {
                        var avroSchemaEventType = (AvroSchemaEventType) selectFromType;
                        var avro = new ConfigurationEventTypeAvro();
                        avro.SetAvroSchema(avroSchemaEventType.Schema);
                        targetType = statementContext.EventAdapterService.AddAvroType(
                            typeName, avro, false, false, false, true, false);
                    }
                    else if (selectFromType is MapEventType)
                    {
                        var mapType = (MapEventType)selectFromType;
                        targetType = statementContext.EventAdapterService.AddNestableMapType(
                            typeName, mapType.Types, null, false, false, false, true, false);
                    }
                    else if (selectFromType is BeanEventType)
                    {
                        var beanType = (BeanEventType)selectFromType;
                        targetType = statementContext.EventAdapterService.AddBeanTypeByName(
                            typeName, beanType.UnderlyingType, true);
                    }
                    else
                    {
                        IDictionary<string, object> addOnTypes = new Dictionary<string, object>();
                        targetType = statementContext.EventAdapterService.AddWrapperType(
                            typeName, selectFromType, addOnTypes, true, false);
                    }
                }
            }

            var filter = new FilterSpecCompiled(targetType, typeName, new IList<FilterSpecParam>[0], null);
            return new Pair<FilterSpecCompiled, SelectClauseSpecRaw>(filter, newSelectClauseSpecRaw);
        }

        private static IList<NamedWindowSelectedProps> CompileLimitedSelect(
            IContainer container,
            SelectClauseSpecRaw spec,
            string eplStatement,
            EventType singleType,
            string selectFromTypeName,
            string engineURI,
            ExprEvaluatorContext exprEvaluatorContext,
            EngineImportService engineImportService,
            EventAdapterService eventAdapterService,
            string statementName,
            int statementId,
            Attribute[] annotations,
            StatementExtensionSvcContext statementExtensionSvcContext)
        {
            var selectProps = new List<NamedWindowSelectedProps>();
            var streams = new StreamTypeServiceImpl(
                new[] { singleType },
                new[] { "stream_0" },
                new[] { false },
                engineURI, false);

            var validationContext = new ExprValidationContext(
                container,
                streams, engineImportService,
                statementExtensionSvcContext,
                null, null, null, null,
                exprEvaluatorContext,
                eventAdapterService,
                statementName, statementId, annotations,
                null, 
                null, 
                false, false, false, false, null, false);

            foreach (var exprSpec in spec.SelectExprList.OfType<SelectClauseExprRawSpec>())
            {
                ExprNode validatedExpression;
                try
                {
                    validatedExpression = ExprNodeUtility.GetValidatedSubtree(
                        ExprNodeOrigin.SELECT, exprSpec.SelectExpression, validationContext);
                }
                catch (ExprValidationException e)
                {
                    throw new EPStatementException(e.Message, e, eplStatement);
                }

                // determine an element name if none assigned
                var asName = exprSpec.OptionalAsName;
                if (asName == null)
                {
                    asName = ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(validatedExpression);
                }

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

                var validatedElement = new NamedWindowSelectedProps(
                    validatedExpression.ExprEvaluator.ReturnType, asName, fragmentType);
                selectProps.Add(validatedElement);
            }

            return selectProps;
        }

        private static void RegisterNonPropertyGetters(
            FilterSpecCompiled filter,
            string statementName,
            FilterNonPropertyRegisteryService filterNonPropertyRegisteryService)
        {
            foreach (var row in filter.Parameters)
            {
                foreach (var col in row)
                {
                    if (col.Lookupable.IsNonPropertyGetter)
                    {
                        filterNonPropertyRegisteryService.RegisterNonPropertyExpression(statementName, filter.FilterForEventType, col.Lookupable);
                    }
                }
            }
        }

        internal void DestroyInternal(EPStatementDesc desc)
        {
            try
            {
                // fire the statement stop
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QEngineManagementStmtStop(EPStatementState.DESTROYED, Services.EngineURI, desc.EpStatement.StatementId, desc.EpStatement.Name, desc.EpStatement.Text, Services.SchedulingService.Time); }

                // remove referenced event types
                Services.StatementEventTypeRefService.RemoveReferencesStatement(desc.EpStatement.Name);

                // remove the named window lock
                Services.NamedWindowMgmtService.RemoveNamedWindowLock(desc.EpStatement.Name);

                // remove any pattern subexpression counts
                if (Services.PatternSubexpressionPoolSvc != null)
                {
                    Services.PatternSubexpressionPoolSvc.RemoveStatement(desc.EpStatement.Name);
                }

                // remove any match-recognize counts
                if (Services.MatchRecognizeStatePoolEngineSvc != null)
                {
                    Services.MatchRecognizeStatePoolEngineSvc.RemoveStatement(desc.EpStatement.Name);
                }

                var statement = desc.EpStatement;
                if (statement.State == EPStatementState.STARTED)
                {
                    // fire the statement stop
                    desc.StatementContext.StatementStopService.FireStatementStopped();

                    // invoke start-provided stop method
                    var stopMethod = desc.StopMethod;
                    statement.ParentView = null;
                    stopMethod.Stop();
                }

                // call any destroy method that is registered for the statement: this destroy context partitions but not metadata
                if (desc.DestroyMethod != null)
                {
                    desc.DestroyMethod.Destroy();
                }

                // remove referenced non-property getters (after stop to allow lookup of these during stop)
                Services.FilterNonPropertyRegisteryService.RemoveReferencesStatement(desc.EpStatement.Name);

                // remove referenced variables (after stop to allow lookup of these during stop)
                Services.StatementVariableRefService.RemoveReferencesStatement(desc.EpStatement.Name);

                // destroy named window consumers
                Services.NamedWindowConsumerMgmtService.Destroy(desc.StatementContext.StatementName);

                var timeLastStateChange = Services.SchedulingService.Time;
                statement.SetCurrentState(EPStatementState.DESTROYED, timeLastStateChange);

                StmtNameToStmtMap.Remove(statement.Name);
                _stmtNameToIdMap.Remove(statement.Name);
                StmtIdToDescMap.Remove(statement.StatementId);

                if (!_epServiceProvider.IsDestroyed)
                {
                    ((EPRuntimeSPI)_epServiceProvider.EPRuntime).ClearCaches();
                }

                DispatchStatementLifecycleEvent(new StatementLifecycleEvent(statement, StatementLifecycleEvent.LifecycleEventType.STATECHANGE));
            }
            finally
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AEngineManagementStmtStop(); }
            }
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

            /// <summary>
            /// Returns the statement.
            /// </summary>
            /// <value>statement.</value>
            public EPStatementSPI EpStatement { get; private set; }

            /// <summary>
            /// Returns the start method.
            /// </summary>
            /// <value>start method</value>
            public EPStatementStartMethod StartMethod { get; private set; }

            /// <summary>
            /// Returns the stop method.
            /// </summary>
            /// <value>stop method</value>
            public EPStatementStopMethod StopMethod { get; set; }

            /// <summary>
            /// Returns the statement context.
            /// </summary>
            /// <value>statement context</value>
            public StatementContext StatementContext { get; private set; }

            /// <summary>
            /// Gets or sets method to call when destroyed.
            /// </summary>
            /// <value>method</value>
            public EPStatementDestroyMethod DestroyMethod { get; set; }
        }
    }
} // end of namespace
