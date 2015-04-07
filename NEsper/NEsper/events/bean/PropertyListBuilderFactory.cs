///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.events.bean
{
    /// <summary>
    /// Factory for creates a builder/introspector for determining event property
    /// descriptors based on a given class.
    /// </summary>
    public class PropertyListBuilderFactory
    {
        /// <summary>
        /// Creates an implementation for a builer considering the accessor style and code
        /// generation flags for a given class.
        /// </summary>
        /// <param name="optionalLegacyClassConfigs">configures how event property listy is build</param>
        /// <returns>
        /// builder/introspector implementation
        /// </returns>
        public static IPropertyListBuilder CreateBuilder(ConfigurationEventTypeLegacy optionalLegacyClassConfigs)
        {
            if (optionalLegacyClassConfigs == null)
            {
                return new PropertyListBuilderNative(null);
            }
            if (optionalLegacyClassConfigs.AccessorStyle == AccessorStyleEnum.NATIVE)
            {
                return new PropertyListBuilderNative(optionalLegacyClassConfigs);
            }
            if (optionalLegacyClassConfigs.AccessorStyle == AccessorStyleEnum.EXPLICIT)
            {
                return new PropertyListBuilderExplicit(optionalLegacyClassConfigs);
            }
            if (optionalLegacyClassConfigs.AccessorStyle == AccessorStyleEnum.PUBLIC)
            {
                return new PropertyListBuilderPublic(optionalLegacyClassConfigs);
            }
            throw new ArgumentException("Cannot match accessor style to property list builder");
        }
    }
}
