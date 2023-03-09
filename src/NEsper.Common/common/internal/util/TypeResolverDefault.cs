///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.util
{
    public class TypeResolverDefault : TypeResolver
    {
        public static readonly TypeResolver INSTANCE = new TypeResolverDefault();

        private TypeResolverDefault()
        {
        }
        
        public Type ResolveType(
            string typeName,
            bool resolve)
        {
            return TypeHelper.ResolveType(typeName, resolve);
        }
    }
}