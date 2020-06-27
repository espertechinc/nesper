///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.path;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.serde
{
    public class CodegenSharableSerdeEventTyped : CodegenFieldSharable
    {
        private readonly EventType eventType;
        private readonly CodegenSharableSerdeName name;

        public CodegenSharableSerdeEventTyped(
            CodegenSharableSerdeName name,
            EventType eventType)
        {
            this.name = name;
            this.eventType = eventType;
            if (eventType == null || name == null) {
                throw new ArgumentException();
            }
        }

        public Type Type()
        {
            return typeof(DataInputOutputSerdeWCollation<object>);
        }

        public CodegenExpression InitCtorScoped()
        {
            var type = EventTypeUtility.ResolveTypeCodegen(eventType, EPStatementInitServicesConstants.REF);
            return ExprDotMethodChain(EPStatementInitServicesConstants.REF)
                .Get(EPStatementInitServicesConstants.EVENTTYPERESOLVER)
                .Add(EventTypeResolverConstants.GETEVENTSERDEFACTORY)
                .Add(name.MethodName, type);
        }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (CodegenSharableSerdeEventTyped) o;

            if (name != that.name) {
                return false;
            }

            return eventType.Name.Equals(that.eventType.Name);
        }

        public override int GetHashCode()
        {
            var result = name.GetHashCode();
            result = 31 * result + eventType.Name.GetHashCode();
            return result;
        }

        public class CodegenSharableSerdeName
        {
            public static readonly CodegenSharableSerdeName EVENTNULLABLE =
                new CodegenSharableSerdeName("EventNullable", typeof(object));

            public static readonly CodegenSharableSerdeName LISTEVENTS =
                new CodegenSharableSerdeName("ListEvents", typeof(object));

            public static readonly CodegenSharableSerdeName LINKEDMAPEVENTSANDINT =
                new CodegenSharableSerdeName("LinkedMapEventsAndInt", typeof(object));

            public static readonly CodegenSharableSerdeName REFCOUNTEDSETATOMICINTEGER =
                new CodegenSharableSerdeName("RefCountedSetAtomicInteger", typeof(object));

            private CodegenSharableSerdeName(string methodName)
            {
                MethodName = methodName;
                MethodTypeArgs = new Type[0];
            }

            public CodegenSharableSerdeName(
                string methodName,
                params Type[] methodTypeArgs)
            {
                MethodName = methodName;
                MethodTypeArgs = methodTypeArgs;
            }

            public string MethodName { get; }

            public Type[] MethodTypeArgs { get; }
        }
    }
} // end of namespace