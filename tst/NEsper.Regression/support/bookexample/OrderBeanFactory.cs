///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.regressionlib.support.bookexample
{
    public class OrderBeanFactory
    {
        public static OrderBean MakeEventOne()
        {
            var order = new OrderWithItems(
                "PO200901",
                new[] {
                    new OrderItem("A001", "10020", 10, 11.95),
                    new OrderItem("A002", "10021", 25, 7.50),
                    new OrderItem("A003", "10020", 30, 10)
                });
            return new OrderBean(order, GetBookDesc(), Array.Empty<GameDesc>());
        }

        public static OrderBean MakeEventTwo()
        {
            var order = new OrderWithItems(
                "PO200902",
                new[] {
                    new OrderItem("B001", "10022", 5, 99.50)
                });

            return new OrderBean(order, GetBookDesc(), Array.Empty<GameDesc>());
        }

        public static OrderBean MakeEventThree()
        {
            var order = new OrderWithItems(
                "PO200903",
                new[] {
                    new OrderItem("C001", "10025", 52, 99.50),
                    new OrderItem("C001", "10024", 51, 41.50),
                    new OrderItem("C001", "10021", 50, 30.50)
                });

            return new OrderBean(
                order,
                GetBookDesc(),
                new[] {
                    new GameDesc(
                        "GA01",
                        "Castlevania",
                        "Eidos",
                        new[] {
                            new Review(100, "best game ever"),
                            new Review(101, "good platformer")
                        })
                });
        }

        public static OrderBean MakeEventFour()
        {
            var order = new OrderWithItems(
                "PO200904",
                Array.Empty<OrderItem>());
            return new OrderBean(
                order,
                new[] {
                    new BookDesc(
                        "10031",
                        "Foundation 2",
                        "Isaac Asimov",
                        15.00d,
                        new[] {
                            new Review(201, "great book")
                        }),
                    new BookDesc(
                        "10032", "Red Planet", "Robert A Heinlein", 13.00d, Array.Empty<Review>())
                },
                Array.Empty<GameDesc>());
        }

        private static BookDesc[] GetBookDesc()
        {
            return new[] {
                new BookDesc(
                    "10020",
                    "Enders Game",
                    "Orson Scott Card",
                    24.00d,
                    new[] {
                        new Review(1, "best book ever"),
                        new Review(2, "good science fiction")
                    }),
                new BookDesc(
                    "10021",
                    "Foundation 1",
                    "Isaac Asimov",
                    35.00d,
                    new[] {
                        new Review(10, "great book")
                    }),
                new BookDesc(
                    "10022",
                    "Stranger in a Strange Land",
                    "Robert A Heinlein",
                    27.00d,
                    Array.Empty<Review>())
            };
        }
    }
} // end of namespace