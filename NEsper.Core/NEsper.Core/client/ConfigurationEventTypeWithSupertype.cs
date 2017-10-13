///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.util;

namespace com.espertech.esper.client
{
    /// <summary>
    /// Configuration object for event types with super-types and timestamp.
    /// </summary>
    [Serializable]
    public class ConfigurationEventTypeWithSupertype : MetaDefItem
    {
        /// <summary>Ctor. </summary>
        /// <param name="superTypes">super types</param>
        protected ConfigurationEventTypeWithSupertype(ICollection<String> superTypes)
        {
            SuperTypes = new LinkedHashSet<string>(superTypes);
        }

        /// <summary>Ctor. </summary>
        public ConfigurationEventTypeWithSupertype()
        {
            SuperTypes = new LinkedHashSet<String>();
        }

        /// <summary>Returns the super types, if any. </summary>
        /// <value>set of super type names</value>
        public ICollection<string> SuperTypes { get; set; }

        /// <summary>Returns the property name of the property providing the start timestamp value. </summary>
        /// <value>start timestamp property name</value>
        public string StartTimestampPropertyName { get; set; }

        /// <summary>Returns the property name of the property providing the end timestamp value. </summary>
        /// <value>end timestamp property name</value>
        public string EndTimestampPropertyName { get; set; }
    }
}