///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.util
{
    [TestFixture]
    public class TestLikeUtil : CommonTest
    {
        [Test]
        public void TestLike()
        {
            TryMatches("%aa%", '\\',
                    new string[] { "aa", " aa", "aa ", "0aa0", "%aa%" },
                    new string[] { "ba", " bea", "a a", " a a a a", "yyya ay" });

            TryMatches("a%", '\\',
                    new string[] { "a", "allo", "aa ", "aa0", "aa%" },
                    new string[] { " a", "ba", "\\a", " aa", "ya ay" });

            TryMatches("%ab", '\\',
                    new string[] { "dgdgab", "ab", " ab", "addhf ab ab", "  ab" },
                    new string[] { " a", "ba", "a", "ac", "ay" });

            TryMatches("c%ab", '\\',
                    new string[] { "cab", "cgfhghab", "c ab", "cddhf ab ab", "c  ab" },
                    new string[] { "c aa", "c ba", " ab", "c aba", "c b" });

            TryMatches("c%ab", '\\',
                    new string[] { "cab", "cgfhghab", "c ab", "cddhf ab ab", "c  ab" },
                    new string[] { "c aa", "c ba", " ab", "c aba", "c b" });

            TryMatches("c%ab%c", '\\',
                    new string[] { "cabc", "cgfhghabc", "c ab  c", "cddhf ab ab c", "c  abc" },
                    new string[] { "c aa", "c ab", " ab c", "c ab a", "c ba c" });

            TryMatches("_d%c", '\\',
                    new string[] { "adbc", "adc", "adbfhfhfhhfc", "xd99c", "4d%c" },
                    new string[] { "ccdac", "qdtb", "yydc", "9d9e", "111gd" });

            TryMatches("___a", '\\',
                    new string[] { "aaaa", "736a", "   a", "oooa", "___a" },
                    new string[] { "  a", "    a", "uua", "uuuua", "9999b" });

            TryMatches("___a___", '\\',
                    new string[] { "bbbabbb", "bbba   ", "   abbb", "   a   ", "%%%a%%%" },
                    new string[] { "   a  ", "  a   ", "  a ", "    a    ", "x   a   " });

            TryMatches("%_a_%", '\\',
                    new string[] { "dhdhdhdh a djdjdj", " a ", "kdkd a ", " a dkdkd", "   a   ", "%%%a%%%" },
                    new string[] { "a", " a", "a ", "    b    ", " qa" });

            TryMatches("!%do", '!',
                    new string[] { "%do" },
                    new string[] { "!do", "do", "ado" });

            TryMatches("!_do", '!',
                    new string[] { "_do" },
                    new string[] { "!do", "do", "ado" });
        }

        private void TryMatches(string pattern, char escape, string[] stringMatching, string[] stringNotMatching)
        {
            LikeUtil helper = new LikeUtil(pattern, escape, false);

            for (int i = 0; i < stringMatching.Length; i++)
            {
                string text = "Expected match for pattern '" + pattern +
                        "' and string '" + stringMatching[i] + "'";
                Assert.IsTrue(helper.Compare(stringMatching[i]), text);
            }

            for (int i = 0; i < stringNotMatching.Length; i++)
            {
                string text = "Expected mismatch for pattern '" + pattern +
                        "' and string '" + stringNotMatching[i] + "'";
                Assert.IsFalse(helper.Compare(stringNotMatching[i]), text);
            }
        }
    }
} // end of namespace