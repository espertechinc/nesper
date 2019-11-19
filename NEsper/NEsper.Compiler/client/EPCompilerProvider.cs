///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compiler.@internal.util;

namespace com.espertech.esper.compiler.client
{
    /// <summary>
    ///     Provides compiler instances.
    /// </summary>
    public class EPCompilerProvider
    {
        /// <summary>
        ///     Return a compiler instance.
        /// </summary>
        /// <value>compiler</value>
        public static EPCompiler Compiler => new EPCompilerImpl();
    }
} // end of namespace