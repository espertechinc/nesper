///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.util;
using com.espertech.esper.codegen.core;

namespace com.espertech.esper.codegen.compile
{
    public interface ICodegenCompiler
    {
        bool IsDebugEnabled { get; }

        EventPropertyGetter Compile(
            ICodegenClass clazz,
            ClassLoaderProvider classLoaderProvider,
            Type interfaceClass,
            string classLevelComment);
    }
}