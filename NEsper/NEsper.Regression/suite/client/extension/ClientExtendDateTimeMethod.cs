///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.datetimemethod;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.epl.methodbase;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.client.extension
{
	public class ClientExtendDateTimeMethod
	{
		public static ICollection<RegressionExecution> Executions()
		{
			var execs = new List<RegressionExecution>();
			execs.Add(new ClientExtendDateTimeMethodTransform());
			execs.Add(new ClientExtendDateTimeMethodReformat());
			execs.Add(new ClientExtendDateTimeMethodInvalid());
			return execs;
		}

		private class ClientExtendDateTimeMethodInvalid : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				// validate factory returns no forge
				TryInvalidCompile(
					env,
					"select DateTimeEx.someDTMInvalidNoOp() from SupportDateTime",
					"Failed to validate select-clause expression 'DateTimeEx.someDTMInvalidNoOp()': Plug-in datetime method provider class");

				// validate pre-made argument test
				TryInvalidCompile(
					env,
					"select DateTimeEx.dtmInvalidMethodNotExists('x') from SupportDateTime",
					"Failed to validate select-clause expression 'DateTimeEx.dtmInvalidMethodNotExists('x')': Failed to resolve enumeration method, date-time method or mapped property 'DateTimeEx.dtmInvalidMethodNotExists('x')': Failed to validate date-time method 'dtmInvalidMethodNotExists', expected a Integer-type result for expression parameter 0 but received java.lang.String");

				// validate static method not matching
				TryInvalidCompile(
					env,
					"select DateTimeOffset.dtmInvalidMethodNotExists(1) from SupportDateTime",
					"Failed to validate select-clause expression 'DateTimeOffset.dtmInvalidMethodNotExists(1)': Failed to find static method for date-time method extension: Unknown method ArrayList.dtmInvalidMethod");

				// validate not provided
				TryInvalidCompile(
					env,
					"select DateTimeEx.dtmInvalidNotProvided() from SupportDateTime",
					"Failed to validate select-clause expression 'DateTimeEx.dtmInvalidNotProvided()': Plugin datetime method does not provide a forge for input type java.util.Calendar");
			}
		}

		private class ClientExtendDateTimeMethodTransform : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@name('s0') select " +
				          "LongDate.roll('date', true) as c0, " +
				          "DateTimeEx.roll('date', true) as c1, " +
				          "DateTimeOffset.roll('date', true) as c2, " +
				          "DateTime.roll('date', true) as c3 " +
				          " from SupportDateTime";
				env.CompileDeploy(epl).AddListener("s0");

				var @event = SupportDateTime.Make("2002-05-30T09:01:02.000");
				var dateExpected = @event.DateTimeEx.Clone();
				dateExpected.AddDays(1);

				env.SendEventBean(@event);
				EPAssertionUtil.AssertProps(
					env.Listener("s0").AssertOneGetNewAndReset(),
					"c0,c1,c2,c3,c4".SplitCsv(),
					new object[] {
						dateExpected.UtcMillis,
						dateExpected,
						dateExpected.DateTime,
						dateExpected.DateTime.DateTime
					});

				env.UndeployAll();
			}
		}

		private class ClientExtendDateTimeMethodReformat : RegressionExecution
		{
			public void Run(RegressionEnvironment env)
			{
				var epl = "@name('s0') select " +
				          "LongDate.asArrayOfString() as c0, " +
				          "DateTimeEx.asArrayOfString() as c1, " +
				          "DateTimeOffset.asArrayOfString() as c2, " +
				          "DateTime.asArrayOfString() as c3 " +
				          " from SupportDateTime";
				env.CompileDeploy(epl).AddListener("s0");

				var @event = SupportDateTime.Make("2002-05-30T09:01:02.000");

				env.SendEventBean(@event);
				var expected = "30,5,2002".SplitCsv();
				EPAssertionUtil.AssertProps(
					env.Listener("s0").AssertOneGetNewAndReset(),
					"c0,c1,c2,c3".SplitCsv(),
					new object[] {
						expected,
						expected,
						expected,
						expected
					});

				env.UndeployAll();
			}
		}

		public class MyLocalDTMForgeFactoryRoll : DateTimeMethodForgeFactory
		{
			private readonly static DotMethodFP[] FOOTPRINTS = new DotMethodFP[] {
				new DotMethodFP(
					DotMethodFPInputEnum.SCALAR_ANY,
					new DotMethodFPParam("an string-type calendar field name", EPLExpressionParamType.SPECIFIC, typeof(string)),
					new DotMethodFPParam("a boolean-type roll indicator", EPLExpressionParamType.SPECIFIC, typeof(bool)))
			};

			public DateTimeMethodDescriptor Initialize(DateTimeMethodInitializeContext context)
			{
				return new DateTimeMethodDescriptor(FOOTPRINTS);
			}

			public DateTimeMethodOps Validate(DateTimeMethodValidateContext context)
			{
				var roll = new DateTimeMethodOpsModify();
				roll.DateTimeExOp = new DateTimeMethodModeStaticMethod(typeof(MyLocalDTMRollUtility), "RollOne");
				roll.DateTimeOffsetOp = new DateTimeMethodModeStaticMethod(typeof(MyLocalDTMRollUtility), "RollTwo");
				roll.DateTimeOp = new DateTimeMethodModeStaticMethod(typeof(MyLocalDTMRollUtility), "RollThree");
				return roll;
			}
		}

		public class MyLocalDTMRollUtility
		{
			public static void RollOne(
				DateTimeEx dateTime,
				string fieldName,
				bool flagValue)
			{
				switch (fieldName) {
					case "date":
						if (flagValue) {
							dateTime.AddDays(1);
						}
						else {
							dateTime.AddDays(-1);
						}

						break;

					default:
						throw new EPException("Invalid field name '" + fieldName + "'");
				}
			}

			public static DateTimeOffset RollTwo(
				DateTimeOffset dateTime,
				string fieldName,
				bool flagValue)
			{
				switch (fieldName) {
					case "date":
						return dateTime.AddDays(1);

					default:
						throw new EPException("Invalid field name '" + fieldName + "'");
				}
			}

			public static DateTime RollThree(
				DateTime dateTime,
				string fieldName,
				bool flagValue)
			{
				switch (fieldName) {
					case "date":
						return dateTime.AddDays(1);

					default:
						throw new EPException("Invalid field name '" + fieldName + "'");
				}
			}
		}

		public class MyLocalDTMForgeFactoryArrayOfString : DateTimeMethodForgeFactory
		{
			private readonly static DotMethodFP[] FOOTPRINTS = new DotMethodFP[] {
				new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY)
			};

			public DateTimeMethodDescriptor Initialize(DateTimeMethodInitializeContext context)
			{
				return new DateTimeMethodDescriptor(FOOTPRINTS);
			}

			public DateTimeMethodOps Validate(DateTimeMethodValidateContext context)
			{
				var asArrayOfString = new DateTimeMethodOpsReformat();
				asArrayOfString.ReturnType = typeof(string[]);
				asArrayOfString.LongOp = new DateTimeMethodModeStaticMethod(typeof(MyLocalDTMArrayOfStringUtility), "AsArrayOfStringOne");
				asArrayOfString.DateTimeExOp = new DateTimeMethodModeStaticMethod(typeof(MyLocalDTMArrayOfStringUtility), "AsArrayOfStringThree");
				asArrayOfString.DateTimeOffsetOp = new DateTimeMethodModeStaticMethod(typeof(MyLocalDTMArrayOfStringUtility), "AsArrayOfStringFour");
				asArrayOfString.DateTimeOp = new DateTimeMethodModeStaticMethod(typeof(MyLocalDTMArrayOfStringUtility), "AsArrayOfStringFive");
				return asArrayOfString;
			}
		}

		public class MyLocalDTMArrayOfStringUtility
		{
			public static string[] AsArrayOfStringOne(long date)
			{
				var calendar = DateTimeEx.UtcInstance(date);
				return AsArrayOfStringThree(calendar);
			}

			public static string[] AsArrayOfStringThree(DateTimeEx value)
			{
				return new string[] {
					value.Day.ToString(),
					value.Month.ToString(),
					value.Year.ToString()
				};
			}

			public static string[] AsArrayOfStringFour(DateTimeOffset value)
			{
				return new string[] {
					value.Day.ToString(),
					value.Month.ToString(),
					value.Year.ToString()
				};
			}

			public static string[] AsArrayOfStringFive(DateTime value)
			{
				return new string[] {
					value.Day.ToString(),
					value.Month.ToString(),
					value.Year.ToString()
				};
			}
		}

		public class MyLocalDTMForgeFactoryInvalidMethodNotExists : DateTimeMethodForgeFactory
		{
			private readonly static DotMethodFP[] FOOTPRINTS = new DotMethodFP[] {
				new DotMethodFP(
					DotMethodFPInputEnum.SCALAR_ANY,
					new DotMethodFPParam("an int-type dummy", EPLExpressionParamType.SPECIFIC, typeof(int?)))
			};

			public DateTimeMethodDescriptor Initialize(DateTimeMethodInitializeContext context)
			{
				return new DateTimeMethodDescriptor(FOOTPRINTS);
			}

			public DateTimeMethodOps Validate(DateTimeMethodValidateContext context)
			{
				var valueChange = new DateTimeMethodOpsModify();
				valueChange.DateTimeOffsetOp = new DateTimeMethodModeStaticMethod(
					typeof(List<string>),
					"dtmInvalidMethod");
				return valueChange;
			}
		}

		public class MyLocalDTMForgeFactoryInvalidNotProvided : DateTimeMethodForgeFactory
		{
			private readonly static DotMethodFP[] FOOTPRINTS = new DotMethodFP[] {
				new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY)
			};

			public DateTimeMethodDescriptor Initialize(DateTimeMethodInitializeContext context)
			{
				return new DateTimeMethodDescriptor(FOOTPRINTS);
			}

			public DateTimeMethodOps Validate(DateTimeMethodValidateContext context)
			{
				return new DateTimeMethodOpsModify();
			}
		}

		public class MyLocalDTMForgeFactoryInvalidReformat : DateTimeMethodForgeFactory
		{
			private readonly static DotMethodFP[] FOOTPRINTS = new DotMethodFP[] {
				new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY)
			};

			public DateTimeMethodDescriptor Initialize(DateTimeMethodInitializeContext context)
			{
				return new DateTimeMethodDescriptor(FOOTPRINTS);
			}

			public DateTimeMethodOps Validate(DateTimeMethodValidateContext context)
			{
				return null;
			}
		}

		public class MyLocalDTMForgeFactoryInvalidNoOp : DateTimeMethodForgeFactory
		{
			private readonly static DotMethodFP[] FOOTPRINTS = new DotMethodFP[] {
				new DotMethodFP(DotMethodFPInputEnum.SCALAR_ANY)
			};

			public DateTimeMethodDescriptor Initialize(DateTimeMethodInitializeContext context)
			{
				return new DateTimeMethodDescriptor(FOOTPRINTS);
			}

			public DateTimeMethodOps Validate(DateTimeMethodValidateContext context)
			{
				return null;
			}
		}
	}
} // end of namespace
