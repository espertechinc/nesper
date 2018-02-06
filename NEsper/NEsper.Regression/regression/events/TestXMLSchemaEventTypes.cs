///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
	public class TestXMLSchemaEventTypes
    {
	    private static string CLASSLOADER_SCHEMA_URI = "regression/typeTestSchema.xsd";

	    private EPServiceProvider _epService;

        [Test]
	    public void TestSchemaXMLTypes() {
	        Configuration configuration = SupportConfigFactory.GetConfiguration();
	        ConfigurationEventTypeXMLDOM eventTypeMeta = new ConfigurationEventTypeXMLDOM();
	        eventTypeMeta.RootElementName = "typesEvent";
	        string schemaUri = ResourceManager.ResolveResourceURL(CLASSLOADER_SCHEMA_URI).ToString();
	        eventTypeMeta.SchemaResource = schemaUri;
	        configuration.AddEventType("TestTypesEvent", eventTypeMeta);

	        _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);
	        }

	        string stmtSelectWild = "select * from TestTypesEvent";
	        EPStatement wildStmt = _epService.EPAdministrator.CreateEPL(stmtSelectWild);
	        EventType type = wildStmt.EventType;
	        SupportEventTypeAssertionUtil.AssertConsistency(type);

	        object[][] types = new object[][] {
	            new object[] {"attrNonPositiveInteger", typeof(int?)},
	            new object[] {"attrNonNegativeInteger", typeof(int?)},
	            new object[] {"attrNegativeInteger", typeof(int?)},
	            new object[] {"attrPositiveInteger", typeof(int?)},
	            new object[] {"attrLong", typeof(long?)},
	            new object[] {"attrUnsignedLong", typeof(ulong?)},
	            new object[] {"attrInt", typeof(int?)},
	            new object[] {"attrUnsignedInt", typeof(uint?)},
	            new object[] {"attrDecimal", typeof(double?)},
	            new object[] {"attrInteger", typeof(int?)},
	            new object[] {"attrFloat", typeof(float?)},
	            new object[] {"attrDouble", typeof(double?)},
	            new object[] {"attrString", typeof(string)},
	            new object[] {"attrShort", typeof(short?)},
	            new object[] {"attrUnsignedShort", typeof(ushort?)},
	            new object[] {"attrByte", typeof(byte?)},
	            new object[] {"attrUnsignedByte", typeof(byte?)},
	            new object[] {"attrBoolean", typeof(bool?)},
	            new object[] {"attrDateTime", typeof(string)},
	            new object[] {"attrDate", typeof(string)},
	            new object[] {"attrTime", typeof(string)}
	        };

	        for (int i = 0; i < types.Length; i++) {
	            string name = types[i][0].ToString();
	            EventPropertyDescriptor desc = type.GetPropertyDescriptor(name);
	            Type expected = (Type) types[i][1];
                Assert.AreEqual(expected, desc.PropertyType, "Failed for " + name);
	        }

	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();
	        }
	    }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
