///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.agg.core
{
	public class AggregationClassNames
	{
	    private const string CLASSNAME_AGGREGATIONSERVICEFACTORYPROVIDER = "AggFactoryProvider";
	    private const string CLASSNAME_AGGREGATIONSERVICEFACTORY = "AggFactory";
	    private const string CLASSNAME_AGGREGATIONSERVICE = "AggSvc";
	    private const string CLASSNAME_AGGREGATIONROW_TOP = "AggRowTop";
	    private const string CLASSNAME_AGGREGATIONROW_LVL = "AggRowLvl";
	    private const string CLASSNAME_AGGREGATIONROWFACTORY = "AggRowFactoryTop";
	    private const string CLASSNAME_AGGREGATIONROWSERDE = "AggRowSerdeTop";
	    private const string CLASSNAME_AGGREGATIONROWSERDE_LVL = "AggRowSerdeLvl";
	    private const string CLASSNAME_AGGREGATIONROWFACTORY_LVL = "AggRowFactoryLvl";

	    private readonly string _optionalPostfix;
	    private string _rowTop = CLASSNAME_AGGREGATIONROW_TOP;
	    private string _rowFactory = CLASSNAME_AGGREGATIONROWFACTORY;
	    private string _rowSerde = CLASSNAME_AGGREGATIONROWSERDE;
	    private string _provider = CLASSNAME_AGGREGATIONSERVICEFACTORYPROVIDER;
	    private string _service = CLASSNAME_AGGREGATIONSERVICE;
	    private string _serviceFactory = CLASSNAME_AGGREGATIONSERVICEFACTORY;

	    public AggregationClassNames() : this(null) {
	    }

	    public AggregationClassNames(string optionalPostfix) {
	        _optionalPostfix = optionalPostfix;
	        if (optionalPostfix != null) {
	            _rowTop += optionalPostfix;
	            _rowFactory += optionalPostfix;
	            _rowSerde += optionalPostfix;
	            _provider += optionalPostfix;
	            _service += optionalPostfix;
	            _serviceFactory += optionalPostfix;
	        }
	    }

	    public string RowTop => _rowTop;

	    public string RowFactoryTop => _rowFactory;

	    public string RowSerdeTop => _rowSerde;

	    public string GetRowPerLevel(int level) {
	        string name = CLASSNAME_AGGREGATIONROW_LVL + "_" + level;
	        if (_optionalPostfix != null) {
	            name += _optionalPostfix;
	        }
	        return name;
	    }

	    public string GetRowSerdePerLevel(int level) {
	        string name = CLASSNAME_AGGREGATIONROWSERDE_LVL + "_" + level;
	        if (_optionalPostfix != null) {
	            name += _optionalPostfix;
	        }
	        return name;
	    }

	    public string GetRowFactoryPerLevel(int level) {
	        string name = CLASSNAME_AGGREGATIONROWFACTORY_LVL + "_" + level;
	        if (_optionalPostfix != null) {
	            name += _optionalPostfix;
	        }
	        return name;
	    }

	    public string Provider => _provider;

	    public string Service => _service;

	    public string ServiceFactory => _serviceFactory;
	}
} // end of namespace
