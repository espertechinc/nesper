///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.client.util
{
    /// <summary>
    /// Provider of lookup of a class name resolving into a class.
    /// </summary>
    public interface ClassForNameProvider
    {
        /// <summary>Name.</summary>
        // string NAME = "ClassForNameProvider";
    
        /// <summary>
        /// Lookup class name returning class.
        /// </summary>
        /// <param name="className">to look up</param>
        /// <exception cref="ClassNotFoundException">if the class cannot be found</exception>
        /// <returns>class</returns>
        Type ClassForName(string className) ;
    }
} // end of namespace
