<?php
header("Access-Control-Allow-Origin: *");
header('Content-Type: application/json; charset=utf-8');
require_once(dirname(__FILE__) . "/../common.php");

$full = 0;
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
if ($userid == 0) {
	$full = 1;
	$userid = authorize($_SERVER['PHP_AUTH_USER'], $_SERVER['PHP_AUTH_PW'], 1);
	if ($userid == 0) {
		header('WWW-Authenticate: Basic');
		http_response_code(401);
		echo "401 Unauthorized";
		die();
	}
}

$result = array();
$result["currentPage"] = (int)$_REQUEST["currentpage"];
$result["pageSize"] = (int)$_REQUEST["pagesize"];
if ($result["pageSize"] == 0) {
	$result["pageSize"] = 20;
}
$result["totalPages"] = $result["currentPage"] + 1; // Temporary guess
$result["games"] = array();

$db = getDB();
$games = null;
if ($full == 1) {
	$games = $db->prepare("select shortname, gamename, (select count(*) from userstat u2 where u2.achieved = 1 and g.id = u2.gameid and u2.userid = :USERID) as earned, (select count(*) from trophy t where g.id = t.gameid) as total_trophy, (select max(u2.id) from userstat u2 where u2.achieved = 1 and g.id = u2.gameid and u2.userid = :USERID) as sortfield from game g where g.id in (select gameid from userstat u where u.userid = :USERID) order by sortfield desc");
	$games->bindValue(':USERID', $userid);
}
else
{
	$games = $db->prepare("select shortname, gamename, (select count(*) from userstat u2 where u2.achieved = 1 and g.id = u2.gameid and u2.userid = :USERID) as earned, (select count(*) from trophy t where g.id = t.gameid) as total_trophy, (select max(u2.id) from userstat u2 where u2.achieved = 1 and g.id = u2.gameid and u2.userid = :USERID) as sortfield from game g where g.id in (select gameid from userstat u where u.userid = :USERID) order by sortfield desc limit :LIMIT offset :OFFSET");
	$games->bindValue(':USERID', $userid);
	$games->bindValue(':LIMIT', $result["pageSize"]);
	$games->bindValue(':OFFSET', $result["pageSize"] * $result["currentPage"]);
}
if ($games->execute()) {
	$which = 0;
	while ($row = $games->fetch()) {
		$result["games"][$which] = array();
		$result["games"][$which]["shortname"] = $row["ShortName"];
		$result["games"][$which]["name"] = $row["GameName"];
		$result["games"][$which]["earned"] = (string)$row[2];
		$result["games"][$which]["total"] = (string)$row[3];
		$which += 1;
	}
	if ($which >= $result["pageSize"]) {
	}
}

if ($full == 1) {
	echo json_encode($result["games"]);
}
else {
	echo json_encode($result);
}
?>
