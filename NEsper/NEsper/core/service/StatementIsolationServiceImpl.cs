///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
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
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private readonly IDictionary<String, EPServiceProviderIsolatedImpl> _isolatedProviders;
        private EPServicesContext _epServicesContext;
        private volatile int _currentUnitId = 0;
    
        /// <summary>Ctor. </summary>
        public StatementIsolationServiceImpl()
        {
            _isolatedProviders = new ConcurrentDictionary<String, EPServiceProviderIsolatedImpl>();
        }

        /// <summary>Set the engine service context. </summary>
        /// <value>services context</value>
        public EPServicesContext ServicesContext
        {
            get { return _epServicesContext; }
            set { _epServicesContext = value; }
        }

        public EPServiceProviderIsolated GetIsolationUnit(String name, int? optionalUnitId)
        {
            var serviceProviderIsolated = _isolatedProviders.Get(name);
            if (serviceProviderIsolated != null)
            {
                return serviceProviderIsolated;
            }

            var filterService = FilterServiceProvider.NewService(_epServicesContext.ConfigSnapshot.EngineDefaults.ExecutionConfig.FilterServiceProfile, true);
            var scheduleService = new SchedulingServiceImpl(_epServicesContext.TimeSource);
            var services = new EPIsolationUnitServices(name, _currentUnitId, filterService, scheduleService);
            serviceProviderIsolated = new EPServiceProviderIsolatedImpl(name, services, _epServicesContext, _isolatedProviders);
            _isolatedProviders.Put(name, serviceProviderIsolated);
            return serviceProviderIsolated;
        }
    
        public void Dispose()
        {
            _isolatedProviders.Clear();        
        }

        public string[] IsolationUnitNames
        {
            get
            {
                ICollection<String> keyset = _isolatedProviders.Keys;
                return keyset.ToArray();
            }
        }

        public void BeginIsolatingStatements(String name, int unitId, IList<EPStatement> stmt)
        {
            if (Log.IsInfoEnabled)
            {
                Log.Info("Begin isolating statements " + Print(stmt) + " unit " + name + " id " + unitId);
            }
        }

        public void CommitIsolatingStatements(String name, int unitId, IList<EPStatement> stmt)
        {
            if (Log.IsInfoEnabled)
            {
                Log.Info("Completed isolating statements " + Print(stmt) + " unit " + name + " id " + unitId);
            }
        }

        public void RollbackIsolatingStatements(String name, int unitId, IList<EPStatement> stmt)
        {
            if (Log.IsInfoEnabled)
            {
                Log.Info("Failed isolating statements " + Print(stmt) + " unit " + name + " id " + unitId);
            }
        }

        public void BeginUnisolatingStatements(String name, int unitId, IList<EPStatement> stmt)
        {
            if (Log.IsInfoEnabled)
            {
                Log.Info("Begin un-isolating statements " + Print(stmt) + " unit " + name + " id " + unitId);
            }
        }

        public void CommitUnisolatingStatements(String name, int unitId, IList<EPStatement> stmt)
        {
            if (Log.IsInfoEnabled)
            {
                Log.Info("Completed un-isolating statements " + Print(stmt) + " unit " + name + " id " + unitId);
            }
        }
    
        public void RollbackUnisolatingStatements(String name, int unitId, IList<EPStatement> stmt)
        {
            if (Log.IsInfoEnabled)
            {
                Log.Info("Failed un-isolating statements " + Print(stmt) + " unit " + name + " id " + unitId);
            }
        }
    
        public void NewStatement(String stmtId, String stmtName, EPIsolationUnitServices isolatedServices)
        {
            Log.Info("New statement '" + stmtName + "' unit " + isolatedServices.Name);
        }
    
        private String Print(IEnumerable<EPStatement> stmts)
        {
            var buf = new StringBuilder();
            var delimiter = "";
            foreach (EPStatement stmt in stmts)
            {
                buf.Append(delimiter);
                buf.Append(stmt.Name);
                delimiter = ", ";
            }
            return buf.ToString();
        }
    }
}
