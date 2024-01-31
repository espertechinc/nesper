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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.support.context
{
    public class SupportContextPropUtil
    {
        public static void AssertContextProps(
            RegressionEnvironment env,
            string stmtName,
            string contextName,
            int[] ids,
            string fieldsCSV,
            object[][] values)
        {
            if (fieldsCSV != null) {
                ClassicAssert.AreEqual(ids.Length, values.Length);
            }
            else {
                ClassicAssert.IsNull(values);
            }

            env.AssertStatement(
                stmtName,
                stmt => {
                    var num = -1;
                    foreach (var id in ids) {
                        num++;
                        var props = env.Runtime.ContextPartitionService.GetContextProperties(
                            stmt.DeploymentId,
                            contextName,
                            id);
                        AssertProps(id, contextName, props, fieldsCSV, values?[num], true);
                    }
                });
        }

        /// <summary>
        ///     Values:
        ///     - by id first
        ///     - by level second
        ///     - by field third
        /// </summary>
        public static void AssertContextPropsNested(
            RegressionEnvironment env,
            string stmtName,
            string contextName,
            int[] ids,
            string[] nestedContextNames,
            string[] fieldsCSVPerCtx,
            object[][][] values)
        {
            env.AssertStatement(
                stmtName,
                stmt => {
                    var line = -1;
                    foreach (var id in ids) {
                        line++;
                        var props = env.Runtime.ContextPartitionService.GetContextProperties(
                            stmt.DeploymentId,
                            contextName,
                            id);
                        ClassicAssert.AreEqual(contextName, props.Get("name"));
                        ClassicAssert.AreEqual(id, props.Get("id"));

                        ClassicAssert.AreEqual(nestedContextNames.Length, fieldsCSVPerCtx.Length);
                        for (var level = 0; level < nestedContextNames.Length; level++) {
                            AssertProps(
                                id,
                                nestedContextNames[level],
                                (IDictionary<string, object>)props.Get(nestedContextNames[level]),
                                fieldsCSVPerCtx[level],
                                values[line][level],
                                false);
                        }
                    }
                });
        }

        private static void AssertProps(
            int id,
            string contextName,
            IDictionary<string, object> props,
            string fieldsCSV,
            object[] values,
            bool assertId)
        {
            var fields = fieldsCSV == null ? Array.Empty<string>() : fieldsCSV.SplitCsv();

            if (values != null) {
                ClassicAssert.AreEqual(values.Length, fields.Length);
            }

            ClassicAssert.AreEqual(contextName, props.Get("name"));
            if (assertId) {
                ClassicAssert.AreEqual(id, props.Get("id"));
            }

            var col = -1;
            foreach (var field in fields) {
                col++;
                var expected = values[col];
                var actual = props.Get(field);
                if (actual is EventBean actualEventBean) {
                    actual = actualEventBean.Underlying;
                }

                ClassicAssert.AreEqual(expected, actual, "Mismatch id " + id + " field " + field);
            }
        }
    }
} // end of namespace