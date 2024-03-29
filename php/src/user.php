<?php
header("Access-Control-Allow-Origin: *");
header('Content-Type: application/json; charset=utf-8');
require_once(dirname(__FILE__) . "/common.php");

$result = array();

$db = getUserDB();
$admin = false;
$userid = -1;
if (isAuthorized()) {
	$admin = $_SESSION['admin'];
	$userid = $_SESSION['id'];
}
$query = "select * from user u where (u.visible = 1 and u.approved = 1)";
if ($admin) {
	$query = "select * from user u";
}
else if ($userid != -1) {
	$query .= " or u.id = :USERID";
}
$games = $db->prepare($query);
if ((!$admin) && ($userid != -1)) {
	$games->bindValue(':USERID', $userid);
}
if ($games->execute()) {
	$which = 0;
	while ($row = $games->fetch()) {
		$result[$which] = array();
		$result[$which]["userName"] = $row["UserName"];
		$result[$which]["password"] = "";
		if ($admin) {
			$result[$which]["admin"] = ($row["Admin"] == 1) ? true : false;
			if ($row["Approved"] == 1)
				$result[$which]["canApprove"] = false;
			else
				$result[$which]["canApprove"] = true;
		}
		else {
			$result[$which]["admin"] = false;
			$result[$which]["canApprove"] = false;
		}
		$result[$which]["visible"] = ($row["Visible"] == 1) ? true : false;
		$result[$which]["approved"] = ($row["Approved"] == 1) ? true : false;
		$which += 1;
	}
}

echo json_encode($result);
?>
