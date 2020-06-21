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

namespace com.espertech.esper.common.client
{
    /// <summary>
    ///     The assembly of a compile EPL module or EPL fire-and-forget query.
    /// </summary>
    [Serializable]
    public class EPCompiled
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="assemblies">assemblies containing classes</param>
        /// <param name="manifest">the manifest</param>
        public EPCompiled(
            ICollection<Assembly> assemblies,
            EPCompiledManifest manifest)
        {
            Assemblies = assemblies;
            Manifest = manifest;
        }

        /// <summary>
        ///     Returns a set of assemblies.
        /// </summary>
        /// <value>classes</value>
        public ICollection<Assembly> Assemblies { get; }

        /// <summary>
        ///     Returns a manifest object
        /// </summary>
        /// <returns>manifest</returns>
        public EPCompiledManifest Manifest { get; }
    }
} // end of namespace