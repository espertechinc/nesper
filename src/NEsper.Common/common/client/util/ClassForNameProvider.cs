///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.util
{
    /// <summary>
    /// Provider of lookup of a class name resolving into a class.
    /// </summary>
    public interface ClassForNameProvider
    {
        /// <summary>
        /// Lookup class name returning class.
        /// </summary>
        /// <param name="className">to look up</param>
        /// <returns>class</returns>
        Type ClassForName(string className);
    }

    public class ClassForNameProviderConstants
    {
        public const string NAME = "ClassForNameProvider";
    }
} // end of namespace