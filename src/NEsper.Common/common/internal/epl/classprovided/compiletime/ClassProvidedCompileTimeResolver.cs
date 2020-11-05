///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.classprovided.core;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat.collections;
namespace com.espertech.esper.common.@internal.epl.classprovided.compiletime
{
	public interface ClassProvidedCompileTimeResolver : CompileTimeResolver {
	    ClassProvided ResolveClass(string name);

	    Pair<Type, ImportSingleRowDesc> ResolveSingleRow(string name);

	    Type ResolveAggregationFunction(string name);

	    Pair<Type, string[]> ResolveAggregationMultiFunction(string name);

	    bool IsEmpty();

	    void AddTo(ICollection<Type> types);

	    void RemoveFrom(ICollection<Type> types);
	}
} // end of namespace
