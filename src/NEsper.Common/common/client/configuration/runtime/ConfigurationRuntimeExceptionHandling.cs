///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.hook.exception;
using com.espertech.esper.common.client.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.configuration.runtime
{
    /// <summary>
    ///     Configuration object for defining exception handling behavior.
    /// </summary>
    [Serializable]
    public class ConfigurationRuntimeExceptionHandling
    {
        /// <summary>
        ///     Returns the policy to instruct the runtime whether a module un-deploy rethrows runtime exceptions
        ///     that are encountered during the undeploy. By default we are logging exceptions.
        /// </summary>
        /// <returns>indicator</returns>
        public UndeployRethrowPolicy UndeployRethrowPolicy { get; set; } = UndeployRethrowPolicy.WARN;

        /// <summary>
        ///     Returns the list of exception handler factory class names,
        ///     see <seealso cref="ExceptionHandlerFactory" />
        /// </summary>
        /// <value>list of fully-qualified class names</value>
        public IList<string> HandlerFactories { get; set; }

        /// <summary>
        ///     Add an exception handler factory class name.
        ///     <para />
        ///     Provide a fully-qualified class name of the implementation
        ///     of the <seealso cref="ExceptionHandlerFactory" />interface.
        /// </summary>
        /// <param name="exceptionHandlerFactoryClassName">class name of exception handler factory</param>
        public void AddClass(string exceptionHandlerFactoryClassName)
        {
            if (HandlerFactories == null) {
                HandlerFactories = new List<string>();
            }

            HandlerFactories.Add(exceptionHandlerFactoryClassName);
        }

        /// <summary>
        ///     Add a list of exception handler class names.
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
        ///     Add an exception handler factory class.
        ///     <para />
        ///     The class provided should implement the
        ///     <seealso cref="ExceptionHandlerFactory" />interface.
        /// </summary>
        /// <param name="exceptionHandlerFactoryClass">class of implementation</param>
        public void AddClass(Type exceptionHandlerFactoryClass)
        {
            AddClass(exceptionHandlerFactoryClass.FullName);
        }
    }
} // end of namespace