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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.spec;
using com.espertech.esper.filter;
using com.espertech.esper.schedule;

namespace com.espertech.esper.core.service
{
    /// <summary>Implementation for the admin interface. </summary>
    public class EPAdministratorIsolatedImpl : EPAdministratorIsolatedSPI
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private readonly String _isolatedServiceName;
        private readonly EPIsolationUnitServices _services;
        private readonly EPServicesContext _unisolatedServices;
        private readonly EPRuntimeIsolatedImpl _isolatedRuntime;
        private readonly ICollection<String> _statementNames = new HashSet<string>().AsSyncCollection();
    
        /// <summary>Ctor. </summary>
        /// <param name="isolatedServiceName">name of the isolated service</param>
        /// <param name="services">isolated services</param>
        /// <param name="unisolatedServices">engine services</param>
        /// <param name="isolatedRuntime">the runtime for this isolated service</param>
        public EPAdministratorIsolatedImpl(String isolatedServiceName, EPIsolationUnitServices services, EPServicesContext unisolatedServices, EPRuntimeIsolatedImpl isolatedRuntime)
        {
            _isolatedServiceName = isolatedServiceName;
            _services = services;
            _unisolatedServices = unisolatedServices;
            _isolatedRuntime = isolatedRuntime;
        }
    
        public EPStatement CreateEPL(String eplStatement, String statementName, Object userObject)
        {
            return CreateEPLStatementId(eplStatement, statementName, userObject, null)  ;
        }
    
        public EPStatement CreateEPLStatementId(String eplStatement, String statementName, Object userObject, String statementId)
        {
            var defaultStreamSelector = _unisolatedServices.ConfigSnapshot.EngineDefaults.StreamSelectionConfig.DefaultStreamSelector.MapFromSODA();
            var statementSpec = EPAdministratorHelper.CompileEPL(eplStatement, eplStatement, true, statementName, _unisolatedServices, defaultStreamSelector);
            var statement = _unisolatedServices.StatementLifecycleSvc.CreateAndStart(statementSpec, eplStatement, false, statementName, userObject, _services, statementId, null);
            var stmtSpi = (EPStatementSPI) statement;
            stmtSpi.StatementContext.InternalEventEngineRouteDest = _isolatedRuntime;
            stmtSpi.ServiceIsolated = _isolatedServiceName;
            _statementNames.Add(stmtSpi.Name);
            return statement;
        }

        public IList<string> StatementNames
        {
            get { return _statementNames.ToArray(); }
        }

        public void AddStatement(String name)
        {
            _statementNames.Add(name);   // for recovery
        }
    
        public void AddStatement(EPStatement stmt) {
    
            AddStatement(new[] {stmt});
        }
    
        public void AddStatement(EPStatement[] stmt)
        {
            using (_unisolatedServices.EventProcessingRwLock.WriteLock.Acquire())
            {
                try
                {
                    long fromTime = _unisolatedServices.SchedulingService.Time;
                    long toTime = _services.SchedulingService.Time;
                    long delta = toTime - fromTime;

                    // perform checking
                    ICollection<String> statementIds = new HashSet<String>();
                    foreach (EPStatement aStmt in stmt)
                    {
                        if (aStmt == null)
                        {
                            throw new EPServiceIsolationException(
                                "Illegal argument, a null value was provided in the statement list");
                        }
                        var stmtSpi = (EPStatementSPI) aStmt;
                        statementIds.Add(stmtSpi.StatementId);

                        if (aStmt.ServiceIsolated != null)
                        {
                            throw new EPServiceIsolationException("Statement named '" + aStmt.Name +
                                                                  "' already in service isolation under '" +
                                                                  stmtSpi.ServiceIsolated + "'");
                        }
                    }

                    // start txn
                    _unisolatedServices.StatementIsolationService.BeginIsolatingStatements(_isolatedServiceName,
                                                                                          _services.UnitId, stmt);

                    FilterSet filters = _unisolatedServices.FilterService.Take(statementIds);
                    ScheduleSet schedules = _unisolatedServices.SchedulingService.Take(statementIds);

                    _services.FilterService.Apply(filters);
                    _services.SchedulingService.Apply(schedules);

                    foreach (EPStatement aStmt in stmt)
                    {
                        var stmtSpi = (EPStatementSPI) aStmt;
                        stmtSpi.StatementContext.FilterService = _services.FilterService;
                        stmtSpi.StatementContext.SchedulingService = _services.SchedulingService;
                        stmtSpi.StatementContext.InternalEventEngineRouteDest = _isolatedRuntime;
                        stmtSpi.StatementContext.ScheduleAdjustmentService.Adjust(delta);
                        _statementNames.Add(stmtSpi.Name);
                        stmtSpi.ServiceIsolated = _isolatedServiceName;
                    }

                    // commit txn
                    _unisolatedServices.StatementIsolationService.CommitIsolatingStatements(_isolatedServiceName,
                                                                                           _services.UnitId, stmt);
                }
                catch (EPServiceIsolationException ex)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _unisolatedServices.StatementIsolationService.RollbackIsolatingStatements(_isolatedServiceName,
                                                                                             _services.UnitId, stmt);

                    String message = "Unexpected exception taking statements: " + ex.Message;
                    Log.Error(message, ex);
                    throw new EPException(message, ex);
                }
            }
        }
    
        public void RemoveStatement(EPStatement stmt)
        {
            RemoveStatement(new[] {stmt});
        }
    
        public void RemoveStatement(IList<EPStatement> stmt) {
    
            using(_unisolatedServices.EventProcessingRwLock.WriteLock.Acquire())
            {
                try
                {
                    long fromTime = _services.SchedulingService.Time;
                    long toTime = _unisolatedServices.SchedulingService.Time;
                    long delta = toTime - fromTime;

                    ICollection<String> statementIds = new HashSet<String>();
                    foreach (EPStatement aStmt in stmt)
                    {
                        if (aStmt == null)
                        {
                            throw new EPServiceIsolationException(
                                "Illegal argument, a null value was provided in the statement list");
                        }

                        var stmtSpi = (EPStatementSPI) aStmt;
                        statementIds.Add(stmtSpi.StatementId);

                        if (aStmt.ServiceIsolated == null)
                        {
                            throw new EPServiceIsolationException("Statement named '" + aStmt.Name +
                                                                  "' is not currently in service isolation");
                        }
                        if (!aStmt.ServiceIsolated.Equals(_isolatedServiceName))
                        {
                            throw new EPServiceIsolationException("Statement named '" + aStmt.Name +
                                                                  "' not in this service isolation but under service isolation '" +
                                                                  aStmt.Name + "'");
                        }
                    }

                    // start txn
                    _unisolatedServices.StatementIsolationService.BeginUnisolatingStatements(
                        _isolatedServiceName,
                        _services.UnitId,
                        stmt);

                    FilterSet filters = _services.FilterService.Take(statementIds);
                    ScheduleSet schedules = _services.SchedulingService.Take(statementIds);

                    _unisolatedServices.FilterService.Apply(filters);
                    _unisolatedServices.SchedulingService.Apply(schedules);

                    foreach (EPStatement aStmt in stmt)
                    {
                        var stmtSpi = (EPStatementSPI) aStmt;
                        stmtSpi.StatementContext.FilterService = _unisolatedServices.FilterService;
                        stmtSpi.StatementContext.SchedulingService = _unisolatedServices.SchedulingService;
                        stmtSpi.StatementContext.InternalEventEngineRouteDest =
                            _unisolatedServices.InternalEventEngineRouteDest;
                        stmtSpi.StatementContext.ScheduleAdjustmentService.Adjust(delta);
                        _statementNames.Remove(stmtSpi.Name);
                        stmtSpi.ServiceIsolated = null;
                    }

                    // commit txn
                    _unisolatedServices.StatementIsolationService.CommitUnisolatingStatements(
                        _isolatedServiceName, _services.UnitId, stmt);
                }
                catch (EPServiceIsolationException ex)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _unisolatedServices.StatementIsolationService.RollbackUnisolatingStatements(_isolatedServiceName,
                                                                                               _services.UnitId, stmt);

                    String message = "Unexpected exception taking statements: " + ex.Message;
                    Log.Error(message, ex);
                    throw new EPException(message, ex);
                }
            }
        }
    
        /// <summary>Remove all statements from isolated services, such as upon destroy. </summary>
        public void RemoveAllStatements()
        {
            var statements = new List<EPStatement>();
            foreach (String stmtName in _statementNames)
            {
                EPStatement stmt = _unisolatedServices.StatementLifecycleSvc.GetStatementByName(stmtName);
                if (stmt == null)
                {
                    Log.Debug("Statement '" + stmtName + "', the statement could not be found");
                    continue;
                }
    
                if (stmt.ServiceIsolated != null && (!stmt.ServiceIsolated.Equals(_isolatedServiceName)))
                {
                    Log.Error("Error returning statement '" + stmtName + "', the internal isolation information is incorrect, isolated service for statement is currently '" +
                            stmt.ServiceIsolated + "' and mismatches this isolated services named '" + _isolatedServiceName + "'");
                    continue;
                }
    
                statements.Add(stmt);
            }
    
            RemoveStatement(statements.ToArray());
        }
    }
}
