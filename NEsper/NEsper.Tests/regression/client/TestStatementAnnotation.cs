///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.annotation;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    using DescriptionAttribute = esper.client.annotation.DescriptionAttribute;

    [TestFixture]
    public class TestStatementAnnotation
    {
        private readonly String NEWLINE = Environment.NewLine;

        private EPServiceProvider _epService;

        [Test]
        public void TestInvalid()
        {
            var configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddEventType("Bean", typeof(SupportBean).FullName);
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            _epService.EPAdministrator.Configuration.AddImport("com.espertech.esper.regression.client");

            TryInvalid("@MyAnnotationNested(NestableSimple=@MyAnnotationNestableSimple, NestableValues=@MyAnnotationNestableValues, NestableNestable=@MyAnnotationNestableNestable) select * from Bean", false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationNestableNestable' requires a value for attribute 'Value' [@MyAnnotationNested(NestableSimple=@MyAnnotationNestableSimple, NestableValues=@MyAnnotationNestableValues, NestableNestable=@MyAnnotationNestableNestable) select * from Bean]");

            TryInvalid("@MyAnnotationNested(NestableNestable=@MyAnnotationNestableNestable('A'), NestableSimple=1) select * from Bean", false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationNested' requires a MyAnnotationNestableSimpleAttribute-typed value for attribute 'NestableSimple' but received a System.Int32-typed value [@MyAnnotationNested(NestableNestable=@MyAnnotationNestableNestable('A'), NestableSimple=1) select * from Bean]");

            TryInvalid("@MyAnnotationValuePair(StringVal='abc') select * from Bean", false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationValuePair' requires a value for attribute 'BooleanVal' [@MyAnnotationValuePair(StringVal='abc') select * from Bean]");

            TryInvalid("MyAnnotationValueArray(Value=5) select * from Bean", true,
                    "Incorrect syntax near 'MyAnnotationValueArray' [MyAnnotationValueArray(Value=5) select * from Bean]");

            TryInvalid("@MyAnnotationValueArray(Value=null) select * from Bean", false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationValueArray' requires a value for attribute 'DoubleArray' [@MyAnnotationValueArray(Value=null) select * from Bean]");

            TryInvalid("@MyAnnotationValueArray(IntArray={},DoubleArray={},StringArray={null},Value={}) select * from Bean", false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationValueArray' requires a non-null value for array elements for attribute 'StringArray' [@MyAnnotationValueArray(IntArray={},DoubleArray={},StringArray={null},Value={}) select * from Bean]");

            TryInvalid("@MyAnnotationValueArray(IntArray={},DoubleArray={},StringArray={1},Value={}) select * from Bean", false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationValueArray' requires a System.String-typed value for array elements for attribute 'StringArray' but received a System.Int32-typed value [@MyAnnotationValueArray(IntArray={},DoubleArray={},StringArray={1},Value={}) select * from Bean]");

            TryInvalid("@MyAnnotationValue(Value='a', Value='a') select * from Bean", false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationValue' has duplicate attribute values for attribute 'Value' [@MyAnnotationValue(Value='a', Value='a') select * from Bean]");
            TryInvalid("@ABC select * from Bean", false,
                    "Failed to process statement annotations: Failed to resolve @-annotation class: Could not load class by name 'ABCAttribute', please check imports [@ABC select * from Bean]");

            TryInvalid("@MyAnnotationSimple(5) select * from Bean", false,
                    "Failed to process statement annotations: Failed to find property Value in annotation type MyAnnotationSimple [@MyAnnotationSimple(5) select * from Bean]");
            TryInvalid("@MyAnnotationSimple(null) select * from Bean", false,
                    "Failed to process statement annotations: Failed to find property Value in annotation type MyAnnotationSimple [@MyAnnotationSimple(null) select * from Bean]");

            TryInvalid("@MyAnnotationValue select * from Bean", false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationValue' requires a value for attribute 'Value' [@MyAnnotationValue select * from Bean]");

            TryInvalid("@MyAnnotationValue(5) select * from Bean", false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationValue' requires a String-typed value for attribute 'Value' but received a System.Int32-typed value [@MyAnnotationValue(5) select * from Bean]");
            TryInvalid("@MyAnnotationValueArray(Value=\"ABC\", IntArray={}, DoubleArray={}, StringArray={}) select * from Bean", false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationValueArray' requires a System.Int64[]-typed value for attribute 'Value' but received a System.String-typed value [@MyAnnotationValueArray(Value=\"ABC\", IntArray={}, DoubleArray={}, StringArray={}) select * from Bean]");
            TryInvalid("@MyAnnotationValueEnum(a.b.CC) select * from Bean", false,
                    "Annotation enumeration value 'a.b.CC' not recognized as an enumeration class, please check imports or type used [@MyAnnotationValueEnum(a.b.CC) select * from Bean]");

            TryInvalid("@Hint('XXX') select * from Bean", false,
                    "Failed to process statement annotations: Hint annotation value 'XXX' is not one of the known values [@Hint('XXX') select * from Bean]");
            TryInvalid("@Hint('ITERATE_ONLY,XYZ') select * from Bean", false,
                    "Failed to process statement annotations: Hint annotation value 'XYZ' is not one of the known values [@Hint('ITERATE_ONLY,XYZ') select * from Bean]");
            TryInvalid("@Hint('testit=5') select * from Bean", false,
                    "Failed to process statement annotations: Hint annotation value 'testit' is not one of the known values [@Hint('testit=5') select * from Bean]");
            TryInvalid("@Hint('RECLAIM_GROUP_AGED') select * from Bean", false,
                    "Failed to process statement annotations: Hint 'RECLAIM_GROUP_AGED' requires a parameter value [@Hint('RECLAIM_GROUP_AGED') select * from Bean]");
            TryInvalid("@Hint('ITERATE_ONLY,RECLAIM_GROUP_AGED') select * from Bean", false,
                    "Failed to process statement annotations: Hint 'RECLAIM_GROUP_AGED' requires a parameter value [@Hint('ITERATE_ONLY,RECLAIM_GROUP_AGED') select * from Bean]");
            TryInvalid("@Hint('ITERATE_ONLY=5,RECLAIM_GROUP_AGED=5') select * from Bean", false,
                    "Failed to process statement annotations: Hint 'ITERATE_ONLY' does not accept a parameter value [@Hint('ITERATE_ONLY=5,RECLAIM_GROUP_AGED=5') select * from Bean]");
            TryInvalid("@Hint('index(name)xxx') select * from Bean", false,
                        "Failed to process statement annotations: Hint 'INDEX' has additional text after parentheses [@Hint('index(name)xxx') select * from Bean]");
            TryInvalid("@Hint('index') select * from Bean", false,
                        "Failed to process statement annotations: Hint 'INDEX' requires additional parameters in parentheses [@Hint('index') select * from Bean]");
        
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        private void TryInvalid(String stmtText, bool isSyntax, String message)
        {
            try
            {
                _epService.EPAdministrator.CreateEPL(stmtText);
                Assert.Fail();
            }
            catch (EPStatementSyntaxException ex)
            {
                Assert.IsTrue(isSyntax);
                Assert.AreEqual(message, ex.Message);
            }
            catch (EPStatementException ex)
            {
                Assert.IsFalse(isSyntax);
                Assert.AreEqual(message, ex.Message);
            }
        }

        [Test]
        public void TestBuiltin()
        {
            var configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddEventType("Bean", typeof(SupportBean).FullName);
            configuration.AddImport(typeof(MyAnnotationNestableValuesAttribute).Namespace);
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            var stmtText = "@Name('MyTestStmt') @Description('MyTestStmt description') @Tag(Name=\"UserId\", Value=\"Value\") select * from Bean";
            var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            Assert.IsTrue((((EPStatementSPI)stmt).IsNameProvided));
            RunAssertion(stmt);
            stmt.Dispose();
            var name = (NameAttribute)AnnotationUtil.FindAttribute(stmt.Annotations, typeof(NameAttribute));
            Assert.AreEqual("MyTestStmt", name.Value);

            // try lowercase
            var stmtTextLower = "@Name('MyTestStmt') @Description('MyTestStmt description') @Tag(Name=\"UserId\", Value=\"Value\") select * from Bean";
            stmt = _epService.EPAdministrator.CreateEPL(stmtTextLower);
            RunAssertion(stmt);
            stmt.Dispose();

            // try pattern
            stmtText = "@Name('MyTestStmt') @Description('MyTestStmt description') @Tag(Name='UserId', Value='Value') every Bean";
            stmt = _epService.EPAdministrator.CreatePattern(stmtText);
            RunAssertion(stmt);
            stmt.Dispose();

            stmtText = "@Name('MyTestStmt') @Description('MyTestStmt description') @Tag(Name=\"UserId\", Value=\"Value\") every Bean";
            stmt = _epService.EPAdministrator.CreatePattern(stmtText);
            RunAssertion(stmt);

            _epService.EPAdministrator.CreateEPL("@Hint('ITERATE_ONLY') select * from Bean");
            _epService.EPAdministrator.CreateEPL("@Hint('ITERATE_ONLY,DISABLE_RECLAIM_GROUP') select * from Bean");
            _epService.EPAdministrator.CreateEPL("@Hint('ITERATE_ONLY,DISABLE_RECLAIM_GROUP,ITERATE_ONLY') select * from Bean");
            _epService.EPAdministrator.CreateEPL("@Hint('  iterate_only ') select * from Bean");

            // test statement name override
            stmtText = "@Name('MyAnnotatedName') select * from Bean";
            stmt = _epService.EPAdministrator.CreateEPL(stmtText, "MyABCStmt");
            Assert.AreEqual("MyABCStmt", stmt.Name);

            // hint tests
            Assert.IsNull(HintEnum.DISABLE_RECLAIM_GROUP.GetHint(null));
            Assert.IsNull(HintEnum.DISABLE_RECLAIM_GROUP.GetHint(new Attribute[0]));

            var annos =
                _epService.EPAdministrator.CreateEPL("@Hint('DISABLE_RECLAIM_GROUP') select * from Bean").Annotations.ToArray();
            Assert.AreEqual("DISABLE_RECLAIM_GROUP", HintEnum.DISABLE_RECLAIM_GROUP.GetHint(annos).Value);

            annos = _epService.EPAdministrator.CreateEPL("@Hint('ITERATE_ONLY,ITERATE_ONLY,DISABLE_RECLAIM_GROUP,ITERATE_ONLY') select * from Bean").Annotations.ToArray();
            Assert.AreEqual("ITERATE_ONLY,ITERATE_ONLY,DISABLE_RECLAIM_GROUP,ITERATE_ONLY", HintEnum.DISABLE_RECLAIM_GROUP.GetHint(annos).Value);

            annos = _epService.EPAdministrator.CreateEPL("@Hint('ITERATE_ONLY,reclaim_group_aged=10') select * from Bean").Annotations.ToArray();
            var hint = HintEnum.RECLAIM_GROUP_AGED.GetHint(annos);
            Assert.AreEqual("10", HintEnum.RECLAIM_GROUP_AGED.GetHintAssignedValue(hint));

            annos = _epService.EPAdministrator.CreateEPL("@Hint('reclaim_group_aged=11') select * from Bean").Annotations.ToArray();
            hint = HintEnum.RECLAIM_GROUP_AGED.GetHint(annos);
            Assert.AreEqual("11", HintEnum.RECLAIM_GROUP_AGED.GetHintAssignedValue(hint));

            annos = _epService.EPAdministrator.CreateEPL("@Hint('index(one, two)') select * from Bean").Annotations.ToArray();
            Assert.AreEqual("one, two", HintEnum.INDEX.GetHintAssignedValues(annos)[0]);

            // NoLock
            stmt = _epService.EPAdministrator.CreateEPL("@NoLock select * from Bean");
            Assert.NotNull(AnnotationUtil.FindAttribute(stmt.Annotations.ToArray(), typeof(NoLockAttribute)));
            Assert.AreEqual(1, AnnotationUtil.FindAttributes(stmt.Annotations.ToArray(), typeof(NoLockAttribute)).Count);
        
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        private void RunAssertion(EPStatement stmt)
        {
            var annotations = stmt.Annotations.ToArray();
            annotations = SortAlpha(annotations);
            Assert.AreEqual(3, annotations.Length);

            Assert.AreEqual(typeof(DescriptionAttribute), annotations[0].GetType());
            Assert.AreEqual("MyTestStmt description", ((DescriptionAttribute)annotations[0]).Value);
            Assert.AreEqual("@Description(\"MyTestStmt description\")", annotations[0].ToString());

            Assert.AreEqual(typeof(NameAttribute), annotations[1].GetType());
            Assert.AreEqual("MyTestStmt", ((NameAttribute)annotations[1]).Value);
            Assert.AreEqual("MyTestStmt", stmt.Name);
            Assert.AreEqual("@Name(\"MyTestStmt\")", annotations[1].ToString());

            Assert.AreEqual(typeof(TagAttribute), annotations[2].GetType());
            Assert.AreEqual("UserId", ((TagAttribute)annotations[2]).Name);
            Assert.AreEqual("Value", ((TagAttribute)annotations[2]).Value);
            Assert.AreEqual("@Tag(Name=\"UserId\", Value=\"Value\")", annotations[2].ToString());

            Assert.IsFalse(annotations[2].Equals(annotations[1]));
            Assert.IsTrue(annotations[1].Equals(annotations[1]));
            Assert.IsTrue(annotations[1].GetHashCode() != 0);
        }

        [Test]
        public void TestClientAppAnnotationSimple()
        {
            var configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddEventType("Bean", typeof(SupportBean).FullName);
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            _epService.EPAdministrator.Configuration.AddImport("com.espertech.esper.regression.client");
            _epService.EPAdministrator.Configuration.AddImport(typeof(SupportEnum));

            var stmtText =
                    "@MyAnnotationSimple " +
                            "@MyAnnotationValue('abc') " +
                            "@MyAnnotationValueDefaulted " +
                            "@MyAnnotationValueEnum(SupportEnum=com.espertech.esper.support.bean.SupportEnum.ENUM_VALUE_3) " +
                            "@MyAnnotationValuePair(StringVal='a',IntVal=-1,LongVal=2,BooleanVal=True,CharVal='x',ByteVal=10,ShortVal=20,DoubleVal=2.5) " +
                            "@Name('STMTONE') " +
                            "select * from Bean";
            var stmtTextFormatted = "@MyAnnotationSimple" + NEWLINE +
                    "@MyAnnotationValue('abc')" + NEWLINE +
                    "@MyAnnotationValueDefaulted" + NEWLINE +
                    "@MyAnnotationValueEnum(SupportEnum=com.espertech.esper.support.bean.SupportEnum.ENUM_VALUE_3)" + NEWLINE +
                    "@MyAnnotationValuePair(StringVal='a',IntVal=-1,LongVal=2,BooleanVal=True,CharVal='x',ByteVal=10,ShortVal=20,DoubleVal=2.5)" + NEWLINE +
                    "@Name('STMTONE')" + NEWLINE +
                    "select *" + NEWLINE +
                    "from Bean";
            var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            var spi = (EPStatementSPI)stmt;
            Assert.AreEqual("select * from Bean", spi.ExpressionNoAnnotations);
            Assert.IsTrue(spi.IsNameProvided);

            var annotations = stmt.Annotations.ToArray();
            annotations = SortAlpha(annotations);
            Assert.AreEqual(6, annotations.Length);

            Assert.AreEqual(typeof(MyAnnotationSimpleAttribute), annotations[0].GetType());
            Assert.AreEqual("abc", ((MyAnnotationValueAttribute)annotations[1]).Value);
            Assert.AreEqual("XYZ", ((MyAnnotationValueDefaultedAttribute)annotations[2]).Value);
            Assert.AreEqual("STMTONE", ((NameAttribute)annotations[5]).Value);

            var enumval = (MyAnnotationValueEnumAttribute)annotations[3];
            Assert.AreEqual(SupportEnum.ENUM_VALUE_2, enumval.SupportEnumDef);
            Assert.AreEqual(SupportEnum.ENUM_VALUE_3, enumval.SupportEnum);

            var pair = (MyAnnotationValuePairAttribute)annotations[4];
            Assert.AreEqual("a", pair.StringVal);
            Assert.AreEqual(-1, pair.IntVal);
            Assert.AreEqual(2l, pair.LongVal);
            Assert.AreEqual(true, pair.BooleanVal);
            Assert.AreEqual('x', pair.CharVal);
            Assert.AreEqual(10, pair.ByteVal);
            Assert.AreEqual(20, pair.ShortVal);
            Assert.AreEqual(2.5, pair.DoubleVal);
            Assert.AreEqual("def", pair.StringValDef);
            Assert.AreEqual(100, pair.IntValDef);
            Assert.AreEqual(200l, pair.LongValDef);
            Assert.AreEqual(true, pair.BooleanValDef);
            Assert.AreEqual('D', pair.CharValDef);
            Assert.AreEqual(1.1, pair.DoubleValDef);

            // statement model
            var model = _epService.EPAdministrator.CompileEPL(stmtText);
            Assert.AreEqual(stmtText, model.ToEPL());
            var textFormatted = model.ToEPL(new EPStatementFormatter(true));
            Assert.AreEqual(stmtTextFormatted, textFormatted);
            var stmtTwo = _epService.EPAdministrator.Create(model);
            Assert.AreEqual(stmtTwo.Text, model.ToEPL());
            Assert.AreEqual(6, stmtTwo.Annotations.Count);

            // test array
            stmtText =
                    "@MyAnnotationValueArray(Value={1, 2, 3}, IntArray={4, 5}, DoubleArray={}, \nStringArray={\"X\"})\n" +
                            "/* Test */ select * \nfrom Bean";
            stmt = _epService.EPAdministrator.CreateEPL(stmtText);

            annotations = stmt.Annotations.ToArray();
            annotations = SortAlpha(annotations);
            Assert.AreEqual(1, annotations.Length);

            var array = (MyAnnotationValueArrayAttribute)annotations[0];
            Assert.IsTrue(CompatExtensions.DeepEquals(ToObjectArray(array.Value), new Object[] { 1L, 2L, 3L }));
            Assert.IsTrue(CompatExtensions.DeepEquals(ToObjectArray(array.IntArray), new Object[] { 4, 5 }));
            Assert.IsTrue(CompatExtensions.DeepEquals(ToObjectArray(array.DoubleArray), new Object[] { }));
            Assert.IsTrue(CompatExtensions.DeepEquals(ToObjectArray(array.StringArray), new Object[] { "X" }));
            Assert.IsTrue(CompatExtensions.DeepEquals(ToObjectArray(array.StringArrayDef), new Object[] { "XYZ" }));

            // statement model
            model = _epService.EPAdministrator.CompileEPL(stmtText);
            Assert.AreEqual("@MyAnnotationValueArray(Value={1,2,3},IntArray={4,5},DoubleArray={},StringArray={'X'}) select * from Bean", model.ToEPL());
            stmtTwo = _epService.EPAdministrator.Create(model);
            Assert.AreEqual(stmtTwo.Text, model.ToEPL());
            Assert.AreEqual(1, stmtTwo.Annotations.Count);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestSPI()
        {
            var configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddEventType("Bean", typeof(SupportBean).FullName);
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            _epService.EPAdministrator.Configuration.AddImport("com.espertech.esper.regression.client");

            var testdata = new String[][]{
                    new String[]
                    {
                            "@MyAnnotationSimple /* test */ select * from Bean",
                            "/* test */ select * from Bean"
                    },
                    new String[]
                    {
                            "/* test */ select * from Bean",
                            "/* test */ select * from Bean"
                    },
                    new String[]
                    {
                            "@MyAnnotationValueArray(Value={1, 2, 3}, IntArray={4, 5}, DoubleArray={}, StringArray={\"X\"})    select * from Bean",
                            "select * from Bean"
                    },
                    new String[]
                    {
                            "@MyAnnotationSimple\nselect * from Bean",
                            "select * from Bean"
                    },
                    new String[]
                    {
                            "@MyAnnotationSimple\n@MyAnnotationSimple\nselect * from Bean",
                            "select * from Bean"
                    },
                    new String[]
                    {
                            "@MyAnnotationValueArray(Value={1, 2, 3}, IntArray={4, 5}, DoubleArray={}, \nStringArray={\"X\"})\n" +
                                    "/* Test */ select * \nfrom Bean",
                            "/* Test */ select * \r\nfrom Bean"
                    },
            };

            for (var i = 0; i < testdata.Length; i++)
            {
                var innerStmt = _epService.EPAdministrator.CreateEPL(testdata[i][0]) as EPStatementSPI;
                Assert.That(innerStmt, Is.Not.Null);
                Assert.AreEqual(testdata[i][1], innerStmt.ExpressionNoAnnotations, "Error on " + testdata[i][0]);
                Assert.IsFalse(innerStmt.IsNameProvided);
            }

            var stmt = _epService.EPAdministrator.CreateEPL(testdata[0][0], "nameProvided");
            Assert.IsTrue(((EPStatementSPI)stmt).IsNameProvided);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestClientAppAnnotationNested()
        {
            var configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddEventType("Bean", typeof(SupportBean).FullName);
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            _epService.EPAdministrator.Configuration.AddImport("com.espertech.esper.regression.client");

            var stmtText =
                    "@MyAnnotationNested(\n" +
                            "            NestableSimple=@MyAnnotationNestableSimple,\n" +
                            "            NestableValues=@MyAnnotationNestableValues(Val=999, Arr={2, 1}),\n" +
                            "            NestableNestable=@MyAnnotationNestableNestable(\"CDF\")\n" +
                            "    ) " +
                            "select * from Bean";
            var stmt = _epService.EPAdministrator.CreateEPL(stmtText);

            var annotations = stmt.Annotations.ToArray();
            annotations = SortAlpha(annotations);
            Assert.AreEqual(1, annotations.Length);

            var nested = (MyAnnotationNestedAttribute)annotations[0];
            Assert.NotNull(nested.NestableSimple);
            Assert.IsTrue(CompatExtensions.DeepEquals(ToObjectArray((nested.NestableValues.Arr)), new Object[] { 2, 1 }));
            Assert.AreEqual(999, nested.NestableValues.Val);
            Assert.AreEqual("CDF", nested.NestableNestable.Value);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        private static Attribute[] SortAlpha(IEnumerable<Attribute> annotations)
        {
            if (annotations == null)
            {
                return null;
            }

            return annotations.OrderBy(o => o.GetType().Name).ToArray();
        }

        private static Object[] ToObjectArray<T>(T[] array)
        {
            var length = array.Length;
            var result = new Object[length];
            for (var ii = 0; ii < length; ii++)
            {
                result[ii] = array[ii];
            }
            return result;
        }
    }
}
