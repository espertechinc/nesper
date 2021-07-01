///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.configuration.common
{
    /// <summary>
    ///     Configuration object for event types with super-types and timestamp.
    /// </summary>
    [Serializable]
    public class ConfigurationCommonEventTypeWithSupertype
    {
        private string endTimestampPropertyName;
        private string startTimestampPropertyName;
        private ISet<string> superTypes;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="superTypes">super types</param>
        protected ConfigurationCommonEventTypeWithSupertype(ISet<string> superTypes)
        {
            this.superTypes = new LinkedHashSet<string>(superTypes);
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        public ConfigurationCommonEventTypeWithSupertype()
        {
            superTypes = new LinkedHashSet<string>();
        }

        /// <summary>
        ///     Sets the super types.
        /// </summary>
        /// <value>set of super type names</value>
        public ISet<string> SuperTypes {
            set => superTypes = value;
            get => superTypes;
        }

        /// <summary>
        ///     Returns the property name of the property providing the start timestamp value.
        /// </summary>
        /// <returns>start timestamp property name</returns>
        public string StartTimestampPropertyName {
            get => startTimestampPropertyName;
            set => startTimestampPropertyName = value;
        }

        /// <summary>
        ///     Returns the property name of the property providing the end timestamp value.
        /// </summary>
        /// <returns>end timestamp property name</returns>
        public string EndTimestampPropertyName {
            get => endTimestampPropertyName;
            set => endTimestampPropertyName = value;
        }
    }
} // end of namespace