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
curl -s -X POST $TESTURL/api/gamerzilla/trophy/set --user "test:test" -d 'game=platform&trophy=Jump' >/dev/null
curl -s -X POST $TESTURL/api/gamerzilla/games --user "test:test" > results/games2.json
jdiff results/games2.json expected/games2.json >diff/games2.diff
check_jdiff diff/games2.diff "Set Trophy Test: "
curl -s -X POST $TESTURL/api/gamerzilla/trophy/set/stat --user "test:test" -d 'game=platform&trophy=Collector&progress=20' >/dev/null
curl -s -X POST $TESTURL/api/gamerzilla/game --user "test:test" -d 'game=platform' >results/platform1.json
jdiff results/platform1.json expected/platform1.json >diff/platform1.diff
check_jdiff diff/platform1.diff "Set Trophy Stat Test: "
curl -s -F 'imagefile=@request/test.png' -F 'game=random' $TESTURL/api/gamerzilla/game/image --user "test:test" >/dev/null
curl -s -d 'game=random' $TESTURL/api/gamerzilla/game/image/show --user "test:test" >results/test.png
compare -metric PSNR  results/test.png request/test.png diff/show-diff.png 2>/dev/null
if [ $? -eq 0 ] ; then
     echo "Set/Show Game Image Test: Success";
else
     echo "Set/Show Game Image Test: Failed";
fi
curl -s -F 'falseimagefile=@request/false.png' -F 'trueimagefile=@request/true.png' -F 'game=random' -F 'trophy=Win Game' $TESTURL/api/gamerzilla/trophy/image --user "test:test" >/dev/null
curl -s -d 'game=random&trophy=Win Game&achieved=0' $TESTURL/api/gamerzilla/trophy/image/show --user "test:test" >results/false.png
compare -metric PSNR  results/false.png request/false.png diff/false-diff.png 2>/dev/null
if [ $? -eq 0 ] ; then
     echo "Set/Show False Trophy Image Test: Success";
else
     echo "Set/Show False Trophy Image Test: Failed";
fi
curl -s -d 'game=random&trophy=Win Game&achieved=1' $TESTURL/api/gamerzilla/trophy/image/show --user "test:test" >results/true.png
compare -metric PSNR  results/true.png request/true.png diff/true-diff.png 2>/dev/null
if [ $? -eq 0 ] ; then
     echo "Set/Show True Trophy Image Test: Success";
else
     echo "Set/Show True Trophy Image Test: Failed";
fi
