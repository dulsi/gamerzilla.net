#!/bin/bash

check_jdiff() {
 local str="{}"
 if [[ $(< $1) != $str ]]; then
     echo $2;
     return 1;
    else
     return 0;
 fi
}

if [ -z $TESTURL ]; then export TESTURL=http://localhost:5000 ; fi
rm -Rf results diff
mkdir results
mkdir diff
curl -s $TESTURL/api/user > results/user.json
jdiff results/user.json expected/user.json >diff/user.diff
check_jdiff diff/user.diff "Failed user test"
curl -s -X POST -d @request/add.post $TESTURL/api/gamerzilla/game/add --user "test:test" >/dev/null
curl -s -X POST -d @request/add2.post $TESTURL/api/gamerzilla/game/add --user "test:test" >/dev/null
curl -s $TESTURL/api/user > results/games1.json
jdiff results/games1.json expected/games1.json >diff/games1.diff
check_jdiff diff/user.diff "Failed add game test"
