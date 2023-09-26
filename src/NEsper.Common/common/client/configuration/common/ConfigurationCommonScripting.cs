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
    ///     Holds scripting settings.
    /// </summary>
    [Serializable]
    public class ConfigurationCommonScripting
    {
        /// <summary>
        ///     Ctor - sets up defaults.
        /// </summary>
        internal ConfigurationCommonScripting()
        {
            Engines = new HashSet<string>();
        }

        /// <summary>
        ///     Returns indicator whether query plan logging is enabled or not.
        /// </summary>
        /// <value>indicator</value>
        public ISet<string> Engines { get; set; }

        /// <summary>
        /// Adds an engine type name.
        /// </summary>
        /// <param name="typeName"></param>
        public ConfigurationCommonScripting AddEngine(string typeName)
        {
            Engines.Add(typeName);
            return this;
        }

        /// <summary>
        /// Adds an engine type name.
        /// </summary>
        /// <param name="engineType">the engine type</param>
        public ConfigurationCommonScripting AddEngine(Type engineType)
        {
            return AddEngine(engineType.FullName);
        }

        /// <summary>
        /// Adds an engine type name.
        /// </summary>
        /// <typeparam name="T">the engine type</typeparam>
        public ConfigurationCommonScripting AddEngine<T>()
        {
            return AddEngine(typeof(T));
        }
    }
} // end of namespace