///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Implementation of the isolated service provider.
    /// </summary>
    public class EPServiceProviderIsolatedImpl : EPServiceProviderIsolatedSPI
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly String _name;
        private readonly EPRuntimeIsolatedSPI _runtime;
        private readonly EPAdministratorIsolatedImpl _admin;
        private readonly EPIsolationUnitServices _isolatedServices;
        private readonly IDictionary<String, EPServiceProviderIsolatedImpl> _providers;

        /// <summary>Ctor. </summary>
        /// <param name="name">name of isolated service</param>
        /// <param name="isolatedServices">filter and scheduling service isolated</param>
        /// <param name="unisolatedSvc">engine services</param>
        /// <param name="providers">names and isolated service providers</param>
        public EPServiceProviderIsolatedImpl(
            String name,
            EPIsolationUnitServices isolatedServices,
            EPServicesContext unisolatedSvc,
            IDictionary<String, EPServiceProviderIsolatedImpl> providers)
        {
            _name = name;
            _providers = providers;
            _isolatedServices = isolatedServices;

            _runtime = unisolatedSvc.EpRuntimeIsolatedFactory.Make(isolatedServices, unisolatedSvc);
            _admin = new EPAdministratorIsolatedImpl(name, isolatedServices, unisolatedSvc, _runtime);
        }

        public EPIsolationUnitServices IsolatedServices
        {
            get { return _isolatedServices; }
        }

        public EPRuntimeIsolated EPRuntime
        {
            get { return _runtime; }
        }

        public EPAdministratorIsolated EPAdministrator
        {
            get { return _admin; }
        }

        public string Name
        {
            get { return _name; }
        }

        public void Dispose()
        {
            _providers.Remove(_name);

            _admin.RemoveAllStatements();
        }
    }
}
