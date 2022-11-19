<?php
header("Access-Control-Allow-Origin: *");
require_once(dirname(__FILE__) . "/../../common.php");

$userid = authorize($_SERVER['PHP_AUTH_USER'], $_SERVER['PHP_AUTH_PW']);
if ($userid == 0) {
	header('WWW-Authenticate: Basic');
	http_response_code(401);
	echo "401 Unauthorized";
	die();
}
$id = -1;
$db = getDB();
$game = $db->prepare("select id, shortname, gamename, versionnum from game g where g.shortname = :GAME");
$game->bindValue(':GAME', $_REQUEST["game"]);
if ($game->execute()) {
	if ($row = $game->fetch()) {
		$id = $row["Id"];
	}
	else {
		http_response_code(404);
		die();
	}
}
else {
	http_response_code(404);
	die();
}
$trophyid = -1;
$trophy = $db->prepare("select id from trophy where gameid = :GAME and trophyname = :TROPHY");
$trophy->bindValue(':GAME', $id);
$trophy->bindValue(':TROPHY', $_REQUEST["trophy"]);
if ($trophy->execute()) {
	if ($row = $trophy->fetch()) {
		$trophyid = $row["Id"];
	}
	else {
		http_response_code(404);
		die();
	}
}
else {
	http_response_code(404);
	die();
}

setStat($db, $id, $trophyid, $userid, 1, 0);
?>
