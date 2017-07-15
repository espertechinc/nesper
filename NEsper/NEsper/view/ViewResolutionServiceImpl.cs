///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.virtualdw;

namespace com.espertech.esper.view
{
    /// <summary>
    /// Resolves view namespace and name to view factory class, using configuration.
    /// </summary>
    public class ViewResolutionServiceImpl : ViewResolutionService {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private readonly PluggableObjectRegistry viewObjects;
        private readonly string optionalNamedWindowName;
        private readonly Type virtualDataWindowViewFactory;
    
        public ViewResolutionServiceImpl(PluggableObjectRegistry viewObjects, string optionalNamedWindowName, Type virtualDataWindowViewFactory) {
            this.viewObjects = viewObjects;
            this.optionalNamedWindowName = optionalNamedWindowName;
            this.virtualDataWindowViewFactory = virtualDataWindowViewFactory;
        }
    
        public ViewFactory Create(string nameSpace, string name) {
            if (Log.IsDebugEnabled) {
                Log.Debug(".create Creating view factory, @namespace =" + nameSpace + " name=" + name);
            }
    
            Type viewFactoryClass = null;
    
            Pair<Type, PluggableObjectEntry> pair = viewObjects.Lookup(nameSpace, name);
            if (pair != null) {
                if (pair.Second.Type == PluggableObjectType.VIEW) {
                    // Handle named windows in a configuration that always declares a system-wide virtual view factory
                    if (optionalNamedWindowName != null && virtualDataWindowViewFactory != null) {
                        return new VirtualDWViewFactoryImpl(virtualDataWindowViewFactory, optionalNamedWindowName, null);
                    }
    
                    viewFactoryClass = pair.First;
                } else if (pair.Second.Type == PluggableObjectType.VIRTUALDW) {
                    if (optionalNamedWindowName == null) {
                        throw new ViewProcessingException("Virtual data window requires use with a named window in the create-window syntax");
                    }
                    return new VirtualDWViewFactoryImpl(pair.First, optionalNamedWindowName, pair.Second.CustomConfigs);
                } else {
                    throw new ViewProcessingException("Invalid object type '" + pair.Second + "' for view '" + name + "'");
                }
            }
    
            if (viewFactoryClass == null) {
                string message = nameSpace == null ?
                        "View name '" + name + "' is not a known view name" :
                        "View name '" + nameSpace + ":" + name + "' is not a known view name";
                throw new ViewProcessingException(message);
            }
    
            ViewFactory viewFactory;
            try {
                viewFactory = (ViewFactory) viewFactoryClass.NewInstance();
    
                if (Log.IsDebugEnabled) {
                    Log.Debug(".create Successfully instantiated view");
                }
            } catch (ClassCastException e) {
                string message = "Error casting view factory instance to " + typeof(ViewFactory).Name + " interface for view '" + name + "'";
                throw new ViewProcessingException(message, e);
            } catch (IllegalAccessException e) {
                string message = "Error invoking view factory constructor for view '" + name;
                message += "', no invocation access for Type.newInstance";
                throw new ViewProcessingException(message, e);
            } catch (InstantiationException e) {
                string message = "Error invoking view factory constructor for view '" + name;
                message += "' using Type.newInstance";
                throw new ViewProcessingException(message, e);
            }
    
            return viewFactory;
        }
    }
} // end of namespace
