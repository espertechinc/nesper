///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.XPath;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
	public class TestXMLSchemaEvent
    {
	    public const string CLASSLOADER_SCHEMA_URI = "regression/simpleSchema.xsd";
	    public const string CLASSLOADER_SCHEMA_WITH_ALL_URI = "regression/simpleSchemaWithAll.xsd";
	    public const string CLASSLOADER_SCHEMA_WITH_RESTRICTION_URI = "regression/simpleSchemaWithRestriction.xsd";

	    private EPServiceProvider _epService;
	    private SupportUpdateListener _updateListener;

        [Test]
	    public void TestSchemaXMLWSchemaWithRestriction() {
	        var config = SupportConfigFactory.GetConfiguration();
	        var eventTypeMeta = new ConfigurationEventTypeXMLDOM();
	        eventTypeMeta.RootElementName = "order";

	        var schemaStream = ResourceManager.GetResourceAsStream(CLASSLOADER_SCHEMA_WITH_RESTRICTION_URI);
	        Assert.IsNotNull(schemaStream);

            var schemaReader = new StreamReader(schemaStream);
            var schemaText = schemaReader.ReadToEnd();
            schemaReader.Close();
            schemaStream.Close();

	        eventTypeMeta.SchemaText = schemaText;
	        config.AddEventType("OrderEvent", eventTypeMeta);

	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);
	        }
	        _updateListener = new SupportUpdateListener();

	        var text = "select order_amount from OrderEvent";
	        var stmt = _epService.EPAdministrator.CreateEPL(text);
	        stmt.AddListener(_updateListener);

	        SupportXML.SendEvent(_epService.EPRuntime,
	                             "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
	                             "<order>\n" +
	                             "<order_amount>202.1</order_amount>" +
	                             "</order>");
	        var theEvent = _updateListener.LastNewData[0];
	        Assert.AreEqual(typeof(double), theEvent.Get("order_amount").GetType());
	        Assert.AreEqual(202.1d, theEvent.Get("order_amount"));
	        _updateListener.Reset();
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();
	        }
	        _updateListener = null;
	    }

        [Test]
	    public void TestSchemaXMLWSchemaWithAll() {
	        var config = SupportConfigFactory.GetConfiguration();
	        var eventTypeMeta = new ConfigurationEventTypeXMLDOM();
	        eventTypeMeta.RootElementName = "event-page-visit";
	        var schemaUri = ResourceManager.ResolveResourceURL(CLASSLOADER_SCHEMA_WITH_ALL_URI).ToString();
	        eventTypeMeta.SchemaResource = schemaUri;
	        eventTypeMeta.AddNamespacePrefix("ss", "samples:schemas:simpleSchemaWithAll");
	        eventTypeMeta.AddXPathProperty("url", "/ss:event-page-visit/ss:url", XPathResultType.String);
	        config.AddEventType("PageVisitEvent", eventTypeMeta);

	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);
	        }
	        _updateListener = new SupportUpdateListener();

	        // url='page4'
	        var text = "select a.url as sesja from pattern [ every a=PageVisitEvent(url='page1') ]";
	        var stmt = _epService.EPAdministrator.CreateEPL(text);
	        stmt.AddListener(_updateListener);

	        SupportXML.SendEvent(_epService.EPRuntime,
	                             "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
	                             "<event-page-visit xmlns=\"samples:schemas:simpleSchemaWithAll\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"samples:schemas:simpleSchemaWithAll simpleSchemaWithAll.xsd\">\n" +
	                             "<url>page1</url>" +
	                             "</event-page-visit>");
	        var theEvent = _updateListener.LastNewData[0];
	        Assert.AreEqual("page1", theEvent.Get("sesja"));
	        _updateListener.Reset();

	        SupportXML.SendEvent(_epService.EPRuntime,
	                             "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
	                             "<event-page-visit xmlns=\"samples:schemas:simpleSchemaWithAll\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"samples:schemas:simpleSchemaWithAll simpleSchemaWithAll.xsd\">\n" +
	                             "<url>page2</url>" +
	                             "</event-page-visit>");
	        Assert.IsFalse(_updateListener.IsInvoked);

	        var type = _epService.EPAdministrator.CreateEPL("select * from PageVisitEvent").EventType;
            EPAssertionUtil.AssertEqualsAnyOrder(new EventPropertyDescriptor[] {
	                                                 new EventPropertyDescriptor("sessionId", typeof(XmlNode), null, false, false, false, false, true),
	                                                 new EventPropertyDescriptor("customerId", typeof(XmlNode), null, false, false, false, false, true),
	                                                 new EventPropertyDescriptor("url", typeof(string), typeof(char), false, false, true, false, false),
	                                                 new EventPropertyDescriptor("method", typeof(XmlNode), null, false, false, false, false, true),
	                                             }, type.PropertyDescriptors);
	    }

        [Test]
	    public void TestSchemaXMLQuery_XPathBacked() {
	        _epService = EPServiceProviderManager.GetDefaultProvider(GetConfig(true));
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);
	        }
	        _updateListener = new SupportUpdateListener();

	        var stmtSelectWild = "select * from TestXMLSchemaType";
	        var wildStmt = _epService.EPAdministrator.CreateEPL(stmtSelectWild);
	        var type = wildStmt.EventType;
	        SupportEventTypeAssertionUtil.AssertConsistency(type);

            EPAssertionUtil.AssertEqualsAnyOrder(new EventPropertyDescriptor[] {
	                                                 new EventPropertyDescriptor("nested1", typeof(XmlNode), null, false, false, false, false, false),
	                                                 new EventPropertyDescriptor("prop4", typeof(string), typeof(char), false, false, true, false, false),
	                                                 new EventPropertyDescriptor("nested3", typeof(XmlNode), null, false, false, false, false, false),
	                                                 new EventPropertyDescriptor("customProp", typeof(double?), null, false, false, false, false, false),
	                                             }, type.PropertyDescriptors);

	        var stmt =
	            "select nested1 as nodeProp," +
	            "prop4 as nested1Prop," +
	            "nested1.prop2 as nested2Prop," +
	            "nested3.nested4('a').prop5[1] as complexProp," +
	            "nested1.nested2.prop3[2] as indexedProp," +
	            "customProp," +
	            "prop4.attr2 as attrOneProp," +
	            "nested3.nested4[2].id as attrTwoProp" +
	            " from TestXMLSchemaType#length(100)";

	        var selectStmt = _epService.EPAdministrator.CreateEPL(stmt);
	        selectStmt.AddListener(_updateListener);
	        type = selectStmt.EventType;
	        SupportEventTypeAssertionUtil.AssertConsistency(type);
            EPAssertionUtil.AssertEqualsAnyOrder(new EventPropertyDescriptor[] {
	                                                 new EventPropertyDescriptor("nodeProp", typeof(XmlNode), null, false, false, false, false, false),
	                                                 new EventPropertyDescriptor("nested1Prop", typeof(string), typeof(char), false, false, true, false, false),
	                                                 new EventPropertyDescriptor("nested2Prop", typeof(bool?), null, false, false, false, false, false),
	                                                 new EventPropertyDescriptor("complexProp", typeof(string), typeof(char), false, false, true, false, false),
	                                                 new EventPropertyDescriptor("indexedProp", typeof(int?), null, false, false, false, false, false),
	                                                 new EventPropertyDescriptor("customProp", typeof(double?), null, false, false, false, false, false),
	                                                 new EventPropertyDescriptor("attrOneProp", typeof(bool?), null, false, false, false, false, false),
	                                                 new EventPropertyDescriptor("attrTwoProp", typeof(string), typeof(char), false, false, true, false, false),
	                                             }, type.PropertyDescriptors);

	        var eventDoc = SupportXML.SendDefaultEvent(_epService.EPRuntime, "test");

	        Assert.IsNotNull(_updateListener.LastNewData);
	        var theEvent = _updateListener.LastNewData[0];

	        Assert.AreSame(eventDoc.DocumentElement.ChildNodes.Item(0), theEvent.Get("nodeProp"));
	        Assert.AreEqual("SAMPLE_V6", theEvent.Get("nested1Prop"));
	        Assert.AreEqual(true, theEvent.Get("nested2Prop"));
	        Assert.AreEqual("SAMPLE_V8", theEvent.Get("complexProp"));
	        Assert.AreEqual(5, theEvent.Get("indexedProp"));
	        Assert.AreEqual(3.0, theEvent.Get("customProp"));
	        Assert.AreEqual(true, theEvent.Get("attrOneProp"));
	        Assert.AreEqual("c", theEvent.Get("attrTwoProp"));
	    }

        [Test]
	    public void TestSchemaXMLQuery_DOMGetterBacked() {
	        _epService = EPServiceProviderManager.GetDefaultProvider(GetConfig(false));
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);
	        }
	        _updateListener = new SupportUpdateListener();

	        var stmtSelectWild = "select * from TestXMLSchemaType";
	        var wildStmt = _epService.EPAdministrator.CreateEPL(stmtSelectWild);
	        var type = wildStmt.EventType;
	        SupportEventTypeAssertionUtil.AssertConsistency(type);

            EPAssertionUtil.AssertEqualsAnyOrder(new EventPropertyDescriptor[] {
	                                                 new EventPropertyDescriptor("nested1", typeof(XmlNode), null, false, false, false, false, true),
	                                                 new EventPropertyDescriptor("prop4", typeof(string), typeof(char), false, false, true, false, false),
	                                                 new EventPropertyDescriptor("nested3", typeof(XmlNode), null, false, false, false, false, true),
	                                                 new EventPropertyDescriptor("customProp", typeof(double?), null, false, false, false, false, false),
	                                             }, type.PropertyDescriptors);

	        var stmt =
	            "select nested1 as nodeProp," +
	            "prop4 as nested1Prop," +
	            "nested1.prop2 as nested2Prop," +
	            "nested3.nested4('a').prop5[1] as complexProp," +
	            "nested1.nested2.prop3[2] as indexedProp," +
	            "customProp," +
	            "prop4.attr2 as attrOneProp," +
	            "nested3.nested4[2].id as attrTwoProp" +
	            " from TestXMLSchemaType#length(100)";

	        var selectStmt = _epService.EPAdministrator.CreateEPL(stmt);
	        selectStmt.AddListener(_updateListener);
	        type = selectStmt.EventType;
	        SupportEventTypeAssertionUtil.AssertConsistency(type);
            EPAssertionUtil.AssertEqualsAnyOrder(new EventPropertyDescriptor[] {
	                                                 new EventPropertyDescriptor("nodeProp", typeof(XmlNode), null, false, false, false, false, true),
	                                                 new EventPropertyDescriptor("nested1Prop", typeof(string), typeof(char), false, false, true, false, false),
	                                                 new EventPropertyDescriptor("nested2Prop", typeof(bool?), null, false, false, false, false, false),
	                                                 new EventPropertyDescriptor("complexProp", typeof(string), typeof(char), false, false, true, false, false),
	                                                 new EventPropertyDescriptor("indexedProp", typeof(int?), null, false, false, false, false, false),
	                                                 new EventPropertyDescriptor("customProp", typeof(double?), null, false, false, false, false, false),
	                                                 new EventPropertyDescriptor("attrOneProp", typeof(bool?), null, false, false, false, false, false),
	                                                 new EventPropertyDescriptor("attrTwoProp", typeof(string), typeof(char), false, false, true, false, false),
	                                             }, type.PropertyDescriptors);

	        var eventDoc = SupportXML.SendDefaultEvent(_epService.EPRuntime, "test");

	        Assert.IsNotNull(_updateListener.LastNewData);
	        var theEvent = _updateListener.LastNewData[0];

	        Assert.AreSame(eventDoc.DocumentElement.ChildNodes.Item(0), theEvent.Get("nodeProp"));
	        Assert.AreEqual("SAMPLE_V6", theEvent.Get("nested1Prop"));
	        Assert.AreEqual(true, theEvent.Get("nested2Prop"));
	        Assert.AreEqual("SAMPLE_V8", theEvent.Get("complexProp"));
	        Assert.AreEqual(5, theEvent.Get("indexedProp"));
	        Assert.AreEqual(3.0, theEvent.Get("customProp"));
	        Assert.AreEqual(true, theEvent.Get("attrOneProp"));
	        Assert.AreEqual("c", theEvent.Get("attrTwoProp"));
	    }

        [Test]
	    public void TestAddRemoveType() {
	        _epService = EPServiceProviderManager.GetDefaultProvider(GetConfig(false));
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);
	        }
	        _updateListener = new SupportUpdateListener();
	        var configOps = _epService.EPAdministrator.Configuration;

	        // test remove type with statement used (no force)
	        configOps.AddEventType("MyXMLEvent", GetConfigTestType("p01", false));
	        var stmt = _epService.EPAdministrator.CreateEPL("select p01 from MyXMLEvent", "stmtOne");
	        EPAssertionUtil.AssertEqualsExactOrder(configOps.GetEventTypeNameUsedBy("MyXMLEvent").ToArray(), new string[] {"stmtOne"});

	        try {
	            configOps.RemoveEventType("MyXMLEvent", false);
	        } catch (ConfigurationException ex) {
	            Assert.IsTrue(ex.Message.Contains("MyXMLEvent"));
	        }

	        // destroy statement and type
	        stmt.Dispose();
	        Assert.IsTrue(configOps.GetEventTypeNameUsedBy("MyXMLEvent").IsEmpty());
	        Assert.IsTrue(configOps.IsEventTypeExists("MyXMLEvent"));
	        Assert.IsTrue(configOps.RemoveEventType("MyXMLEvent", false));
	        Assert.IsFalse(configOps.RemoveEventType("MyXMLEvent", false));    // try double-remove
	        Assert.IsFalse(configOps.IsEventTypeExists("MyXMLEvent"));
	        try {
	            _epService.EPAdministrator.CreateEPL("select p01 from MyXMLEvent");
                Assert.Fail();
	        } catch (EPException) {
	            // expected
	        }

	        // add back the type
	        configOps.AddEventType("MyXMLEvent", GetConfigTestType("p20", false));
	        Assert.IsTrue(configOps.IsEventTypeExists("MyXMLEvent"));
	        Assert.IsTrue(configOps.GetEventTypeNameUsedBy("MyXMLEvent").IsEmpty());

	        // compile
	        _epService.EPAdministrator.CreateEPL("select p20 from MyXMLEvent", "stmtTwo");
	        EPAssertionUtil.AssertEqualsExactOrder(configOps.GetEventTypeNameUsedBy("MyXMLEvent").ToArray(), new string[] {"stmtTwo"});
	        try {
	            _epService.EPAdministrator.CreateEPL("select p01 from MyXMLEvent");
                Assert.Fail();
	        } catch (EPException) {
	            // expected
	        }

	        // remove with force
	        try {
	            configOps.RemoveEventType("MyXMLEvent", false);
	        } catch (ConfigurationException ex) {
	            Assert.IsTrue(ex.Message.Contains("MyXMLEvent"));
	        }
	        Assert.IsTrue(configOps.RemoveEventType("MyXMLEvent", true));
	        Assert.IsFalse(configOps.IsEventTypeExists("MyXMLEvent"));
	        Assert.IsTrue(configOps.GetEventTypeNameUsedBy("MyXMLEvent").IsEmpty());

	        // add back the type
	        configOps.AddEventType("MyXMLEvent", GetConfigTestType("p03", false));
	        Assert.IsTrue(configOps.IsEventTypeExists("MyXMLEvent"));

	        // compile
	        _epService.EPAdministrator.CreateEPL("select p03 from MyXMLEvent");
	        try {
	            _epService.EPAdministrator.CreateEPL("select p20 from MyXMLEvent");
                Assert.Fail();
	        } catch (EPException) {
	            // expected
	        }
	    }

        [Test]
	    public void TestInvalid() {
	        _epService = EPServiceProviderManager.GetDefaultProvider(GetConfig(false));
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);
	        }

	        try {
	            _epService.EPAdministrator.CreateEPL("select element1 from TestXMLSchemaType#length(100)");
	            Assert.Fail();
	        } catch (EPStatementException ex) {
	            Assert.AreEqual("Error starting statement: Failed to validate select-clause expression 'element1': Property named 'element1' is not valid in any stream [select element1 from TestXMLSchemaType#length(100)]", ex.Message);
	        }
	    }

	    private Configuration GetConfig(bool isUseXPathPropertyExpression) {
	        var configuration = SupportConfigFactory.GetConfiguration();
	        configuration.AddEventType("TestXMLSchemaType", GetConfigTestType(null, isUseXPathPropertyExpression));
	        return configuration;
	    }

	    private ConfigurationEventTypeXMLDOM GetConfigTestType(string additionalXPathProperty, bool isUseXPathPropertyExpression) {
	        var eventTypeMeta = new ConfigurationEventTypeXMLDOM();
	        eventTypeMeta.RootElementName = "simpleEvent";
	        var schemaUri = ResourceManager.ResolveResourceURL(CLASSLOADER_SCHEMA_URI).ToString();
	        eventTypeMeta.SchemaResource = schemaUri;
	        eventTypeMeta.AddNamespacePrefix("ss", "samples:schemas:simpleSchema");
	        eventTypeMeta.AddXPathProperty("customProp", "count(/ss:simpleEvent/ss:nested3/ss:nested4)", XPathResultType.Number);
	        eventTypeMeta.IsXPathPropertyExpr = isUseXPathPropertyExpression;
	        if (additionalXPathProperty != null) {
	            eventTypeMeta.AddXPathProperty(additionalXPathProperty, "count(/ss:simpleEvent/ss:nested3/ss:nested4)", XPathResultType.Number);
	        }
	        return eventTypeMeta;
	    }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
