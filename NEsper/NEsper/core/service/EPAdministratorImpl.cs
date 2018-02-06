///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.client.deploy;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.deploy;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.pattern;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Implementation for the admin interface.
    /// </summary>
    public class EPAdministratorImpl : EPAdministratorSPI
    {
        private const string SUBS_PARAM_INVALID_USE = "Invalid use of substitution parameters marked by '?' in statement, use the prepare method to prepare statements with substitution parameters";

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

            _deploymentAdminService = new EPDeploymentAdminImpl(
                this,
                _services.LockManager,
                _services.EventProcessingRWLock,
                _services.ResourceManager,
                adminContext.Services.DeploymentStateService,
                adminContext.Services.StatementEventTypeRefService,
                adminContext.Services.EventAdapterService,
                adminContext.Services.StatementIsolationService,
                adminContext.Services.FilterService,
                _services.ConfigSnapshot.EngineDefaults.Expression.TimeZone,
                _services.ConfigSnapshot.EngineDefaults.ExceptionHandling.UndeployRethrowPolicy);
        }

        public EPDeploymentAdmin DeploymentAdmin
        {
            get { return _deploymentAdminService; }
        }

        public EPStatement CreatePattern(string onExpression)
        {
            return CreatePatternStmt(onExpression, null, null, null);
        }

        public EPStatement CreateEPL(string eplStatement)
        {
            return CreateEPLStmt(eplStatement, null, null, null);
        }

        public EPStatement CreatePattern(string expression, string statementName)
        {
            return CreatePatternStmt(expression, statementName, null, null);
        }

        public EPStatement CreatePattern(string expression, string statementName, object userobject)
        {
            return CreatePatternStmt(expression, statementName, userobject, null);
        }

        public EPStatement CreateEPL(string eplStatement, string statementName)
        {
            return CreateEPLStmt(eplStatement, statementName, null, null);
        }

        public EPStatement CreateEPLStatementId(string eplStatement, string statementName, object userobject, int statementId)
        {
            return CreateEPLStmt(eplStatement, statementName, userobject, statementId);
        }

        public EPStatement CreateEPL(string eplStatement, string statementName, object userobject)
        {
            return CreateEPLStmt(eplStatement, statementName, userobject, null);
        }

        public EPStatement CreatePattern(string expression, object userobject)
        {
            return CreatePatternStmt(expression, null, userobject, null);
        }

        public EPStatement CreatePatternStatementId(string pattern, string statementName, object userobject, int statementId)
        {
            return CreatePatternStmt(pattern, statementName, userobject, statementId);
        }

        public EPStatement CreateEPL(string eplStatement, object userobject)
        {
            return CreateEPLStmt(eplStatement, null, userobject, null);
        }

        private EPStatement CreatePatternStmt(string expression, string statementName, object userobject, int? optionalStatementId)
        {
            var rawPattern = EPAdministratorHelper.CompilePattern(expression, expression, true, _services, SelectClauseStreamSelectorEnum.ISTREAM_ONLY);
            return _services.StatementLifecycleSvc.CreateAndStart(rawPattern, expression, true, statementName, userobject, null, optionalStatementId, null);
        }

        private EPStatement CreateEPLStmt(string eplStatement, string statementName, object userobject, int? optionalStatementId)
        {
            var statementSpec = EPAdministratorHelper.CompileEPL(eplStatement, eplStatement, true, statementName, _services, _defaultStreamSelector);
            var statement = _services.StatementLifecycleSvc.CreateAndStart(statementSpec, eplStatement, false, statementName, userobject, null, optionalStatementId, null);

            Log.Debug(".createEPLStmt Statement created and started");
            return statement;
        }

        public EPStatement Create(EPStatementObjectModel sodaStatement)
        {
            return Create(sodaStatement, null);
        }

        public EPStatement CreateModelStatementId(EPStatementObjectModel sodaStatement, string statementName, object userobject, int statementId)
        {
            return Create(sodaStatement, statementName, userobject, statementId);
        }

        public EPStatement Create(EPStatementObjectModel sodaStatement, string statementName, object userobject)
        {
            return Create(sodaStatement, statementName, userobject, null);
        }

        public EPStatement Create(EPStatementObjectModel sodaStatement, string statementName, object userobject, int? optionalStatementId)
        {
            // Specifies the statement
            var statementSpec = MapSODAToRaw(sodaStatement);
            var eplStatement = sodaStatement.ToEPL();

            var statement = _services.StatementLifecycleSvc.CreateAndStart(statementSpec, eplStatement, false, statementName, userobject, null, optionalStatementId, sodaStatement);

            Log.Debug(".createEPLStmt Statement created and started");
            return statement;
        }

        public EPStatement Create(EPStatementObjectModel sodaStatement, string statementName)
        {
            // Specifies the statement
            var statementSpec = MapSODAToRaw(sodaStatement);
            var eplStatement = sodaStatement.ToEPL();

            var statement = _services.StatementLifecycleSvc.CreateAndStart(statementSpec, eplStatement, false, statementName, null, null, null, sodaStatement);

            Log.Debug(".createEPLStmt Statement created and started");
            return statement;
        }

        public EPPreparedStatement PrepareEPL(string eplExpression)
        {
            // compile to specification
            var statementSpec = EPAdministratorHelper.CompileEPL(eplExpression, eplExpression, true, null, _services, _defaultStreamSelector);

            // map to object model thus finding all substitution parameters and their indexes
            var unmapped = StatementSpecMapper.Unmap(statementSpec);

            // the prepared statement is the object model plus a list of substitution parameters
            // map to specification will refuse any substitution parameters that are unfilled
            return new EPPreparedStatementImpl(unmapped.ObjectModel, unmapped.SubstitutionParams, eplExpression);
        }

        public EPPreparedStatement PreparePattern(string patternExpression)
        {
            var rawPattern = EPAdministratorHelper.CompilePattern(patternExpression, patternExpression, true, _services, SelectClauseStreamSelectorEnum.ISTREAM_ONLY);

            // map to object model thus finding all substitution parameters and their indexes
            var unmapped = StatementSpecMapper.Unmap(rawPattern);

            // the prepared statement is the object model plus a list of substitution parameters
            // map to specification will refuse any substitution parameters that are unfilled
            return new EPPreparedStatementImpl(unmapped.ObjectModel, unmapped.SubstitutionParams, null);
        }

        public EPStatement Create(EPPreparedStatement prepared, string statementName, object userobject, int? optionalStatementId)
        {
            var impl = (EPPreparedStatementImpl)prepared;

            var statementSpec = MapSODAToRaw(impl.Model);
            var eplStatement = impl.Model.ToEPL();

            return _services.StatementLifecycleSvc.CreateAndStart(statementSpec, eplStatement, false, statementName, userobject, null, optionalStatementId, impl.Model);
        }

        public EPStatement Create(EPPreparedStatement prepared, string statementName)
        {
            return Create(prepared, statementName, null, null);
        }

        public EPStatement Create(EPPreparedStatement prepared, string statementName, object userobject)
        {
            return Create(prepared, statementName, userobject, null);
        }

        public EPStatement CreatePreparedEPLStatementId(EPPreparedStatementImpl prepared, string statementName, object userobject, int statementId)
        {
            return Create(prepared, statementName, userobject, statementId);
        }

        public EPStatement Create(EPPreparedStatement prepared)
        {
            return Create(prepared, null);
        }

        public EPStatementObjectModel CompileEPL(string eplStatement)
        {
            var statementSpec = EPAdministratorHelper.CompileEPL(eplStatement, eplStatement, true, null, _services, _defaultStreamSelector);
            var unmapped = StatementSpecMapper.Unmap(statementSpec);
            if (unmapped.SubstitutionParams.Count != 0)
            {
                throw new EPException(SUBS_PARAM_INVALID_USE);
            }
            return unmapped.ObjectModel;
        }

        public EPStatement GetStatement(string name)
        {
            return _services.StatementLifecycleSvc.GetStatementByName(name);
        }

        public string GetStatementNameForId(int statementId)
        {
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

        public StatementSpecRaw CompileEPLToRaw(string epl)
        {
            return EPAdministratorHelper.CompileEPL(epl, epl, true, null, _services, _defaultStreamSelector);
        }

        public EPStatementObjectModel MapRawToSODA(StatementSpecRaw raw)
        {
            var unmapped = StatementSpecMapper.Unmap(raw);
            if (unmapped.SubstitutionParams.Count != 0)
            {
                throw new EPException(SUBS_PARAM_INVALID_USE);
            }
            return unmapped.ObjectModel;
        }

        public StatementSpecRaw MapSODAToRaw(EPStatementObjectModel model)
        {
            return StatementSpecMapper.Map(
                _services.Container,
                model,
                _services.EngineImportService,
                _services.VariableService,
                _services.ConfigSnapshot,
                _services.SchedulingService,
                _services.EngineURI,
                _services.PatternNodeFactory,
                _services.NamedWindowMgmtService,
                _services.ContextManagementService,
                _services.ExprDeclaredService,
                _services.TableService);
        }

        public EvalFactoryNode CompilePatternToNode(string pattern)
        {
            var raw = EPAdministratorHelper.CompilePattern(pattern, pattern, false, _services, SelectClauseStreamSelectorEnum.ISTREAM_ONLY);
            return ((PatternStreamSpecRaw)raw.StreamSpecs[0]).EvalFactoryNode;
        }

        public EPStatementObjectModel CompilePatternToSODAModel(string expression)
        {
            var rawPattern = EPAdministratorHelper.CompilePattern(expression, expression, true, _services, SelectClauseStreamSelectorEnum.ISTREAM_ONLY);
            return MapRawToSODA(rawPattern);
        }

        public ExprNode CompileExpression(string expression)
        {
            var toCompile = "select * from System.object#time(" + expression + ")";
            var raw = EPAdministratorHelper.CompileEPL(toCompile, expression, false, null, _services, SelectClauseStreamSelectorEnum.ISTREAM_ONLY);
            return raw.StreamSpecs[0].ViewSpecs[0].ObjectParameters[0];
        }

        public Expression CompileExpressionToSODA(string expression)
        {
            var node = CompileExpression(expression);
            return StatementSpecMapper.Unmap(node);
        }

        public PatternExpr CompilePatternToSODA(string expression)
        {
            var node = CompilePatternToNode(expression);
            return StatementSpecMapper.Unmap(node);
        }

        public AnnotationPart CompileAnnotationToSODA(string annotationExpression)
        {
            var toCompile = annotationExpression + " select * from System.object";
            var raw = EPAdministratorHelper.CompileEPL(toCompile, annotationExpression, false, null, _services, SelectClauseStreamSelectorEnum.ISTREAM_ONLY);
            return StatementSpecMapper.Unmap(raw.Annotations[0]);
        }

        public MatchRecognizeRegEx CompileMatchRecognizePatternToSODA(string matchRecogPatternExpression)
        {
            var toCompile = "select * from System.object Match_recognize(measures a.b as c pattern (" + matchRecogPatternExpression + ") define A as true)";
            var raw = EPAdministratorHelper.CompileEPL(toCompile, matchRecogPatternExpression, false, null, _services, SelectClauseStreamSelectorEnum.ISTREAM_ONLY);
            return StatementSpecMapper.Unmap(raw.MatchRecognizeSpec.Pattern);
        }

        public EPContextPartitionAdmin ContextPartitionAdmin
        {
            get { return new EPContextPartitionAdminImpl(_services); }
        }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}
