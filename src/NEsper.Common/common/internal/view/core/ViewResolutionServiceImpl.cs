///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.virtualdw;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.util.serde;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.view.core
{
    /// <summary>
    ///     Resolves view namespace and name to view factory class, using configuration.
    /// </summary>
    public class ViewResolutionServiceImpl : ViewResolutionService
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ViewResolutionServiceImpl));

        private readonly IContainer container;
        private readonly PluggableObjectRegistry viewObjects;

        public ViewResolutionServiceImpl(
            IContainer container,
            PluggableObjectRegistry viewObjects)
        {
            this.container = container;
            this.viewObjects = viewObjects;
        }

        public ViewFactoryForge Create(
            string nameSpace,
            string name,
            string optionalCreateNamedWindowName)
        {
            if (Log.IsDebugEnabled) {
                Log.Debug(".create Creating view factory, namespace=" + nameSpace + " name=" + name);
            }

            Type viewFactoryClass = null;

            var pair = viewObjects.Lookup(nameSpace, name);
            if (pair != null) {
                if (pair.Second.PluggableType == PluggableObjectType.VIEW) {
                    viewFactoryClass = pair.First;
                }
                else if (pair.Second.PluggableType == PluggableObjectType.VIRTUALDW) {
                    if (optionalCreateNamedWindowName == null) {
                        throw new ViewProcessingException(
                            "Virtual data window requires use with a named window in the create-window syntax");
                    }

                    return new VirtualDWViewFactoryForge(
                        container.SerializerFactory(),
                        pair.First,
                        optionalCreateNamedWindowName,
                        pair.Second.CustomConfigs);
                }
                else {
                    throw new ViewProcessingException(
                        "Invalid object type '" + pair.Second + "' for view '" + name + "'");
                }
            }

            if (viewFactoryClass == null) {
                var message = nameSpace == null
                    ? "View name '" + name + "' is not a known view name"
                    : "View name '" + nameSpace + ":" + name + "' is not a known view name";
                throw new ViewProcessingException(message);
            }

            ViewFactoryForge forge;
            try {
                forge = TypeHelper.Instantiate<ViewFactoryForge>(viewFactoryClass);

                if (Log.IsDebugEnabled) {
                    Log.Debug(".create Successfully instantiated view");
                }
            }
            catch (TypeInstantiationException e) {
                var message = "Error instantiating view factory instance to " +
                              typeof(ViewFactoryForge).CleanName() +
                              " interface for view '" +
                              name +
                              "'";
                throw new ViewProcessingException(message, e);
            }
            catch (InvalidCastException e) {
                var message = "Error casting view factory instance to " +
                              typeof(ViewFactoryForge).CleanName() +
                              " interface for view '" +
                              name +
                              "'";
                throw new ViewProcessingException(message, e);
            }

            return forge;
        }
    }
} // end of namespace