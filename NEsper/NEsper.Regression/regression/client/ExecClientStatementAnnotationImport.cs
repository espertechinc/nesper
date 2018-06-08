///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

namespace com.espertech.esper.regression.client
{
    public class ExecClientStatementAnnotationImport : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddAnnotationImport(typeof(SupportEnum));
            configuration.AddAnnotationImport(typeof(MyAnnotationValueEnumAttribute));
            configuration.AddEventType<SupportBean>();
        }
    
        public override void Run(EPServiceProvider epService) {
            // init-time import
            epService.EPAdministrator.CreateEPL("@MyAnnotationValueEnum(SupportEnum = SupportEnum.ENUM_VALUE_1) " +
                    "select * from SupportBean");
    
            // try invalid annotation not yet imported
            string epl = "@MyAnnotationValueEnumTwo(SupportEnum = SupportEnum.ENUM_VALUE_1) select * from SupportBean";
            SupportMessageAssertUtil.TryInvalid(epService, epl, "Failed to process statement annotations: Failed to resolve @-annotation");
    
            // runtime import
            epService.EPAdministrator.Configuration.AddAnnotationImport(typeof(MyAnnotationValueEnumTwoAttribute));
            epService.EPAdministrator.CreateEPL(epl);
    
            // try invalid use : these are annotation-specific imports of an annotation and an enum
            SupportMessageAssertUtil.TryInvalid(epService, "select * from MyAnnotationValueEnumTwo",
                    "Failed to resolve event type: Event type or class named");
            SupportMessageAssertUtil.TryInvalid(epService, "select SupportEnum.ENUM_VALUE_1 from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'SupportEnum.ENUM_VALUE_1'");
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    }
} // end of namespace
