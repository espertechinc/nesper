///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.artifact;
using com.espertech.esper.common.@internal.epl.classprovided.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.classprovided.compiletime
{
    public class ClassProvidedCompileTimeResolverEmpty : ClassProvidedCompileTimeResolver
    {
        public static readonly ClassProvidedCompileTimeResolverEmpty INSTANCE =
            new ClassProvidedCompileTimeResolverEmpty();

        private ClassProvidedCompileTimeResolverEmpty()
        {
        }

        public ClassProvided ResolveClass(string name)
        {
            return null;
        }

        public Pair<Type, ImportSingleRowDesc> ResolveSingleRow(string name)
        {
            return null;
        }

        public Type ResolveAggregationFunction(string name)
        {
            return null;
        }

        public Pair<Type, string[]> ResolveAggregationMultiFunction(string name)
        {
            return null;
        }

        public bool IsEmpty()
        {
            return true;
        }

        public void AddTo(ICollection<Artifact> artifacts)
        {
        }

        public void RemoveFrom(ICollection<Artifact> artifacts)
        {
        }
    }
} // end of namespace