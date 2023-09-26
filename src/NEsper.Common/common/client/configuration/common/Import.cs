﻿///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;

namespace com.espertech.esper.common.client.configuration.common
{
    [Serializable]
    public abstract class Import
    {
        public abstract Type Resolve(
            string providedTypeName,
#if DEPRECATED
            ClassForNameProvider classForNameProvider
#else
            TypeResolver typeResolver
#endif
        );
    }
}