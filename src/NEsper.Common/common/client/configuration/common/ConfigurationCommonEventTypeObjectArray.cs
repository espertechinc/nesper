///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.common.client.configuration.common
{
    /// <summary>
    ///     Configuration object for Object array-based event types.
    /// </summary>
    public class ConfigurationCommonEventTypeObjectArray : ConfigurationCommonEventTypeWithSupertype
    {
        /// <summary>
        ///     Message for single supertype for object-arrays.
        /// </summary>
        public const string SINGLE_SUPERTYPE_MSG = "Object-array event types only allow a single supertype";

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="superTypes">super types</param>
        public ConfigurationCommonEventTypeObjectArray(ISet<string> superTypes)
            : base(superTypes)
        {
            if (superTypes.Count > 1) {
                throw new ConfigurationException("Object-array event types may not have multiple supertypes");
            }
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        public ConfigurationCommonEventTypeObjectArray()
        {
        }
    }
} // end of namespace