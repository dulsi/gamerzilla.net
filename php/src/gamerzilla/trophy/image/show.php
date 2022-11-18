<?php
header("Access-Control-Allow-Origin: *");
require_once(dirname(__FILE__) . "/../../../common.php");

$db = getDB();
$stmt = $db->prepare("select data from image i, game g, trophy t where i.gameid = g.id and g.shortname = :NAME and i.trophyid = t.id and g.id = t.gameid and t.trophyname = :TROPHY and i.achieved = :ACHIEVED");
$stmt->bindValue(':NAME', $_REQUEST["game"]);
$stmt->bindValue(':TROPHY', $_REQUEST["trophy"]);
$stmt->bindValue(':ACHIEVED', $_REQUEST["achieved"]);
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
