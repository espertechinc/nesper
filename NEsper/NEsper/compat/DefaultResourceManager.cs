///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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

    public sealed class DefaultResourceManager : IResourceManager
    {
        private static List<string> _searchPath;

        /// <summary>
        /// Gets or sets the search path.
        /// </summary>
        /// <value>The search path.</value>
        public IEnumerable<string> SearchPath
        {
            get => _searchPath;
            set => _searchPath = new List<string>(value);
        }

        /// <summary>
        /// Adds to the search path
        /// </summary>
        /// <param name="searchPathElement"></param>

        public void AddSearchPathElement(String searchPathElement)
        {
            if (!_searchPath.Contains(searchPathElement))
            {
                _searchPath.Add(searchPathElement);
            }
        }

        /// <summary>
        /// Resolves a resource and returns the file INFO.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="searchPath">The search path.</param>
        /// <returns></returns>

        public FileInfo ResolveResourceFile(string name, string searchPath)
        {
            name = name.Replace('/', '\\').TrimStart('\\');

            var filename = Path.Combine(searchPath, name.Replace('/', '\\'));
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
        /// Resolves a resource and returns the file INFO.
        /// </summary>
        /// <param name="name">The name.</param>

        public FileInfo ResolveResourceFile(string name)
        {
            foreach (var pathElement in SearchPath)
            {
                var fileInfo = ResolveResourceFile(name, pathElement);
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

        public Uri ResolveResourceURL(string name)
        {
            if (Uri.IsWellFormedUriString(name, UriKind.Absolute)) {
                return new Uri(name, UriKind.Absolute);
            }

            var fileInfo = ResolveResourceFile(name);
            if (fileInfo != null)
            {
                var builder = new UriBuilder();
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

        public Stream GetResourceAsStream(string name)
        {
            if (Uri.IsWellFormedUriString(name, UriKind.Absolute))
            {
                var uri = new Uri(name, UriKind.Absolute);
                return (new WebClient()).OpenRead(uri);
            }

            // Currently using file-based search and lookup.  This needs to be expanded
            // to cover a broader search-lookup strategy that includes true web-based
            // pathing and internal stream lookups like those in the manifest.
            
            var fileInfo = ResolveResourceFile(name);
            if (fileInfo != null)
            {
                Stream stream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read);
                return stream;
            }

            return null;
        }

        /// <summary>
        /// Adds the default search path.
        /// </summary>
        public void AddDefaultSearchPath()
        {
            _searchPath.Add(Environment.CurrentDirectory);
            _searchPath.Add(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            _searchPath.Add(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultResourceManager" /> class.
        /// </summary>
        /// <param name="useDefaultSearchPath">if set to <c>true</c> [use default search path].</param>
        /// <param name="searchPath">The search path.</param>
        public DefaultResourceManager(bool useDefaultSearchPath, params string[] searchPath)
        {
            _searchPath = new List<string>();

            if (searchPath != null) {
                foreach (var path in searchPath) {
                    var testPath = Path.GetFullPath(path);
                    if (Directory.Exists(testPath)) {
                        _searchPath.Add(testPath);
                    }
                }
            }

            if (useDefaultSearchPath)
            {
                AddDefaultSearchPath();
            }
        }
    }
}
