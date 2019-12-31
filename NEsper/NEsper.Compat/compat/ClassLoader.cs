///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.compat
{
    public interface ClassLoader
    {
        Stream GetResourceAsStream(string resourceName);

        /// <summary>Gets the class.</summary>
        /// <param name="className">Name of the class.</param>
        /// <returns></returns>
        Type GetClass(string className);
    }
}
