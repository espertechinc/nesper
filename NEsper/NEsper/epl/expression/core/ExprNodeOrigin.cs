///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.epl.expression.core
{
    public enum ExprNodeOrigin
    {
        SELECT,
        WHERE,
        GROUPBY,
        HAVING,
        METHODINVJOIN,
        DATABASEPOLL,
        CONTEXT,
        CONTEXTDISTINCT,
        CONTEXTCONDITION,
        VARIABLEASSIGN,
        DATAFLOW,
        DATAFLOWBEACON,
        DATAFLOWFILTER,
        UPDATEASSIGN,
        PLUGINSINGLEROWPARAM,
        AGGPARAM,
        OUTPUTLIMIT,
        DECLAREDEXPRPARAM,
        DECLAREDEXPRBODY,
        ALIASEXPRBODY,
        ORDERBY,
        SCRIPTPARAMS,
        FOLLOWEDBYMAX,
        PATTERNMATCHUNTILBOUNDS,
        PATTERNGUARD,
        PATTERNEVERYDISTINCT,
        PATTERNOBSERVER,
        DOTNODEPARAMETER,
        DOTNODE,
        CONTAINEDEVENT,
        CREATEWINDOWFILTER,
        CREATETABLECOLUMN,
        CREATEINDEXCOLUMN,
        CREATEINDEXPARAMETER,
        SUBQUERYSELECT,
        FILTER,
        FORCLAUSE,
        VIEWPARAMETER,
        MATCHRECOGDEFINE,
        MATCHRECOGMEASURE,
        MATCHRECOGPARTITION,
        MATCHRECOGINTERVAL,
        MATCHRECOGPATTERN,
        JOINON,
        MERGEMATCHCOND,
        MERGEMATCHWHERE,
        HINT
    }

    public static class ExprNodeOriginExtensions
    {
        public static string GetClauseName(this ExprNodeOrigin enumValue)
        {
            switch (enumValue)
            {
                case ExprNodeOrigin.SELECT:
                    return ("select-clause");
                case ExprNodeOrigin.WHERE:
                    return ("where-clause");
                case ExprNodeOrigin.GROUPBY:
                    return ("group-by-clause");
                case ExprNodeOrigin.HAVING:
                    return ("having-clause");
                case ExprNodeOrigin.METHODINVJOIN:
                    return ("from-clause method-invocation");
                case ExprNodeOrigin.DATABASEPOLL:
                    return ("from-clause database-access parameter");
                case ExprNodeOrigin.CONTEXT:
                    return ("context declaration");
                case ExprNodeOrigin.CONTEXTDISTINCT:
                    return ("context distinct-clause");
                case ExprNodeOrigin.CONTEXTCONDITION:
                    return ("context condition");
                case ExprNodeOrigin.VARIABLEASSIGN:
                    return ("variable-assignment");
                case ExprNodeOrigin.DATAFLOW:
                    return ("dataflow operator");
                case ExprNodeOrigin.DATAFLOWBEACON:
                    return ("beacon dataflow operator");
                case ExprNodeOrigin.DATAFLOWFILTER:
                    return ("filter dataflow operator");
                case ExprNodeOrigin.UPDATEASSIGN:
                    return ("update assignment");
                case ExprNodeOrigin.PLUGINSINGLEROWPARAM:
                    return ("single-row function parameter");
                case ExprNodeOrigin.AGGPARAM:
                    return ("aggregation function parameter");
                case ExprNodeOrigin.OUTPUTLIMIT:
                    return ("output limit");
                case ExprNodeOrigin.DECLAREDEXPRPARAM:
                    return ("declared expression parameter");
                case ExprNodeOrigin.DECLAREDEXPRBODY:
                    return ("declared expression body");
                case ExprNodeOrigin.ALIASEXPRBODY:
                    return ("alias expression body");
                case ExprNodeOrigin.ORDERBY:
                    return ("order-by-clause");
                case ExprNodeOrigin.SCRIPTPARAMS:
                    return ("script parameter");
                case ExprNodeOrigin.FOLLOWEDBYMAX:
                    return ("pattern followed-by max");
                case ExprNodeOrigin.PATTERNMATCHUNTILBOUNDS:
                    return ("pattern match-until bounds");
                case ExprNodeOrigin.PATTERNGUARD:
                    return ("pattern guard");
                case ExprNodeOrigin.PATTERNEVERYDISTINCT:
                    return ("pattern every-distinct");
                case ExprNodeOrigin.PATTERNOBSERVER:
                    return ("pattern observer");
                case ExprNodeOrigin.DOTNODEPARAMETER:
                    return ("method-chain parameter");
                case ExprNodeOrigin.DOTNODE:
                    return ("method-chain");
                case ExprNodeOrigin.CONTAINEDEVENT:
                    return ("contained-event");
                case ExprNodeOrigin.CREATEWINDOWFILTER:
                    return ("create-window filter");
                case ExprNodeOrigin.CREATETABLECOLUMN:
                    return ("table-column");
                case ExprNodeOrigin.CREATEINDEXCOLUMN:
                    return ("create-index index-column");
                case ExprNodeOrigin.CREATEINDEXPARAMETER:
                    return ("create-index index-parameter");
                case ExprNodeOrigin.SUBQUERYSELECT:
                    return ("subquery select-clause");
                case ExprNodeOrigin.FILTER:
                    return ("filter");
                case ExprNodeOrigin.FORCLAUSE:
                    return ("for-clause");
                case ExprNodeOrigin.VIEWPARAMETER:
                    return ("view parameter");
                case ExprNodeOrigin.MATCHRECOGDEFINE:
                    return ("match-recognize define");
                case ExprNodeOrigin.MATCHRECOGMEASURE:
                    return ("match-recognize measure");
                case ExprNodeOrigin.MATCHRECOGPARTITION:
                    return ("match-recognize partition");
                case ExprNodeOrigin.MATCHRECOGINTERVAL:
                    return ("match-recognize interval");
                case ExprNodeOrigin.MATCHRECOGPATTERN:
                    return ("match-recognize pattern");
                case ExprNodeOrigin.JOINON:
                    return ("on-clause join");
                case ExprNodeOrigin.MERGEMATCHCOND:
                    return ("match condition");
                case ExprNodeOrigin.MERGEMATCHWHERE:
                    return ("match where-clause");
                case ExprNodeOrigin.HINT:
                    return ("hint");
            }

            throw new ArgumentException("invalid value for enumValue", "enumValue");
        }
    }
} // end of namespace
