///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.module;

namespace com.espertech.esper.common.@internal.context.module
{
    public interface ModuleProvider
    {
        string ModuleName { get; }

        IDictionary<ModuleProperty, object> ModuleProperties { get; }

        ModuleDependenciesRuntime ModuleDependencies { get; }

        void InitializeEventTypes(EPModuleEventTypeInitServices svc);

        void InitializeNamedWindows(EPModuleNamedWindowInitServices svc);

        void InitializeIndexes(EPModuleIndexInitServices svc);

        void InitializeContexts(EPModuleContextInitServices svc);

        void InitializeVariables(EPModuleVariableInitServices svc);

        void InitializeExprDeclareds(EPModuleExprDeclaredInitServices svc);

        void InitializeTables(EPModuleTableInitServices svc);

        void InitializeScripts(EPModuleScriptInitServices svc);
        
        void InitializeClassProvided(EPModuleClassProvidedInitServices svc);

        IList<StatementProvider> Statements { get; }
    }
} // end of namespace