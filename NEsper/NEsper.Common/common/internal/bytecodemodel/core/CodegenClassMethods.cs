///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.bytecodemodel.core
{
    public class CodegenClassMethods
    {
        public IList<CodegenMethodWGraph> PublicMethods { get; } = new List<CodegenMethodWGraph>(2);

        public IList<CodegenMethodWGraph> PrivateMethods { get; } = new List<CodegenMethodWGraph>();
    }
} // end of namespace