///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

using com.espertech.esper.collection;
using com.espertech.esper.supportunit.epl.parse;

using NUnit.Framework;

namespace com.espertech.esper.epl.parse
{
    [TestFixture]
    public class TestASTFilterSpecHelper 
    {
        [Test]
        public void TestGetPropertyName()
        {
            String PROPERTY = "a('aa').b[1].c";
    
            // Should parse and result in the exact same property name
            Pair<ITree, CommonTokenStream> parsed = SupportParserHelper.ParseEventProperty(PROPERTY);
            ITree propertyNameExprNode = parsed.First.GetChild(0);
            ASTUtil.DumpAST(propertyNameExprNode);
            String propertyName = ((IRuleNode) propertyNameExprNode).GetText();
            Assert.AreEqual(PROPERTY, propertyName);
    
            // Try AST with tokens separated, same property name
            parsed = SupportParserHelper.ParseEventProperty("a(    'aa'   ). b [ 1 ] . c");
            propertyNameExprNode = parsed.First.GetChild(0);
            propertyName = ((IRuleNode) propertyNameExprNode).GetText();
            Assert.AreEqual(PROPERTY, propertyName);
        }
    
        [Test]
        public void TestGetPropertyNameEscaped()
        {
            String PROPERTY = "a\\.b\\.c";
            Pair<ITree, CommonTokenStream> parsed = SupportParserHelper.ParseEventProperty(PROPERTY);
            ITree propertyNameExprNode = parsed.First.GetChild(0);
            ASTUtil.DumpAST(propertyNameExprNode);
            String propertyName = ((IRuleNode)propertyNameExprNode).GetText();
            Assert.AreEqual(PROPERTY, propertyName);
        }
    
        [Test]
        public void TestEscapeDot()
        {
            String [][] inout = new String[][] {
                    new [] {"a", "a"},
                    new [] {"", ""},
                    new [] {" ", " "},
                    new [] {".", "\\."},
                    new [] {". .", "\\. \\."},
                    new [] {"a.", "a\\."},
                    new [] {".a", "\\.a"},
                    new [] {"a.b", "a\\.b"},
                    new [] {"a..b", "a\\.\\.b"},
                    new [] {"a\\.b", "a\\.b"},
                    new [] {"a\\..b", "a\\.\\.b"},
                    new [] {"a.\\..b", "a\\.\\.\\.b"},
                    new [] {"a.b.c", "a\\.b\\.c"}
            };
    
            for (int i = 0; i < inout.Length; i++)
            {
                String @in = inout[i][0];
                String expected = inout[i][1];
                Assert.AreEqual(expected, ASTUtil.EscapeDot(@in), "for input " + @in);
            }
        }
    
        [Test]
        public void TestUnescapeIndexOf()
        {
            Object [][] inout = new Object[][] {
                    new Object[] {"a", -1},
                    new Object[] {"", -1},
                    new Object[] {" ", -1},
                    new Object[] {".", 0},
                    new Object[] {" . .", 1},
                    new Object[] {"a.", 1},
                    new Object[] {".a", 0},
                    new Object[] {"a.b", 1},
                    new Object[] {"a..b", 1},
                    new Object[] {"a\\.b", -1},
                    new Object[] {"a.\\..b", 1},
                    new Object[] {"a\\..b", 3},
                    new Object[] {"a.b.c", 1},
                    new Object[] {"abc.", 3}
            };
    
            for (int i = 0; i < inout.Length; i++)
            {
                var @in = (String) inout[i][0];
                var expected = (int?) inout[i][1];
                Assert.AreEqual(expected, ASTUtil.UnescapedIndexOfDot(@in), "for input " + @in);
            }
        }
    
        [Test]
        public void TestUnescapeDot()
        {
            String [][] inout = new String[][] {
                    new [] {"a", "a"},
                    new [] {"", ""},
                    new [] {" ", " "},
                    new [] {".", "."},
                    new [] {" . .", " . ."},
                    new [] {"a\\.", "a."},
                    new [] {"\\.a", ".a"},
                    new [] {"a\\.b", "a.b"},
                    new [] {"a.b", "a.b"},
                    new [] {".a", ".a"},
                    new [] {"a.", "a."},
                    new [] {"a\\.\\.b", "a..b"},
                    new [] {"a\\..\\.b", "a...b"},
                    new [] {"a.\\..b", "a...b"},
                    new [] {"a\\..b", "a..b"},
                    new [] {"a.b\\.c", "a.b.c"},
            };
    
            for (int i = 0; i < inout.Length; i++)
            {
                String @in = inout[i][0];
                String expected = inout[i][1];
                Assert.AreEqual(expected, ASTUtil.UnescapeDot(@in), "for input " + @in);
            }
        }
    }
}
