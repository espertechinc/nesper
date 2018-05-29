///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.container;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util.support;


using NUnit.Framework;

namespace com.espertech.esper.regression.events.xml
{
    public class ExecEventXMLSchemaEventTypes : RegressionExecution {
        private const string CLASSLOADER_SCHEMA_URI = "regression/typeTestSchema.xsd";
    
        public override void Configure(Configuration configuration) {
            var eventTypeMeta = new ConfigurationEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "typesEvent";
            string schemaUri = SupportContainer.Instance.ResourceManager().ResolveResourceURL(CLASSLOADER_SCHEMA_URI).ToString();
            eventTypeMeta.SchemaResource = schemaUri;
            configuration.AddEventType("TestTypesEvent", eventTypeMeta);
        }
    
        public override void Run(EPServiceProvider epService) {
            string stmtSelectWild = "select * from TestTypesEvent";
            EPStatement wildStmt = epService.EPAdministrator.CreateEPL(stmtSelectWild);
            EventType type = wildStmt.EventType;
            SupportEventTypeAssertionUtil.AssertConsistency(type);
    
            var types = new object[][]{
                    new object[] {"attrNonPositiveInteger", typeof(int?)},
                    new object[] {"attrNonNegativeInteger", typeof(int?)},
                    new object[] {"attrNegativeInteger", typeof(int?)},
                    new object[] {"attrPositiveInteger", typeof(int?)},
                    new object[] {"attrLong", typeof(long?)},
                    new object[] {"attrUnsignedLong", typeof(ulong?) },
                    new object[] {"attrInt", typeof(int?)},
                    new object[] {"attrUnsignedInt", typeof(uint?)},
                    new object[] {"attrDecimal", typeof(double?)},
                    new object[] {"attrInteger", typeof(int?)},
                    new object[] {"attrFloat", typeof(float?)},
                    new object[] {"attrDouble", typeof(double?)},
                    new object[] {"attrString", typeof(string)},
                    new object[] {"attrShort", typeof(short?)},
                    new object[] {"attrUnsignedShort", typeof(ushort?)},
                    new object[] {"attrByte", typeof(byte?) },
                    new object[] {"attrUnsignedByte", typeof(byte?) },
                    new object[] {"attrBoolean", typeof(bool?)},
                    new object[] {"attrDateTime", typeof(string)},
                    new object[] {"attrDate", typeof(string)},
                    new object[] {"attrTime", typeof(string)}};
    
            for (int i = 0; i < types.Length; i++) {
                string name = types[i][0].ToString();
                EventPropertyDescriptor desc = type.GetPropertyDescriptor(name);
                Type expected = (Type) types[i][1];
                Assert.AreEqual(expected, desc.PropertyType, "Failed for " + name);
            }
        }
    }
} // end of namespace
