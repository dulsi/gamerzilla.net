<?php
header("Access-Control-Allow-Origin: *");
header('Content-Type: application/json; charset=utf-8');
require_once(dirname(__FILE__) . "/common.php");

$result = array();

$db = getUserDB();
$admin = false;
if (isAuthorized()) {
	$admin = $_SESSION['admin'];
}
$query = "select * from user u where u.visible = 1 and u.approved = 1";
if ($admin) {
	$query = "select * from user u";
}
$games = $db->prepare($query);
if ($games->execute()) {
	$which = 0;
	while ($row = $games->fetch()) {
		$result[$which] = array();
		$result[$which]["id"] = $row["Id"];
		$result[$which]["userName"] = $row["UserName"];
		$result[$which]["password"] = "";
		if ($admin) {
			$result[$which]["admin"] = ($row["Admin"] == 1) ? true : false;
		}
		else {
			$result[$which]["admin"] = false;
		}
		$result[$which]["visible"] = ($row["Visible"] == 1) ? true : false;
		$result[$which]["approved"] = ($row["Approved"] == 1) ? true : false;
		$which += 1;
	}
}

echo json_encode($result);
?>
