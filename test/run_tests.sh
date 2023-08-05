#!/bin/bash

check_jdiff() {
 local str="{}"
 if [[ $(< $1) != $str ]]; then
     echo $2 "Failed";
     return 1;
    else
     echo $2 "Success";
     return 0;
 fi
}

if [ -z $TESTURL ]; then export TESTURL=http://localhost:5000 ; fi
rm -Rf results diff
mkdir results
mkdir diff
curl -s $TESTURL/api/user > results/user.json
jdiff results/user.json expected/user.json >diff/user.diff
check_jdiff diff/user.diff "User Test:"
curl -s -X POST -d @request/add.post $TESTURL/api/gamerzilla/game/add --user "test:test" >/dev/null
curl -s -X POST -d @request/add2.post $TESTURL/api/gamerzilla/game/add --user "test:test" >/dev/null
curl -s -X POST $TESTURL/api/gamerzilla/games --user "test:test" > results/games1.json
jdiff results/games1.json expected/games1.json >diff/games1.diff
check_jdiff diff/games1.diff "Add Game Test: "
curl -s -X POST http://localhost:5000/api/gamerzilla/trophy/set --user "test:test" -d 'game=platform&trophy=Jump' >/dev/null
curl -s -X POST $TESTURL/api/gamerzilla/games --user "test:test" > results/games2.json
jdiff results/games2.json expected/games2.json >diff/games2.diff
check_jdiff diff/games2.diff "Set Trophy Test: "
