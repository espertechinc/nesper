///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.module;

namespace com.espertech.esper.common.client.hook.recompile
{
    /// <summary>
    /// Context for use with <seealso cref="EPRecompileProvider" /></summary>
    public class EPRecompileProviderContext
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="compiled">compiler output</param>
        /// <param name="configuration">runtime configuration</param>
        /// <param name="deploymentId">deployment id</param>
        /// <param name="moduleName">module name</param>
        /// <param name="moduleProperties">module properties</param>
        /// <param name="path">compile path</param>
        /// <param name="statementIdFirstStatement">statement id of the first statement in the module</param>
        /// <param name="userObjectsRuntime">user objects</param>
        /// <param name="statementNamesWhenProvidedByAPI">statement names when provided as part of deployment</param>
        /// <param name="substitutionParameters">substitution parameters when provided as part of deployment</param>
        public EPRecompileProviderContext(
            EPCompiled compiled,
            Configuration configuration,
            string deploymentId,
            string moduleName,
            IDictionary<ModuleProperty, object> moduleProperties,
            IList<EPCompiled> path,
            int statementIdFirstStatement,
            IDictionary<int, object> userObjectsRuntime,
            IDictionary<int, string> statementNamesWhenProvidedByAPI,
            IDictionary<int, IDictionary<int, object>> substitutionParameters)
        {
            Compiled = compiled;
            Configuration = configuration;
            DeploymentId = deploymentId;
            ModuleName = moduleName;
            ModuleProperties = moduleProperties;
            Path = path;
            StatementIdFirstStatement = statementIdFirstStatement;
            UserObjectsRuntime = userObjectsRuntime;
            StatementNamesWhenProvidedByAPI = statementNamesWhenProvidedByAPI;
            SubstitutionParameters = substitutionParameters;
        }

        public string DeploymentId { get; }

        public string ModuleName { get; }

        public IDictionary<ModuleProperty, object> ModuleProperties { get; }

        public int StatementIdFirstStatement { get; }

        public EPCompiled Compiled { get; }

        public IDictionary<int, object> UserObjectsRuntime { get; }

        public IDictionary<int, string> StatementNamesWhenProvidedByAPI { get; }

        public IDictionary<int, IDictionary<int, object>> SubstitutionParameters { get; }

        public IList<EPCompiled> Path { get; }

        public Configuration Configuration { get; }
    }
} // end of namespace