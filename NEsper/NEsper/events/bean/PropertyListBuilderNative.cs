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

namespace com.espertech.esper.events.bean
{
    /// <summary>
    /// Implementation for a property list builder that considers POCO methods
    /// and properties as the exposed event properties, plus any explicitly 
    /// configured props.
    /// </summary>
    public class PropertyListBuilderNative : PropertyListBuilder
    {
        private readonly ConfigurationEventTypeLegacy _optionalLegacyConfig;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="optionalLegacyConfig">configures legacy type, or null informationhas been supplied. </param>
        public PropertyListBuilderNative(ConfigurationEventTypeLegacy optionalLegacyConfig)
        {
            this._optionalLegacyConfig = optionalLegacyConfig;
        }

        public IList<InternalEventPropDescriptor> AssessProperties(Type type)
        {
            IList<InternalEventPropDescriptor> result = PropertyHelper.GetProperties(type);
            if (_optionalLegacyConfig != null) {
                PropertyListBuilderExplicit.GetExplicitProperties(result, type, _optionalLegacyConfig);
            }

            return result;
        }
    }
}
