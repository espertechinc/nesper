///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.compat
{
    public interface IResourceManager
    {
        /// <summary>
        /// Resolves a resource and returns the file INFO.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="searchPath">The search path.</param>
        /// <returns></returns>
        FileInfo ResolveResourceFile(string name, string searchPath);

        /// <summary>
        /// Resolves a resource and returns the file INFO.
        /// </summary>
        /// <param name="name">The name.</param>
        FileInfo ResolveResourceFile(string name);

        /// <summary>
        /// Resolves a resource and the URL for the resource
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Uri ResolveResourceURL(string name);

        /// <summary>
        /// Attempts to retrieve the resource identified by the specified
        /// name as a stream.  If the stream can not be retrieved, this
        /// method returns null.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Stream GetResourceAsStream(string name);
    }
}