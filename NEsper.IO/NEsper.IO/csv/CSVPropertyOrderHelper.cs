using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esperio.csv
{
    /// <summary>
    /// A utility for resolving property order information based on a
    /// propertyTypes map and the first record of a CSV file (which
    /// might represent the title row).
    /// </summary>

    public class CSVPropertyOrderHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Resolve the order of the properties that appear in the CSV file,from the first row of the CSV file.
        /// </summary>
        /// <param name="firstRow">the first record of the CSV file</param>
        /// <param name="propertyTypes">describes the event to send into the EPRuntime</param>
        /// <returns>
        /// the property names in the order in which they occur in the file
        /// </returns>

        public static String[] ResolvePropertyOrder(String[] firstRow, IDictionary<String, Object> propertyTypes)
        {
            Log.Debug(".ResolvePropertyOrder firstRow==" + firstRow.Render());
            String[] result = null;

            if (IsValidTitleRow(firstRow, propertyTypes))
            {
                result = firstRow;
                Log.Debug(".ResolvePropertyOrder using valid title row, propertyOrder==" + result.Render());
            }
            else
            {
                throw new EPException("Cannot resolve the order of properties in the CSV file");
            }

            return result;
        }

        private static bool IsValidTitleRow(String[] row, IDictionary<String, Object> propertyTypes)
        {
            if (propertyTypes == null)
            {
                return true;
            }
            else
            {
                return IsValidRowLength(row, propertyTypes) && EachPropertyNameRepresented(row, propertyTypes);
            }
        }

        private static bool EachPropertyNameRepresented(String[] row, IDictionary<String, Object> propertyTypes)
        {
            ICollection<String> rowSet = new HashSet<String>(row);
            return propertyTypes.Keys.All(rowSet.Contains);
        }

        private static bool IsValidRowLength(String[] row, IDictionary<String, Object> propertyTypes)
        {
            Log.Debug(".IsValidRowLength");
            if (row == null)
            {
                return false;
            }

            // same row size, or with timestamp column added, or longer due to flatten nested properties
            return
                (row.Length == propertyTypes.Count) ||
                (row.Length == propertyTypes.Count + 1) ||
                (row.Length > propertyTypes.Count);
        }
    }
}