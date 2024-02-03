///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.bytecodemodel.@base
{
    public class ProxyCodegenSymbolProvider : CodegenSymbolProvider
    {
        public Action<IDictionary<string, Type>> ProcProvide;

        public ProxyCodegenSymbolProvider()
        {
        }

        public ProxyCodegenSymbolProvider(Action<IDictionary<string, Type>> procProvide)
        {
            ProcProvide = procProvide;
        }

        public void Provide(IDictionary<string, Type> symbols)
        {
            ProcProvide.Invoke(symbols);
        }
    }
}