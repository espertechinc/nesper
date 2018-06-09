///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr.expr
{
    public class ExecExprBitWiseOperators : RegressionExecution
    {
        private static readonly byte FIRST_EVENT = 1;
        private static readonly short SECOND_EVENT = 2;
#pragma warning disable CS0675
        private static readonly int THIRD_EVENT = FIRST_EVENT | SECOND_EVENT;
#pragma warning restore CS0675
        private static readonly long FOURTH_EVENT = 4;
        private static readonly bool FITH_EVENT = false;
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionBitWiseOperators_OM(epService);
            RunAssertionBitWiseOperators(epService);
        }
    
        private void RunAssertionBitWiseOperators_OM(EPServiceProvider epService) {
            string epl = "select bytePrimitive&byteBoxed as myFirstProperty, " +
                    "ShortPrimitive|ShortBoxed as mySecondProperty, " +
                    "IntPrimitive|IntBoxed as myThirdProperty, " +
                    "LongPrimitive^LongBoxed as myFourthProperty, " +
                    "BoolPrimitive&BoolBoxed as myFifthProperty " +
                    "from " + typeof(SupportBean).FullName + "#length(3)";
    
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create()
                    .Add(Expressions.BinaryAnd().Add("bytePrimitive").Add("byteBoxed"), "myFirstProperty")
                    .Add(Expressions.BinaryOr().Add("ShortPrimitive").Add("ShortBoxed"), "mySecondProperty")
                    .Add(Expressions.BinaryOr().Add("IntPrimitive").Add("IntBoxed"), "myThirdProperty")
                    .Add(Expressions.BinaryXor().Add("LongPrimitive").Add("LongBoxed"), "myFourthProperty")
                    .Add(Expressions.BinaryAnd().Add("BoolPrimitive").Add("BoolBoxed"), "myFifthProperty");

            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean).FullName).AddView("length", Expressions.Constant(3)));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
            Assert.AreEqual(epl, model.ToEPL());
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            RunBitWiseOperators(epService, listener);
    
            stmt.Dispose();
        }
    
        private void RunAssertionBitWiseOperators(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
            EPStatement stmt = SetUpBitWiseStmt(epService, listener);
            RunBitWiseOperators(epService, listener);
            stmt.Dispose();
        }
    
        private void RunBitWiseOperators(EPServiceProvider epService, SupportUpdateListener listener) {
            SendEvent(epService, FIRST_EVENT, new byte?(FIRST_EVENT), SECOND_EVENT, new short?(SECOND_EVENT),
                    FIRST_EVENT, new int?(THIRD_EVENT), 3L, new long?(FOURTH_EVENT),
                    FITH_EVENT, new bool?(FITH_EVENT));
    
            EventBean received = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual((byte) 1, received.Get("myFirstProperty"));
            Assert.IsTrue(((short) (received.Get("mySecondProperty")) & SECOND_EVENT) == SECOND_EVENT);
            Assert.IsTrue(((int?) (received.Get("myThirdProperty")) & FIRST_EVENT) == FIRST_EVENT);
            Assert.AreEqual(7L, received.Get("myFourthProperty"));
            Assert.AreEqual(false, received.Get("myFifthProperty"));
        }
    
        private EPStatement SetUpBitWiseStmt(EPServiceProvider epService, SupportUpdateListener listener) {
            string epl = "select (bytePrimitive & byteBoxed) as myFirstProperty, " +
                    "(ShortPrimitive | ShortBoxed) as mySecondProperty, " +
                    "(IntPrimitive | IntBoxed) as myThirdProperty, " +
                    "(LongPrimitive ^ LongBoxed) as myFourthProperty, " +
                    "(BoolPrimitive & BoolBoxed) as myFifthProperty " +
                    " from " + typeof(SupportBean).FullName + "#length(3) ";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += listener.Update;
    
            EventType type = stmt.EventType;
            Assert.AreEqual(typeof(byte?), type.GetPropertyType("myFirstProperty"));
            Assert.AreEqual(typeof(short?), type.GetPropertyType("mySecondProperty"));
            Assert.AreEqual(typeof(int?), type.GetPropertyType("myThirdProperty"));
            Assert.AreEqual(typeof(long?), type.GetPropertyType("myFourthProperty"));
            Assert.AreEqual(typeof(bool?), type.GetPropertyType("myFifthProperty"));
    
            return stmt;
        }
    
        protected void SendEvent(
            EPServiceProvider epService,
            byte bytePrimitive, 
            byte? byteBoxed, 
            short shortPrimitive, 
            short? shortBoxed,
            int intPrimitive, 
            int? intBoxed, 
            long longPrimitive, 
            long? longBoxed,
            bool boolPrimitive, 
            bool? boolBoxed)
        {
            var bean = new SupportBean();
            bean.BytePrimitive = bytePrimitive;
            bean.ByteBoxed = byteBoxed;
            bean.ShortPrimitive = shortPrimitive;
            bean.ShortBoxed = shortBoxed;
            bean.IntPrimitive = intPrimitive;
            bean.IntBoxed = intBoxed;
            bean.LongPrimitive = longPrimitive;
            bean.LongBoxed = longBoxed;
            bean.BoolPrimitive = boolPrimitive;
            bean.BoolBoxed = boolBoxed;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
