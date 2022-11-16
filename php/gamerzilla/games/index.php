<?php
header("Access-Control-Allow-Origin: *");
header('Content-Type: application/json; charset=utf-8');
require_once(dirname(__FILE__) . "/../../common.php");

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
else if (isAuthorized()) {
	$userid = $_SESSION['id'];
}
else {
	http_response_code(401);
	echo "401 Unauthorized";
	die();
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
$games = $db->prepare("select shortname, gamename, (select count(*) from userstat u2 where u2.achieved = 1 and g.id = u2.gameid and u2.userid = :USERID) as earned, (select count(*) from trophy t where g.id = t.gameid) as total_trophy, (select max(u2.id) from userstat u2 where u2.achieved = 1 and g.id = u2.gameid and u2.userid = :USERID) as sortfield from game g where g.id in (select gameid from userstat u where u.userid = :USERID) order by sortfield desc limit :LIMIT offset :OFFSET");
$games->bindValue(':USERID', $userid);
$games->bindValue(':LIMIT', $result["pageSize"]);
$games->bindValue(':OFFSET', $result["pageSize"] * $result["currentPage"]);
if ($games->execute()) {
	$which = 0;
	while ($row = $games->fetch()) {
		$result["games"][$which] = array();
		$result["games"][$which]["shortname"] = $row["ShortName"];
		$result["games"][$which]["name"] = $row["GameName"];
		$result["games"][$which]["earned"] = $row[2];
		$result["games"][$which]["total"] = $row[3];
		$which += 1;
	}
	if ($which >= $result["pageSize"]) {
	}
}

echo json_encode($result);
?>
