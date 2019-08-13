///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.epl.annotation;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.client;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

using static com.espertech.esper.common.client.scopetest.EPAssertionUtil;
using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;
using static com.espertech.esper.regressionlib.support.client.AnnotationAssertUtil;

using DescriptionAttribute = com.espertech.esper.common.client.annotation.DescriptionAttribute;

namespace com.espertech.esper.regressionlib.suite.client.runtime
{
    public class ClientRuntimeStatementAnnotation
    {
        private static readonly string NEWLINE = Environment.NewLine;

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new ClientRuntimeStatementAnnotationBuiltin());
            execs.Add(new ClientRuntimeStatementAnnotationAppSimple());
            execs.Add(new ClientRuntimeStatementAnnotationAppNested());
            execs.Add(new ClientRuntimeStatementAnnotationInvalid());
            execs.Add(new ClientRuntimeStatementAnnotationSpecificImport());
            return execs;
        }

        private static void TryInvalidAnnotation(
            RegressionEnvironment env,
            string stmtText,
            bool isSyntax,
            string message)
        {
            try {
                EPCompilerProvider.Compiler.Compile(stmtText, new CompilerArguments(env.Configuration));
                Assert.Fail();
            }
            catch (EPCompileException ex) {
                var first = ex.Items[0];
                Assert.AreEqual(isSyntax, first is EPCompileExceptionSyntaxItem);
                Assert.AreEqual(message, ex.Message);
            }
        }

        private static void AssertStatement(RegressionEnvironment env)
        {
            Assert.AreEqual(2, env.Statement("s0").Annotations.Length);

            var array = (MyAnnotationValueArrayAttribute) env
                .Statement("s0")
                .Annotations[0];
            Assert.IsTrue(ToObjectArray(array.Value).DeepEquals(new object[] {1L, 2L, 3L}));
            Assert.IsTrue(ToObjectArray(array.IntArray).DeepEquals(new object[] {4, 5}));
            Assert.IsTrue(ToObjectArray(array.DoubleArray).DeepEquals(new object[] { }));
            Assert.IsTrue(ToObjectArray(array.StringArray).DeepEquals(new object[] {"X"}));
            Assert.IsTrue(ToObjectArray(array.StringArrayDef).DeepEquals(new object[] {"XYZ"}));
        }

        private static void TryAssertion(EPStatement stmt)
        {
            var annotations = stmt.Annotations;
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

        public class ClientRuntimeStatementAnnotationAppSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@MyAnnotationSimple " +
                    "@MyAnnotationValue('abc') " +
                    "@MyAnnotationValueDefaulted " +
                    "@MyAnnotationValueEnum(SupportEnum=" +
                    typeof(SupportEnum).FullName +
                    ".ENUM_VALUE_3) " +
                    "@MyAnnotationValuePair(stringVal='a',intVal=-1,longVal=2,booleanVal=true,charVal='x',byteVal=10,shortVal=20,doubleVal=2.5) " +
                    "@Name('STMTONE') " +
                    "select * from SupportBean";
                var stmtTextFormatted =
                    "@MyAnnotationSimple" +
                    NEWLINE +
                    "@MyAnnotationValue('abc')" +
                    NEWLINE +
                    "@MyAnnotationValueDefaulted" +
                    NEWLINE +
                    "@MyAnnotationValueEnum(SupportEnum=" +
                    typeof(SupportEnum).FullName +
                    ".ENUM_VALUE_3)" +
                    NEWLINE +
                    "@MyAnnotationValuePair(stringVal='a',intVal=-1,longVal=2,booleanVal=true,charVal='x',byteVal=10,shortVal=20,doubleVal=2.5)" +
                    NEWLINE +
                    "@Name('STMTONE')" +
                    NEWLINE +
                    "select *" +
                    NEWLINE +
                    "from SupportBean";
                env.CompileDeploy(stmtText);

                var annotations = env.Statement("STMTONE").Annotations;
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

                env.UndeployAll();

                // statement model
                var model = env.EplToModel(stmtText);
                Assert.AreEqual(stmtText, model.ToEPL());
                var textFormatted = model.ToEPL(new EPStatementFormatter(true));
                Assert.AreEqual(stmtTextFormatted, textFormatted);
                env.CompileDeploy(model).AddListener("STMTONE");
                Assert.AreEqual(6, env.Statement("STMTONE").Annotations.Length);
                env.UndeployAll();

                // test array
                stmtText =
                    "@MyAnnotationValueArray(value={1,2,3},IntArray={4,5},doubleArray={},stringArray={'X'}) @Name('s0') select * from SupportBean";
                env.CompileDeploy(stmtText);

                Assert.That(() => env.Statement("s0").Annotations, Throws.Nothing);

                AssertStatement(env);
                env.UndeployAll();

                // statement model
                env.EplToModelCompileDeploy(stmtText);
                AssertStatement(env);
                env.UndeployAll();
            }
        }

        public class ClientRuntimeStatementAnnotationInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryInvalidAnnotation(
                    env,
                    "@MyAnnotationNested(nestableSimple=@MyAnnotationNestableSimple, nestableValues=@MyAnnotationNestableValues, nestableNestable=@MyAnnotationNestableNestable) select * from Bean",
                    false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationNestableNestable' requires a value for attribute 'value' [@MyAnnotationNested(nestableSimple=@MyAnnotationNestableSimple, nestableValues=@MyAnnotationNestableValues, nestableNestable=@MyAnnotationNestableNestable) select * from Bean]");

                TryInvalidAnnotation(
                    env,
                    "@MyAnnotationNested(nestableNestable=@MyAnnotationNestableNestable('A'), nestableSimple=1) select * from Bean",
                    false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationNested' requires a MyAnnotationNestableSimple-typed value for attribute 'nestableSimple' but received a Integer-typed value [@MyAnnotationNested(nestableNestable=@MyAnnotationNestableNestable('A'), nestableSimple=1) select * from Bean]");

                TryInvalidAnnotation(
                    env,
                    "@MyAnnotationValuePair(stringVal='abc') select * from Bean",
                    false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationValuePair' requires a value for attribute 'booleanVal' [@MyAnnotationValuePair(stringVal='abc') select * from Bean]");

                TryInvalidAnnotation(
                    env,
                    "MyAnnotationValueArray(value=5) select * from Bean",
                    true,
                    "Incorrect syntax near 'MyAnnotationValueArray' [MyAnnotationValueArray(value=5) select * from Bean]");

                TryInvalidAnnotation(
                    env,
                    "@MyAnnotationValueArray(value=null) select * from Bean",
                    false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationValueArray' requires a value for attribute 'doubleArray' [@MyAnnotationValueArray(value=null) select * from Bean]");

                TryInvalidAnnotation(
                    env,
                    "@MyAnnotationValueArray(IntArray={},doubleArray={},stringArray={null},value={}) select * from Bean",
                    false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationValueArray' requires a non-null value for array elements for attribute 'stringArray' [@MyAnnotationValueArray(IntArray={},doubleArray={},stringArray={null},value={}) select * from Bean]");

                TryInvalidAnnotation(
                    env,
                    "@MyAnnotationValueArray(IntArray={},doubleArray={},stringArray={1},value={}) select * from Bean",
                    false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationValueArray' requires a String-typed value for array elements for attribute 'stringArray' but received a Integer-typed value [@MyAnnotationValueArray(IntArray={},doubleArray={},stringArray={1},value={}) select * from Bean]");

                TryInvalidAnnotation(
                    env,
                    "@MyAnnotationValue(value='a', value='a') select * from Bean",
                    false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationValue' has duplicate attribute values for attribute 'value' [@MyAnnotationValue(value='a', value='a') select * from Bean]");
                TryInvalidAnnotation(
                    env,
                    "@ABC select * from Bean",
                    false,
                    "Failed to process statement annotations: Failed to resolve @-annotation class: Could not load annotation class by name 'ABC', please check imports [@ABC select * from Bean]");

                TryInvalidAnnotation(
                    env,
                    "@MyAnnotationSimple(5) select * from Bean",
                    false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationSimple' does not have an attribute 'value' [@MyAnnotationSimple(5) select * from Bean]");
                TryInvalidAnnotation(
                    env,
                    "@MyAnnotationSimple(null) select * from Bean",
                    false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationSimple' does not have an attribute 'value' [@MyAnnotationSimple(null) select * from Bean]");

                TryInvalidAnnotation(
                    env,
                    "@MyAnnotationValue select * from Bean",
                    false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationValue' requires a value for attribute 'value' [@MyAnnotationValue select * from Bean]");

                TryInvalidAnnotation(
                    env,
                    "@MyAnnotationValue(5) select * from Bean",
                    false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationValue' requires a String-typed value for attribute 'value' but received a Integer-typed value [@MyAnnotationValue(5) select * from Bean]");
                TryInvalidAnnotation(
                    env,
                    "@MyAnnotationValueArray(value=\"ABC\", IntArray={}, doubleArray={}, stringArray={}) select * from Bean",
                    false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationValueArray' requires a long[]-typed value for attribute 'value' but received a String-typed value [@MyAnnotationValueArray(value=\"ABC\", IntArray={}, doubleArray={}, stringArray={}) select * from Bean]");
                TryInvalidAnnotation(
                    env,
                    "@MyAnnotationValueEnum(a.b.CC) select * from Bean",
                    false,
                    "Annotation enumeration value 'a.b.CC' not recognized as an enumeration class, please check imports or type used [@MyAnnotationValueEnum(a.b.CC) select * from Bean]");

                TryInvalidAnnotation(
                    env,
                    "@Hint('XXX') select * from Bean",
                    false,
                    "Failed to process statement annotations: Hint annotation value 'XXX' is not one of the known values [@Hint('XXX') select * from Bean]");
                TryInvalidAnnotation(
                    env,
                    "@Hint('ITERATE_ONLY,XYZ') select * from Bean",
                    false,
                    "Failed to process statement annotations: Hint annotation value 'XYZ' is not one of the known values [@Hint('ITERATE_ONLY,XYZ') select * from Bean]");
                TryInvalidAnnotation(
                    env,
                    "@Hint('testit=5') select * from Bean",
                    false,
                    "Failed to process statement annotations: Hint annotation value 'testit' is not one of the known values [@Hint('testit=5') select * from Bean]");
                TryInvalidAnnotation(
                    env,
                    "@Hint('RECLAIM_GROUP_AGED') select * from Bean",
                    false,
                    "Failed to process statement annotations: Hint 'RECLAIM_GROUP_AGED' requires a parameter value [@Hint('RECLAIM_GROUP_AGED') select * from Bean]");
                TryInvalidAnnotation(
                    env,
                    "@Hint('ITERATE_ONLY,RECLAIM_GROUP_AGED') select * from Bean",
                    false,
                    "Failed to process statement annotations: Hint 'RECLAIM_GROUP_AGED' requires a parameter value [@Hint('ITERATE_ONLY,RECLAIM_GROUP_AGED') select * from Bean]");
                TryInvalidAnnotation(
                    env,
                    "@Hint('ITERATE_ONLY=5,RECLAIM_GROUP_AGED=5') select * from Bean",
                    false,
                    "Failed to process statement annotations: Hint 'ITERATE_ONLY' does not accept a parameter value [@Hint('ITERATE_ONLY=5,RECLAIM_GROUP_AGED=5') select * from Bean]");
                TryInvalidAnnotation(
                    env,
                    "@Hint('index(name)xxx') select * from Bean",
                    false,
                    "Failed to process statement annotations: Hint 'INDEX' has additional text after parentheses [@Hint('index(name)xxx') select * from Bean]");
                TryInvalidAnnotation(
                    env,
                    "@Hint('index') select * from Bean",
                    false,
                    "Failed to process statement annotations: Hint 'INDEX' requires additional parameters in parentheses [@Hint('index') select * from Bean]");
            }
        }

        public class ClientRuntimeStatementAnnotationAppNested : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunNestedSimple(env);
                RunNestedArray(env);
            }

            private void RunNestedSimple(RegressionEnvironment env)
            {
                var stmtText =
                    "@MyAnnotationNested(\n" +
                    "            nestableSimple=@MyAnnotationNestableSimple,\n" +
                    "            nestableValues=@MyAnnotationNestableValues(val=999, arr={2, 1}),\n" +
                    "            nestableNestable=@MyAnnotationNestableNestable(\"CDF\")\n" +
                    "    ) " +
                    "@Name('s0') select * from SupportBean";
                env.CompileDeploy(stmtText);

                var annotations = env.Statement("s0").Annotations;
                annotations = SortAlpha(annotations);
                Assert.AreEqual(2, annotations.Length);

                var nested = (MyAnnotationNestedAttribute) annotations[0];
                Assert.IsNotNull(nested.NestableSimple);
                Assert.IsTrue(
                    ToObjectArray(nested.NestableValues.Arr).DeepEquals(new object[] {2, 1}));
                Assert.AreEqual(999, nested.NestableValues.Val);
                Assert.AreEqual("CDF", nested.NestableNestable.Value);

                env.UndeployAll();
            }

            private void RunNestedArray(RegressionEnvironment env)
            {
                var stmtText =
                    "@MyAnnotationWArrayAndClass(priorities = {@Priority(1), @Priority(3)}, classOne = System.String.class, classTwo = Integer.class) @Name('s0') select * from SupportBean";
                env.CompileDeploy(stmtText);

                var annotations = env.Statement("s0").Annotations;
                annotations = SortAlpha(annotations);
                Assert.AreEqual(2, annotations.Length);

                var nested = (MyAnnotationWArrayAndClassAttribute) annotations[0];
                Assert.AreEqual(1, nested.Priorities[0].Value);
                Assert.AreEqual(3, nested.Priorities[1].Value);
                Assert.AreEqual(typeof(string), nested.ClassOne);
                Assert.AreEqual(typeof(int?), nested.ClassTwo);

                env.UndeployAll();
            }
        }

        public class ClientRuntimeStatementAnnotationBuiltin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                epl =
                    "@Name('MyTestStmt') @Description('MyTestStmt description') @Tag(Name=\"UserId\", Value=\"value\") select * from SupportBean";
                env.CompileDeploy(epl).AddListener("MyTestStmt");
                TryAssertion(env.Statement("MyTestStmt"));
                var name = (NameAttribute) AnnotationUtil.FindAnnotation(
                    env.Statement("MyTestStmt").Annotations,
                    typeof(NameAttribute));
                Assert.AreEqual("MyTestStmt", name.Value);
                env.UndeployAll();

                // try lowercase
                epl =
                    "@Name('MyTestStmt') @description('MyTestStmt description') @tag(Name=\"UserId\", Value=\"value\") select * from SupportBean";
                env.CompileDeploy(epl).AddListener("MyTestStmt");
                TryAssertion(env.Statement("MyTestStmt"));
                env.UndeployAll();

                // try fully-qualified
                epl = "@" +
                      typeof(NameAttribute).Name +
                      "('MyTestStmt') @Description('MyTestStmt description') @Tag(Name=\"UserId\", Value=\"value\") select * from SupportBean";
                env.CompileDeploy(epl).AddListener("MyTestStmt");
                TryAssertion(env.Statement("MyTestStmt"));
                env.UndeployAll();

                // hint tests
                Assert.IsNull(HintEnum.DISABLE_RECLAIM_GROUP.GetHint(null));
                Assert.IsNull(HintEnum.DISABLE_RECLAIM_GROUP.GetHint(new Attribute[0]));
                env.CompileDeploy("@Hint('ITERATE_ONLY') select * from SupportBean");
                env.CompileDeploy("@Hint('ITERATE_ONLY,DISABLE_RECLAIM_GROUP') select * from SupportBean");
                env.CompileDeploy("@Hint('ITERATE_ONLY,DISABLE_RECLAIM_GROUP,ITERATE_ONLY') select * from SupportBean");
                env.CompileDeploy("@Hint('  iterate_only ') select * from SupportBean");

                var annos = env.CompileDeploy("@Hint('DISABLE_RECLAIM_GROUP') @Name('s0') select * from SupportBean")
                    .Statement("s0")
                    .Annotations;
                Assert.AreEqual("DISABLE_RECLAIM_GROUP", HintEnum.DISABLE_RECLAIM_GROUP.GetHint(annos).Value);

                annos = env.CompileDeploy(
                        "@Hint('ITERATE_ONLY,ITERATE_ONLY,DISABLE_RECLAIM_GROUP,ITERATE_ONLY') @Name('s1') select * from SupportBean")
                    .Statement("s1")
                    .Annotations;
                Assert.AreEqual(
                    "ITERATE_ONLY,ITERATE_ONLY,DISABLE_RECLAIM_GROUP,ITERATE_ONLY",
                    HintEnum.DISABLE_RECLAIM_GROUP.GetHint(annos).Value);

                annos = env.CompileDeploy(
                        "@Hint('ITERATE_ONLY,reclaim_group_aged=10') @Name('s2') select * from SupportBean")
                    .Statement("s2")
                    .Annotations;
                var hint = HintEnum.RECLAIM_GROUP_AGED.GetHint(annos);
                Assert.AreEqual("10", HintEnum.RECLAIM_GROUP_AGED.GetHintAssignedValue(hint));

                annos = env.CompileDeploy("@Hint('reclaim_group_aged=11') @Name('s3') select * from SupportBean")
                    .Statement("s3")
                    .Annotations;
                hint = HintEnum.RECLAIM_GROUP_AGED.GetHint(annos);
                Assert.AreEqual("11", HintEnum.RECLAIM_GROUP_AGED.GetHintAssignedValue(hint));

                annos = env.CompileDeploy("@Hint('index(one, two)') @Name('s4') select * from SupportBean")
                    .Statement("s4")
                    .Annotations;
                Assert.AreEqual("one, two", HintEnum.INDEX.GetHintAssignedValues(annos)[0]);

                env.UndeployAll();

                // NoLock
                env.CompileDeploy("@Name('s0') @NoLock select * from SupportBean");
                Assert.AreEqual(
                    1,
                    AnnotationUtil.FindAnnotations(env.Statement("s0").Annotations, typeof(NoLockAttribute)).Count);

                env.UndeployAll();
            }
        }

        public class ClientRuntimeStatementAnnotationSpecificImport : RegressionExecution
        {
            [MyAnnotationValueEnum(SupportEnum = SupportEnum.ENUM_VALUE_1)]
            public void Run(RegressionEnvironment env)
            {
                TryAssertionNoClassNameRequired(env, SupportEnum.ENUM_VALUE_2, "ENUM_VALUE_2");
                TryAssertionNoClassNameRequired(env, SupportEnum.ENUM_VALUE_3, "ENUM_value_3");
                TryAssertionNoClassNameRequired(env, SupportEnum.ENUM_VALUE_1, "enum_value_1");
            }

            private void TryAssertionNoClassNameRequired(
                RegressionEnvironment env,
                SupportEnum expected,
                string text)
            {
                env.CompileDeploy(
                    "@MyAnnotationValueEnum(SupportEnum = " + text + ") @Name('s0') select * from SupportBean");
                var anno = (MyAnnotationValueEnumAttribute) env.Statement("s0").Annotations[0];
                Assert.AreEqual(expected, anno.SupportEnum);
                env.UndeployAll();
            }
        }

        public class ClientRuntimeAnnotationImportInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // init-time import
                env.CompileDeploy(
                    "@MyAnnotationValueEnum(SupportEnum = SupportEnum.ENUM_VALUE_1) " +
                    "select * from SupportBean");

                // try invalid annotation not yet imported
                var epl = "@MyAnnotationValueEnumTwo(SupportEnum = SupportEnum.ENUM_VALUE_1) select * from SupportBean";
                TryInvalidCompile(env, epl, "Failed to process statement annotations: Failed to resolve @-annotation");

                // try invalid use : these are annotation-specific imports of an annotation and an enum
                TryInvalidCompile(
                    env,
                    "select * from MyAnnotationValueEnumTwo",
                    "Failed to resolve event type, named window or table by name 'MyAnnotationValueEnumTwo'");
                TryInvalidCompile(
                    env,
                    "select SupportEnum.ENUM_VALUE_1 from SupportBean",
                    "Failed to validate select-clause expression 'SupportEnum.ENUM_VALUE_1'");

                env.UndeployAll();
            }
        }
    }
} // end of namespace