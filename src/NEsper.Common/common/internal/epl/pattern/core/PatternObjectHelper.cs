///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.pattern.observer;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.pattern.core
{
    /// <summary>
    ///     Helper producing a repository of built-in pattern objects.
    /// </summary>
    public class PatternObjectHelper
    {
        static PatternObjectHelper()
        {
            BuiltinPatternObjects = new PluggableObjectCollection();
            foreach (var guardEnum in EnumHelper.GetValues<GuardEnum>()) {
                BuiltinPatternObjects.AddObject(
                    guardEnum.GetNamespace(),
                    guardEnum.GetName(),
                    guardEnum.GetClazz(),
                    PluggableObjectType.PATTERN_GUARD);
            }

            foreach (var observerEnum in EnumHelper.GetValues<ObserverEnum>()) {
                BuiltinPatternObjects.AddObject(
                    observerEnum.GetNamespace(),
                    observerEnum.GetName(),
                    observerEnum.GetImplementationType(),
                    PluggableObjectType.PATTERN_OBSERVER);
            }
        }

        /// <summary>
        ///     Returns the built-in pattern objects.
        /// </summary>
        /// <value>collection of built-in pattern objects.</value>
        public static PluggableObjectCollection BuiltinPatternObjects { get; }
    }
} // end of namespace