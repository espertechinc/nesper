///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

namespace com.espertech.esper.compat
{
    /// <summary>
    /// AssemblyResolver provides resolution of assemblies that may or may not be materialized into the process space.
    /// <param name="assemblyName">Name of the assembly.</param>
    /// <returns></returns>
    /// </summary>
    public delegate Assembly AssemblyResolver(AssemblyName assemblyName);
}
