<?php
header("Access-Control-Allow-Origin: *");
header('Content-Type: application/json; charset=utf-8');
require_once(dirname(__FILE__) . "/../common.php");

$userid = 0;
if ($_SERVER["REQUEST_METHOD"] == "GET") {
	$username = $_REQUEST["username"];
	if ($username != "") {
		$userid = findUser($username);
	}
	else if (isAuthorized()) {
		$userid = $_SESSION['id'];
	}
}
else {
	$userid = authorize($_SERVER['PHP_AUTH_USER'], $_SERVER['PHP_AUTH_PW'], 1);
	if ($userid == 0) {
		header('WWW-Authenticate: Basic');
		http_response_code(401);
		echo "401 Unauthorized";
		die();
	}
}

$result = array();
$id = -1;
$db = getDB();
$game = $db->prepare("select id, shortname, gamename, versionnum from game g where g.shortname = :GAME");
$game->bindValue(':GAME', $_REQUEST["game"]);
if ($game->execute()) {
	if ($row = $game->fetch()) {
		$result["shortname"] = $row["ShortName"];
		$result["name"] = $row["GameName"];
		$result["version"] = (string)$row["VersionNum"];
		$id = $row["Id"];
	}
	$result["trophy"] = array();
	$trophies = $db->prepare("select t.trophyname, t.trophydescription, t.maxprogress, s.achieved, s.progress from trophy t left outer join userstat s on t.gameid = s.gameid and t.id = s.trophyid and s.userid = :USERID where t.gameid = :GAME");
	$trophies->bindValue(':USERID', $userid);
	$trophies->bindValue(':GAME', $id);
	if ($trophies->execute()) {
		$which = 0;
		while ($row = $trophies->fetch()) {
			$result["trophy"][$which] = array();
			$result["trophy"][$which]["trophy_name"] = $row["TrophyName"];
			$result["trophy"][$which]["trophy_desc"] = $row["TrophyDescription"];
			$result["trophy"][$which]["achieved"] = ($row["Achieved"] ? "1" : "0");
			$result["trophy"][$which]["progress"] = (string)((int)$row["Progress"]);
			$result["trophy"][$which]["max_progress"] = (string)$row["MaxProgress"];
			$which += 1;
		}
	}
}

echo json_encode($result);
?>
