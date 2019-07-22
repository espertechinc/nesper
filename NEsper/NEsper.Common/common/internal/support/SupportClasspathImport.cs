///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.support
{
    public class SupportClasspathImport
    {
        public static ImportServiceCompileTime GetInstance(IContainer container)
        {
            return container.ResolveSingleton(
                () => new ImportServiceCompileTime(
                    container,
                    null,
                    null,
                    null,
                    MathContext.DECIMAL32,
                    false,
                    false));
        }
    }
} // end of namespace