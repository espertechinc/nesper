///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;
using com.espertech.esper.epl.spec;
using com.espertech.esper.pattern.guard;
using com.espertech.esper.pattern.observer;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// Helper producing a repository of built-in pattern objects.
    /// </summary>
	public class PatternObjectHelper
	{
        static PatternObjectHelper()
	    {
	        BuiltinPatternObjects = new PluggableObjectCollection();
            foreach (GuardEnum guardEnum in EnumHelper.GetValues<GuardEnum>())
	        {
	            BuiltinPatternObjects.AddObject(
                    guardEnum.GetNamespace(), 
                    guardEnum.GetName(), 
                    guardEnum.GetClazz(), 
                    PluggableObjectType.PATTERN_GUARD);
	        }

            foreach (ObserverEnum observerEnum in EnumHelper.GetValues<ObserverEnum>())
	        {
	            BuiltinPatternObjects.AddObject(
                    observerEnum.GetNamespace(),
                    observerEnum.GetName(),
                    observerEnum.GetImplementationType(), 
                    PluggableObjectType.PATTERN_OBSERVER);
	        }
	    }

        /// <summary>Returns the built-in pattern objects.</summary>
        /// <returns>collection of built-in pattern objects.</returns>
        public static PluggableObjectCollection BuiltinPatternObjects { get; private set; }
	}
} // End of namespace
