///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using Avro.Generic;

using com.espertech.esper.regressionlib.framework;

using NEsper.Avro.Extensions;

namespace com.espertech.esper.regressionlib.suite.@event.avro
{
    public class EventAvroSupertypeInsertInto : RegressionExecution
    {
        private static readonly string[] FIELDS = new string[] { "Symbol" };

        public void Run(RegressionEnvironment env)
        {
            var epl = "@name('input') @public @buseventtype create avro schema Input(Symbol string, Price double);\n" +
                      "\n" +
                      "@public @buseventtype create avro schema SuperType(Symbol string);\n" +
                      "@public @buseventtype create avro schema B() inherits SuperType;\n" +
                      "@public @buseventtype create avro schema A() inherits SuperType;\n" +
                      "\n" +
                      "insert into B select Symbol from Input(Symbol = 'B');\n" +
                      "insert into A select Symbol from Input(Symbol = 'A');\n" +
                      "\n" +
                      "@name('ss') select * from SuperType;\n" +
                      "@name('sa') select * from A;\n" +
                      "@name('sb') select * from B;\n";
            env.CompileDeploy(epl).AddListener("ss").AddListener("sa").AddListener("sb");

            SendEvent(env, "B");
            AssertReceived(env, "ss", "B");
            AssertReceived(env, "sb", "B");
            env.AssertListenerNotInvoked("sa");

            SendEvent(env, "A");
            AssertReceived(env, "ss", "A");
            AssertReceived(env, "sa", "A");
            env.AssertListenerNotInvoked("sb");

            env.UndeployAll();
        }

        private void SendEvent(
            RegressionEnvironment env,
            string symbol)
        {
            var schema = env.RuntimeAvroSchemaByDeployment("input", "Input").AsRecordSchema();
            var rec = new GenericRecord(schema);
            rec.Put("Symbol", symbol);
            rec.Put("Price", 1d);
            env.SendEventAvro(rec, "Input");
        }

        private void AssertReceived(
            RegressionEnvironment env,
            string statementName,
            string symbol)
        {
            env.AssertPropsNew(statementName, FIELDS, new object[] { symbol });
        }
    }
} // end of namespace