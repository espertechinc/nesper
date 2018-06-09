///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.declexpr;
using com.espertech.esper.epl.expression.table;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.variable;
using com.espertech.esper.pattern;
using com.espertech.esper.plugin;
using com.espertech.esper.schedule;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.spec
{
    /// <summary>
    /// Context for mapping a SODA statement to a statement specification, or multiple for subqueries,
    /// and obtaining certain optimization information from a statement.
    /// </summary>
    public class StatementSpecMapContext
    {
        private IDictionary<string, ExpressionDeclItem> _expressionDeclarations;
        private IDictionary<string, ExpressionScriptProvided> _scripts;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="engineImportService">engine imports</param>
        /// <param name="variableService">variable names</param>
        /// <param name="configuration">the configuration</param>
        /// <param name="schedulingService">The scheduling service.</param>
        /// <param name="engineURI">The engine URI.</param>
        /// <param name="patternNodeFactory">The pattern node factory.</param>
        /// <param name="namedWindowMgmtService">The named window service.</param>
        /// <param name="contextManagementService">The context management service.</param>
        /// <param name="exprDeclaredService">The expr declared service.</param>
        /// <param name="contextDescriptor">optional context description</param>
        /// <param name="tableService">The table service.</param>
        public StatementSpecMapContext(
            IContainer container,
            EngineImportService engineImportService,
            VariableService variableService, 
            ConfigurationInformation configuration,
            SchedulingService schedulingService, 
            string engineURI, 
            PatternNodeFactory patternNodeFactory,
            NamedWindowMgmtService namedWindowMgmtService, 
            ContextManagementService contextManagementService,
            ExprDeclaredService exprDeclaredService, 
            ContextDescriptor contextDescriptor,
            TableService tableService)
        {
            Container = container;
            PlugInAggregations = new LazyAllocatedMap<ConfigurationPlugInAggregationMultiFunction, PlugInAggregationMultiFunctionFactory>();
            TableExpressions = new HashSet<ExprTableAccessNode>();
            EngineImportService = engineImportService;
            VariableService = variableService;
            Configuration = configuration;
            VariableNames = new HashSet<string>();
            SchedulingService = schedulingService;
            EngineURI = engineURI;
            PatternNodeFactory = patternNodeFactory;
            NamedWindowMgmtService = namedWindowMgmtService;
            ContextManagementService = contextManagementService;
            ExprDeclaredService = exprDeclaredService;
            ContextDescriptor = contextDescriptor;
            TableService = tableService;
        }

        /// <summary>
        /// Returns the engine import service.
        /// </summary>
        /// <value>service</value>
        public EngineImportService EngineImportService { get; private set; }

        /// <summary>
        /// Returns the variable service.
        /// </summary>
        /// <value>service</value>
        public VariableService VariableService { get; private set; }

        /// <summary>
        /// Returns true if a statement has variables.
        /// </summary>
        /// <value>true for variables found</value>
        public bool HasVariables { get; set; }

        /// <summary>
        /// Returns the configuration.
        /// </summary>
        /// <value>config</value>
        public ConfigurationInformation Configuration { get; private set; }

        /// <summary>
        /// Returns variables.
        /// </summary>
        /// <value>variables</value>
        public ISet<string> VariableNames { get; private set; }

        public SchedulingService SchedulingService { get; private set; }

        public string EngineURI { get; private set; }

        public PatternNodeFactory PatternNodeFactory { get; private set; }

        public NamedWindowMgmtService NamedWindowMgmtService { get; private set; }

        public IDictionary<string, ExpressionDeclItem> ExpressionDeclarations
        {
            get
            {
                if (_expressionDeclarations == null)
                {
                    return Collections.GetEmptyMap<string, ExpressionDeclItem>();
                }
                return _expressionDeclarations;
            }
        }

        public void AddExpressionDeclarations(ExpressionDeclItem item)
        {
            if (_expressionDeclarations == null)
            {
                _expressionDeclarations = new Dictionary<string, ExpressionDeclItem>();
            }
            _expressionDeclarations.Put(item.Name, item);
        }

        public IDictionary<string, ExpressionScriptProvided> Scripts
        {
            get
            {
                if (_scripts == null)
                {
                    return Collections.GetEmptyMap<string, ExpressionScriptProvided>();
                }
                return _scripts;
            }
        }

        public void AddScript(ExpressionScriptProvided item)
        {
            if (_scripts == null)
            {
                _scripts = new Dictionary<string, ExpressionScriptProvided>();
            }
            _scripts.Put(item.Name, item);
        }

        public ContextManagementService ContextManagementService { get; private set; }

        public string ContextName { get; set; }

        public ExprDeclaredService ExprDeclaredService { get; private set; }

        public LazyAllocatedMap<ConfigurationPlugInAggregationMultiFunction, PlugInAggregationMultiFunctionFactory> PlugInAggregations { get; private set; }

        public ContextDescriptor ContextDescriptor { get; private set; }

        public TableService TableService { get; private set; }

        public ISet<ExprTableAccessNode> TableExpressions { get; private set; }

        public IContainer Container { get; private set; }

        public ILockManager LockManager =>
            Container.Resolve<ILockManager>();

        public IReaderWriterLockManager ReaderWriterLockManager =>
            Container.Resolve<IReaderWriterLockManager>();

        public IThreadLocalManager ThreadLocalManager =>
            Container.Resolve<IThreadLocalManager>();
    }
} // end of namespace
