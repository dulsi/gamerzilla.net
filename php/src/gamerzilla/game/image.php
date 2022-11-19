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

$image = imagecreatefrompng($_FILES['imagefile']['tmp_name']);
if ($image === false) {
	http_response_code(400);
	die();
}
$image = imagescale($image, 368, 172);
if ($image === false) {
	http_response_code(400);
	die();
}
$stream = fopen('php://memory','r+');
imagepng($image, $stream);
rewind($stream);

$gameAdd = $db->prepare("insert into image(gameid, trophyid, achieved, data) values (:GAME, -1, false, :DATA)");
$gameAdd->bindValue(':GAME', $id);
$gameAdd->bindValue(':DATA', stream_get_contents($stream));
$gameAdd->execute();
?>
