///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.bytecodemodel.core
{
    public class CodegenClassProperties
    {
        public IList<CodegenPropertyWGraph> PublicProperties { get; } = new List<CodegenPropertyWGraph>();
        public IList<CodegenPropertyWGraph> PrivateProperties { get; } = new List<CodegenPropertyWGraph>();
        public int Count => PublicProperties.Count + PrivateProperties.Count;
    }
} // end of namespace