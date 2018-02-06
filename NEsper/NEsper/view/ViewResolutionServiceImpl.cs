///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using com.espertech.esper.compat.container;
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
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly PluggableObjectRegistry _viewObjects;
        private readonly string _optionalNamedWindowName;
        private readonly Type _virtualDataWindowViewFactory;
    
        public ViewResolutionServiceImpl(PluggableObjectRegistry viewObjects, string optionalNamedWindowName, Type virtualDataWindowViewFactory)
        {
            _viewObjects = viewObjects;
            _optionalNamedWindowName = optionalNamedWindowName;
            _virtualDataWindowViewFactory = virtualDataWindowViewFactory;
        }
    
        public ViewFactory Create(IContainer container, string nameSpace, string name)
        {
            if (Log.IsDebugEnabled) {
                Log.Debug(".create Creating view factory, @namespace =" + nameSpace + " name=" + name);
            }
    
            Type viewFactoryClass = null;
    
            var pair = _viewObjects.Lookup(nameSpace, name);
            if (pair != null)
            {
                if (pair.Second.PluggableType == PluggableObjectType.VIEW)
                {
                    // Handle named windows in a configuration that always declares a system-wide virtual view factory
                    if (_optionalNamedWindowName != null && _virtualDataWindowViewFactory != null)
                    {
                        return new VirtualDWViewFactoryImpl(_virtualDataWindowViewFactory, _optionalNamedWindowName, null);
                    }

                    viewFactoryClass = pair.First;
                }
                else if (pair.Second.PluggableType == PluggableObjectType.VIRTUALDW)
                {
                    if (_optionalNamedWindowName == null)
                    {
                        throw new ViewProcessingException(
                            "Virtual data window requires use with a named window in the create-window syntax");
                    }
                    return new VirtualDWViewFactoryImpl(pair.First, _optionalNamedWindowName, pair.Second.CustomConfigs);
                }
                else
                {
                    throw new ViewProcessingException(
                        "Invalid object type '" + pair.Second + "' for view '" + name + "'");
                }
            }

            if (viewFactoryClass == null)
            {
                var message = nameSpace == null ?
                        "View name '" + name + "' is not a known view name" :
                        "View name '" + nameSpace + ":" + name + "' is not a known view name";
                throw new ViewProcessingException(message);
            }
    
            ViewFactory viewFactory;
            try {
                viewFactory = container.CreateInstance<ViewFactory>(viewFactoryClass);  
                if (Log.IsDebugEnabled) {
                    Log.Debug(".create Successfully instantiated view");
                }
            }
            catch (InvalidCastException e)
            {
                var message = "Error casting view factory instance to " + typeof(ViewFactory).FullName + " interface for view '" + name + "'";
                throw new ViewProcessingException(message, e);
            }
            catch (TypeInstantiationException ex)
            {
                var message = "Error invoking view factory constructor for view '" + name;
                message += "' using Activator.CreateInstance";
                throw new ViewProcessingException(message, ex);
            }
            catch (TargetInvocationException ex)
            {
                var message = "Error invoking view factory constructor for view '" + name;
                message += "' using Activator.CreateInstance";
                throw new ViewProcessingException(message, ex);
            }
            catch (MethodAccessException ex)
            {
                var message = "Error invoking view factory constructor for view '" + name;
                message += "', no invocation access for Activator.CreateInstance";
                throw new ViewProcessingException(message, ex);
            }
            catch (MemberAccessException ex)
            {
                var message = "Error invoking view factory constructor for view '" + name;
                message += "', no invocation access for Activator.CreateInstance";
                throw new ViewProcessingException(message, ex);
            }
    
            return viewFactory;
        }
    }
} // end of namespace
