///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;

namespace com.espertech.esper.regression.support
{
    public class EventCollectionFactory : SupportBeanConstants
    {
        /// <summary>
        /// Make a A to G data set for testing with external clocking
        /// </summary>
        public static EventCollection GetEventSetOne(long baseTime, long numMSecBetweenEvents)
        {
            LinkedHashMap<String, Object> testData = MakeMixedSet();
            LinkedHashMap<String, long?> times = MakeExternalClockTimes(testData, baseTime, numMSecBetweenEvents);
            return new EventCollection(testData, times);
        }
    
        /// <summary>
        /// Make a A only data set for testing with external clocking
        /// </summary>
        public static EventCollection GetSetTwoExternalClock(long baseTime, long numMSecBetweenEvents)
        {
            LinkedHashMap<String, Object> testData = MakeTestDataUniform();
            LinkedHashMap<String, long?> times = MakeExternalClockTimes(testData, baseTime, numMSecBetweenEvents);
            return new EventCollection(testData, times);
        }
    
        public static EventCollection GetSetThreeExternalClock(long baseTime, long numMSecBetweenEvents)
        {
            LinkedHashMap<String, Object> testData = MakeTestDataNumeric();
            LinkedHashMap<String, long?> times = MakeExternalClockTimes(testData, baseTime, numMSecBetweenEvents);
            return new EventCollection(testData, times);
        }
    
        public static EventCollection GetSetFourExternalClock(long baseTime, long numMSecBetweenEvents)
        {
            LinkedHashMap<String, Object> testData = MakeTestDataS0();
            LinkedHashMap<String, long?> times = MakeExternalClockTimes(testData, baseTime, numMSecBetweenEvents);
            return new EventCollection(testData, times);
        }
    
        public static EventCollection GetSetFiveInterfaces()
        {
            LinkedHashMap<String, Object> testData = MakeTestDataInterfaces();
            LinkedHashMap<String, long?> times = MakeExternalClockTimes(testData, 0, 100);
            return new EventCollection(testData, times);
        }
    
        public static EventCollection GetSetSixComplexProperties()
        {
            LinkedHashMap<String, Object> testData = MakeTestDataComplexProps();
            LinkedHashMap<String, long?> times = MakeExternalClockTimes(testData, 0, 100);
            return new EventCollection(testData, times);
        }
    
        private static LinkedHashMap<String, Object> MakeMixedSet()
        {
            LinkedHashMap<String, Object> testData = new LinkedHashMap<String, Object>();
    
            testData["A1"] = new SupportBean_A("A1");
            testData["B1"] = new SupportBean_B("B1");
            testData["C1"] = new SupportBean_C("C1");
            testData["B2"] = new SupportBean_B("B2");
            testData["A2"] = new SupportBean_A("A2");
            testData["D1"] = new SupportBean_D("D1");
            testData["E1"] = new SupportBean_E("E1");
            testData["F1"] = new SupportBean_F("F1");
            testData["D2"] = new SupportBean_D("D2");
            testData["B3"] = new SupportBean_B("B3");
            testData["G1"] = new SupportBean_G("G1");
            testData["D3"] = new SupportBean_D("D3");
    
            return testData;
        }
    
        // Make time values sending events exactly every seconds, starting at time zero, first event after 1 second
        private static LinkedHashMap<String, long?> MakeExternalClockTimes(LinkedHashMap<String, Object> testData,
                                                                          long baseTime,
                                                                          long numMSecBetweenEvents)
        {
            LinkedHashMap<String, long?> testDataTimers = new LinkedHashMap<String, long?>();
    
            testDataTimers.Put(EventCollection.ON_START_EVENT_ID, baseTime);
    
            foreach (String id in testData.Keys)
            {
                baseTime += numMSecBetweenEvents;
                testDataTimers.Put(id, baseTime);
            }
    
            return testDataTimers;
        }
    
        private static LinkedHashMap<String, Object> MakeTestDataUniform()
        {
            LinkedHashMap<String, Object> testData = new LinkedHashMap<String, Object>();
    
            testData["B1"] = new SupportBean_A("B1");
            testData["B2"] = new SupportBean_A("B2");
            testData["B3"] = new SupportBean_A("B3");
            testData["A4"] = new SupportBean_A("A4");
            testData["A5"] = new SupportBean_A("A5");
            testData["A6"] = new SupportBean_A("A6");
    
            return testData;
        }
    
        private static LinkedHashMap<String, Object> MakeTestDataNumeric()
        {
            LinkedHashMap<String, Object> testData = new LinkedHashMap<String, Object>();
    
            testData["N1"] = new SupportBean_N( 01, -56, 44.0,  -60.5,  true,  true);
            testData["N2"] = new SupportBean_N( 66,  59, 48.0, 70.999,  true, false);
            testData["N3"] = new SupportBean_N( 87,  -5, 44.5,  -23.5, false,  true);
            testData["N4"] = new SupportBean_N( 86, -98, 42.1,  -79.5,  true,  true);
            testData["N5"] = new SupportBean_N( 00, -33, 48.0,  44.45,  true, false);
            testData["N6"] = new SupportBean_N( 55, -55, 44.0,  -60.5, false,  true);
            testData["N7"] = new SupportBean_N( 34,  92, 39.0,  -66.5, false,  true);
            testData["N8"] = new SupportBean_N(100,  66, 47.5,   45.0,  true, false);
    
            return testData;
        }
    
        private static LinkedHashMap<String, Object> MakeTestDataS0()
        {
            LinkedHashMap<String, Object> testData = new LinkedHashMap<String, Object>();
    
            // B arrives 3 times
            // G arrives twice, in a row
            // F and C arrive twice
            testData["e1"] = new SupportBean_S0(1,   "A");
            testData["e2"] = new SupportBean_S0(2,   "B");   // B
            testData["e3"] = new SupportBean_S0(3,   "C");                   // C
            testData["e4"] = new SupportBean_S0(4,   "D");
            testData["e5"] = new SupportBean_S0(5,   "E");
            testData["e6"] = new SupportBean_S0(6,   "B");   // B
            testData["e7"] = new SupportBean_S0(7,   "F");               // F
            testData["e8"] = new SupportBean_S0(8,   "C");                   // C
            testData["e9"] = new SupportBean_S0(9,   "G");           // G
            testData["e10"] = new SupportBean_S0(10, "G");           // G
            testData["e11"] = new SupportBean_S0(11, "B");   // B
            testData["e12"] = new SupportBean_S0(12, "F");               // F
    
            return testData;
        }
    
        /// <summary>
        /// ISupportBaseAB ISupportA ISupportB ISupportABCImpl
        /// </summary>
        /// <unknown>@return</unknown>
    
        private static LinkedHashMap<String, Object> MakeTestDataInterfaces()
        {
            LinkedHashMap<String, Object> testData = new LinkedHashMap<String, Object>();
    
            testData["e1"] = new ISupportCImpl("C1");
            testData["e2"] = new ISupportABCImpl("A1", "B1", "BaseB", "C1");
            testData["e3"] = new ISupportAImpl("A1", "BaseAB");
            testData["e4"] = new ISupportBImpl("B1", "BaseAB");
            testData["e5"] = new ISupportDImpl("D1", "BaseD", "BaseDBase");
            testData["e6"] = new ISupportBCImpl("B2", "BaseAB2", "C2");
            testData["e7"] = new ISupportBaseABImpl("BaseAB3");
            testData["e8"] = new SupportOverrideOneA("OA1", "O1", "OBase");
            testData["e9"] = new SupportOverrideOneB("OB1", "O2", "OBase");
            testData["e10"] = new SupportOverrideOne("O3", "OBase");
            testData["e11"] = new SupportOverrideBase("OBase");
            testData["e12"] = new ISupportAImplSuperGImplPlus("G1", "A3", "BaseAB4", "B4", "C2");
            testData["e13"] = new ISupportAImplSuperGImpl("G2", "A14", "BaseAB5");
    
            return testData;
        }
    
        private static LinkedHashMap<String, Object> MakeTestDataComplexProps()
        {
            LinkedHashMap<String, Object> testData = new LinkedHashMap<String, Object>();
    
            testData["e1"] = SupportBeanComplexProps.MakeDefaultBean();
            testData["e2"] = SupportBeanCombinedProps.MakeDefaultBean();
    
            return testData;
        }
    }
}
