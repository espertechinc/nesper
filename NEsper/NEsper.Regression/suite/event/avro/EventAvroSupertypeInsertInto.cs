///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using Avro.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.@event.avro
{
	public class EventAvroSupertypeInsertInto : RegressionExecution
	{
		private static readonly string[] FIELDS = new string[] {"symbol"};

		public void Run(RegressionEnvironment env)
		{
			string epl = "@name('input') @public @buseventtype create avro schema Input(symbol string, price double);\n" +
			             "\n" +
			             "@public @buseventtype create avro schema SuperType(symbol string);\n" +
			             "@public @buseventtype create avro schema B() inherits SuperType;\n" +
			             "@public @buseventtype create avro schema A() inherits SuperType;\n" +
			             "\n" +
			             "insert into B select symbol from Input(symbol = 'B');\n" +
			             "insert into A select symbol from Input(symbol = 'A');\n" +
			             "\n" +
			             "@Name('ss') select * from SuperType;\n" +
			             "@Name('sa') select * from A;\n" +
			             "@Name('sb') select * from B;\n";
			env.CompileDeploy(epl).AddListener("ss").AddListener("sa").AddListener("sb");

			SendEvent(env, "B");
			AssertReceived(env, "ss", "B");
			AssertReceived(env, "sb", "B");
			Assert.IsFalse(env.Listener("sa").IsInvoked);

			SendEvent(env, "A");
			AssertReceived(env, "ss", "A");
			AssertReceived(env, "sa", "A");
			Assert.IsFalse(env.Listener("sb").IsInvoked);

			env.UndeployAll();
		}

		private void SendEvent(
			RegressionEnvironment env,
			string symbol)
		{
			var schema = SupportAvroUtil.GetAvroSchema(env.Statement("input").EventType).AsRecordSchema();
			var rec = new GenericRecord(schema);
			rec.Put("symbol", symbol);
			env.SendEventAvro(rec, "Input");
		}

		private void AssertReceived(
			RegressionEnvironment env,
			string statementName,
			string symbol)
		{
			EPAssertionUtil.AssertProps(env.Listener(statementName).AssertOneGetNewAndReset(), FIELDS, new object[] {symbol});
		}
	}
} // end of namespace
