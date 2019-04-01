///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using com.espertech.esper.collection;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.pattern.guard;
using com.espertech.esper.common.@internal.epl.pattern.observer;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.pattern.core
{
    /// <summary>
    ///     Resolves pattern object namespace and name to guard or observer factory class, using configuration.
    /// </summary>
    public class PatternObjectResolutionServiceImpl : PatternObjectResolutionService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly PluggableObjectCollection patternObjects;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="patternObjects">is the pattern plug-in objects configured</param>
        public PatternObjectResolutionServiceImpl(PluggableObjectCollection patternObjects)
        {
            this.patternObjects = patternObjects;
        }

        public ObserverForge Create(PatternObserverSpec spec)
        {
            var result = CreateForge(spec, PluggableObjectType.PATTERN_OBSERVER);
            ObserverForge forge;
            try {
                forge = (ObserverForge) result;

                if (Log.IsDebugEnabled) {
                    Log.Debug(".create Successfully instantiated observer");
                }
            }
            catch (InvalidCastException e) {
                var message = "Error casting observer factory instance to " + typeof(ObserverFactory).Name +
                              " interface for observer '" + spec.ObjectName + "'";
                throw new PatternObjectException(message, e);
            }

            return forge;
        }

        public GuardForge Create(PatternGuardSpec spec)
        {
            var result = CreateForge(spec, PluggableObjectType.PATTERN_GUARD);
            GuardForge forge;
            try {
                forge = (GuardForge) result;

                if (Log.IsDebugEnabled) {
                    Log.Debug(".create Successfully instantiated guard");
                }
            }
            catch (InvalidCastException e) {
                var message = "Error casting guard forge instance to " + typeof(GuardForge).Name +
                              " interface for guard '" + spec.ObjectName + "'";
                throw new PatternObjectException(message, e);
            }

            return forge;
        }

        private object CreateForge(ObjectSpec spec, PluggableObjectType type)
        {
            if (Log.IsDebugEnabled) {
                Log.Debug(".create Creating factory, spec=" + spec);
            }

            // Find the factory class for this pattern object
            Type forgeClass = null;

            var namespaceMap =
                patternObjects.Pluggables.Get(spec.ObjectNamespace);
            if (namespaceMap != null) {
                var pair = namespaceMap.Get(spec.ObjectName);
                if (pair != null) {
                    if (pair.Second.PluggableType == type) {
                        forgeClass = pair.First;
                    }
                    else {
                        // invalid type: expecting observer, got guard
                        if (type == PluggableObjectType.PATTERN_GUARD) {
                            throw new PatternObjectException(
                                "Pattern observer function '" + spec.ObjectName +
                                "' cannot be used as a pattern guard");
                        }

                        throw new PatternObjectException(
                            "Pattern guard function '" + spec.ObjectName + "' cannot be used as a pattern observer");
                    }
                }
            }

            if (forgeClass == null) {
                if (type == PluggableObjectType.PATTERN_GUARD) {
                    var message = "Pattern guard name '" + spec.ObjectName + "' is not a known pattern object name";
                    throw new PatternObjectException(message);
                }

                if (type == PluggableObjectType.PATTERN_OBSERVER) {
                    var message = "Pattern observer name '" + spec.ObjectName + "' is not a known pattern object name";
                    throw new PatternObjectException(message);
                }

                throw new PatternObjectException("Pattern object type '" + type + "' not known");
            }

            object result;
            try {
                result = forgeClass.NewInstance();
            }
            catch (MemberAccessException e) {
                var message = "Error invoking pattern object factory constructor for object '" + spec.ObjectName;
                message += "', no invocation access for Class.newInstance";
                throw new PatternObjectException(message, e);
            }

            return result;
        }
    }
} // end of namespace