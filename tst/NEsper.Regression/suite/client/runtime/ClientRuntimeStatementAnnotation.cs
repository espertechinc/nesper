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

using static com.espertech.esper.common.client.scopetest.EPAssertionUtil;
using static com.espertech.esper.regressionlib.support.client.AnnotationAssertUtil;

using NUnit.Framework;

using DescriptionAttribute = com.espertech.esper.common.client.annotation.DescriptionAttribute;

namespace com.espertech.esper.regressionlib.suite.client.runtime
{
    public class ClientRuntimeStatementAnnotation
    {
        private static readonly string NEWLINE = Environment.NewLine;

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithBuiltin(execs);
            WithAppSimple(execs);
            WithAppNested(execs);
            WithInvalid(execs);
            WithSpecificImport(execs);
            WithRecursive(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithRecursive(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeStatementAnnotationRecursive());
            return execs;
        }

        public static IList<RegressionExecution> WithSpecificImport(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeStatementAnnotationSpecificImport());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeStatementAnnotationInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithAppNested(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeStatementAnnotationAppNested());
            return execs;
        }

        public static IList<RegressionExecution> WithAppSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeStatementAnnotationAppSimple());
            return execs;
        }

        public static IList<RegressionExecution> WithBuiltin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeStatementAnnotationBuiltin());
            return execs;
        }

        public class ClientRuntimeStatementAnnotationRecursive : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@MyAnnotationAPIEventType create schema ABC();\n" +
                    "@name('s0') select * from ABC;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventMap(EmptyDictionary<string, object>.Instance, "ABC");
                env.AssertEventNew("s0", @event => { });

                env.UndeployAll();
            }
        }

        public class ClientRuntimeStatementAnnotationAppSimple : RegressionExecution
        {
            // [MyAnnotationSimple]
            // [MyAnnotationValue (Value = "abc")]
            // [MyAnnotationValuePair (
            // 	StringVal = "a",
            // 	IntVal = -1,
            // 	LongVal = 2,
            // 	BooleanVal = true,
            // 	CharVal = 'x',
            // 	ByteVal = 10,
            // 	ShortVal = 20,
            // 	DoubleVal = 2.5
            // )]
            // [MyAnnotationValueDefaulted]
            // [MyAnnotationValueArray (
            // 	Value = new long[] { 1, 2, 3 },
            // 	IntArray = new int[] { 4, 5 },
            // 	DoubleArray = new double[] { },
            // 	StringArray = new string[] { "X" }
            // )]
            // [MyAnnotationValueEnum (SupportEnum = SupportEnum.ENUM_VALUE_3)]
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@MyAnnotationSimple " +
                    "@MyAnnotationValue('abc') " +
                    "@MyAnnotationValueDefaulted " +
                    "@MyAnnotationValueEnum(SupportEnum=" +
                    typeof(SupportEnum).FullName +
                    ".ENUM_VALUE_3) " +
                    "@MyAnnotationValuePair(StringVal='a',IntVal=-1,LongVal=2,BooleanVal=True,CharVal='x',ByteVal=10,ShortVal=20,DoubleVal=2.5) " +
                    "@name('STMTONE') " +
                    "select * from SupportBean";
                var stmtTextFormatted = 
                    "@MyAnnotationSimple" + NEWLINE +
                    "@MyAnnotationValue('abc')" + NEWLINE + 
                    "@MyAnnotationValueDefaulted" + NEWLINE +
                    "@MyAnnotationValueEnum(SupportEnum=" + typeof(SupportEnum).FullName + ".ENUM_VALUE_3)" + NEWLINE +
                    "@MyAnnotationValuePair(StringVal='a',IntVal=-1,LongVal=2,BooleanVal=True,CharVal='x',ByteVal=10,ShortVal=20,DoubleVal=2.5)" + NEWLINE +
                    "@name('STMTONE')" + NEWLINE +
                    "select *" + NEWLINE + "from SupportBean";
                env.CompileDeploy(stmtText);

                env.AssertStatement(
                    "STMTONE",
                    statement => {
                        var annotations = statement.Annotations;
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
                    });

                env.UndeployAll();

                // statement model
                var model = env.EplToModel(stmtText);
                Assert.AreEqual(stmtText, model.ToEPL());
                var textFormatted = model.ToEPL(new EPStatementFormatter(true));
                Assert.AreEqual(stmtTextFormatted, textFormatted);
                env.CompileDeploy(model).AddListener("STMTONE");
                env.AssertStatement("STMTONE", statement => Assert.AreEqual(6, statement.Annotations.Length));
                env.UndeployAll();

                // test array
                stmtText = "@MyAnnotationValueArray(Value={1,2,3},IntArray={4,5},DoubleArray={},StringArray={'X'}) @Name('s0') select * from SupportBean";
                env.CompileDeploy(stmtText);

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
                    "@MyAnnotationNested(NestableSimple=@MyAnnotationNestableSimple, NestableValues=@MyAnnotationNestableValues, NestableNestable=@MyAnnotationNestableNestable) select * from Bean",
                    false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationNestableNestableAttribute' requires a value for attribute 'Value' [@MyAnnotationNested(NestableSimple=@MyAnnotationNestableSimple, NestableValues=@MyAnnotationNestableValues, NestableNestable=@MyAnnotationNestableNestable) select * from Bean]");

                TryInvalidAnnotation(
                    env,
                    "@MyAnnotationNested(NestableNestable=@MyAnnotationNestableNestable('A'), NestableSimple=1) select * from Bean",
                    false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationNestedAttribute' requires a MyAnnotationNestableSimpleAttribute-typed value for attribute 'NestableSimple' but received a Int32-typed value [@MyAnnotationNested(NestableNestable=@MyAnnotationNestableNestable('A'), NestableSimple=1) select * from Bean]");

                TryInvalidAnnotation(
                    env,
                    "@MyAnnotationValuePair(StringVal='abc') select * from Bean",
                    false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationValuePairAttribute' requires a value for attribute 'BooleanVal' [@MyAnnotationValuePair(StringVal='abc') select * from Bean]");

                TryInvalidAnnotation(
                    env,
                    "MyAnnotationValueArray(Value=5) select * from Bean",
                    true,
                    "Incorrect syntax near 'MyAnnotationValueArray' [MyAnnotationValueArray(Value=5) select * from Bean]");

                TryInvalidAnnotation(
                    env,
                    "@MyAnnotationValueArray(Value=null) select * from Bean",
                    false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationValueArrayAttribute' requires a value for attribute 'DoubleArray' [@MyAnnotationValueArray(Value=null) select * from Bean]");

                TryInvalidAnnotation(
                    env,
                    "@MyAnnotationValueArray(IntArray={},DoubleArray={},StringArray={null},Value={}) select * from Bean",
                    false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationValueArrayAttribute' requires a non-null value for array elements for attribute 'StringArray' [@MyAnnotationValueArray(IntArray={},DoubleArray={},StringArray={null},Value={}) select * from Bean]");

                TryInvalidAnnotation(
                    env,
                    "@MyAnnotationValueArray(IntArray={},DoubleArray={},StringArray={1},Value={}) select * from Bean",
                    false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationValueArray' requires a String-typed value for array elements for attribute 'stringArray' but received a Integer-typed value [@MyAnnotationValueArray(intArray={},doubleArray={},stringArray={1},value={}) select * from Bean]");

                TryInvalidAnnotation(
                    env,
                    "@MyAnnotationValue(Value='a', Value='a') select * from Bean",
                    false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationValueAttribute' has duplicate attribute values for attribute 'Value' [@MyAnnotationValue(Value='a', Value='a') select * from Bean]");
                TryInvalidAnnotation(
                    env,
                    "@ABC select * from Bean",
                    false,
                    "Failed to process statement annotations: Failed to resolve @-annotation class: Could not load annotation class by name 'ABC', please check imports [@ABC select * from Bean]");

                TryInvalidAnnotation(
                    env,
                    "@MyAnnotationSimple(5) select * from Bean",
                    false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationSimpleAttribute' does not have an attribute 'Value' [@MyAnnotationSimple(5) select * from Bean]");
                TryInvalidAnnotation(
                    env,
                    "@MyAnnotationSimple(null) select * from Bean",
                    false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationSimpleAttribute' does not have an attribute 'Value' [@MyAnnotationSimple(null) select * from Bean]");

                TryInvalidAnnotation(
                    env,
                    "@MyAnnotationValue select * from Bean",
                    false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationValueAttribute' requires a value for attribute 'Value' [@MyAnnotationValue select * from Bean]");

#if WORKS_IN_DOTNET // In dotnet, we find a caster for int to string, in java they do not
                TryInvalidAnnotation(
                    env,
                    "@MyAnnotationValue(5) select * from Bean",
                    false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationValue' requires a String-typed value for attribute 'value' but received a Integer-typed value [@MyAnnotationValue(5) select * from Bean]");
#endif
                
                TryInvalidAnnotation(
                    env,
                    "@MyAnnotationValueArray(Value=\"ABC\", IntArray={}, DoubleArray={}, StringArray={}) select * from Bean",
                    false,
                    "Failed to process statement annotations: Annotation 'MyAnnotationValueArrayAttribute' requires a Int64[]-typed value for attribute 'Value' but received a String-typed value [@MyAnnotationValueArray(Value=\"ABC\", IntArray={}, DoubleArray={}, StringArray={}) select * from Bean]");
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

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        public class ClientRuntimeStatementAnnotationAppNested : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunNestedSimple(env);
                RunNestedArray(env);
            }

            //[MyAnnotationNested (
            //	NestableSimple = new MyAnnotationNestableSimpleAttribute (),
            //	NestableValues = new MyAnnotationNestableValuesAttribute { Val = 999, Arr = new int[] { 2, 1 } },
            //	NestableNestable = new MyAnnotationNestableNestableAttribute { Value = "CDF" }
            //)]
            private void RunNestedSimple(RegressionEnvironment env)
            {
                var stmtText =
                    "@MyAnnotationNested(\n" +
                    "            NestableSimple=@MyAnnotationNestableSimple,\n" +
                    "            NestableValues=@MyAnnotationNestableValues(Val=999, Arr={2, 1}),\n" +
                    "            NestableNestable=@MyAnnotationNestableNestable(\"CDF\")\n" +
                    "    ) " +
                    "@name('s0') select * from SupportBean";
                env.CompileDeploy(stmtText);

                env.AssertStatement(
                    "s0",
                    statement => {
                        var annotations = statement.Annotations;
                        annotations = SortAlpha(annotations);
                        Assert.AreEqual(2, annotations.Length);

                        var nested = (MyAnnotationNestedAttribute)annotations[0];
                        Assert.IsNotNull(nested.NestableSimple);
                        Assert.IsTrue(Arrays.DeepEquals(ToObjectArray(nested.NestableValues.Arr), new object[] { 2, 1 }));
                        Assert.AreEqual(999, nested.NestableValues.Val);
                        Assert.AreEqual("CDF", nested.NestableNestable.Value);
                    });

                env.UndeployAll();
            }

            //[MyAnnotationWArrayAndClass (
            //	Priorities = new [] { new PriorityAttribute (1), new PriorityAttribute (3) },
            //	ClassOne = typeof (string),
            //	ClassTwo = typeof (int?)
            //)]
            private void RunNestedArray(RegressionEnvironment env)
            {
                var stmtText =
                    "@MyAnnotationWArrayAndClass(Priorities = {@Priority(1), @Priority(3)}, ClassOne = System.String.class, ClassTwo = System.Int32.class) " +
                    "@Name('s0') select * from SupportBean";
                env.CompileDeploy(stmtText);

                env.AssertStatement(
                    "s0",
                    statement => {
                        var annotations = statement.Annotations;
                        annotations = SortAlpha(annotations);
                        Assert.AreEqual(2, annotations.Length);

                        var nested = (MyAnnotationWArrayAndClassAttribute)annotations[0];
                        Assert.AreEqual(1, nested.Priorities[0].Value);
                        Assert.AreEqual(3, nested.Priorities[1].Value);
                        Assert.AreEqual(typeof(string), nested.ClassOne);
                        Assert.AreEqual(typeof(int?), nested.ClassTwo);
                    });

                env.UndeployAll();
            }
        }

        public class ClientRuntimeStatementAnnotationBuiltin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                epl = "@Name('MyTestStmt') @Description('MyTestStmt description') @Tag(Name=\"UserId\", Value=\"value\") " +
                      "select * from SupportBean";
                env.CompileDeploy(epl).AddListener("MyTestStmt");
                env.AssertStatement(
                    "MyTestStmt",
                    statement => {
                        TryAssertion(statement);
                        var name = (NameAttribute)AnnotationUtil.FindAnnotation(
                            statement.Annotations,
                            typeof(NameAttribute));
                        Assert.AreEqual("MyTestStmt", name.Value);
                    });
                env.UndeployAll();

                // try lowercase
                epl = "@Name('MyTestStmt') @Description('MyTestStmt description') @Tag(Name=\"UserId\", Value=\"value\") " +
                      " select * from SupportBean";
                env.CompileDeploy(epl).AddListener("MyTestStmt");
                env.AssertStatement("MyTestStmt", ClientRuntimeStatementAnnotation.TryAssertion);
                env.UndeployAll();

                // try fully-qualified
                epl = "@" +
                      nameof(NameAttribute) +
                      "('MyTestStmt') @Description('MyTestStmt description') @Tag(Name=\"UserId\", Value=\"value\") " +
                      "select * from SupportBean";
                env.CompileDeploy(epl).AddListener("MyTestStmt");
                env.AssertStatement("MyTestStmt", ClientRuntimeStatementAnnotation.TryAssertion);
                env.UndeployAll();

                // hint tests
                Assert.IsNull(HintEnum.DISABLE_RECLAIM_GROUP.GetHint(null));
                Assert.IsNull(HintEnum.DISABLE_RECLAIM_GROUP.GetHint(Array.Empty<Attribute>()));
                env.CompileDeploy("@Hint('ITERATE_ONLY') select * from SupportBean");
                env.CompileDeploy("@Hint('ITERATE_ONLY,DISABLE_RECLAIM_GROUP') select * from SupportBean");
                env.CompileDeploy("@Hint('ITERATE_ONLY,DISABLE_RECLAIM_GROUP,ITERATE_ONLY') select * from SupportBean");
                env.CompileDeploy("@Hint('  iterate_only ') select * from SupportBean");

                env.CompileDeploy("@Hint('DISABLE_RECLAIM_GROUP') @name('s0') select * from SupportBean");
                env.AssertStatement(
                    "s0",
                    statement => Assert.AreEqual(
                        "DISABLE_RECLAIM_GROUP",
                        HintEnum.DISABLE_RECLAIM_GROUP.GetHint(statement.Annotations).Value));

                env.CompileDeploy(
                    "@Hint('ITERATE_ONLY,ITERATE_ONLY,DISABLE_RECLAIM_GROUP,ITERATE_ONLY') @name('s1') select * from SupportBean");
                env.AssertStatement(
                    "s1",
                    statement => Assert.AreEqual(
                        "ITERATE_ONLY,ITERATE_ONLY,DISABLE_RECLAIM_GROUP,ITERATE_ONLY",
                        HintEnum.DISABLE_RECLAIM_GROUP.GetHint(statement.Annotations).Value));

                env.CompileDeploy("@Hint('ITERATE_ONLY,reclaim_group_aged=10') @name('s2') select * from SupportBean");
                env.AssertStatement(
                    "s2",
                    statement => {
                        var hint = HintEnum.RECLAIM_GROUP_AGED.GetHint(statement.Annotations);
                        Assert.AreEqual("10", HintEnum.RECLAIM_GROUP_AGED.GetHintAssignedValue(hint));
                    });

                env.CompileDeploy("@Hint('reclaim_group_aged=11') @name('s3') select * from SupportBean");
                env.AssertStatement(
                    "s3",
                    statement => {
                        var hint = HintEnum.RECLAIM_GROUP_AGED.GetHint(statement.Annotations);
                        Assert.AreEqual("11", HintEnum.RECLAIM_GROUP_AGED.GetHintAssignedValue(hint));
                    });

                env.CompileDeploy("@Hint('index(one, two)') @name('s4') select * from SupportBean");
                env.AssertStatement(
                    "s4",
                    statement => Assert.AreEqual(
                        "one, two",
                        HintEnum.INDEX.GetHintAssignedValues(statement.Annotations)[0]));

                env.UndeployAll();

                // NoLock
                env.CompileDeploy("@name('s0') @NoLock select * from SupportBean");
                env.AssertStatement(
                    "s0",
                    statement => Assert.AreEqual(
                        1,
                        AnnotationUtil.FindAnnotations(statement.Annotations, typeof(NoLockAttribute)).Count));

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
                    "@MyAnnotationValueEnum(SupportEnum = " + text + ") @name('s0') select * from SupportBean");
                env.AssertStatement(
                    "s0",
                    statement => {
                        var anno = (MyAnnotationValueEnumAttribute)statement.Annotations[0];
                        Assert.AreEqual(expected, anno.SupportEnum);
                    });
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
                env.TryInvalidCompile(epl, "Failed to process statement annotations: Failed to resolve @-annotation");

                // try invalid use : these are annotation-specific imports of an annotation and an enum
                env.TryInvalidCompile(
                    "select * from MyAnnotationValueEnumTwo",
                    "Failed to resolve event type, named window or table by name 'MyAnnotationValueEnumTwo'");
                env.TryInvalidCompile(
                    "select SupportEnum.ENUM_VALUE_1 from SupportBean",
                    "Failed to validate select-clause expression 'SupportEnum.ENUM_VALUE_1'");

                env.UndeployAll();
            }
        }

        private static void TryInvalidAnnotation(
            RegressionEnvironment env,
            string stmtText,
            bool isSyntax,
            string message)
        {
            try {
                env.Compiler.Compile(stmtText, new CompilerArguments(env.Configuration));
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
            env.AssertStatement(
                "s0",
                statement => {
                    Assert.AreEqual(2, statement.Annotations.Length);

                    var array = (MyAnnotationValueArrayAttribute)statement.Annotations[0];
                    Assert.IsTrue(Arrays.DeepEquals(ToObjectArray(array.Value), new object[] { 1L, 2L, 3L }));
                    Assert.IsTrue(Arrays.DeepEquals(ToObjectArray(array.IntArray), new object[] { 4, 5 }));
                    Assert.IsTrue(Arrays.DeepEquals(ToObjectArray(array.DoubleArray), new object[] { }));
                    Assert.IsTrue(Arrays.DeepEquals(ToObjectArray(array.StringArray), new object[] { "X" }));
                    Assert.IsTrue(Arrays.DeepEquals(ToObjectArray(array.StringArrayDef), new object[] { "XYZ" }));
                });
        }

        private static void TryAssertion(EPStatement stmt)
        {
            var annotations = stmt.Annotations;
            annotations = SortAlpha(annotations);
            Assert.AreEqual(3, annotations.Length);

            Assert.AreEqual(typeof(DescriptionAttribute), annotations[0].GetType());
            Assert.AreEqual("MyTestStmt description", ((DescriptionAttribute)annotations[0]).Value);
            Assert.AreEqual("@Description(\"MyTestStmt description\")", annotations[0].ToString());

            Assert.AreEqual(typeof(NameAttribute), annotations[1].GetType());
            Assert.AreEqual("MyTestStmt", ((NameAttribute)annotations[1]).Value);
            Assert.AreEqual("MyTestStmt", stmt.Name);
            Assert.AreEqual("@name(\"MyTestStmt\")", annotations[1].ToString());

            Assert.AreEqual(typeof(TagAttribute), annotations[2].GetType());
            Assert.AreEqual("UserId", ((TagAttribute)annotations[2]).Name);
            Assert.AreEqual("value", ((TagAttribute)annotations[2]).Value);
            Assert.AreEqual("@Tag(name=\"UserId\", value=\"value\")", annotations[2].ToString());

            Assert.IsFalse(annotations[2].Equals(annotations[1]));
            Assert.IsTrue(annotations[1].Equals(annotations[1]));
            Assert.IsTrue(annotations[1].GetHashCode() != 0);
        }
    }
} // end of namespace