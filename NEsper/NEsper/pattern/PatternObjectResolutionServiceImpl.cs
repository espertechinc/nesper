///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.spec;
using com.espertech.esper.pattern.guard;
using com.espertech.esper.pattern.observer;
using com.espertech.esper.util;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// Resolves pattern object namespace and name to guard or observer factory class, using configuration.
    /// </summary>
    public class PatternObjectResolutionServiceImpl : PatternObjectResolutionService
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IContainer _container;
        private readonly PluggableObjectCollection _patternObjects;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="patternObjects">is the pattern plug-in objects configured</param>
        public PatternObjectResolutionServiceImpl(IContainer container, PluggableObjectCollection patternObjects)
        {
            _container = container;
            _patternObjects = patternObjects;
        }

        public ObserverFactory Create(PatternObserverSpec spec)
        {
            Object result = CreateFactory(spec, PluggableObjectType.PATTERN_OBSERVER);
            ObserverFactory factory;
            try
            {
                factory = (ObserverFactory)result;

                if (Log.IsDebugEnabled)
                {
                    Log.Debug(".create Successfully instantiated observer");
                }
            }
            catch (InvalidCastException e)
            {
                String message = "Error casting observer factory instance to " + typeof(ObserverFactory).FullName + " interface for observer '" + spec.ObjectName + "'";
                throw new PatternObjectException(message, e);
            }
            return factory;
        }

        public GuardFactory Create(PatternGuardSpec spec)
        {
            Object result = CreateFactory(spec, PluggableObjectType.PATTERN_GUARD);
            GuardFactory factory;
            try
            {
                factory = (GuardFactory)result;

                if (Log.IsDebugEnabled)
                {
                    Log.Debug(".create Successfully instantiated guard");
                }
            }
            catch (InvalidCastException e)
            {
                String message = "Error casting guard factory instance to " + typeof(GuardFactory).FullName + " interface for guard '" + spec.ObjectName + "'";
                throw new PatternObjectException(message, e);
            }
            return factory;
        }

        private Object CreateFactory(ObjectSpec spec, PluggableObjectType type)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug(".create Creating factory, spec=" + spec);
            }

            // Find the factory class for this pattern object
            Type factoryClass = null;

            IDictionary<String, Pair<Type, PluggableObjectEntry>> namespaceMap = _patternObjects.Pluggables.Get(spec.ObjectNamespace);
            if (namespaceMap != null)
            {
                Pair<Type, PluggableObjectEntry> pair = namespaceMap.Get(spec.ObjectName);
                if (pair != null)
                {
                    if (pair.Second.PluggableType == type)
                    {
                        factoryClass = pair.First;
                    }
                    else
                    {
                        // invalid type: expecting observer, got guard
                        if (type == PluggableObjectType.PATTERN_GUARD)
                        {
                            throw new PatternObjectException("Pattern observer function '" + spec.ObjectName + "' cannot be used as a pattern guard");
                        }
                        else
                        {
                            throw new PatternObjectException("Pattern guard function '" + spec.ObjectName + "' cannot be used as a pattern observer");
                        }
                    }
                }
            }

            if (factoryClass == null)
            {
                if (type == PluggableObjectType.PATTERN_GUARD)
                {
                    String message = "Pattern guard name '" + spec.ObjectName + "' is not a known pattern object name";
                    throw new PatternObjectException(message);
                }
                else if (type == PluggableObjectType.PATTERN_OBSERVER)
                {
                    String message = "Pattern observer name '" + spec.ObjectName + "' is not a known pattern object name";
                    throw new PatternObjectException(message);
                }
                else
                {
                    throw new PatternObjectException("Pattern object type '" + type + "' not known");
                }
            }

            Object result;
            try {
                result = _container.CreateInstance<object>(factoryClass);
            }
            catch (Exception ex) when (
                ex is TypeInstantiationException ||
                ex is TargetInvocationException ||
                ex is ArgumentException || 
                ex is MemberAccessException) {
                String message = "Error invoking pattern object factory constructor for object '" + spec.ObjectName;
                message += "', no invocation access for Container.CreateInstance";
                throw new PatternObjectException(message, ex);
            }
            return result;
        }
    }
}
