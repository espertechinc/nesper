///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.hint
{
    public class ExcludePlanHintExprUtil
    {
        internal static readonly ObjectArrayEventType OAEXPRESSIONTYPE;

        static ExcludePlanHintExprUtil()
        {
            LinkedHashMap<string, object> properties = new LinkedHashMap<string, object>();
            properties.Put("from_streamnum", typeof(int?));
            properties.Put("to_streamnum", typeof(int?));
            properties.Put("from_streamname", typeof(string));
            properties.Put("to_streamname", typeof(string));
            properties.Put("opname", typeof(string));
            properties.Put("exprs", typeof(string[]));
            string eventTypeName = EventTypeNameUtil.GetAnonymousTypeNameExcludePlanHint();
            EventTypeMetadata eventTypeMetadata = new EventTypeMetadata(
                eventTypeName,
                null,
                EventTypeTypeClass.EXCLUDEPLANHINTDERIVED,
                EventTypeApplicationType.OBJECTARR,
                NameAccessModifier.TRANSIENT,
                EventTypeBusModifier.NONBUS,
                false,
                EventTypeIdPair.Unassigned());
            OAEXPRESSIONTYPE = BaseNestableEventUtil.MakeOATypeCompileTime(
                eventTypeMetadata,
                properties,
                null,
                null,
                null,
                null,
                new BeanEventTypeFactoryDisallow(EventBeanTypedEventFactoryCompileTime.INSTANCE),
                null);
        }

        public static EventBean ToEvent(
            int fromStreamnum,
            int toStreamnum,
            string fromStreamname,
            string toStreamname,
            string opname,
            ExprNode[] expressions)
        {
            string[] texts = new string[expressions.Length];
            for (int i = 0; i < expressions.Length; i++) {
                texts[i] = ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(expressions[i]);
            }

            object[] @event = new object[] {fromStreamnum, toStreamnum, fromStreamname, toStreamname, opname, texts};
            return new ObjectArrayEventBean(@event, OAEXPRESSIONTYPE);
        }

        public static ExprForge ToExpression(
            string hint,
            StatementRawInfo rawInfo,
            StatementCompileTimeServices services)
        {
            ExprNode expr = services.CompilerServices.CompileExpression(hint, services);
            ExprNode validated = EPLValidationUtil.ValidateSimpleGetSubtree(
                ExprNodeOrigin.HINT,
                expr,
                OAEXPRESSIONTYPE,
                false,
                rawInfo,
                services);
            return validated.Forge;
        }
    }
} // end of namespace