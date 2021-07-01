///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.regressionlib.support.filter
{
	public class SupportFilterPlanTriplet {
	    private string lookupable;
	    private FilterOperator op;
	    private string value;
	    private string controlConfirm;

	    public SupportFilterPlanTriplet(string lookupable, FilterOperator op, string value) {
	        this.lookupable = lookupable;
	        this.op = op;
	        this.value = value;
	    }

	    public SupportFilterPlanTriplet(string lookupable, FilterOperator op, string value, string controlConfirm) {
	        this.lookupable = lookupable;
	        this.op = op;
	        this.value = value;
	        this.controlConfirm = controlConfirm;
	    }

	    public string Lookupable => lookupable;

	    public FilterOperator Op => op;

	    public string Value => value;

	    public string ControlConfirm => controlConfirm;
	}
} // end of namespace
