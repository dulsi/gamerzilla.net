<?php
header("Access-Control-Allow-Origin: *");
header('Content-Type: application/json; charset=utf-8');
require_once(dirname(__FILE__) . "/../../common.php");

$userid = authorize($_SERVER['PHP_AUTH_USER'], $_SERVER['PHP_AUTH_PW'], 1);
if ($userid == 0) {
	header('WWW-Authenticate: Basic');
	http_response_code(401);
	echo "401 Unauthorized";
	die();
}

$gameInfo = json_decode($_REQUEST["game"], true);

$result = array();
$id = -1;
$db = getDB();
$game = $db->prepare("select id, shortname, gamename, versionnum from game g where g.shortname = :GAME");
$game->bindValue(':GAME', $gameInfo["shortname"]);
$authority = 1;
if ($game->execute()) {
	if ($row = $game->fetch()) {
		$id = $row["Id"];
		if (((int)$gameInfo["version"]) > $row["VersionNum"]) {
			$gameUpd = $db->prepare("update game set versionnum = :VERSION where id = :ID");
			$gameUpd->bindValue(':ID', $gameid);
			$gameUpd->bindValue(':VERSION', $gameInfo["version"]);
			$gameUpd->execute();
			$authority = 2;
		}
		else if (((int)$gameInfo["version"]) < $row["VersionNum"]) {
			$gameInfo["version"] = (string)$row["VersionNum"];
		}
	}
	else {
		$gameAdd = $db->prepare("insert into game(shortname, gamename, versionnum) values (:SHORT, :NAME, :VERSION)");
		$gameAdd->bindValue(':SHORT', $gameInfo["shortname"]);
		$gameAdd->bindValue(':NAME', $gameInfo["name"]);
		$gameAdd->bindValue(':VERSION', $gameInfo["version"]);
		$gameAdd->execute();
		$game = $db->prepare("select id, shortname, gamename, versionnum from game g where g.shortname = :GAME");
		$game->bindValue(':GAME', $gameInfo["shortname"]);
		if ($game->execute()) {
			if ($row = $game->fetch()) {
				$id = $row["Id"];
			}
			else {
				die();
			}
		}
	}
	$used = array();
	$trophies = $db->prepare("select t.id, t.trophyname, t.trophydescription, t.maxprogress, s.achieved, s.progress from trophy t left outer join userstat s on t.gameid = s.gameid and t.id = s.trophyid and s.userid = :USERID where t.gameid = :GAME");
	$trophies->bindValue(':USERID', $userid);
	$trophies->bindValue(':GAME', $id);
	if ($trophies->execute()) {
		$which = 0;
		while ($row = $trophies->fetch()) {
			foreach ($gameInfo["trophy"] as &$found) {
				if ($found["trophy_name"] == $row["TrophyName"]) {
					array_push($used, $found["trophy_name"]);
					if ($row["TrophyDescription"] != $found["trophy_desc"]) {
						if ($authority == 1) {
							$found["trophy_desc"] = $row["TrophyDescription"];
							$found["max_progress"] = $row["MaxProgress"];
						}
						else {
							$trophyUpd = $db->prepare("update trophy set trophydescription = :DESC, maxprogress = :MAX where id = :ID");
							$trophyUpd->bindValue(':DESC', $found["trophy_desc"]);
							$trophyUpd->bindValue(':MAX', $found["max_progress"]);
							$trophyUpd->bindValue(':ID', $row["Id"]);
							$trophyUpd->execute();
						}
					}
					if ((array_key_exists("achieved", $found) && $found["achieved"] != "0") || (array_key_exists("progress", $found) && $found["progress"] != "0")) {
						if (($row["Achieved"] === null) && ($row["Progress"] == null)) {
							addUserStat($db, $id, $userid, $row["Id"], $found["achieved"], $found["progress"]);
						}
						else {
							updateUserStat($db, $id, $userid, $row["Id"], $found["achieved"], $found["progress"]);
						}
					}
				}
			}
			if (!in_array($row["TrophyName"], $used)) {
				$newTrophy = array();
				$newTrophy["trophy_name"] = $row["TrophyName"];
				$newTrophy["trophy_desc"] = $row["TrophyDescription"];
				$newTrophy["achieved"] = ($row["Achieved"] ? "1" : "0");
				$newTrophy["progress"] = (string)((int)$row["Progress"]);
				$newTrophy["max_progress"] = (string)$row["MaxProgress"];
				array_add($gameInfo["trophy"], $newTrophy);
			}
		}
		foreach ($gameInfo["trophy"] as &$found) {
			if (!in_array($found["trophy_name"], $used)) {
				$trophyAdd = $db->prepare("insert into trophy(gameid, trophyname, trophydescription, maxprogress) values (:GAME, :NAME, :DESC, :MAX)");
				$trophyAdd->bindValue(':GAME', $id);
				$trophyAdd->bindValue(':NAME', $found["trophy_name"]);
				$trophyAdd->bindValue(':DESC', $found["trophy_desc"]);
				$trophyAdd->bindValue(':MAX', $found["max_progress"]);
				$trophyAdd->execute();
				if ((array_key_exists("achieved", $found) && $found["achieved"] != "0") || (array_key_exists("progress", $found) && $found["progress"] != "0")) {
					$trophy = $db->prepare("select id from trophy where gameid = :GAME and trophyname = :NAME");
					$trophy->bindValue(':GAME', $id);
					$trophy->bindValue(':NAME', $found["trophy_name"]);
					if ($trophy->execute()) {
						if ($row = $trophy->fetch()) {
							addUserStat($db, $id, $userid, $row["Id"], $found["achieved"], $found["progress"]);
						}
						else {
							die();
						}
					}
				}
			}
		}
	}
}

echo json_encode($gameInfo);
?>
