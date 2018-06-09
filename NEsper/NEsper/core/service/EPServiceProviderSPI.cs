///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.core.deploy;
using com.espertech.esper.core.thread;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.metric;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.variable;
using com.espertech.esper.events;
using com.espertech.esper.events.vaevent;
using com.espertech.esper.filter;
using com.espertech.esper.schedule;
using com.espertech.esper.script;
using com.espertech.esper.timer;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// A service provider interface that makes available internal engine services.
    /// </summary>
    public interface EPServiceProviderSPI : EPServiceProvider
    {
        /// <summary>Returns statement management service for the engine. </summary>
        /// <value>the StatementLifecycleSvc</value>
        StatementLifecycleSvc StatementLifecycleSvc { get; }

        /// <summary>Get the EventAdapterService for this engine. </summary>
        /// <value>the EventAdapterService</value>
        EventAdapterService EventAdapterService { get; }

        /// <summary>Get the SchedulingService for this engine. </summary>
        /// <value>the SchedulingService</value>
        SchedulingService SchedulingService { get; }

        /// <summary>Get the SchedulingMgmtService for this engine. </summary>
        /// <value>the SchedulingMgmtService</value>
        SchedulingMgmtService SchedulingMgmtService { get; }

        /// <summary>Returns the filter service. </summary>
        /// <value>filter service</value>
        FilterService FilterService { get; }

        /// <summary>Returns the timer service. </summary>
        /// <value>timer service</value>
        TimerService TimerService { get; }

        /// <summary>Returns the named window service. </summary>
        /// <value>named window service</value>
        NamedWindowMgmtService NamedWindowMgmtService { get; }

        /// <summary>Returns the table service.</summary>
        /// <value>The table service.</value>
        TableService TableService { get; }

        /// <summary>Returns the current configuration. </summary>
        /// <value>configuration information</value>
        ConfigurationInformation ConfigurationInformation { get; }

        /// <summary>Returns the extension services context. </summary>
        /// <value>extension services context</value>
        EngineLevelExtensionServicesContext ExtensionServicesContext { get; }

        /// <summary>Returns metrics reporting. </summary>
        /// <value>metrics reporting</value>
        MetricReportingService MetricReportingService { get; }

        /// <summary>Returns variable services. </summary>
        /// <value>services</value>
        VariableService VariableService { get; }

        /// <summary>Returns value-added type service. </summary>
        /// <value>value types</value>
        ValueAddEventService ValueAddEventService { get; }

        /// <summary>Returns statement event type reference service. </summary>
        /// <value>statement-type reference service</value>
        StatementEventTypeRef StatementEventTypeRef { get; }

        /// <summary>Returns threading service for the engine. </summary>
        /// <value>the ThreadingService</value>
        ThreadingService ThreadingService { get; }

        /// <summary>Returns engine environment context such as plugin loader references. </summary>
        /// <value>environment context</value>
        Directory EngineEnvContext { get; }

        /// <summary>Returns services. </summary>
        /// <value>services</value>
        EPServicesContext ServicesContext { get; }

        /// <summary>Returns context factory. </summary>
        /// <value>factory</value>
        StatementContextFactory StatementContextFactory { get; }

        /// <summary>Returns engine imports. </summary>
        /// <value>engine imports</value>
        EngineImportService EngineImportService { get; }

        /// <summary>Returns time provider. </summary>
        /// <value>time provider</value>
        TimeProvider TimeProvider { get; }

        StatementIsolationService StatementIsolationService { get; }

        DeploymentStateService DeploymentStateService { get; }

        ContextManagementService ContextManagementService { get; }

        /// <summary>
        /// Gets the scripting service.
        /// </summary>
        /// <value>The scripting service.</value>
        ScriptingService ScriptingService { get; }

        void SetConfiguration(Configuration configuration);

        void PostInitialize();

        void Initialize(long? currentTime);
    }

    public static class EPServiceProviderConstants
    {
        /// <summary>For the default provider instance, which carries a null provider URI, the URI value is "default". </summary>
        public static readonly String DEFAULT_ENGINE_URI = "default";

        /// <summary>For the default provider instance, which carries a "default" provider URI, the property name qualification and stream name qualification may use "default". </summary>
        public static readonly String DEFAULT_ENGINE_URI_QUALIFIER = "default";
    }
}
