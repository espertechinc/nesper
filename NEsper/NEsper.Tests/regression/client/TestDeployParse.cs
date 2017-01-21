///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.deploy;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
    public class TestDeployParse
    {
        private static String newline = Environment.NewLine;
    
        private EPServiceProvider _epService;
        private EPDeploymentAdmin _deploySvc;
    
        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _deploySvc = _epService.EPAdministrator.DeploymentAdmin;
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestParse()
        {
            Module module = _deploySvc.Read("regression/test_module_4.epl");
            AssertModule(module, null, "abd", null, new String[] {
                    "select * from ABC",
                    "/* Final comment */"
                }, new bool[] {false, true},
                    new int[] {3, 8},
                    new int[] {12, 0},
                    new int[] {37, 0}
            );
    
            module = _deploySvc.Read("regression/test_module_1.epl");
            AssertModule(module, "abc", "def,jlk", null, new String[] {
                    "select * from A",
                    "select * from B" + newline +
                        "where C=d",
                    "/* Test ; Comment */" + newline +
                            "update ';' where B=C",
                    "update D"
                }
            );
    
            module = _deploySvc.Read("regression/test_module_2.epl");
            AssertModule(module, "abc.def.hij", "def.hik,jlk.aja", null, new String[] {
                    "// Note 4 white spaces after * and before from" + newline + "select * from A",
                    "select * from B",
                    "select *    " + newline + "    from C",
                }
            );
    
            module = _deploySvc.Read("regression/test_module_3.epl");
            AssertModule(module, null, null, null, new String[] {
                    "create window ABC",
                    "select * from ABC"
                }
            );
    
            module = _deploySvc.Read("regression/test_module_5.epl");
            AssertModule(module, "abd.def", null, null, new String[0]);
    
            module = _deploySvc.Read("regression/test_module_6.epl");
            AssertModule(module, null, null, null, new String[0]);
    
            module = _deploySvc.Read("regression/test_module_7.epl");
            AssertModule(module, null, null, null, new String[0]);
    
            module = _deploySvc.Read("regression/test_module_8.epl");
            AssertModule(module, "def.jfk", null, null, new String[0]);
            
            module = _deploySvc.Parse("module mymodule; uses mymodule2; import abc; select * from MyEvent;");
            AssertModule(module, "mymodule", "mymodule2", "abc", new String[] {
                    "select * from MyEvent"
                });
    
            module = _deploySvc.Read("regression/test_module_11.epl");
            AssertModule(module, null, null, "com.mycompany.pck1", new String[0]);
    
            module = _deploySvc.Read("regression/test_module_10.epl");
            AssertModule(module, "abd.def", "one.use,two.use", "com.mycompany.pck1,com.mycompany.*", new String[] {
                    "select * from A",
                }
            );
    
            Assert.AreEqual("org.mycompany.events", _deploySvc.Parse("module org.mycompany.events; select * from System.Object;").Name);
            Assert.AreEqual("glob.Update.me", _deploySvc.Parse("module glob.Update.me; select * from System.Object;").Name);
            Assert.AreEqual("seconds.until.every.where", _deploySvc.Parse("uses seconds.until.every.where; select * from System.Object;").Uses.ToArray()[0]);
            Assert.AreEqual("seconds.until.every.where", _deploySvc.Parse("import seconds.until.every.where; select * from System.Object;").Imports.ToArray()[0]);
    
            // Test script square brackets
            module = _deploySvc.Read("regression/test_module_13.epl");
            Assert.AreEqual(1, module.Items.Count);
    
            module = _deploySvc.Read("regression/test_module_14.epl");
            Assert.AreEqual(4, module.Items.Count);
    
            module = _deploySvc.Read("regression/test_module_15.epl");
            Assert.AreEqual(1, module.Items.Count);
        }
    
        [Test]
        public void TestParseFail() {
            TryInvalidIO("regression/dummy_not_there.epl",
                       "Failed to find resource 'regression/dummy_not_there.epl' in classpath");
    
            TryInvalidParse("regression/test_module_1_fail.epl",
                       "Keyword 'module' must be followed by a name or package name (set of names separated by dots) for resource 'regression/test_module_1_fail.epl'");
    
            TryInvalidParse("regression/test_module_2_fail.epl",
                       "Keyword 'uses' must be followed by a name or package name (set of names separated by dots) for resource 'regression/test_module_2_fail.epl'");
    
            TryInvalidParse("regression/test_module_3_fail.epl",
                       "Keyword 'module' must be followed by a name or package name (set of names separated by dots) for resource 'regression/test_module_3_fail.epl'");
    
            TryInvalidParse("regression/test_module_4_fail.epl",
                       "Keyword 'uses' must be followed by a name or package name (set of names separated by dots) for resource 'regression/test_module_4_fail.epl'");
    
            TryInvalidParse("regression/test_module_5_fail.epl",
                       "Keyword 'import' must be followed by a name or package name (set of names separated by dots) for resource 'regression/test_module_5_fail.epl'");
    
            TryInvalidParse("regression/test_module_6_fail.epl",
                       "The 'module' keyword must be the first declaration in the module file for resource 'regression/test_module_6_fail.epl'");
    
            TryInvalidParse("regression/test_module_7_fail.epl",
                       "Duplicate use of the 'module' keyword for resource 'regression/test_module_7_fail.epl'");
    
            TryInvalidParse("regression/test_module_8_fail.epl",
                       "The 'uses' and 'import' keywords must be the first declaration in the module file or follow the 'module' declaration");
    
            TryInvalidParse("regression/test_module_9_fail.epl",
                       "The 'uses' and 'import' keywords must be the first declaration in the module file or follow the 'module' declaration");

            // try control chars
            TryInvalidControlCharacters();
        }

        private void TryInvalidControlCharacters() 
        {
            String epl = string.Format("select * \u008F from {0}", typeof(SupportBean).FullName);
            try {
                Assert.That(epl[9], Is.EqualTo('\u008F'));
                _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
                Assert.Fail();
            }
            catch (ParseException ex) {
                SupportMessageAssertUtil.AssertMessage(ex, "Unrecognized control characters found in text, failed to parse text ");
            }
        }
    
        private void TryInvalidIO(String resource, String message) {
            try {
                _deploySvc.Read(resource);
                Assert.Fail();
            }
            catch (IOException ex) {
                Assert.AreEqual(message, ex.Message);
            }
        }
    
        private void TryInvalidParse(String resource, String message) {
            try {
                _deploySvc.Read(resource);
                Assert.Fail();
            }
            catch (ParseException ex) {
                Assert.AreEqual(message, ex.Message);
            }
        }
    
        private void AssertModule(Module module, String name, String usesCSV, String importsCSV, String[] statements) {
            AssertModule(module, name, usesCSV, importsCSV, statements, new bool[statements.Length], new int[statements.Length], new int[statements.Length], new int[statements.Length]);
        }
    
        private void AssertModule(Module module, String name, String usesCSV, String importsCSV, String[] statementsExpected,
                                  bool[] commentsExpected,
                                  int[] lineNumsExpected,
                                  int[] charStartsExpected,
                                  int[] charEndsExpected) {
            Assert.AreEqual(name, module.Name);
    
            String[] expectedUses = usesCSV == null ? new String[0] : usesCSV.Split(',');
            EPAssertionUtil.AssertEqualsExactOrder(expectedUses, module.Uses.ToArray());
    
            String[] expectedImports = importsCSV == null ? new String[0] : importsCSV.Split(',');
            EPAssertionUtil.AssertEqualsExactOrder(expectedImports, module.Imports.ToArray());
    
            String[] stmtsFound = new String[module.Items.Count];
            bool[] comments = new bool[module.Items.Count];
            int[] lineNumsFound = new int[module.Items.Count];
            int[] charStartsFound = new int[module.Items.Count];
            int[] charEndsFound = new int[module.Items.Count];
    
            for (int i = 0; i < module.Items.Count; i++) {
                stmtsFound[i] = module.Items[i].Expression;
                comments[i] = module.Items[i].IsCommentOnly;
                lineNumsFound[i] = module.Items[i].LineNumber;
                charStartsFound[i] = module.Items[i].CharPosStart;
                charEndsFound[i] = module.Items[i].CharPosEnd;
            }
                    
            EPAssertionUtil.AssertEqualsExactOrder(statementsExpected, stmtsFound);
            EPAssertionUtil.AssertEqualsExactOrder(commentsExpected, comments);
    
            bool isCompareLineNums = false;
            foreach (int l in lineNumsExpected) {
                if (l > 0) {
                    isCompareLineNums = true;
                }
            }
            if (isCompareLineNums) {
                EPAssertionUtil.AssertEqualsExactOrder(lineNumsExpected, lineNumsFound);
                // Start and end character position can be platform-dependent
                // commented-out: EPAssertionUtil.AssertEqualsExactOrder(charStartsExpected, charStartsFound);
                // commented-out: EPAssertionUtil.AssertEqualsExactOrder(charEndsExpected, charEndsFound);
            }
        }
    }
}
