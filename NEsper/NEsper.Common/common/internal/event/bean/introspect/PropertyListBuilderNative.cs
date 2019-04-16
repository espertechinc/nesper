///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.@event.bean.core;

namespace com.espertech.esper.common.@internal.@event.bean.introspect
{
    /// <summary>
    ///     Implementation for a property list builder that considers JavaBean-style methods
    ///     as the exposed event properties, plus any explicitly configured props.
    /// </summary>
    public class PropertyListBuilderNative : PropertyListBuilder
    {
        private readonly ConfigurationCommonEventTypeBean optionalLegacyConfig;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="optionalLegacyConfig">
        ///     configures legacy type, or null informationhas been supplied.
        /// </param>
        public PropertyListBuilderNative(ConfigurationCommonEventTypeBean optionalLegacyConfig)
        {
            this.optionalLegacyConfig = optionalLegacyConfig;
        }

        public IList<PropertyStem> AssessProperties(Type clazz)
        {
            var result = PropertyHelper.GetProperties(clazz);
            if (optionalLegacyConfig != null) {
                PropertyListBuilderExplicit.GetExplicitProperties(result, clazz, optionalLegacyConfig);
            }

            return result;
        }
    }
} // end of namespace