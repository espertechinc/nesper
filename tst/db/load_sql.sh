#!/bin/bash

# Define variables
DB_NAME="esper"
DB_USER="esper"
SQL_FILE="/docker-entrypoint-initdb.d/create_testdb.sql"

# Check if PostgreSQL is running
if [ -z "$(pg_isready -h localhost -U $DB_USER)" ]; then
    echo "PostgreSQL is not running. Start your PostgreSQL container first."
    exit 1
fi

# Load SQL commands from foo.sql into the database
psql -h localhost -U $DB_USER -d $DB_NAME -f $SQL_FILE

echo "SQL commands from $SQL_FILE loaded into the $DB_NAME database."
