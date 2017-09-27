///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.parse;
using com.espertech.esper.epl.spec;
using com.espertech.esper.supportunit.epl.parse;

using NUnit.Framework;

namespace com.espertech.esper.rowregex
{
    [TestFixture]
    public class TestEventRowRegexHelper 
    {
        [Test]
        public void TestVariableAnalysis()
        {
            var patternTests = new String[][] {
                    new[] {"A", "[A]", "[]"},
                    new[] {"A B", "[A, B]", "[]"},
                    new[] {"A B*", "[A]", "[B]"},
                    new[] {"A B B", "[A]", "[B]"},
                    new[] {"A B A", "[B]", "[A]"},
                    new[] {"A B+ C", "[A, C]", "[B]"},
                    new[] {"A B?", "[A, B]", "[]"},
                    new[] {"(A B)* C", "[C]", "[A, B]"},
                    new[] {"D (A B)+ (G H)? C", "[D, G, H, C]", "[A, B]"},
                    new[] {"A B | A C", "[A, B, C]", "[]"},
                    new[] {"(A B*) | (A+ C)", "[C]", "[B, A]"},
                    new[] {"(A | B) | (C | A)", "[A, B, C]", "[]"},
            };
    
            for (int i = 0; i < patternTests.Length; i++)
            {
                String pattern = patternTests[i][0];
                String expression = "select * from MyEvent#keepall() match_recognize (" +
                        "  partition by TheString measures A.TheString as a_string pattern ( " + pattern + ") define A as (A.value = 1) )";

                EPLTreeWalkerListener walker = SupportParserHelper.ParseAndWalkEPL(expression);
                StatementSpecRaw raw = walker.StatementSpec;
    
                RowRegexExprNode parent = raw.MatchRecognizeSpec.Pattern;
                var singles = new FIFOHashSet<String>();
                var multiples = new FIFOHashSet<String>();
                
                EventRowRegexHelper.RecursiveInspectVariables(parent, false, singles, multiples);

                String @out = "Failed in :" + pattern +
                              " result is : single " + singles.Render() +
                              " multiple " + multiples.Render();

                Assert.AreEqual(patternTests[i][1], singles.Render(), @out);
                Assert.AreEqual(patternTests[i][2], multiples.Render(), @out);
            }
        }

        [Test]
        public void TestVisibilityAnalysis()
        {
            var patternTests = new String[][]
            {
                new [] {"A", "[]"},
                new [] {"A B", "[B=[A]]"},
                new [] {"A B*", "[B=[A]]"},
                new [] {"A B B", "[B=[A]]"},
                new [] {"A B A", "[A=[B], B=[A]]"},
                new [] {"A B+ C", "[B=[A], C=[A, B]]"},
                new [] {"(A B)+ C", "[B=[A], C=[A, B]]"},
                new [] {"D (A B)+ (G H)? C", "[A=[D], B=[A, D], C=[A, B, D, G, H], G=[A, B, D], H=[A, B, D, G]]"},
                new [] {"A B | A C", "[B=[A], C=[A]]"},
                new [] {"(A B*) | (A+ C)", "[B=[A], C=[A]]"},
                new [] {"A (B | C) D", "[B=[A], C=[A], D=[A, B, C]]"},
                new [] {"(((A))) (((B))) (( C | (D E)))", "[B=[A], C=[A, B], D=[A, B], E=[A, B, D]]"},
                new [] {"(A | B) C", "[C=[A, B]]"},
                new [] {"(A | B) (C | A)", "[A=[B], C=[A, B]]"},
            };

            for (int i = 0; i < patternTests.Length; i++)
            {
                String pattern = patternTests[i][0];
                String expected = patternTests[i][1];
                String expression = "select * from MyEvent#keepall() match_recognize (" +
                        "  partition by string measures A.string as a_string pattern ( " + pattern + ") define A as (A.value = 1) )";

                EPLTreeWalkerListener walker = SupportParserHelper.ParseAndWalkEPL(expression);
                StatementSpecRaw raw = walker.StatementSpec;

                RowRegexExprNode parent = raw.MatchRecognizeSpec.Pattern;

                IDictionary<String, ISet<String>> visibility = EventRowRegexHelper.DetermineVisibility(parent);

                // sort, for comparing
                var visibilitySorted = new LinkedHashMap<String, IList<String>>();
                var tagsSorted = new List<String>(visibility.Keys);
                tagsSorted.Sort();
                foreach (string tag in tagsSorted) {
                    var sorted = new List<String>(visibility.Get(tag));
                    sorted.Sort();
                    visibilitySorted.Put(tag, sorted);
                }
                Assert.AreEqual(expected, visibilitySorted.Render(), "Failed in :" + pattern);
            }
        }
    }
}
