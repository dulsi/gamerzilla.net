# Gamerzilla Python

## Development setup

Inside the python/src directory create a virtual python environment with:

```
python3 -m venv .
source ./bin/activate
```

Install the needed python packages with:

```
pip install flask flask_session flask_cors pillow
```

Set the environment variable FLASK_APP and run the python project in
flask:

```
export FLASK_APP=main.py
python3 -m flask run
```

For the frontend you simply need to go to frontend directory and run
these commands:

```
npm install
npm start
```
