///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.path;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; // exprDotMethodChain;

// GETEVENTSERDEFACTORY;

namespace com.espertech.esper.common.@internal.serde.compiletime.sharable
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
	        return typeof(DataInputOutputSerde);
	    }

	    public CodegenExpression InitCtorScoped() {
	        CodegenExpression type = EventTypeUtility
		        .ResolveTypeCodegen(eventType, EPStatementInitServicesConstants.REF);
	        return ExprDotMethodChain(EPStatementInitServicesConstants.REF)
		        .Get(EPStatementInitServicesConstants.EVENTTYPERESOLVER)
		        .Add(EventTypeResolverConstants.GETEVENTSERDEFACTORY)
		        .Add(name.MethodName, type);
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


	    public class CodegenSharableSerdeName
	    {
		    public static readonly CodegenSharableSerdeName NULLABLEEVENTMAYCOLLATE =
			    new CodegenSharableSerdeName("NullableEventMayCollate");

		    public static readonly CodegenSharableSerdeName LISTEVENTS =
			    new CodegenSharableSerdeName("ListEvents");

		    public static readonly CodegenSharableSerdeName LINKEDHASHMAPEVENTSANDINT =
			    new CodegenSharableSerdeName("LinkedHashMapEventsAndInt");

		    public static readonly CodegenSharableSerdeName REFCOUNTEDSETATOMICINTEGER =
			    new CodegenSharableSerdeName("RefCountedSetAtomicInteger");

		    public string MethodName { get; }

		    CodegenSharableSerdeName(string methodName)
		    {
			    this.MethodName = methodName;
		    }

	    }
	}
} // end of namespace
