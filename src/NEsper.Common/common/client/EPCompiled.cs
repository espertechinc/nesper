///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.common.client.assembly;
using com.espertech.esper.compat.collections;

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
        /// <param name="assembliesWithImage">assemblies containing classes</param>
        /// <param name="manifest">the manifest</param>
        public EPCompiled(
            ICollection<Pair<Assembly, byte[]>> assembliesWithImage,
            EPCompiledManifest manifest,
            CompilationContext compilationContext)
        {
            AssembliesWithImage = assembliesWithImage;
            Manifest = manifest;
            CompilationContext = compilationContext;
        }

        /// <summary>
        ///     Returns a set of assemblies.
        /// </summary>
        public IEnumerable<Assembly> Assemblies {
            get => AssembliesWithImage.Select(_ => _.First);
        }

        /// <summary>
        ///     Returns a set of assemblies with images.
        /// </summary>
        public ICollection<Pair<Assembly, byte[]>> AssembliesWithImage { get; }

        /// <summary>
        ///     Returns a manifest object
        /// </summary>
        /// <returns>manifest</returns>
        public EPCompiledManifest Manifest { get; }
        
        /// <summary>
        /// Returns the compilation context.
        /// </summary>
        public CompilationContext CompilationContext { get; }
    }
} // end of namespace