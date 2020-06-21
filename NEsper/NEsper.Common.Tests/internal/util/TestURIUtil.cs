///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.util
{
    [TestFixture]
    public class TestUriUtil : AbstractCommonTest
    {
        private static void RunAssertion(
            Uri uriTested,
            IDictionary<Uri, object> input,
            ICollection<KeyValuePair<Uri, object>> result,
            string[] expected)
        {
            // assert
            Assert.AreEqual(expected.Length, result.Count, "found: " + result + " for URI " + uriTested);
            var index = 0;
            foreach (var entry in result)
            {
                var expectedUri = new Uri(expected[index], UriKind.RelativeOrAbsolute);
                var message = "mismatch for line " + index;
                Assert.AreEqual(expectedUri, entry.Key, message);
                Assert.AreEqual(input.Get(expectedUri), entry.Value, message);
                index++;
            }
        }

        [Test, RunInApplicationDomain]
        public void TestSortRelevance()
        {
            object[][] uris = {
                new object[] {"a/relative/one", -1},
                new object[] {"other:mailto:test", 0},
                new object[] {"other://a", 1},
                new object[] {"type://a/b2/c1", 2},
                new object[] {"type://a/b3", 3},
                new object[] {"type://a/b2/c2", 4},
                new object[] {"type://a", 5},
                new object[] {"type://x?query#fragment&param", 6},
                new object[] {"type://a/b1/c1", 7},
                new object[] {"type://a/b1/c2", 8},
                new object[] {"type://a/b1/c2/d1", 9},
                new object[] {"type://a/b2", 10},
                new object[] {"type://x/a?query#fragment&param", 11},
                new object[] {"type://x/a/b?query#fragment&param", 12},
                new object[] {"/a/b/c", 13},
                new object[] {"/a", 14},
                new object[] {"//a/b/c", 15},
                new object[] {"//a", 16}
            };

            Uri uri;

            // setup input
            IDictionary<Uri, object> input = new Dictionary<Uri, object>();
            foreach (var uri1 in uris) {
                Assert.DoesNotThrow(() => {
                    try {
                        uri = new Uri((string) uri1[0], UriKind.RelativeOrAbsolute);
                        input.Put(uri, uri1[1]);
                    }
                    catch (UriFormatException e) {
                        Console.WriteLine(e);
                    }
                });
            }

            ICollection<KeyValuePair<Uri, object>> result;
            string[] expected;

            uri = new Uri("type://x/a/b?qqq", UriKind.RelativeOrAbsolute);
            result = URIUtil.FilterSort(uri, input);
            expected = new[] { "type://x/a/b?query#fragment&param", "type://x/a?query#fragment&param", "type://x?query#fragment&param" };
            RunAssertion(uri, input, result, expected);

            // unspecific child
            uri = new Uri("type://a/b2", UriKind.RelativeOrAbsolute);
            result = URIUtil.FilterSort(uri, input);
            expected = new[] { "type://a/b2", "type://a" };
            RunAssertion(uri, input, result, expected);

            // very specific child
            uri = new Uri("type://a/b2/c2/d/e", UriKind.RelativeOrAbsolute);
            result = URIUtil.FilterSort(uri, input);
            expected = new[] { "type://a/b2/c2", "type://a/b2", "type://a" };
            RunAssertion(uri, input, result, expected);

            // less specific child
            uri = new Uri("type://a/b1/c2", UriKind.RelativeOrAbsolute);
            result = URIUtil.FilterSort(uri, input);
            expected = new[] { "type://a/b1/c2", "type://a" };
            RunAssertion(uri, input, result, expected);

            // unspecific child
            uri = new Uri("type://a/b4", UriKind.RelativeOrAbsolute);
            result = URIUtil.FilterSort(uri, input);
            expected = new[] { "type://a" };
            RunAssertion(uri, input, result, expected);

            uri = new Uri("type://b/b1", UriKind.RelativeOrAbsolute);
            result = URIUtil.FilterSort(uri, input);
            expected = new string[] { };
            RunAssertion(uri, input, result, expected);

            uri = new Uri("type://a/b1/c2/d1/e1/f1", UriKind.RelativeOrAbsolute);
            result = URIUtil.FilterSort(uri, input);
            expected = new[] { "type://a/b1/c2/d1", "type://a/b1/c2", "type://a" };
            RunAssertion(uri, input, result, expected);

            uri = new Uri("other:mailto:test", UriKind.RelativeOrAbsolute);
            result = URIUtil.FilterSort(uri, input);
            expected = new[] { "other:mailto:test" };
            RunAssertion(uri, input, result, expected);

            uri = new Uri("type://x/a?qqq", UriKind.RelativeOrAbsolute);
            result = URIUtil.FilterSort(uri, input);
            expected = new[] { "type://x/a?query#fragment&param", "type://x?query#fragment&param" };
            RunAssertion(uri, input, result, expected);

            uri = new Uri("other://x/a?qqq", UriKind.RelativeOrAbsolute);
            result = URIUtil.FilterSort(uri, input);
            expected = new string[] { };
            RunAssertion(uri, input, result, expected);

            // this is seen as relative, must be a full hit (no path checking)
            uri = new Uri("/a/b", UriKind.RelativeOrAbsolute);
            result = URIUtil.FilterSort(uri, input);
            expected = new string[] { };
            RunAssertion(uri, input, result, expected);

            // this is seen as relative
            uri = new Uri("/a/b/c", UriKind.RelativeOrAbsolute);
            result = URIUtil.FilterSort(uri, input);
            expected = new[] { "/a/b/c" };
            RunAssertion(uri, input, result, expected);

            // this is seen as relative
            uri = new Uri("//a/b", UriKind.RelativeOrAbsolute);
            result = URIUtil.FilterSort(uri, input);
            expected = new string[] { };
            RunAssertion(uri, input, result, expected);

            // this is seen as relative
            uri = new Uri("//a/b/c", UriKind.RelativeOrAbsolute);
            result = URIUtil.FilterSort(uri, input);
            expected = new[] { "//a/b/c" };
            RunAssertion(uri, input, result, expected);
        }
    }
} // end of namespace
