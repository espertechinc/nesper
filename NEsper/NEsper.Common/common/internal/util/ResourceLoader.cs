///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    ///     Utility class for loading or resolving external resources via URL and class path.
    /// </summary>
    public class ResourceLoader
    {
        /// <summary>
        ///     Resolve a resource into a URL using the URL string or classpath-relative filename and
        ///     using a name for any exceptions thrown.
        /// </summary>
        /// <param name="resourceName">is the name for use in exceptions</param>
        /// <param name="urlOrClasspathResource">is a URL string or classpath-relative filename</param>
        /// <param name="classLoader">class loader</param>
        /// <returns>URL or null if resolution was unsuccessful</returns>
        /// <throws>FileNotFoundException resource not found</throws>
        public static Uri ResolveClassPathOrURLResource(
            string resourceName,
            string urlOrClasspathResource,
            ClassLoader classLoader)
        {
            Uri url;
            try {
                url = new Uri(urlOrClasspathResource);
            }
            catch (MalformedURLException ex) {
                url = GetClasspathResourceAsURL(resourceName, urlOrClasspathResource, classLoader);
            }

            return url;
        }

        /// <summary>
        ///     Returns an URL from an application resource in the classpath.
        ///     <para />
        ///     The method first removes the '/' character from the resource name if
        ///     the first character is '/'.
        ///     <para />
        ///     The lookup order is as follows:
        ///     <para />
        ///     If a thread context class loader exists, use <tt>Thread.currentThread().getResourceAsStream</tt>to obtain an
        ///     InputStream.
        ///     <para />
        ///     If no input stream was returned, use the <tt>Configuration.class.getResourceAsStream</tt>.
        ///     to obtain an InputStream.
        ///     <para />
        ///     If no input stream was returned, use the <tt>Configuration.class.getClassLoader().getResourceAsStream</tt>.
        ///     to obtain an InputStream.
        ///     <para />
        ///     If no input stream was returned, throw an Exception.
        /// </summary>
        /// <param name="resourceName">is the name for use in exceptions</param>
        /// <param name="resource">is the classpath-relative filename to resolve into a URL</param>
        /// <param name="classLoader">class loader</param>
        /// <returns>URL for resource</returns>
        /// <throws>FileNotFoundException resource not found</throws>
        public static Uri GetClasspathResourceAsURL(
            string resourceName,
            string resource,
            ClassLoader classLoader)
        {
            var stripped = resource.StartsWith("/") ? resource.Substring(1) : resource;

            Uri url = null;
            if (classLoader != null) {
                url = classLoader.GetResource(stripped);
            }

            if (url == null) {
                url = typeof(ResourceLoader).GetResource(resource);
            }

            if (url == null) {
                url = typeof(ResourceLoader).ClassLoader.GetResource(stripped);
            }

            if (url == null) {
                throw new FileNotFoundException(resourceName + " resource '" + resource + "' not found");
            }

            return url;
        }
    }
} // end of namespace