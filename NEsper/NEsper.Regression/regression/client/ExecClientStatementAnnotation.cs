///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.annotation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    using DescriptionAttribute = esper.client.annotation.DescriptionAttribute;

    public class ExecClientStatementAnnotation : RegressionExecution {
        private static readonly string NEWLINE = Environment.NewLine;
    
        public override void Configure(Configuration configuration)
        {
            configuration.AddEventType<SupportBean>("Bean");
            configuration.AddImport<MyAnnotationValueEnumAttribute>();
            configuration.AddNamespaceImport<MyAnnotationNestableValuesAttribute>();
            configuration.AddAnnotationImport<SupportEnum>();
            configuration.AddEventType<SupportBean>();
        }
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddImport("com.espertech.esper.regression.client");
            epService.EPAdministrator.Configuration.AddImport(typeof(SupportEnum));
    
            RunAssertionAnnotationSpecificImport(epService);
            RunAssertionInvalid(epService);
            RunAssertionBuiltin(epService);
            RunAssertionClientAppAnnotationSimple(epService);
            RunAssertionSPI(epService);
            RunAssertionClientAppAnnotationNested(epService);
        }
    
        [MyAnnotationValueEnum(SupportEnum = SupportEnum.ENUM_VALUE_1)]
        private void RunAssertionAnnotationSpecificImport(EPServiceProvider epService) {
            TryAssertionNoClassNameRequired(epService);
        }
    
        private void TryAssertionNoClassNameRequired(EPServiceProvider epService) {
            TryAssertionNoClassNameRequired(epService, SupportEnum.ENUM_VALUE_2, "ENUM_VALUE_2");
            TryAssertionNoClassNameRequired(epService, SupportEnum.ENUM_VALUE_3, "ENUM_value_3");
            TryAssertionNoClassNameRequired(epService, SupportEnum.ENUM_VALUE_1, "enum_value_1");
        }
    
        private void TryAssertionNoClassNameRequired(EPServiceProvider epService, SupportEnum expected, string text) {
            var stmt = epService.EPAdministrator.CreateEPL("@MyAnnotationValueEnum(SupportEnum = " + text + ") select * from SupportBean");
            var anno = (MyAnnotationValueEnumAttribute) stmt.Annotations.First();
            Assert.That(expected, Is.EqualTo(anno.SupportEnum));
            stmt.Dispose();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            TryInvalid(epService, "@MyAnnotationNested(NestableSimple=@MyAnnotationNestableSimple, NestableValues=@MyAnnotationNestableValues, NestableNestable=@MyAnnotationNestableNestable) select * from Bean", false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationNestableNestable' requires a value for attribute 'Value' [@MyAnnotationNested(NestableSimple=@MyAnnotationNestableSimple, NestableValues=@MyAnnotationNestableValues, NestableNestable=@MyAnnotationNestableNestable) select * from Bean]");
    
            TryInvalid(epService, "@MyAnnotationNested(NestableNestable=@MyAnnotationNestableNestable('A'), NestableSimple=1) select * from Bean", false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationNested' requires a MyAnnotationNestableSimpleAttribute-typed value for attribute 'NestableSimple' but received a System.Int32-typed value [@MyAnnotationNested(NestableNestable=@MyAnnotationNestableNestable('A'), NestableSimple=1) select * from Bean]");
    
            TryInvalid(epService, "@MyAnnotationValuePair(StringVal='abc') select * from Bean", false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationValuePair' requires a value for attribute 'BooleanVal' [@MyAnnotationValuePair(StringVal='abc') select * from Bean]");
    
            TryInvalid(epService, "MyAnnotationValueArray(Value=5) select * from Bean", true,
                    "Incorrect syntax near 'MyAnnotationValueArray' [MyAnnotationValueArray(Value=5) select * from Bean]");
    
            TryInvalid(epService, "@MyAnnotationValueArray(Value=null) select * from Bean", false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationValueArray' requires a value for attribute 'DoubleArray' [@MyAnnotationValueArray(Value=null) select * from Bean]");
    
            TryInvalid(epService, "@MyAnnotationValueArray(IntArray={},DoubleArray={},StringArray={null},Value={}) select * from Bean", false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationValueArray' requires a non-null value for array elements for attribute 'StringArray' [@MyAnnotationValueArray(IntArray={},DoubleArray={},StringArray={null},Value={}) select * from Bean]");
    
            TryInvalid(epService, "@MyAnnotationValueArray(IntArray={},DoubleArray={},StringArray={1},Value={}) select * from Bean", false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationValueArray' requires a System.String-typed value for array elements for attribute 'StringArray' but received a System.Int32-typed value [@MyAnnotationValueArray(IntArray={},DoubleArray={},StringArray={1},Value={}) select * from Bean]");
    
            TryInvalid(epService, "@MyAnnotationValue(Value='a', Value='a') select * from Bean", false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationValue' has duplicate attribute values for attribute 'Value' [@MyAnnotationValue(Value='a', Value='a') select * from Bean]");
            TryInvalid(epService, "@ABC select * from Bean", false,
                    "Failed to process statement annotations: Failed to resolve @-annotation class: Could not load annotation class by name 'ABCAttribute', please check imports [@ABC select * from Bean]");

            TryInvalid(epService, "@MyAnnotationSimple(5) select * from Bean", false,
                    "Failed to process statement annotations: Failed to find property Value in annotation type MyAnnotationSimple [@MyAnnotationSimple(5) select * from Bean]");
            TryInvalid(epService, "@MyAnnotationSimple(null) select * from Bean", false,
                    "Failed to process statement annotations: Failed to find property Value in annotation type MyAnnotationSimple [@MyAnnotationSimple(null) select * from Bean]");

            TryInvalid(epService, "@MyAnnotationValue select * from Bean", false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationValue' requires a value for attribute 'Value' [@MyAnnotationValue select * from Bean]");
    
            TryInvalid(epService, "@MyAnnotationValue(5) select * from Bean", false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationValue' requires a String-typed value for attribute 'Value' but received a System.Int32-typed value [@MyAnnotationValue(5) select * from Bean]");
            TryInvalid(epService, "@MyAnnotationValueArray(Value=\"ABC\", IntArray={}, DoubleArray={}, StringArray={}) select * from Bean", false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationValueArray' requires a System.Int64[]-typed value for attribute 'Value' but received a System.String-typed value [@MyAnnotationValueArray(Value=\"ABC\", IntArray={}, DoubleArray={}, StringArray={}) select * from Bean]");
            TryInvalid(epService, "@MyAnnotationValueEnum(a.b.CC) select * from Bean", false,
                    "Annotation enumeration value 'a.b.CC' not recognized as an enumeration class, please check imports or type used [@MyAnnotationValueEnum(a.b.CC) select * from Bean]");
    
            TryInvalid(epService, "@Hint('XXX') select * from Bean", false,
                    "Failed to process statement annotations: Hint annotation value 'XXX' is not one of the known values [@Hint('XXX') select * from Bean]");
            TryInvalid(epService, "@Hint('ITERATE_ONLY,XYZ') select * from Bean", false,
                    "Failed to process statement annotations: Hint annotation value 'XYZ' is not one of the known values [@Hint('ITERATE_ONLY,XYZ') select * from Bean]");
            TryInvalid(epService, "@Hint('testit=5') select * from Bean", false,
                    "Failed to process statement annotations: Hint annotation value 'testit' is not one of the known values [@Hint('testit=5') select * from Bean]");
            TryInvalid(epService, "@Hint('RECLAIM_GROUP_AGED') select * from Bean", false,
                    "Failed to process statement annotations: Hint 'RECLAIM_GROUP_AGED' requires a parameter value [@Hint('RECLAIM_GROUP_AGED') select * from Bean]");
            TryInvalid(epService, "@Hint('ITERATE_ONLY,RECLAIM_GROUP_AGED') select * from Bean", false,
                    "Failed to process statement annotations: Hint 'RECLAIM_GROUP_AGED' requires a parameter value [@Hint('ITERATE_ONLY,RECLAIM_GROUP_AGED') select * from Bean]");
            TryInvalid(epService, "@Hint('ITERATE_ONLY=5,RECLAIM_GROUP_AGED=5') select * from Bean", false,
                    "Failed to process statement annotations: Hint 'ITERATE_ONLY' does not accept a parameter value [@Hint('ITERATE_ONLY=5,RECLAIM_GROUP_AGED=5') select * from Bean]");
            TryInvalid(epService, "@Hint('Index(name)xxx') select * from Bean", false,
                    "Failed to process statement annotations: Hint 'INDEX' has additional text after parentheses [@Hint('Index(name)xxx') select * from Bean]");
            TryInvalid(epService, "@Hint('index') select * from Bean", false,
                    "Failed to process statement annotations: Hint 'INDEX' requires additional parameters in parentheses [@Hint('index') select * from Bean]");
        }
    
        private void TryInvalid(EPServiceProvider epService, string stmtText, bool isSyntax, string message) {
            try {
                epService.EPAdministrator.CreateEPL(stmtText);
                Assert.Fail();
            } catch (EPStatementSyntaxException ex) {
                Assert.IsTrue(isSyntax);
                Assert.AreEqual(message, ex.Message);
            } catch (EPStatementException ex) {
                Assert.IsFalse(isSyntax);

                Assert.AreEqual(message, ex.Message);
            }
        }
    
        private void RunAssertionBuiltin(EPServiceProvider epService) {
            var stmtText = "@Name('MyTestStmt') @Description('MyTestStmt description') @Tag(Name=\"UserId\", Value=\"value\") select * from Bean";
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            Assert.IsTrue(((EPStatementSPI) stmt).IsNameProvided);
            TryAssertion(stmt);
            stmt.Dispose();
            var name = (NameAttribute) AnnotationUtil.FindAnnotation(stmt.Annotations, typeof(NameAttribute));
            Assert.AreEqual("MyTestStmt", name.Value);
    
            // try lowercase
            var stmtTextLower = "@Name('MyTestStmt') @Description('MyTestStmt description') @Tag(Name=\"UserId\", Value=\"value\") select * from Bean";
            stmt = epService.EPAdministrator.CreateEPL(stmtTextLower);
            TryAssertion(stmt);
            stmt.Dispose();
    
            // try pattern
            stmtText = "@Name('MyTestStmt') @Description('MyTestStmt description') @Tag(Name='UserId', Value='value') every Bean";
            stmt = epService.EPAdministrator.CreatePattern(stmtText);
            TryAssertion(stmt);
            stmt.Dispose();
    
            stmtText = "@" + typeof(NameAttribute).FullName + "('MyTestStmt') @Description('MyTestStmt description') @Tag(Name=\"UserId\", Value=\"value\") every Bean";
            stmt = epService.EPAdministrator.CreatePattern(stmtText);
            TryAssertion(stmt);
    
            epService.EPAdministrator.CreateEPL("@Hint('ITERATE_ONLY') select * from Bean");
            epService.EPAdministrator.CreateEPL("@Hint('ITERATE_ONLY,DISABLE_RECLAIM_GROUP') select * from Bean");
            epService.EPAdministrator.CreateEPL("@Hint('ITERATE_ONLY,DISABLE_RECLAIM_GROUP,ITERATE_ONLY') select * from Bean");
            epService.EPAdministrator.CreateEPL("@Hint('  iterate_only ') select * from Bean");
    
            // test statement name override
            stmtText = "@Name('MyAnnotatedName') select * from Bean";
            stmt = epService.EPAdministrator.CreateEPL(stmtText, "MyABCStmt");
            Assert.AreEqual("MyABCStmt", stmt.Name);
    
            // hint tests
            Assert.IsNull(HintEnum.DISABLE_RECLAIM_GROUP.GetHint(null));
            Assert.IsNull(HintEnum.DISABLE_RECLAIM_GROUP.GetHint(new Attribute[0]));
    
            var annos = epService.EPAdministrator.CreateEPL("@Hint('DISABLE_RECLAIM_GROUP') select * from Bean").Annotations;
            Assert.AreEqual("DISABLE_RECLAIM_GROUP", HintEnum.DISABLE_RECLAIM_GROUP.GetHint(annos).Value);
    
            annos = epService.EPAdministrator.CreateEPL("@Hint('ITERATE_ONLY,ITERATE_ONLY,DISABLE_RECLAIM_GROUP,ITERATE_ONLY') select * from Bean").Annotations;
            Assert.AreEqual("ITERATE_ONLY,ITERATE_ONLY,DISABLE_RECLAIM_GROUP,ITERATE_ONLY", HintEnum.DISABLE_RECLAIM_GROUP.GetHint(annos).Value);
    
            annos = epService.EPAdministrator.CreateEPL("@Hint('ITERATE_ONLY,reclaim_group_aged=10') select * from Bean").Annotations;
            var hint = HintEnum.RECLAIM_GROUP_AGED.GetHint(annos);
            Assert.AreEqual("10", HintEnum.RECLAIM_GROUP_AGED.GetHintAssignedValue(hint));
    
            annos = epService.EPAdministrator.CreateEPL("@Hint('reclaim_group_aged=11') select * from Bean").Annotations;
            hint = HintEnum.RECLAIM_GROUP_AGED.GetHint(annos);
            Assert.AreEqual("11", HintEnum.RECLAIM_GROUP_AGED.GetHintAssignedValue(hint));
    
            annos = epService.EPAdministrator.CreateEPL("@Hint('Index(one, two)') select * from Bean").Annotations;
            Assert.AreEqual("one, two", HintEnum.INDEX.GetHintAssignedValues(annos)[0]);
    
            stmt.Dispose();
    
            // NoLock
            stmt = epService.EPAdministrator.CreateEPL("@NoLock select * from Bean");
            Assert.IsNotNull(AnnotationUtil.FindAnnotation(stmt.Annotations, typeof(NoLockAttribute)));
            Assert.AreEqual(1, AnnotationUtil.FindAnnotations(stmt.Annotations, typeof(NoLockAttribute)).Count);
    
            stmt.Dispose();
        }
    
        private void TryAssertion(EPStatement stmt)
        {
            var annotations = stmt.Annotations.ToArray();
            annotations = SortAlpha(annotations);
            Assert.AreEqual(3, annotations.Length);
    
            Assert.AreEqual(typeof(DescriptionAttribute), annotations[0].GetType());
            Assert.AreEqual("MyTestStmt description", ((DescriptionAttribute) annotations[0]).Value);
            Assert.AreEqual("@Description(\"MyTestStmt description\")", annotations[0].ToString());
    
            Assert.AreEqual(typeof(NameAttribute), annotations[1].GetType());
            Assert.AreEqual("MyTestStmt", ((NameAttribute) annotations[1]).Value);
            Assert.AreEqual("MyTestStmt", stmt.Name);
            Assert.AreEqual("@Name(\"MyTestStmt\")", annotations[1].ToString());
    
            Assert.AreEqual(typeof(TagAttribute), annotations[2].GetType());
            Assert.AreEqual("UserId", ((TagAttribute) annotations[2]).Name);
            Assert.AreEqual("value", ((TagAttribute) annotations[2]).Value);
            Assert.AreEqual("@Tag(Name=\"UserId\", Value=\"value\")", annotations[2].ToString());
    
            Assert.IsFalse(annotations[2].Equals(annotations[1]));
            Assert.IsTrue(annotations[1].Equals(annotations[1]));
            Assert.IsTrue(annotations[1].GetHashCode() != 0);
        }
    
        private void RunAssertionClientAppAnnotationSimple(EPServiceProvider epService) {
            var stmtText =
                    "@MyAnnotationSimple " +
                            "@MyAnnotationValue('abc') " +
                            "@MyAnnotationValueDefaulted " +
                            "@MyAnnotationValueEnum(SupportEnum=" + typeof(SupportEnum).FullName + ".ENUM_VALUE_3) " +
                            "@MyAnnotationValuePair(StringVal='a',IntVal=-1,LongVal=2,BooleanVal=True,CharVal='x',ByteVal=10,ShortVal=20,DoubleVal=2.5) " +
                            "@Name('STMTONE') " +
                            "select * from Bean";
            var stmtTextFormatted = "@MyAnnotationSimple" + NEWLINE +
                    "@MyAnnotationValue('abc')" + NEWLINE +
                    "@MyAnnotationValueDefaulted" + NEWLINE +
                    "@MyAnnotationValueEnum(SupportEnum=" + typeof(SupportEnum).FullName + ".ENUM_VALUE_3)" + NEWLINE +
                    "@MyAnnotationValuePair(StringVal='a',IntVal=-1,LongVal=2,BooleanVal=True,CharVal='x',ByteVal=10,ShortVal=20,DoubleVal=2.5)" + NEWLINE +
                    "@Name('STMTONE')" + NEWLINE +
                    "select *" + NEWLINE +
                    "from Bean";
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var spi = (EPStatementSPI) stmt;
            Assert.AreEqual("select * from Bean", spi.ExpressionNoAnnotations);
            Assert.IsTrue(spi.IsNameProvided);
    
            var annotations = stmt.Annotations.ToArray();
            annotations = SortAlpha(annotations);
            Assert.AreEqual(6, annotations.Length);
    
            Assert.AreEqual(typeof(MyAnnotationSimpleAttribute), annotations[0].GetType());
            Assert.AreEqual("abc", ((MyAnnotationValueAttribute) annotations[1]).Value);
            Assert.AreEqual("XYZ", ((MyAnnotationValueDefaultedAttribute) annotations[2]).Value);
            Assert.AreEqual("STMTONE", ((NameAttribute) annotations[5]).Value);
    
            var enumval = (MyAnnotationValueEnumAttribute) annotations[3];
            Assert.AreEqual(SupportEnum.ENUM_VALUE_2, enumval.SupportEnumDef);
            Assert.AreEqual(SupportEnum.ENUM_VALUE_3, enumval.SupportEnum);
    
            var pair = (MyAnnotationValuePairAttribute) annotations[4];
            Assert.AreEqual("a", pair.StringVal);
            Assert.AreEqual(-1, pair.IntVal);
            Assert.AreEqual(2L, pair.LongVal);
            Assert.AreEqual(true, pair.BooleanVal);
            Assert.AreEqual('x', pair.CharVal);
            Assert.AreEqual(10, pair.ByteVal);
            Assert.AreEqual(20, pair.ShortVal);
            Assert.AreEqual(2.5, pair.DoubleVal);
            Assert.AreEqual("def", pair.StringValDef);
            Assert.AreEqual(100, pair.IntValDef);
            Assert.AreEqual(200L, pair.LongValDef);
            Assert.AreEqual(true, pair.BooleanValDef);
            Assert.AreEqual('D', pair.CharValDef);
            Assert.AreEqual(1.1, pair.DoubleValDef);
    
            // statement model
            var model = epService.EPAdministrator.CompileEPL(stmtText);
            Assert.AreEqual(stmtText, model.ToEPL());
            var textFormatted = model.ToEPL(new EPStatementFormatter(true));
            Assert.AreEqual(stmtTextFormatted, textFormatted);
            var stmtTwo = epService.EPAdministrator.Create(model);
            Assert.AreEqual(stmtTwo.Text, model.ToEPL());
            Assert.AreEqual(6, stmtTwo.Annotations.Count);
    
            // test array
            stmtText =
                    "@MyAnnotationValueArray(Value={1, 2, 3}, IntArray={4, 5}, DoubleArray={}, \nStringArray={\"X\"})\n" +
                            "/* Test */ select * \nfrom Bean";
            stmt = epService.EPAdministrator.CreateEPL(stmtText);
    
            annotations = stmt.Annotations.ToArray();
            annotations = SortAlpha(annotations);
            Assert.AreEqual(1, annotations.Length);
    
            var array = (MyAnnotationValueArrayAttribute) annotations.First();
            Assert.IsTrue(CompatExtensions.DeepEquals(ToObjectArray(array.Value), new object[]{1L, 2L, 3L}));
            Assert.IsTrue(CompatExtensions.DeepEquals(ToObjectArray(array.IntArray), new object[]{4, 5}));
            Assert.IsTrue(CompatExtensions.DeepEquals(ToObjectArray(array.DoubleArray), new object[]{}));
            Assert.IsTrue(CompatExtensions.DeepEquals(ToObjectArray(array.StringArray), new object[]{"X"}));
            Assert.IsTrue(CompatExtensions.DeepEquals(ToObjectArray(array.StringArrayDef), new object[]{"XYZ"}));
    
            // statement model
            model = epService.EPAdministrator.CompileEPL(stmtText);
            Assert.AreEqual("@MyAnnotationValueArray(Value={1,2,3},IntArray={4,5},DoubleArray={},StringArray={'X'}) select * from Bean", model.ToEPL());
            stmtTwo = epService.EPAdministrator.Create(model);
            Assert.AreEqual(stmtTwo.Text, model.ToEPL());
            Assert.AreEqual(1, stmtTwo.Annotations.Count);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSPI(EPServiceProvider epService) {
            var testdata = new string[][]{
                new string[]{"@MyAnnotationSimple /* test */ select * from Bean",
                            "/* test */ select * from Bean"},
                new string[]{"/* test */ select * from Bean",
                            "/* test */ select * from Bean"},
                new string[]{"@MyAnnotationValueArray(Value={1, 2, 3}, IntArray={4, 5}, DoubleArray={}, StringArray={\"X\"})    select * from Bean",
                            "select * from Bean"},
                new string[]{"@MyAnnotationSimple\nselect * from Bean",
                            "select * from Bean"},
                new string[]{"@MyAnnotationSimple\n@MyAnnotationSimple\nselect * from Bean",
                            "select * from Bean"},
                new string[]{"@MyAnnotationValueArray(Value={1, 2, 3}, IntArray={4, 5}, DoubleArray={}, \nStringArray={\"X\"})\n" +
                            "/* Test */ select * \nfrom Bean",
                            "/* Test */ select * \r\nfrom Bean"},
            };
    
            foreach (var aTestdata in testdata) {
                var innerStmt = epService.EPAdministrator.CreateEPL(aTestdata[0]);
                var spi = (EPStatementSPI) innerStmt;
                Assert.AreEqual(RemoveNewlines(aTestdata[1]), RemoveNewlines(spi.ExpressionNoAnnotations), "Error on " + aTestdata[0]);
                Assert.IsFalse(((EPStatementSPI) innerStmt).IsNameProvided);
            }
    
            var stmt = epService.EPAdministrator.CreateEPL(testdata[0][0], "nameProvided");
            Assert.IsTrue(((EPStatementSPI) stmt).IsNameProvided);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionClientAppAnnotationNested(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddImport("com.espertech.esper.regression.client");
    
            var stmtText =
                    "@MyAnnotationNested(\n" +
                            "            NestableSimple=@MyAnnotationNestableSimple,\n" +
                            "            NestableValues=@MyAnnotationNestableValues(Val=999, Arr={2, 1}),\n" +
                            "            NestableNestable=@MyAnnotationNestableNestable(\"CDF\")\n" +
                            "    ) " +
                            "select * from Bean";
            var stmt = epService.EPAdministrator.CreateEPL(stmtText);
    
            var annotations = stmt.Annotations;
            annotations = SortAlpha(annotations);
            Assert.AreEqual(1, annotations.Count);
    
            var nested = (MyAnnotationNestedAttribute) annotations.First();
            Assert.IsNotNull(nested.NestableSimple);
            Assert.IsTrue(CompatExtensions.DeepEquals(ToObjectArray(nested.NestableValues.Arr), new object[]{2, 1}));
            Assert.AreEqual(999, nested.NestableValues.Val);
            Assert.AreEqual("CDF", nested.NestableNestable.Value);
    
            stmt.Dispose();
        }

        private static Attribute[] SortAlpha(IEnumerable<Attribute> annotations)
        {
            if (annotations == null)
            {
                return null;
            }

            return annotations.OrderBy(o => o.GetType().Name).ToArray();
        }
    
        private object[] ToObjectArray<T>(T[] value) {
            if (value is Array array)
            {
                var result = new Object[array.Length];
                for (var ii = 0; ii < array.Length; ii++)
                {
                    result[ii] = array.GetValue(ii);
                }
                return result;
            }
            else
            {
                throw new EPRuntimeException("Parameter passed is not an array");
            }
        }
    
        private string RemoveNewlines(string text) {
            return text.Replace("\n", "").Replace("\r", "");
        }
    }
} // end of namespace
