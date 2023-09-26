///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.compile.compiler
{
    public interface CompilerAbstractionClassCollection
    {
        IDictionary<string, byte[]> Classes { get; }

        void Add(IDictionary<string, byte[]> bytes);

        void Remove(string name);
    }
} // end of namespace