///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.client.deploy;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.deploy;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.spec;
using com.espertech.esper.pattern;
using com.espertech.esper.util;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Implementation for the admin interface.
    /// </summary>
    public class EPAdministratorImpl : EPAdministratorSPI
    {
        private const String SUBS_PARAM_INVALID_USE = "Invalid use of substitution parameters marked by '?' in statement, use the prepare method to prepare statements with substitution parameters";

        private EPServicesContext _services;
        private ConfigurationOperations _configurationOperations;
        private readonly SelectClauseStreamSelectorEnum _defaultStreamSelector;
        private readonly EPDeploymentAdmin _deploymentAdminService;
    
        /// <summary>Constructor - takes the services context as argument. </summary>
        /// <param name="adminContext">administrative context</param>
        public EPAdministratorImpl(EPAdministratorContext adminContext)
        {
            _services = adminContext.Services;
            _configurationOperations = adminContext.ConfigurationOperations;
            _defaultStreamSelector = adminContext.DefaultStreamSelector;
    
            ConfigurationEngineDefaults.AlternativeContext alternativeContext = adminContext.Services.ConfigSnapshot.EngineDefaults.AlternativeContextConfig;
            StatementIdGenerator statementIdGenerator = null;
            if (alternativeContext != null && alternativeContext.StatementIdGeneratorFactory != null) {
                var statementIdGeneratorFactory = TypeHelper.Instantiate<StatementIdGeneratorFactory>(alternativeContext.StatementIdGeneratorFactory);
                statementIdGenerator = statementIdGeneratorFactory.Invoke(new StatementIdGeneratorFactoryContext(_services.EngineURI));
            }
            _deploymentAdminService = new EPDeploymentAdminImpl(this, adminContext.Services.DeploymentStateService, adminContext.Services.StatementEventTypeRefService, adminContext.Services.EventAdapterService, adminContext.Services.StatementIsolationService, statementIdGenerator, adminContext.Services.FilterService);
        }

        public EPDeploymentAdmin DeploymentAdmin
        {
            get { return _deploymentAdminService; }
        }

        public EPStatement CreatePattern(String onExpression)
        {
            return CreatePatternStmt(onExpression, null, null, null);
        }
    
        public EPStatement CreateEPL(String eplStatement)
        {
            return CreateEPLStmt(eplStatement, null, null, null);
        }
    
        public EPStatement CreatePattern(String expression, String statementName)
        {
            return CreatePatternStmt(expression, statementName, null, null);
        }
    
        public EPStatement CreatePattern(String expression, String statementName, Object userObject)
        {
            return CreatePatternStmt(expression, statementName, userObject, null);
        }
    
        public EPStatement CreateEPL(String eplStatement, String statementName)
        {
            return CreateEPLStmt(eplStatement, statementName, null, null);
        }
    
        public EPStatement CreateEPLStatementId(String eplStatement, String statementName, Object userObject, String statementId)
        {
            return CreateEPLStmt(eplStatement, statementName, userObject, statementId);
        }
    
        public EPStatement CreateEPL(String eplStatement, String statementName, Object userObject)
        {
            return CreateEPLStmt(eplStatement, statementName, userObject, null);
        }
    
        public EPStatement CreatePattern(String expression, Object userObject)
        {
            return CreatePatternStmt(expression, null, userObject, null);
        }
    
        public EPStatement CreatePatternStatementId(String pattern, String statementName, Object userObject, String statementId) {
            return CreatePatternStmt(pattern, statementName, userObject, statementId);
        }
    
        public EPStatement CreateEPL(String eplStatement, Object userObject)
        {
            return CreateEPLStmt(eplStatement, null, userObject, null);
        }
    
        private EPStatement CreatePatternStmt(String expression, String statementName, Object userObject, String statementId)
        {
            StatementSpecRaw rawPattern = EPAdministratorHelper.CompilePattern(expression, expression, true, _services, SelectClauseStreamSelectorEnum.ISTREAM_ONLY);
            return _services.StatementLifecycleSvc.CreateAndStart(rawPattern, expression, true, statementName, userObject, null, statementId, null);
        }
    
        private EPStatement CreateEPLStmt(String eplStatement, String statementName, Object userObject, String statementId)
        {
            StatementSpecRaw statementSpec = EPAdministratorHelper.CompileEPL(eplStatement, eplStatement, true, statementName, _services, _defaultStreamSelector);
            EPStatement statement = _services.StatementLifecycleSvc.CreateAndStart(statementSpec, eplStatement, false, statementName, userObject, null, statementId, null);
    
            Log.Debug(".createEPLStmt Statement created and started");
            return statement;
        }
    
        public EPStatement Create(EPStatementObjectModel sodaStatement)
        {
            return Create(sodaStatement, null);
        }
    
        public EPStatement CreateModelStatementId(EPStatementObjectModel sodaStatement, String statementName, Object userObject, String statementId) {
            return Create(sodaStatement, statementName, userObject, statementId);
        }
    
        public EPStatement Create(EPStatementObjectModel sodaStatement, String statementName, Object userObject) {
            return Create(sodaStatement, statementName, userObject, null);
        }
    
        public EPStatement Create(EPStatementObjectModel sodaStatement, String statementName, Object userObject, String statementId)
        {
            // Specifies the statement
            StatementSpecRaw statementSpec = MapSODAToRaw(sodaStatement);
            String eplStatement = sodaStatement.ToEPL();
    
            EPStatement statement = _services.StatementLifecycleSvc.CreateAndStart(statementSpec, eplStatement, false, statementName, userObject, null, statementId, sodaStatement);
    
            Log.Debug(".createEPLStmt Statement created and started");
            return statement;
        }
    
        public EPStatement Create(EPStatementObjectModel sodaStatement, String statementName)
        {
            // Specifies the statement
            StatementSpecRaw statementSpec = MapSODAToRaw(sodaStatement);
            String eplStatement = sodaStatement.ToEPL();
    
            EPStatement statement = _services.StatementLifecycleSvc.CreateAndStart(statementSpec, eplStatement, false, statementName, null, null, null, sodaStatement);
    
            Log.Debug(".createEPLStmt Statement created and started");
            return statement;
        }
    
        public EPPreparedStatement PrepareEPL(String eplExpression)
        {
            // compile to specification
            StatementSpecRaw statementSpec = EPAdministratorHelper.CompileEPL(eplExpression, eplExpression, true, null, _services, _defaultStreamSelector);
    
            // map to object model thus finding all substitution parameters and their indexes
            StatementSpecUnMapResult unmapped = StatementSpecMapper.Unmap(statementSpec);
    
            // the prepared statement is the object model plus a list of substitution parameters
            // map to specification will refuse any substitution parameters that are unfilled
            return new EPPreparedStatementImpl(unmapped.ObjectModel, unmapped.SubstitutionParams, eplExpression);
        }
    
        public EPPreparedStatement PreparePattern(String patternExpression)
        {
            StatementSpecRaw rawPattern = EPAdministratorHelper.CompilePattern(patternExpression, patternExpression, true, _services, SelectClauseStreamSelectorEnum.ISTREAM_ONLY);
    
            // map to object model thus finding all substitution parameters and their indexes
            StatementSpecUnMapResult unmapped = StatementSpecMapper.Unmap(rawPattern);
    
            // the prepared statement is the object model plus a list of substitution parameters
            // map to specification will refuse any substitution parameters that are unfilled
            return new EPPreparedStatementImpl(unmapped.ObjectModel, unmapped.SubstitutionParams, null);
        }
    
        public EPStatement Create(EPPreparedStatement prepared, String statementName, Object userObject, String statementId)
        {
            EPPreparedStatementImpl impl = (EPPreparedStatementImpl) prepared;
    
            StatementSpecRaw statementSpec = MapSODAToRaw(impl.Model);
            String eplStatement = impl.Model.ToEPL();
    
            return _services.StatementLifecycleSvc.CreateAndStart(statementSpec, eplStatement, false, statementName, userObject, null, statementId, impl.Model);
        }
    
        public EPStatement Create(EPPreparedStatement prepared, String statementName)
        {
            return Create(prepared, statementName, null, null);
        }
    
        public EPStatement Create(EPPreparedStatement prepared, String statementName, Object userObject) {
            return Create(prepared, statementName, userObject, null);
        }
    
        public EPStatement CreatePreparedEPLStatementId(EPPreparedStatementImpl prepared, String statementName, Object userObject, String statementId) {
            return Create(prepared, statementName, userObject, statementId);
        }
    
        public EPStatement Create(EPPreparedStatement prepared)
        {
            return Create(prepared, null);
        }
    
        public EPStatementObjectModel CompileEPL(String eplStatement)
        {
            StatementSpecRaw statementSpec = EPAdministratorHelper.CompileEPL(eplStatement, eplStatement, true, null, _services, _defaultStreamSelector);
            StatementSpecUnMapResult unmapped = StatementSpecMapper.Unmap(statementSpec);
            if (unmapped.SubstitutionParams.Count != 0)
            {
                throw new EPException(SUBS_PARAM_INVALID_USE);
            }
            return unmapped.ObjectModel;
        }
    
        public EPStatement GetStatement(String name)
        {
            return _services.StatementLifecycleSvc.GetStatementByName(name);
        }
    
        public String GetStatementNameForId(String statementId) {
            return _services.StatementLifecycleSvc.GetStatementNameById(statementId);
        }

        public IList<string> StatementNames
        {
            get { return _services.StatementLifecycleSvc.StatementNames; }
        }

        public void StartAllStatements()
        {
            _services.StatementLifecycleSvc.StartAllStatements();
        }
    
        public void StopAllStatements()
        {
            _services.StatementLifecycleSvc.StopAllStatements();
        }
    
        public void DestroyAllStatements()
        {
            _services.StatementLifecycleSvc.DestroyAllStatements();
        }

        public ConfigurationOperations Configuration
        {
            get { return _configurationOperations; }
        }

        /// <summary>Destroys an engine instance. </summary>
        public void Dispose()
        {
            _services = null;
            _configurationOperations = null;
        }
    
        public StatementSpecRaw CompileEPLToRaw(String epl) {
            return EPAdministratorHelper.CompileEPL(epl, epl, true, null, _services, _defaultStreamSelector);
        }
    
        public EPStatementObjectModel MapRawToSODA(StatementSpecRaw raw) {
            StatementSpecUnMapResult unmapped = StatementSpecMapper.Unmap(raw);
            if (unmapped.SubstitutionParams.Count != 0)
            {
                throw new EPException(SUBS_PARAM_INVALID_USE);
            }
            return unmapped.ObjectModel;
        }
    
        public StatementSpecRaw MapSODAToRaw(EPStatementObjectModel model)
        {
            return StatementSpecMapper.Map(
                model,
                _services.EngineImportService, 
                _services.VariableService,
                _services.ConfigSnapshot,
                _services.SchedulingService, 
                _services.EngineURI,
                _services.PatternNodeFactory,
                _services.NamedWindowService,
                _services.ContextManagementService,
                _services.ExprDeclaredService,
                _services.TableService);
        }
    
        public EvalFactoryNode CompilePatternToNode(String pattern)
        {
            StatementSpecRaw raw = EPAdministratorHelper.CompilePattern(pattern, pattern, false, _services, SelectClauseStreamSelectorEnum.ISTREAM_ONLY);
            return ((PatternStreamSpecRaw) raw.StreamSpecs[0]).EvalFactoryNode;
        }
    
        public EPStatementObjectModel CompilePatternToSODAModel(String expression) {
            StatementSpecRaw rawPattern = EPAdministratorHelper.CompilePattern(expression, expression, true, _services, SelectClauseStreamSelectorEnum.ISTREAM_ONLY);
            return MapRawToSODA(rawPattern);
        }
    
        public ExprNode CompileExpression(String expression)
        {
            String toCompile = "select * from System.Object.win:time(" + expression + ")";
            StatementSpecRaw raw = EPAdministratorHelper.CompileEPL(toCompile, expression, false, null, _services, SelectClauseStreamSelectorEnum.ISTREAM_ONLY);
            return raw.StreamSpecs[0].ViewSpecs[0].ObjectParameters[0];
        }
    
        public Expression CompileExpressionToSODA(String expression)
        {
            ExprNode node = CompileExpression(expression);
            return StatementSpecMapper.Unmap(node);
        }
    
        public PatternExpr CompilePatternToSODA(String expression)
        {
            EvalFactoryNode node = CompilePatternToNode(expression);
            return StatementSpecMapper.Unmap(node);
        }
    
        public AnnotationPart CompileAnnotationToSODA(String annotationExpression)
        {
            String toCompile = annotationExpression + " select * from System.Object";
            StatementSpecRaw raw = EPAdministratorHelper.CompileEPL(toCompile, annotationExpression, false, null, _services, SelectClauseStreamSelectorEnum.ISTREAM_ONLY);
            return StatementSpecMapper.Unmap(raw.Annotations[0]);
        }
    
        public MatchRecognizeRegEx CompileMatchRecognizePatternToSODA(String matchRecogPatternExpression)
        {
            String toCompile = "select * from System.Object Match_recognize(measures a.b as c pattern (" + matchRecogPatternExpression + ") define A as true)";
            StatementSpecRaw raw = EPAdministratorHelper.CompileEPL(toCompile, matchRecogPatternExpression, false, null, _services, SelectClauseStreamSelectorEnum.ISTREAM_ONLY);
            return StatementSpecMapper.Unmap(raw.MatchRecognizeSpec.Pattern);
        }

        public EPContextPartitionAdmin ContextPartitionAdmin
        {
            get { return new EPContextPartitionAdminImpl(_services); }
        }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}
