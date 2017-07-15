///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Net;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.util
{
    /// <summary>
    /// Utility class for loading or resolving external resources via URL and class path.
    /// </summary>
    public class ResourceLoader {
        /// <summary>
        /// Resolve a resource into a URL using the URL string or classpath-relative filename and
        /// using a name for any exceptions thrown.
        /// </summary>
        /// <param name="resourceName">is the name for use in exceptions</param>
        /// <param name="urlOrClasspathResource">is a URL string or classpath-relative filename</param>
        /// <param name="classLoader">class loader</param>
        /// <returns>URL or null if resolution was unsuccessful</returns>
        public static URL ResolveClassPathOrURLResource(string resourceName, string urlOrClasspathResource, ClassLoader classLoader) {
            URL url;
            try {
                url = new URL(urlOrClasspathResource);
            } catch (MalformedURLException ex) {
                url = GetClasspathResourceAsURL(resourceName, urlOrClasspathResource, classLoader);
            }
            return url;
        }
    
        /// <summary>
        /// Returns an URL from an application resource in the classpath.
        /// <para>
        /// The method first removes the '/' character from the resource name if
        /// the first character is '/'.
        /// </para>
        /// <para>
        /// The lookup order is as follows:
        /// </para>
        /// <para>
        /// If a thread context class loader exists, use <tt>Thread.CurrentThread().getResourceAsStream</tt>
        /// to obtain an InputStream.
        /// </para>
        /// <para>
        /// If no input stream was returned, use the <tt>Typeof(Configuration).getResourceAsStream</tt>.
        /// to obtain an InputStream.
        /// </para>
        /// <para>
        /// If no input stream was returned, use the <tt>Typeof(Configuration).ClassLoader.getResourceAsStream</tt>.
        /// to obtain an InputStream.
        /// </para>
        /// <para>
        /// If no input stream was returned, throw an Exception.
        /// </para>
        /// </summary>
        /// <param name="resourceName">is the name for use in exceptions</param>
        /// <param name="resource">is the classpath-relative filename to resolve into a URL</param>
        /// <param name="classLoader">class loader</param>
        /// <returns>URL for resource</returns>
        public static URL GetClasspathResourceAsURL(string resourceName, string resource, ClassLoader classLoader) {
            string stripped = resource.StartsWith("/") ?
                    resource.Substring(1) : resource;
    
            URL url = null;
            if (classLoader != null) {
                url = classLoader.GetResource(stripped);
            }
            if (url == null) {
                url = Typeof(ResourceLoader).GetResource(resource);
            }
            if (url == null) {
                url = Typeof(ResourceLoader).ClassLoader.GetResource(stripped);
            }
            if (url == null) {
                throw new EPException(resourceName + " resource '" + resource + "' not found");
            }
            return url;
        }
    
    
    }
} // end of namespace
