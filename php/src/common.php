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

function addImage($db, $id, $trophyid, $achieved, $data) {
	$gameAdd = $db->prepare("insert into image(gameid, trophyid, achieved, data) values (:GAME, :TROPHY, :ACHIEVED, :DATA)");
	$gameAdd->bindValue(':GAME', $id);
	$gameAdd->bindValue(':TROPHY', $trophyid);
	$gameAdd->bindValue(':ACHIEVED', $achieved);
	$gameAdd->bindValue(':DATA', $data);
	$gameAdd->execute();
}

function resizeImage($filename, $width, $height) {
	$image = imagecreatefrompng($filename);
	if ($image === false) {
		http_response_code(400);
		die();
	}
	$image = imagescale($image, $width, $height);
	if ($image === false) {
		http_response_code(400);
		die();
	}
	$stream = fopen('php://memory','r+');
	imagepng($image, $stream);
	rewind($stream);
	return stream_get_contents($stream);
}

function setStat($db, $id, $trophyid, $userid, $achieved, $progress) {
	$orig = $db->prepare("select * from userstat where gameid = :GAME and trophyid = :TROPHY and userid = :USERID");
	$orig->bindValue(':GAME', $id);
	$orig->bindValue(':TROPHY', $trophyid);
	$orig->bindValue(':USERID', $userid);
	if ($orig->execute()) {
		if ($row = $orig->fetch()) {
			$statUpd = $db->prepare("update userstat set achieved = :ACHIEVED, progress = :PROGRESS where id = :USERSTATID");
			$statUpd->bindValue(':USERSTATID', $row['Id']);
			$statUpd->bindValue(':ACHIEVED', ($achieved > $row['Achieved'] ? $achieved : $row['Achieved']));
			$statUpd->bindValue(':GAME', ($progress > $row['Progress'] ? $progress : $row['Progress']));
			$statUpd->execute();
		}
		else {
			$statAdd = $db->prepare("insert into userstat(gameid, trophyid, userid, achieved, progress) values (:GAME, :TROPHY, :USERID, :ACHIEVED, :PROGRESS)");
			$statAdd->bindValue(':GAME', $id);
			$statAdd->bindValue(':TROPHY', $trophyid);
			$statAdd->bindValue(':USERID', $userid);
			$statAdd->bindValue(':ACHIEVED', $achieved);
			$statAdd->bindValue(':PROGRESS', $progress);
			$statAdd->execute();
		}
	}
	else {
		die();
	}
}

?>
