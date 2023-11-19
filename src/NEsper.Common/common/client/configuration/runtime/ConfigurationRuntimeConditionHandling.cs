///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using com.espertech.esper.common.client.hook.condition;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.configuration.runtime
{
    /// <summary>
    ///     Configuration object for defining condition handling behavior.
    /// </summary>
    public class ConfigurationRuntimeConditionHandling
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ConfigurationRuntimeConditionHandling()
        {
        }

        /// <summary>
        /// Constructor for JSON deserializer.
        /// </summary>
        /// <param name="handlerFactories"></param>
        [JsonConstructor]
        public ConfigurationRuntimeConditionHandling(IList<string> handlerFactories)
        {
            HandlerFactories = handlerFactories;
        }

        /// <summary>
        ///     Returns the list of condition handler factory class names,
        ///     see <seealso cref="ConditionHandlerFactory" />
        /// </summary>
        /// <value>list of fully-qualified class names</value>
        public IList<string> HandlerFactories { get; private set; }

        /// <summary>
        ///     Add an condition handler factory class name.
        ///     <para />
        ///     Provide a fully-qualified class name of the implementation
        ///     of the <seealso cref="ConditionHandlerFactory" />interface.
        /// </summary>
        /// <param name="className">class name of condition handler factory</param>
        public void AddClass(string className)
        {
            if (HandlerFactories == null) {
                HandlerFactories = new List<string>();
            }

            HandlerFactories.Add(className);
        }

        /// <summary>
        ///     Add a list of condition handler class names.
        /// </summary>
        /// <param name="classNames">to add</param>
        public void AddClasses(IList<string> classNames)
        {
            if (HandlerFactories == null) {
                HandlerFactories = new List<string>();
            }

            HandlerFactories.AddAll(classNames);
        }

        /// <summary>
        ///     Add an condition handler factory class.
        ///     <para />
        ///     The class provided should implement the
        ///     <seealso cref="ConditionHandlerFactory" />interface.
        /// </summary>
        /// <param name="clazz">class of implementation</param>
        public void AddClass(Type clazz)
        {
            AddClass(clazz.FullName);
        }
    }
} // end of namespace