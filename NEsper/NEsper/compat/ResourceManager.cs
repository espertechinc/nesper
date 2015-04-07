///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace com.espertech.esper.compat
{
    /// <summary>
    /// Manages access to named resources
    /// </summary>

    public sealed class ResourceManager
    {
        private static List<string> m_searchPath;

        /// <summary>
        /// Gets or sets the search path.
        /// </summary>
        /// <value>The search path.</value>
        public static IEnumerable<string> SearchPath
        {
            get { return m_searchPath; }
            set { m_searchPath = new List<string>(value); }
        }

        /// <summary>
        /// Adds to the search path
        /// </summary>
        /// <param name="searchPathElement"></param>

        public static void AddSearchPathElement(String searchPathElement)
        {
            if (!m_searchPath.Contains(searchPathElement))
            {
                m_searchPath.Add(searchPathElement);
            }
        }

        /// <summary>
        /// Resolves a resource and returns the file info.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="searchPath">The search path.</param>
        /// <returns></returns>

        public static FileInfo ResolveResourceFile(string name, string searchPath)
        {
            name = name.Replace('/', '\\').TrimStart('\\');

            string filename = Path.Combine(searchPath, name.Replace('/', '\\'));
            if (File.Exists(filename))
            {
                return new FileInfo(filename);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Resolves a resource and returns the file info.
        /// </summary>
        /// <param name="name">The name.</param>

        public static FileInfo ResolveResourceFile(string name)
        {
            foreach (String pathElement in SearchPath)
            {
                FileInfo fileInfo = ResolveResourceFile(name, pathElement);
                if ( fileInfo != null )
                {
                    return fileInfo;
                }
            }

            if (File.Exists(name))
            {
                return new FileInfo(name);
            }

            return null;
        }

        /// <summary>
        /// Resolves a resource and the URL for the resource
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>

        public static Uri ResolveResourceURL(string name)
        {
            if (Uri.IsWellFormedUriString(name, UriKind.Absolute)) {
                return new Uri(name, UriKind.Absolute);
            }

            FileInfo fileInfo = ResolveResourceFile(name);
            if (fileInfo != null)
            {
                UriBuilder builder = new UriBuilder();
                builder.Scheme = Uri.UriSchemeFile;
                builder.Host = String.Empty;
                builder.Path = fileInfo.FullName;
                return builder.Uri;
            }

            return null;
        }

        /// <summary>
        /// Attempts to retrieve the resource identified by the specified
        /// name as a stream.  If the stream can not be retrieved, this
        /// method returns null.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>

        public static Stream GetResourceAsStream(string name)
        {
            if (Uri.IsWellFormedUriString(name, UriKind.Absolute))
            {
                var uri = new Uri(name, UriKind.Absolute);
                return (new WebClient()).OpenRead(uri);
            }

            // Currently using file-based search and lookup.  This needs to be expanded
            // to cover a broader search-lookup strategy that includes true web-based
            // pathing and internal stream lookups like those in the manifest.
            
            FileInfo fileInfo = ResolveResourceFile(name);
            if (fileInfo != null)
            {
                Stream stream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read);
                return stream;
            }

            return null;
        }

        static void AddDefaultSearchPath()
        {
            m_searchPath.Add(Environment.CurrentDirectory);
            m_searchPath.Add(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            m_searchPath.Add(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
        }

        /// <summary>
        /// Initializes the class
        /// </summary>

        static ResourceManager()
        {
            m_searchPath = new List<string>();

            var settings = CompatSettings.Default;
            if ( settings != null ) {
                if (settings.SearchPath != null) {
                    foreach( var path in settings.SearchPath ) {
                        var testPath = Path.GetFullPath(path);
                        if (Directory.Exists(testPath)) {
                            m_searchPath.Add(testPath);
                        }
                    }   
                }

                if (settings.UseDefaultSearchPath) {
                    AddDefaultSearchPath();
                }
            } else {
                AddDefaultSearchPath();
            }
        }
    }
}
