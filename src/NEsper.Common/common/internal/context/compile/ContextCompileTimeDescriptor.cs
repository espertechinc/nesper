///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.util;

namespace com.espertech.esper.common.@internal.context.compile
{
    public class ContextCompileTimeDescriptor
    {
        public ContextCompileTimeDescriptor(
            string contextName,
            string contextModuleName,
            NameAccessModifier contextVisibility,
            ContextPropertyRegistry contextPropertyRegistry,
            ContextControllerPortableInfo[] validationInfos)
        {
            ContextName = contextName;
            ContextModuleName = contextModuleName;
            ContextVisibility = contextVisibility;
            ContextPropertyRegistry = contextPropertyRegistry;
            ValidationInfos = validationInfos;
        }

        public string ContextName { get; }

        public string ContextModuleName { get; }

        public NameAccessModifier ContextVisibility { get; }

        public ContextPropertyRegistry ContextPropertyRegistry { get; }

        public ContextControllerPortableInfo[] ValidationInfos { get; }
    }
} // end of namespace