///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.derived
{
    public class DerivedViewTypeUtil
    {
        public static EventType NewType(
            string name,
            LinkedHashMap<string, object> schemaMap,
            ViewForgeEnv env,
            int streamNum)
        {
            string outputEventTypeName =
                env.StatementCompileTimeServices.EventTypeNameGeneratorStatement.GetViewDerived(name, streamNum);
            EventTypeMetadata metadata = new EventTypeMetadata(
                outputEventTypeName,
                env.ModuleName,
                EventTypeTypeClass.VIEWDERIVED,
                EventTypeApplicationType.MAP,
                NameAccessModifier.TRANSIENT,
                EventTypeBusModifier.NONBUS,
                false,
                EventTypeIdPair.Unassigned());
            IDictionary<string, object> propertyTypes = EventTypeUtility.GetPropertyTypesNonPrimitive(schemaMap);
            EventType resultEventType = BaseNestableEventUtil.MakeMapTypeCompileTime(
                metadata,
                propertyTypes,
                null,
                null,
                null,
                null,
                env.BeanEventTypeFactoryProtected,
                env.EventTypeCompileTimeResolver);
            env.EventTypeModuleCompileTimeRegistry.NewType(resultEventType);
            return resultEventType;
        }
    }
} // end of namespace