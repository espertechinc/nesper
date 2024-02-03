///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.util
{
#if DEPRECATED
    /// <summary>Default provider for classname lookups.</summary>
    public class ClassForNameProviderDefault : ClassForNameProvider
    {
        public const string NAME = "ClassForNameProvider";

        public static readonly ClassForNameProviderDefault INSTANCE = new ClassForNameProviderDefault();

        private ClassForNameProviderDefault()
        {
        }

        public Type ClassForName(string className)
        {
#if false
            var simpleType = TypeHelper.GetTypeForSimpleName(className, false, false);
            if (simpleType != null) {
                return simpleType;
            }
#endif

            return TypeHelper.ResolveType(className, true);
        }
    }
#endif
} // end of namespace