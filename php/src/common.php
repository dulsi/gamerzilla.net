<?php
function getDB() {
	return 	new PDO("sqlite:" . dirname(__FILE__) . "/../db/Trophy.db");
}

function getUserDB() {
	return 	new PDO("sqlite:" . dirname(__FILE__) . "/../db/User.db");
}

function isAuthorized() {
	session_start();
	if (isset($_SESSION['id']))
		return true;
	else
		return false;
}

function authorize($username, $password) {
	$userDb = getUserDB();
	$user = $userDb->prepare("select id from user u where u.username = :NAME and u.password = :PASSWORD and u.approved = 1");
	$user->bindValue(':NAME', $username);
	$user->bindValue(':PASSWORD', $password);
	if ($user->execute()) {
		if ($row = $user->fetch()) {
			return $row["Id"];
		}
	}
	return 0;
}

function findUser($username) {
	// Check if admin user
	$userDb = getUserDB();
	$user = $userDb->prepare("select id,visible,approved from user u where u.username = :NAME");
	$user->bindValue(':NAME', $username);
	if ($user->execute()) {
		if ($row = $user->fetch()) {
			if ($row["Visible"] == 1 && $row["Approved"] == 1) {
				return $row["Id"];
			}
		}
	}
	return 0;
}

function addUserStat($db, $gameid, $userid, $trophyid, $achieved, $progress) {
	$statAdd = $db->prepare("insert into userstat(gameid, userid, trophyid, achieved, progress) values (:GAME, :USERID, :TROPHY, :ACHIEVED, :PROGRESS)");
	$statAdd->bindValue(':GAME', $gameid);
	$statAdd->bindValue(':USERID', $userid);
	$statAdd->bindValue(':TROPHY', $trophyid);
	$statAdd->bindValue(':ACHIEVED', $achieved);
	$statAdd->bindValue(':PROGRESS', $progress);
	$statAdd->execute();
}

function updateUserStat($db, $gameid, $userid, $trophyid, $achieved, $progress) {
	$statUpd = $db->prepare("update userstat set achieved = :ACHIEVED, progress = :PROGRESS WHERE gameid = :GAME and userid = :USERID and trophyid = :TROPHY");
	$statUpd->bindValue(':GAME', $gameid);
	$statUpd->bindValue(':USERID', $userid);
	$statUpd->bindValue(':TROPHY', $trophyid);
	$statUpd->bindValue(':ACHIEVED', $achieved);
	$statUpd->bindValue(':PROGRESS', $progress);
	$statUpd->execute();
}

?>