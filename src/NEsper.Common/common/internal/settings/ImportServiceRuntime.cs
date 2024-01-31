///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.artifact;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.settings
{
    public class ImportServiceRuntime : ImportServiceBase
    {
        private readonly IDictionary<string, ConfigurationCommonMethodRef> _methodInvocationRef;
        private readonly IArtifactRepository _artifactRepository;

        public ImportServiceRuntime(
            IContainer container,
            IDictionary<string, object> transientConfiguration,
            TimeAbacus timeAbacus,
            ISet<string> eventTypeAutoNames,
            TimeZoneInfo timeZone,
            IDictionary<string, ConfigurationCommonMethodRef> methodInvocationRef,
            IList<Import> imports,
            IList<Import> annotationImports,
            IArtifactRepository artifactRepositoryDefault)
            : base(container, transientConfiguration, timeAbacus, eventTypeAutoNames)
        {
            TimeZone = timeZone;

            _artifactRepository = artifactRepositoryDefault;
            _methodInvocationRef = methodInvocationRef;

            try {
                foreach (var import in imports) {
                    AddImport(import);
                }

                foreach (var import in annotationImports) {
                    AddAnnotationImport(import);
                }
            }
            catch (ImportException ex) {
                throw new ConfigurationException("Failed to process imports: " + ex.Message, ex);
            }
        }

        public TimeZoneInfo TimeZone { get; }

        public override TypeResolver TypeResolver => TransientConfigurationResolver
            .ResolveTypeResolver(Container, TransientConfiguration, _artifactRepository.TypeResolver);

        public ConfigurationCommonMethodRef GetConfigurationMethodRef(string configurationName)
        {
            return _methodInvocationRef.Get(configurationName);
        }
    }
} // end of namespace