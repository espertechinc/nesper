///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.util;

namespace com.espertech.esper.common.@internal.@event.bean.introspect
{
    /// <summary>
    ///     Factory for creates a builder/introspector for determining event property descriptors
    ///     based on a given class.
    /// </summary>
    public class PropertyListBuilderFactory
    {
        /// <summary>
        ///     Creates an implementation for a builer considering the accessor style and
        ///     code generation flags for a given class.
        /// </summary>
        /// <param name="optionalLegacyClassConfigs">configures how event property listy is build</param>
        /// <returns>builder/introspector implementation</returns>
        public static PropertyListBuilder CreateBuilder(ConfigurationCommonEventTypeBean optionalLegacyClassConfigs)
        {
            if (optionalLegacyClassConfigs == null) {
                return new PropertyListBuilderNative(null);
            }

            if (optionalLegacyClassConfigs.AccessorStyle == AccessorStyle.NATIVE) {
                return new PropertyListBuilderNative(optionalLegacyClassConfigs);
            }

            if (optionalLegacyClassConfigs.AccessorStyle == AccessorStyle.EXPLICIT) {
                return new PropertyListBuilderExplicit(optionalLegacyClassConfigs);
            }

            if (optionalLegacyClassConfigs.AccessorStyle == AccessorStyle.PUBLIC) {
                return new PropertyListBuilderPublic(optionalLegacyClassConfigs);
            }

            throw new ArgumentException("Cannot match accessor style to property list builder");
        }
    }
} // end of namespace