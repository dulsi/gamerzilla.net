# Test Suite for Backend

## Setup

Create sqlite databases with scripts in db. Where you place the database
depends on the backend.

```
sqlite Trophy.db
.read db/Trophy.sql
.quit
sqlite User.db
.read db/User.sql
.quit
```

Run the create user script.

```
sqlite User.db
.read test/create_user.sql
.quit
```

Startup the backend.

Set enviroment variable TESTURL to a base url to access the backend.
TESTURL defaults to http://localhost:5000 if it is not set.

## Run the test script.

```
./run_tests.sh
```
