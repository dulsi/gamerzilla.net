# Bcryptuserdb

This program runs bcrypt on all passwords in the user database specified
on the command line. If it detects any password that appear to be
encrypted already, it will ask if you want to abort.

## How to run

Inside the utils/bcryptuserdb directory create a virtual python environment with:

```
python3 -m venv .
source ./bin/activate
```

Install the needed python packages with:

```
pip install -r requirements.txt
```

Run the program on your user db:

```
python bcryptuserdb /path/to/db/User.db
```
