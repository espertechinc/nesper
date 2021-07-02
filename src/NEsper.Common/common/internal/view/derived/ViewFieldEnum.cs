///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
        UNIVARIATE_STATISTICS_DATAPOINTS,

        /// <summary> Sum.</summary>
        UNIVARIATE_STATISTICS_TOTAL,

        /// <summary> Average.</summary>
        UNIVARIATE_STATISTICS_AVERAGE,

        /// <summary> Standard dev population.</summary>
        UNIVARIATE_STATISTICS_STDDEVPA,

        /// <summary> Standard dev.</summary>
        UNIVARIATE_STATISTICS_STDDEV,

        /// <summary> Variance.</summary>
        UNIVARIATE_STATISTICS_VARIANCE,

        /// <summary> Weighted average.</summary>
        WEIGHTED_AVERAGE_AVERAGE,

        /// <summary> CorrelationStatistics.</summary>
        CORRELATION_CORRELATION,

        /// <summary> Slope.</summary>
        REGRESSION_SLOPE,

        /// <summary> Y-intercept.</summary>
        REGRESSION_YINTERCEPT,

        /// <summary> Count.</summary>
        SIZE_VIEW_SIZE,

        /// <summary>XAverage </summary>
        REGRESSION_XAVERAGE,

        /// <summary>XStandardDeviationPop </summary>
        REGRESSION_XSTANDARDDEVIATIONPOP,

        /// <summary>XStandardDeviationSample </summary>
        REGRESSION_XSTANDARDDEVIATIONSAMPLE,

        /// <summary>XSum </summary>
        REGRESSION_XSUM,

        /// <summary>XVariance </summary>
        REGRESSION_XVARIANCE,

        /// <summary>YAverage </summary>
        REGRESSION_YAVERAGE,

        /// <summary>YStandardDeviationPop </summary>
        REGRESSION_YSTANDARDDEVIATIONPOP,

        /// <summary>YStandardDeviationSample </summary>
        REGRESSION_YSTANDARDDEVIATIONSAMPLE,

        /// <summary>YSum </summary>
        REGRESSION_YSUM,

        /// <summary>YVariance </summary>
        REGRESSION_YVARIANCE,

        /// <summary>dataPoints </summary>
        REGRESSION_DATAPOINTS,

        /// <summary>n </summary>
        REGRESSION_N,

        /// <summary>sumX </summary>
        REGRESSION_SUMX,

        /// <summary>sumXSq </summary>
        REGRESSION_SUMXSQ,

        /// <summary>sumXY </summary>
        REGRESSION_SUMXY,

        /// <summary>sumY </summary>
        REGRESSION_SUMY,

        /// <summary>sumYSq </summary>
        REGRESSION_SUMYSQ
    }

    public static class ViewFieldEnumExtensions
    {
        public static string GetName(this ViewFieldEnum value)
        {
            switch (value) {
                case ViewFieldEnum.UNIVARIATE_STATISTICS_DATAPOINTS:
                    return ("datapoints");

                case ViewFieldEnum.UNIVARIATE_STATISTICS_TOTAL:
                    return ("total");

                case ViewFieldEnum.UNIVARIATE_STATISTICS_AVERAGE:
                    return ("average");

                case ViewFieldEnum.UNIVARIATE_STATISTICS_STDDEVPA:
                    return ("stddevpa");

                case ViewFieldEnum.UNIVARIATE_STATISTICS_STDDEV:
                    return ("stddev");

                case ViewFieldEnum.UNIVARIATE_STATISTICS_VARIANCE:
                    return ("variance");

                case ViewFieldEnum.WEIGHTED_AVERAGE_AVERAGE:
                    return ("average");

                case ViewFieldEnum.CORRELATION_CORRELATION:
                    return ("correlation");

                case ViewFieldEnum.REGRESSION_SLOPE:
                    return ("slope");

                case ViewFieldEnum.REGRESSION_YINTERCEPT:
                    return ("YIntercept");

                case ViewFieldEnum.SIZE_VIEW_SIZE:
                    return ("size");

                case ViewFieldEnum.REGRESSION_XAVERAGE:
                    return ("XAverage");

                case ViewFieldEnum.REGRESSION_XSTANDARDDEVIATIONPOP:
                    return ("XStandardDeviationPop");

                case ViewFieldEnum.REGRESSION_XSTANDARDDEVIATIONSAMPLE:
                    return ("XStandardDeviationSample");

                case ViewFieldEnum.REGRESSION_XSUM:
                    return ("XSum");

                case ViewFieldEnum.REGRESSION_XVARIANCE:
                    return ("XVariance");

                case ViewFieldEnum.REGRESSION_YAVERAGE:
                    return ("YAverage");

                case ViewFieldEnum.REGRESSION_YSTANDARDDEVIATIONPOP:
                    return ("YStandardDeviationPop");

                case ViewFieldEnum.REGRESSION_YSTANDARDDEVIATIONSAMPLE:
                    return ("YStandardDeviationSample");

                case ViewFieldEnum.REGRESSION_YSUM:
                    return ("YSum");

                case ViewFieldEnum.REGRESSION_YVARIANCE:
                    return ("YVariance");

                case ViewFieldEnum.REGRESSION_DATAPOINTS:
                    return ("dataPoints");

                case ViewFieldEnum.REGRESSION_N:
                    return ("n");

                case ViewFieldEnum.REGRESSION_SUMX:
                    return ("sumX");

                case ViewFieldEnum.REGRESSION_SUMXSQ:
                    return ("sumXSq");

                case ViewFieldEnum.REGRESSION_SUMXY:
                    return ("sumXY");

                case ViewFieldEnum.REGRESSION_SUMY:
                    return ("sumY");

                case ViewFieldEnum.REGRESSION_SUMYSQ:
                    return ("sumYSq");
            }

            throw new ArgumentException();
        }
    }
}