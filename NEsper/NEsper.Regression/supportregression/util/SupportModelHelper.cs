///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.soda;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.util
{
    public class SupportModelHelper
    {
        public static EPStatement CreateByCompileOrParse(EPServiceProvider epService, bool soda, String epl)
        {
            return CreateByCompileOrParse(epService, soda, epl, null);
        }

        public static EPStatement CreateByCompileOrParse(EPServiceProvider epService, bool soda, String epl, Object statementUserObject)
        {
            if (!soda)
            {
                return epService.EPAdministrator.CreateEPL(epl, statementUserObject);
            }
            return CompileCreate(epService, epl, statementUserObject);
        }

        public static EPStatement CompileCreate(EPServiceProvider epService, String epl)
        {
            return CompileCreate(epService, epl, null);
        }

        public static EPStatement CompileCreate(EPServiceProvider epService, String epl, Object statementUserObject)
        {
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(epl.Replace("\n", ""), model.ToEPL().Replace("\n", ""));
            EPStatement stmt = epService.EPAdministrator.Create(model, null, statementUserObject);
            Assert.AreEqual(epl.Replace("\n", ""), stmt.Text.Replace("\n", ""));
            return stmt;
        }
    }
}
