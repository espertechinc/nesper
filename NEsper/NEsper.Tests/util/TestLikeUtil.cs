///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using NUnit.Framework;

namespace com.espertech.esper.util
{
    [TestFixture]
    public class TestLikeUtil
    {
        private void TryMatches(String pattern, char escape, String[] stringMatching, String[] stringNotMatching)
        {
            var helper = new LikeUtil(pattern, escape, false);

            for (int i = 0; i < stringMatching.Length; i++) {
                String text = "Expected match for pattern '" + pattern +
                              "' and string '" + stringMatching[i] + "'";
                Assert.IsTrue(helper.Compare(stringMatching[i]), text);
            }

            for (int i = 0; i < stringNotMatching.Length; i++) {
                String text = "Expected mismatch for pattern '" + pattern +
                              "' and string '" + stringNotMatching[i] + "'";
                Assert.IsFalse(helper.Compare(stringNotMatching[i]), text);
            }
        }

        [Test]
        public void TestLike()
        {
            TryMatches("%aa%", '\\',
                       new[] {"aa", " aa", "aa ", "0aa0", "%aa%"},
                       new[] {"ba", " bea", "a a", " a a a a", "yyya ay"});

            TryMatches("a%", '\\',
                       new[] {"a", "allo", "aa ", "aa0", "aa%"},
                       new[] {" a", "ba", "\\a", " aa", "ya ay"});

            TryMatches("%ab", '\\',
                       new[] {"dgdgab", "ab", " ab", "addhf ab ab", "  ab"},
                       new[] {" a", "ba", "a", "ac", "ay"});

            TryMatches("c%ab", '\\',
                       new[] {"cab", "cgfhghab", "c ab", "cddhf ab ab", "c  ab"},
                       new[] {"c aa", "c ba", " ab", "c aba", "c b"});

            TryMatches("c%ab", '\\',
                       new[] {"cab", "cgfhghab", "c ab", "cddhf ab ab", "c  ab"},
                       new[] {"c aa", "c ba", " ab", "c aba", "c b"});

            TryMatches("c%ab%c", '\\',
                       new[] {"cabc", "cgfhghabc", "c ab  c", "cddhf ab ab c", "c  abc"},
                       new[] {"c aa", "c ab", " ab c", "c ab a", "c ba c"});

            TryMatches("_d%c", '\\',
                       new[] {"adbc", "adc", "adbfhfhfhhfc", "xd99c", "4d%c"},
                       new[] {"ccdac", "qdtb", "yydc", "9d9e", "111gd"});

            TryMatches("___a", '\\',
                       new[] {"aaaa", "736a", "   a", "oooa", "___a"},
                       new[] {"  a", "    a", "uua", "uuuua", "9999b"});

            TryMatches("___a___", '\\',
                       new[] {"bbbabbb", "bbba   ", "   abbb", "   a   ", "%%%a%%%"},
                       new[] {"   a  ", "  a   ", "  a ", "    a    ", "x   a   "});

            TryMatches("%_a_%", '\\',
                       new[] {"dhdhdhdh a djdjdj", " a ", "kdkd a ", " a dkdkd", "   a   ", "%%%a%%%"},
                       new[] {"a", " a", "a ", "    b    ", " qa"});

            TryMatches("!%do", '!',
                       new[] {"%do"},
                       new[] {"!do", "do", "ado"});

            TryMatches("!_do", '!',
                       new[] {"_do"},
                       new[] {"!do", "do", "ado"});
        }
    }
}
