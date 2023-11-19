///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.runtime.client.option;

namespace com.espertech.esper.regressionlib.support.client
{
	public class SupportPortableDeploySubstitutionParams // : StatementSubstitutionParameterOption
	{
	    private IDictionary<int, object> valuesByIndex;
	    private IDictionary<string, object> valuesByName;

	    public SupportPortableDeploySubstitutionParams() {
	    }

	    public SupportPortableDeploySubstitutionParams(IDictionary<string, object> valuesByName) {
	        this.valuesByName = valuesByName;
	    }

	    public SupportPortableDeploySubstitutionParams(int index, object value) {
	        valuesByIndex = new LinkedHashMap<int, object>();
	        valuesByIndex.Put(index, value);
	    }

	    public SupportPortableDeploySubstitutionParams(string name, object value) {
	        valuesByName = new LinkedHashMap<string, object>();
	        valuesByName.Put(name, value);
	    }

	    public SupportPortableDeploySubstitutionParams(int indexOne, object valueOne, int indexTwo, object valueTwo) {
	        valuesByIndex = new LinkedHashMap<int, object>();
	        valuesByIndex.Put(indexOne, valueOne);
	        valuesByIndex.Put(indexTwo, valueTwo);
	    }

	    public void SetStatementParameters(StatementSubstitutionParameterContext env) {
	        if (valuesByIndex != null) {
	            foreach (var entry in valuesByIndex) {
	                env.SetObject(entry.Key, entry.Value);
	            }
	        }
	        if (valuesByName != null) {
	            foreach (var entry in valuesByName) {
	                env.SetObject(entry.Key, entry.Value);
	            }
	        }
	    }

	    public SupportPortableDeploySubstitutionParams Add(int index, object value) {
	        if (valuesByName != null) {
	            throw new ArgumentException("Values-by-name exists");
	        }
	        if (valuesByIndex == null) {
	            valuesByIndex = new LinkedHashMap<int, object>();
	        }
	        if (valuesByIndex.ContainsKey(index)) {
	            throw new ArgumentException("Index appears multiple times");
	        }
	        valuesByIndex.Put(index, value);
	        return this;
	    }

	    public SupportPortableDeploySubstitutionParams Add(string name, object value) {
	        if (valuesByIndex != null) {
	            throw new ArgumentException("Values-by-name exists");
	        }
	        if (valuesByName == null) {
	            valuesByName = new LinkedHashMap<string,object>();
	        }
	        if (valuesByName.ContainsKey(name)) {
	            throw new ArgumentException("Name appears multiple times");
	        }
	        valuesByName.Put(name, value);
	        return this;
	    }
	}
} // end of namespace
