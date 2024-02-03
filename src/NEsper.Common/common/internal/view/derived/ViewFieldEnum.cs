///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

namespace com.espertech.esper.common.@internal.view.derived
{
    /// <summary>
    /// Enumerates the valid values for each view's public fields. The name of the field or property can be used
    /// to obtain values from the view rather than using the hardcoded String value for the field.
    /// </summary>
    public enum ViewFieldEnum
    {
        /// <summary> Count.</summary>
        UNIVARIATE_STATISTICS__DATAPOINTS,

        /// <summary> Sum.</summary>
        UNIVARIATE_STATISTICS__TOTAL,

        /// <summary> Average.</summary>
        UNIVARIATE_STATISTICS__AVERAGE,

        /// <summary> Standard dev population.</summary>
        UNIVARIATE_STATISTICS__STDDEVPA,

        /// <summary> Standard dev.</summary>
        UNIVARIATE_STATISTICS__STDDEV,

        /// <summary> Variance.</summary>
        UNIVARIATE_STATISTICS__VARIANCE,

        /// <summary> Weighted average.</summary>
        WEIGHTED_AVERAGE__AVERAGE,

        /// <summary> CorrelationStatistics.</summary>
        CORRELATION__CORRELATION,

        /// <summary> Slope.</summary>
        REGRESSION__SLOPE,

        /// <summary> Y-intercept.</summary>
        REGRESSION__YINTERCEPT,

        /// <summary> Count.</summary>
        SIZE_VIEW__SIZE,

        /// <summary>XAverage </summary>
        REGRESSION__XAVERAGE,

        /// <summary>XStandardDeviationPop </summary>
        REGRESSION__XSTANDARDDEVIATIONPOP,

        /// <summary>XStandardDeviationSample </summary>
        REGRESSION__XSTANDARDDEVIATIONSAMPLE,

        /// <summary>XSum </summary>
        REGRESSION__XSUM,

        /// <summary>XVariance </summary>
        REGRESSION__XVARIANCE,

        /// <summary>YAverage </summary>
        REGRESSION__YAVERAGE,

        /// <summary>YStandardDeviationPop </summary>
        REGRESSION__YSTANDARDDEVIATIONPOP,

        /// <summary>YStandardDeviationSample </summary>
        REGRESSION__YSTANDARDDEVIATIONSAMPLE,

        /// <summary>YSum </summary>
        REGRESSION__YSUM,

        /// <summary>YVariance </summary>
        REGRESSION__YVARIANCE,

        /// <summary>dataPoints </summary>
        REGRESSION__DATAPOINTS,

        /// <summary>n </summary>
        REGRESSION__N,

        /// <summary>sumX </summary>
        REGRESSION__SUMX,

        /// <summary>sumXSq </summary>
        REGRESSION__SUMXSQ,

        /// <summary>sumXY </summary>
        REGRESSION__SUMXY,

        /// <summary>sumY </summary>
        REGRESSION__SUMY,

        /// <summary>sumYSq </summary>
        REGRESSION__SUMYSQ
    }

    public static class ViewFieldEnumExtensions
    {
        public static string GetName(this ViewFieldEnum value)
        {
            switch (value) {
                case ViewFieldEnum.UNIVARIATE_STATISTICS__DATAPOINTS:
                    return "datapoints";

                case ViewFieldEnum.UNIVARIATE_STATISTICS__TOTAL:
                    return "total";

                case ViewFieldEnum.UNIVARIATE_STATISTICS__AVERAGE:
                    return "average";

                case ViewFieldEnum.UNIVARIATE_STATISTICS__STDDEVPA:
                    return "stddevpa";

                case ViewFieldEnum.UNIVARIATE_STATISTICS__STDDEV:
                    return "stddev";

                case ViewFieldEnum.UNIVARIATE_STATISTICS__VARIANCE:
                    return "variance";

                case ViewFieldEnum.WEIGHTED_AVERAGE__AVERAGE:
                    return "average";

                case ViewFieldEnum.CORRELATION__CORRELATION:
                    return "correlation";

                case ViewFieldEnum.REGRESSION__SLOPE:
                    return "slope";

                case ViewFieldEnum.REGRESSION__YINTERCEPT:
                    return "YIntercept";

                case ViewFieldEnum.SIZE_VIEW__SIZE:
                    return "size";

                case ViewFieldEnum.REGRESSION__XAVERAGE:
                    return "XAverage";

                case ViewFieldEnum.REGRESSION__XSTANDARDDEVIATIONPOP:
                    return "XStandardDeviationPop";

                case ViewFieldEnum.REGRESSION__XSTANDARDDEVIATIONSAMPLE:
                    return "XStandardDeviationSample";

                case ViewFieldEnum.REGRESSION__XSUM:
                    return "XSum";

                case ViewFieldEnum.REGRESSION__XVARIANCE:
                    return "XVariance";

                case ViewFieldEnum.REGRESSION__YAVERAGE:
                    return "YAverage";

                case ViewFieldEnum.REGRESSION__YSTANDARDDEVIATIONPOP:
                    return "YStandardDeviationPop";

                case ViewFieldEnum.REGRESSION__YSTANDARDDEVIATIONSAMPLE:
                    return "YStandardDeviationSample";

                case ViewFieldEnum.REGRESSION__YSUM:
                    return "YSum";

                case ViewFieldEnum.REGRESSION__YVARIANCE:
                    return "YVariance";

                case ViewFieldEnum.REGRESSION__DATAPOINTS:
                    return "dataPoints";

                case ViewFieldEnum.REGRESSION__N:
                    return "n";

                case ViewFieldEnum.REGRESSION__SUMX:
                    return "sumX";

                case ViewFieldEnum.REGRESSION__SUMXSQ:
                    return "sumXSq";

                case ViewFieldEnum.REGRESSION__SUMXY:
                    return "sumXY";

                case ViewFieldEnum.REGRESSION__SUMY:
                    return "sumY";

                case ViewFieldEnum.REGRESSION__SUMYSQ:
                    return "sumYSq";
            }

            throw new ArgumentException();
        }
    }
}