///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.bytecodemodel.@base
{
    public class CodegenSymbolProviderEmpty : CodegenSymbolProvider
    {
        public static readonly CodegenSymbolProviderEmpty INSTANCE = new CodegenSymbolProviderEmpty();

        private CodegenSymbolProviderEmpty()
        {
        }

        public void Provide(IDictionary<string, Type> symbols)
        {
        }
    }
} // end of namespace