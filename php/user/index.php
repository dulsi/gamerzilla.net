<?php
header("Access-Control-Allow-Origin: *");
header('Content-Type: application/json; charset=utf-8');
require_once(dirname(__FILE__) . "/../common.php");

$result = array();

$db = getUserDB();
$games = $db->prepare("select * from user u where u.visible = 1 and u.approved = 1");
if ($games->execute()) {
	$which = 0;
	while ($row = $games->fetch()) {
		$result[$which] = array();
		$result[$which]["id"] = $row["Id"];
		$result[$which]["userName"] = $row["UserName"];
		$result[$which]["password"] = "";
		$result[$which]["admin"] = false;
		$result[$which]["visible"] = ($row["Visible"] == 1) ? true : false;
		$result[$which]["approved"] = ($row["Approved"] == 1) ? true : false;
		$which += 1;
	}
}

echo json_encode($result);
?>
