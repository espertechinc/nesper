///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.virtualdw;
using com.espertech.esper.util;

namespace com.espertech.esper.view
{
    /// <summary>
    /// Resolves view namespace and name to view factory class, using configuration.
    /// </summary>
    public class ViewResolutionServiceImpl : ViewResolutionService
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private readonly PluggableObjectRegistry _viewObjects;
        private readonly String _optionalNamedWindowName;
        private readonly Type _virtualDataWindowViewFactory;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="viewObjects">is the view objects to use for resolving views, can be both built-in and plug-in views.</param>
        /// <param name="optionalNamedWindowName">Name of the optional named window.</param>
        /// <param name="virtualDataWindowViewFactory">The virtual data window view factory.</param>
        public ViewResolutionServiceImpl(PluggableObjectRegistry viewObjects, String optionalNamedWindowName, Type virtualDataWindowViewFactory)
        {
            _viewObjects = viewObjects;
            _optionalNamedWindowName = optionalNamedWindowName;
            _virtualDataWindowViewFactory = virtualDataWindowViewFactory;
        }
    
        public ViewFactory Create(String nameSpace, String name)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug(".create Creating view factory, namespace=" + nameSpace + " name=" + name);
            }

            Type viewFactoryClass = null;

            var pair = _viewObjects.Lookup(nameSpace, name);
            if (pair != null)
            {
                if (pair.Second.PluggableType == PluggableObjectType.VIEW )
                {
                    // Handle named windows in a configuration that always declares a system-wide virtual view factory
                    if (_optionalNamedWindowName != null && _virtualDataWindowViewFactory != null) {
                        return new VirtualDWViewFactoryImpl(_virtualDataWindowViewFactory, _optionalNamedWindowName, null);
                    }
    
                    viewFactoryClass = pair.First;
                }
                else if (pair.Second.PluggableType == PluggableObjectType.VIRTUALDW)
                {
                    if (_optionalNamedWindowName == null) {
                        throw new ViewProcessingException("Virtual data window requires use with a named window in the create-window syntax");
                    }
                    return new VirtualDWViewFactoryImpl(pair.First, _optionalNamedWindowName, pair.Second.CustomConfigs);
                }
                else
                {
                    throw new ViewProcessingException("Invalid object type '" + pair.Second + "' for view '" + name + "'");
                }
            }
    
            if (viewFactoryClass == null)
            {
                String message = "View name '" + nameSpace + ":" + name + "' is not a known view name";
                throw new ViewProcessingException(message);
            }
    
            ViewFactory viewFactory;
            try
            {
                viewFactory = (ViewFactory) Activator.CreateInstance(viewFactoryClass);
    
                if (Log.IsDebugEnabled)
                {
                    Log.Debug(".create Successfully instantiated view");
                }
            }
            catch (InvalidCastException e)
            {
                String message = "Error casting view factory instance to " + typeof(ViewFactory).FullName + " interface for view '" + name + "'";
                throw new ViewProcessingException(message, e);
            }
            catch (TypeInstantiationException ex)
            {
                String message = "Error invoking view factory constructor for view '" + name;
                message += "' using Activator.CreateInstance";
                throw new ViewProcessingException(message, ex);
            }
            catch (TargetInvocationException ex)
            {
                String message = "Error invoking view factory constructor for view '" + name;
                message += "' using Activator.CreateInstance";
                throw new ViewProcessingException(message, ex);
            }
            catch (MethodAccessException ex)
            {
                String message = "Error invoking view factory constructor for view '" + name;
                message += "', no invocation access for Activator.CreateInstance";
                throw new ViewProcessingException(message, ex);
            }
            catch (MemberAccessException ex)
            {
                String message = "Error invoking view factory constructor for view '" + name;
                message += "', no invocation access for Activator.CreateInstance";
                throw new ViewProcessingException(message, ex);
            }

            return viewFactory;
        }
    }
}
