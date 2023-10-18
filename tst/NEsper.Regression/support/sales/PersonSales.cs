///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.regressionlib.support.sales
{
    [Serializable]
    public class PersonSales
    {
        public PersonSales(
            IList<Person> persons,
            IList<Sale> sales)
        {
            Persons = persons;
            Sales = sales;
        }

        public IList<Person> Persons { get; }

        public IList<Sale> Sales { get; }

        public static PersonSales Make()
        {
            IList<Person> persons = new List<Person>();
            persons.Add(new Person("Jim", 19));
            persons.Add(new Person("Henry", 20));
            persons.Add(new Person("Peter", 50));
            persons.Add(new Person("Boris", 42));

            IList<Sale> sales = new List<Sale>();
            sales.Add(new Sale(persons[0], persons[1], 1000));
            sales.Add(new Sale(persons[2], persons[3], 5000));

            return new PersonSales(persons, sales);
        }
    }
} // end of namespace