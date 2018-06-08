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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    public class ExecClientDeployParse : RegressionExecution {
        private static readonly string NEWLINE = Environment.NewLine;
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionParse(epService);
            RunAssertionParseFail(epService);
        }
    
        private void RunAssertionParse(EPServiceProvider epService) {
            Module module = epService.EPAdministrator.DeploymentAdmin.Read("regression/test_module_4.epl");
            AssertModule(module, null, "abd", null, new string[]{
                            "select * from ABC",
                            "/* Final comment */"
                    }, new bool[]{false, true},
                    new int[]{3, 8},
                    new int[]{12, 0},
                    new int[]{37, 0}
            );
    
            module = epService.EPAdministrator.DeploymentAdmin.Read("regression/test_module_1.epl");
            AssertModule(module, "abc", "def,jlk", null, new string[]{
                            "select * from A",
                            "select * from B" + NEWLINE +
                                    "where C=d",
                            "/* Test ; Comment */" + NEWLINE +
                                    "update ';' where B=C",
                            "update D"
                    }
            );
    
            module = epService.EPAdministrator.DeploymentAdmin.Read("regression/test_module_2.epl");
            AssertModule(module, "abc.def.hij", "def.hik,jlk.aja", null, new string[]{
                            "// Note 4 white spaces after * and before from" + NEWLINE + "select * from A",
                            "select * from B",
                            "select *    " + NEWLINE + "    from C",
                    }
            );
    
            module = epService.EPAdministrator.DeploymentAdmin.Read("regression/test_module_3.epl");
            AssertModule(module, null, null, null, new string[]{
                            "create window ABC",
                            "select * from ABC"
                    }
            );
    
            module = epService.EPAdministrator.DeploymentAdmin.Read("regression/test_module_5.epl");
            AssertModule(module, "abd.def", null, null, new string[0]);
    
            module = epService.EPAdministrator.DeploymentAdmin.Read("regression/test_module_6.epl");
            AssertModule(module, null, null, null, new string[0]);
    
            module = epService.EPAdministrator.DeploymentAdmin.Read("regression/test_module_7.epl");
            AssertModule(module, null, null, null, new string[0]);
    
            module = epService.EPAdministrator.DeploymentAdmin.Read("regression/test_module_8.epl");
            AssertModule(module, "def.jfk", null, null, new string[0]);
    
            module = epService.EPAdministrator.DeploymentAdmin.Parse("module mymodule; uses mymodule2; import abc; select * from MyEvent;");
            AssertModule(module, "mymodule", "mymodule2", "abc", new string[]{
                    "select * from MyEvent"
            });
    
            module = epService.EPAdministrator.DeploymentAdmin.Read("regression/test_module_11.epl");
            AssertModule(module, null, null, "com.mycompany.pck1", new string[0]);
    
            module = epService.EPAdministrator.DeploymentAdmin.Read("regression/test_module_10.epl");
            AssertModule(module, "abd.def", "one.use,two.use", "com.mycompany.pck1,com.mycompany.*", new string[]{
                            "select * from A",
                    }
            );
    
            Assert.AreEqual("org.mycompany.events", epService.EPAdministrator.DeploymentAdmin.Parse("module org.mycompany.events; select * from System.Object;").Name);
            Assert.AreEqual("glob.update.me", epService.EPAdministrator.DeploymentAdmin.Parse("module glob.update.me; select * from System.Object;").Name);
            Assert.AreEqual("seconds.until.every.where", epService.EPAdministrator.DeploymentAdmin.Parse("uses seconds.until.every.where; select * from System.Object;").Uses.ToArray()[0]);
            Assert.AreEqual("seconds.until.every.where", epService.EPAdministrator.DeploymentAdmin.Parse("import seconds.until.every.where; select * from System.Object;").Imports.ToArray()[0]);
    
            // Test script square brackets
            module = epService.EPAdministrator.DeploymentAdmin.Read("regression/test_module_13.epl");
            Assert.AreEqual(1, module.Items.Count);
    
            module = epService.EPAdministrator.DeploymentAdmin.Read("regression/test_module_14.epl");
            Assert.AreEqual(4, module.Items.Count);
    
            module = epService.EPAdministrator.DeploymentAdmin.Read("regression/test_module_15.epl");
            Assert.AreEqual(1, module.Items.Count);
        }
    
        private void RunAssertionParseFail(EPServiceProvider epService) {
            TryInvalidIO(epService, "regression/dummy_not_there.epl",
                    "Failed to find resource 'regression/dummy_not_there.epl' in classpath");
    
            TryInvalidParse(epService, "regression/test_module_1_fail.epl",
                    "Keyword 'module' must be followed by a name or package name (set of names separated by dots) for resource 'regression/test_module_1_fail.epl'");
    
            TryInvalidParse(epService, "regression/test_module_2_fail.epl",
                    "Keyword 'uses' must be followed by a name or package name (set of names separated by dots) for resource 'regression/test_module_2_fail.epl'");
    
            TryInvalidParse(epService, "regression/test_module_3_fail.epl",
                    "Keyword 'module' must be followed by a name or package name (set of names separated by dots) for resource 'regression/test_module_3_fail.epl'");
    
            TryInvalidParse(epService, "regression/test_module_4_fail.epl",
                    "Keyword 'uses' must be followed by a name or package name (set of names separated by dots) for resource 'regression/test_module_4_fail.epl'");
    
            TryInvalidParse(epService, "regression/test_module_5_fail.epl",
                    "Keyword 'import' must be followed by a name or package name (set of names separated by dots) for resource 'regression/test_module_5_fail.epl'");
    
            TryInvalidParse(epService, "regression/test_module_6_fail.epl",
                    "The 'module' keyword must be the first declaration in the module file for resource 'regression/test_module_6_fail.epl'");
    
            TryInvalidParse(epService, "regression/test_module_7_fail.epl",
                    "Duplicate use of the 'module' keyword for resource 'regression/test_module_7_fail.epl'");
    
            TryInvalidParse(epService, "regression/test_module_8_fail.epl",
                    "The 'uses' and 'import' keywords must be the first declaration in the module file or follow the 'module' declaration");
    
            TryInvalidParse(epService, "regression/test_module_9_fail.epl",
                    "The 'uses' and 'import' keywords must be the first declaration in the module file or follow the 'module' declaration");
    
            // try control chars
            TryInvalidControlCharacters(epService);
        }
    
        private void TryInvalidControlCharacters(EPServiceProvider epService) {
            string epl = "select * \u008F from " + typeof(SupportBean).FullName;
            try {
                epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
                Assert.Fail();
            } catch (ParseException ex) {
                SupportMessageAssertUtil.AssertMessage(ex, "Unrecognized control characters found in text, failed to parse text ");
            }
        }
    
        private void TryInvalidIO(EPServiceProvider epService, string resource, string message) {
            try {
                epService.EPAdministrator.DeploymentAdmin.Read(resource);
                Assert.Fail();
            } catch (IOException ex) {
                Assert.AreEqual(message, ex.Message);
            }
        }
    
        private void TryInvalidParse(EPServiceProvider epService, string resource, string message) {
            try {
                epService.EPAdministrator.DeploymentAdmin.Read(resource);
                Assert.Fail();
            } catch (ParseException ex) {
                Assert.AreEqual(message, ex.Message);
            }
        }
    
        private void AssertModule(Module module, string name, string usesCSV, string importsCSV, string[] statements) {
            AssertModule(module, name, usesCSV, importsCSV, statements, new bool[statements.Length], new int[statements.Length], new int[statements.Length], new int[statements.Length]);
        }
    
        private void AssertModule(Module module, string name, string usesCSV, string importsCSV, string[] statementsExpected,
                                  bool[] commentsExpected,
                                  int[] lineNumsExpected,
                                  int[] charStartsExpected,
                                  int[] charEndsExpected) {
            Assert.AreEqual(name, module.Name);
    
            string[] expectedUses = usesCSV == null ? new string[0] : usesCSV.Split(',');
            EPAssertionUtil.AssertEqualsExactOrder(expectedUses, module.Uses.ToArray());
    
            string[] expectedImports = importsCSV == null ? new string[0] : importsCSV.Split(',');
            EPAssertionUtil.AssertEqualsExactOrder(expectedImports, module.Imports.ToArray());
    
            var stmtsFound = new string[module.Items.Count];
            var comments = new bool[module.Items.Count];
            var lineNumsFound = new int[module.Items.Count];
            var charStartsFound = new int[module.Items.Count];
            var charEndsFound = new int[module.Items.Count];
    
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
} // end of namespace
