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
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.fabric;

namespace com.espertech.esper.compiler.@internal.util
{
    public class CompilableItem
    {
        public CompilableItem(
            string providerClassName,
            IList<CodegenClass> classes,
            CompilableItemPostCompileLatch postCompileLatch,
            ICollection<IArtifact> artifactsProvided,
            ContextCompileTimeDescriptor contextDescriptor,
            FabricCharge fabricCharge)
        {
            ProviderClassName = providerClassName;
            Classes = classes;
            PostCompileLatch = postCompileLatch;
            ArtifactsProvided = artifactsProvided;
            ContextDescriptor = contextDescriptor;
            FabricCharge = fabricCharge;
        }

        public string ProviderClassName { get; }

        public IList<CodegenClass> Classes { get; }

        public CompilableItemPostCompileLatch PostCompileLatch { get; }

        public ICollection<IArtifact> ArtifactsProvided { get; }
        
        public ContextCompileTimeDescriptor ContextDescriptor { get; }
        
        public FabricCharge FabricCharge { get; }
    }
} // end of namespace