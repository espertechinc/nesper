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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.serde
{
	public class CodegenSharableSerdeEventTyped : CodegenFieldSharable {
	    private readonly CodegenSharableSerdeName name;
	    private readonly EventType eventType;

	    public CodegenSharableSerdeEventTyped(CodegenSharableSerdeName name, EventType eventType) {
	        this.name = name;
	        this.eventType = eventType;
	        if (eventType == null || name == null) {
	            throw new ArgumentException();
	        }
	    }

	    public Type Type() {
	        return typeof(DataInputOutputSerdeWCollation);
	    }

	    public CodegenExpression InitCtorScoped() {
	        CodegenExpression type = EventTypeUtility.ResolveTypeCodegen(eventType, EPStatementInitServicesConstants.REF);
	        return ExprDotMethodChain(EPStatementInitServicesConstants.REF).Add(EPStatementInitServicesConstants.GETDATAINPUTOUTPUTSERDEPROVIDER).Add(name.methodName, type);
	    }

	    public class CodegenSharableSerdeName
	    {
	        public static readonly CodegenSharableSerdeName EVENTNULLABLE =
	            new CodegenSharableSerdeName("eventNullable");
	        public static readonly CodegenSharableSerdeName LISTEVENTS =
	            new CodegenSharableSerdeName("listEvents");
	        public static readonly CodegenSharableSerdeName LINKEDHASHMAPEVENTSANDINT =
	            new CodegenSharableSerdeName("linkedHashMapEventsAndInt");
	        public static readonly CodegenSharableSerdeName REFCOUNTEDSETATOMICINTEGER =
	            new CodegenSharableSerdeName("refCountedSetAtomicInteger");

	        CodegenSharableSerdeName(string methodName) {
	            this.MethodName = methodName;
	        }

	        public string MethodName { get; }
	    }

	    public override bool Equals(object o) {
	        if (this == o) return true;
	        if (o == null || GetType() != o.GetType()) return false;

	        CodegenSharableSerdeEventTyped that = (CodegenSharableSerdeEventTyped) o;

	        if (name != that.name) return false;
	        return eventType.Name.Equals(that.eventType.Name);
	    }

	    public override int GetHashCode() {
	        int result = name.GetHashCode();
	        result = 31 * result + eventType.Name.GetHashCode();
	        return result;
	    }
	}
} // end of namespace