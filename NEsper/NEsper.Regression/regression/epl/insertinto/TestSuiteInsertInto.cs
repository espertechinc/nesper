///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.execution;

namespace com.espertech.esper.regression.epl.insertinto
{
    [TestFixture]
    public class TestSuiteInsertInto
    {
        [Test]
        public void TestExecInsertInto() {
            RegressionRunner.Run(new ExecInsertInto());
        }
    
        [Test]
        public void TestExecInsertIntoEmptyPropType() {
            RegressionRunner.Run(new ExecInsertIntoEmptyPropType());
        }
    
        [Test]
        public void TestExecInsertIntoIRStreamFunc() {
            RegressionRunner.Run(new ExecInsertIntoIRStreamFunc());
        }
    
        [Test]
        public void TestExecInsertIntoPopulateCreateStream() {
            RegressionRunner.Run(new ExecInsertIntoPopulateCreateStream());
        }
    
        [Test]
        public void TestExecInsertIntoPopulateCreateStreamAvro() {
            RegressionRunner.Run(new ExecInsertIntoPopulateCreateStreamAvro());
        }
    
        [Test]
        public void TestExecInsertIntoPopulateEventTypeColumn() {
            RegressionRunner.Run(new ExecInsertIntoPopulateEventTypeColumn());
        }
    
        [Test]
        public void TestExecInsertIntoPopulateSingleColByMethodCall() {
            RegressionRunner.Run(new ExecInsertIntoPopulateSingleColByMethodCall());
        }
    
        [Test]
        public void TestExecInsertIntoPopulateUnderlying() {
            RegressionRunner.Run(new ExecInsertIntoPopulateUnderlying());
        }
    
        [Test]
        public void TestExecInsertIntoPopulateUndStreamSelect() {
            RegressionRunner.Run(new ExecInsertIntoPopulateUndStreamSelect());
        }
    
        [Test]
        public void TestExecInsertIntoTransposePattern() {
            RegressionRunner.Run(new ExecInsertIntoTransposePattern());
        }
    
        [Test]
        public void TestExecInsertIntoTransposeStream() {
            RegressionRunner.Run(new ExecInsertIntoTransposeStream());
        }
    
        [Test]
        public void TestExecInsertIntoFromPattern() {
            RegressionRunner.Run(new ExecInsertIntoFromPattern());
        }
    }
} // end of namespace
