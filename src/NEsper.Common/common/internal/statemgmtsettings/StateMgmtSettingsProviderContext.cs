///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.context.controller.category;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.controller.hash;
using com.espertech.esper.common.@internal.context.controller.initterm;
using com.espertech.esper.common.@internal.context.controller.keyed;
using com.espertech.esper.common.@internal.fabric;

namespace com.espertech.esper.common.@internal.statemgmtsettings
{
    public interface StateMgmtSettingsProviderContext
    {
        void Context(
            FabricCharge fabricCharge,
            ContextMetaData detail,
            ContextControllerFactoryForge[] controllerFactoryForges);

        void FilterContextKeyed(
            int nestingLevel,
            FabricCharge fabricCharge,
            IList<ContextSpecKeyedItem> items);

        void FilterContextHash(
            int nestingLevel,
            FabricCharge fabricCharge,
            IList<ContextSpecHashItem> items);

        StateMgmtSetting ContextPartitionId(
            FabricCharge fabricCharge,
            StatementRawInfo statementRawInfo,
            ContextMetaData contextMetaData);

        StateMgmtSetting ContextCategory(
            FabricCharge fabricCharge,
            ContextMetaData detail,
            ContextControllerCategoryFactoryForge forge,
            StatementRawInfo raw,
            int controllerLevel);

        StateMgmtSetting ContextHash(
            FabricCharge fabricCharge,
            ContextMetaData detail,
            ContextControllerHashFactoryForge forge,
            StatementRawInfo raw,
            int controllerLevel);

        StateMgmtSetting ContextKeyed(
            FabricCharge fabricCharge,
            ContextMetaData detail,
            ContextControllerKeyedFactoryForge forge,
            StatementRawInfo raw,
            int controllerLevel);

        StateMgmtSetting ContextKeyedTerm(
            FabricCharge fabricCharge,
            ContextMetaData detail,
            ContextControllerKeyedFactoryForge forge,
            StatementRawInfo raw,
            int controllerLevel);

        StateMgmtSetting ContextInitTerm(
            FabricCharge fabricCharge,
            ContextMetaData detail,
            ContextControllerInitTermFactoryForge forge,
            StatementRawInfo raw,
            int controllerLevel);

        StateMgmtSetting ContextInitTermDistinct(
            FabricCharge fabricCharge,
            ContextMetaData detail,
            ContextControllerInitTermFactoryForge forge,
            StatementRawInfo raw,
            int controllerLevel);
    }
} // end of namespace