<?php
header("Access-Control-Allow-Origin: *");
require_once(dirname(__FILE__) . "/../../../common.php");

$db = getDB();
$stmt = $db->prepare("select data from image, game where image.gameid = game.id and game.shortname = :NAME and image.trophyid = -1");
$stmt->bindValue(':NAME', $_REQUEST["game"]);
if ($stmt->execute()) {
	$stmt->bindColumn(1, $lob, PDO::PARAM_LOB);
	$stmt->fetch(PDO::FETCH_BOUND);
	header('Content-Type: image/png');
	if (version_compare(PHP_VERSION, '7.0.0') >= 0) {
		fpassthru($lob);
	}
	else {
		echo $lob;
	}
}
else
{
	http_response_code(404);
	echo "404 Not Found";
}
?>
