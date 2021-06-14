///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    /// Utility for inspecting and comparing URI.
    /// </summary>
    public class URIUtil
    {
        /// <summary>
        /// Determines whether the specified URI is opaque.  A URI is opaque if
        /// is not hierarchical.  An example of an opaque URL is the mailto URI.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns>
        /// 	<c>true</c> if the specified URI is opaque; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsOpaque(Uri uri)
        {
            return false;
        }


        /// <summary>
        /// Given a child URI and a map of factory URIs, inspect the child URI against the factory
        /// URIs and return a collection of entries for which the child URI falls within or is equals
        /// to the factory URI.
        /// </summary>
        /// <param name="child">is the child URI to match against factory URIs</param>
        /// <param name="uris">is a map of factory URI and an object</param>
        /// <returns>matching factory URIs, if any</returns>
        public static ICollection<KeyValuePair<Uri, V>> FilterSort<V>(
            Uri child,
            IDictionary<Uri, V> uris)
        {
            var childPathIsOpaque = IsOpaque(child);
            var childPathIsRelative = !child.IsAbsoluteUri;
            var childPathElements = ParsePathElements(child);

            var result = new OrderedListDictionary<int, KeyValuePair<Uri, V>>();
            foreach (var entry in uris) {
                var factoryUri = entry.Key;

                // handle opaque (mailto:) and relative (a/b) using equals
                if (childPathIsOpaque || childPathIsRelative || !factoryUri.IsAbsoluteUri || IsOpaque(factoryUri)) {
                    if (factoryUri.Equals(child)) {
                        result.Put(int.MinValue, entry); // Equals is a perfect match
                    }

                    continue;
                }

                // handle absolute URIs, compare scheme and authority if present
                if (((child.Scheme != null) && (factoryUri.Scheme == null)) ||
                    ((child.Scheme == null) && (factoryUri.Scheme != null))) {
                    continue;
                }

                if ((child.Scheme != null) && (!child.Scheme.Equals(factoryUri.Scheme))) {
                    continue;
                }

                if (((child.Authority != null) && (factoryUri.Authority == null)) ||
                    ((child.Authority == null) && (factoryUri.Authority != null))) {
                    continue;
                }

                if ((child.Authority != null) && (child.Authority != factoryUri.Authority)) {
                    continue;
                }

                // Match the child
                string[] factoryPathElements = ParsePathElements(factoryUri);
                int score = ComputeScore(childPathElements, factoryPathElements);
                if (score > 0) {
                    result.Put(score, entry); // Partial match if score is positive
                }
            }

            return result.Values;
        }

        private static string GetPath(Uri uri)
        {
            try {
                return uri.AbsolutePath;
            }
            catch (InvalidOperationException) {
                return uri.OriginalString;
            }
        }

        public static string[] ParsePathElements(Uri uri)
        {
            var path = GetPath(uri);
            if (path == null) {
                return new string[0];
            }

            while (path.StartsWith("/")) {
                path = path.Substring(1);
            }

            var split = path.Split('/');
            if ((split.Length > 0) && (split[0].Length == 0)) {
                return new string[0];
            }

            return split;
        }

        private static int ComputeScore(
            string[] childPathElements,
            string[] factoryPathElements)
        {
            int index = 0;

            if (factoryPathElements.Length == 0) {
                return int.MaxValue; // the most general factory scores the lowest
            }

            while (true) {
                if ((childPathElements.Length > index) &&
                    (factoryPathElements.Length > index)) {
                    if (!(childPathElements[index].Equals(factoryPathElements[index]))) {
                        return 0;
                    }
                }
                else {
                    if (childPathElements.Length <= index) {
                        if (factoryPathElements.Length > index) {
                            return 0;
                        }

                        return int.MaxValue - index - 1;
                    }
                }

                if (factoryPathElements.Length <= index) {
                    break;
                }

                index++;
            }

            return int.MaxValue - index - 1;
        }
    }
}