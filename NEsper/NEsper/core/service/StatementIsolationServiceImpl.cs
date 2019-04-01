///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.filter;
using com.espertech.esper.schedule;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Service to maintain currently active isoalted service providers for an engine.
    /// </summary>
    public class StatementIsolationServiceImpl : StatementIsolationService
    {
        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IDictionary<string, EPServiceProviderIsolatedImpl> _isolatedProviders;
        private EPServicesContext _epServicesContext;
        private volatile int currentUnitId = 0;

        /// <summary>Ctor.</summary>
        public StatementIsolationServiceImpl()
        {
            _isolatedProviders = new ConcurrentDictionary<string, EPServiceProviderIsolatedImpl>();
        }

        /// <summary>
        /// Set the engine service context.
        /// </summary>
        /// <param name="epServicesContext">services context</param>
        public void SetEpServicesContext(EPServicesContext epServicesContext)
        {
            _epServicesContext = epServicesContext;
        }

        public EPServiceProviderIsolated GetIsolationUnit(string name, int? optionalUnitId)
        {
            var serviceProviderIsolated = _isolatedProviders.Get(name);
            if (serviceProviderIsolated != null)
            {
                return serviceProviderIsolated;
            }

            FilterServiceSPI filterService = FilterServiceProvider.NewService(
                _epServicesContext.LockManager,
                _epServicesContext.RWLockManager,
                _epServicesContext.ConfigSnapshot.EngineDefaults.Execution.FilterServiceProfile, 
                true);

            var scheduleService = new SchedulingServiceImpl(
                _epServicesContext.TimeSource,
                _epServicesContext.LockManager);
            var services = new EPIsolationUnitServices(name, currentUnitId, filterService, scheduleService);
            serviceProviderIsolated = new EPServiceProviderIsolatedImpl(
                name, services, _epServicesContext, _isolatedProviders);
            _isolatedProviders.Put(name, serviceProviderIsolated);
            return serviceProviderIsolated;
        }

        public void Dispose()
        {
            _isolatedProviders.Clear();
        }

        public string[] IsolationUnitNames
        {
            get { return _isolatedProviders.Keys.ToArray(); }
        }

        public void BeginIsolatingStatements(string name, int unitId, IList<EPStatement> stmt)
        {
            if (Log.IsInfoEnabled)
            {
                Log.Info("Begin isolating statements " + Print(stmt) + " unit " + name + " id " + unitId);
            }
        }

        public void CommitIsolatingStatements(string name, int unitId, IList<EPStatement> stmt)
        {
            if (Log.IsInfoEnabled)
            {
                Log.Info("Completed isolating statements " + Print(stmt) + " unit " + name + " id " + unitId);
            }
        }

        public void RollbackIsolatingStatements(string name, int unitId, IList<EPStatement> stmt)
        {
            if (Log.IsInfoEnabled)
            {
                Log.Info("Failed isolating statements " + Print(stmt) + " unit " + name + " id " + unitId);
            }
        }

        public void BeginUnisolatingStatements(string name, int unitId, IList<EPStatement> stmt)
        {
            if (Log.IsInfoEnabled)
            {
                Log.Info("Begin un-isolating statements " + Print(stmt) + " unit " + name + " id " + unitId);
            }
        }

        public void CommitUnisolatingStatements(string name, int unitId, IList<EPStatement> stmt)
        {
            if (Log.IsInfoEnabled)
            {
                Log.Info("Completed un-isolating statements " + Print(stmt) + " unit " + name + " id " + unitId);
            }
        }

        public void RollbackUnisolatingStatements(string name, int unitId, IList<EPStatement> stmt)
        {
            if (Log.IsInfoEnabled)
            {
                Log.Info("Failed un-isolating statements " + Print(stmt) + " unit " + name + " id " + unitId);
            }
        }

        public void NewStatement(int stmtId, string stmtName, EPIsolationUnitServices isolatedServices)
        {
            Log.Info("New statement '" + stmtName + "' unit " + isolatedServices.Name);
        }

        private string Print(IEnumerable<EPStatement> stmts)
        {
            var buf = new StringBuilder();
            string delimiter = "";
            foreach (EPStatement stmt in stmts)
            {
                buf.Append(delimiter);
                buf.Append(stmt.Name);
                delimiter = ", ";
            }
            return buf.ToString();
        }
    }
} // end of namespace
