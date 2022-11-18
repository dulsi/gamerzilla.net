<?php
header("Access-Control-Allow-Origin: *");
require_once(dirname(__FILE__) . "/../common.php");
$data = json_decode(file_get_contents('php://input'), true);
$userid = authorize($data["username"], $data["password"]);
if ($userid == 0) {
	http_response_code(401);
	echo "401 Unauthorized";
	die();
}
header('Content-Type: application/json; charset=utf-8');
session_start();
$_SESSION['id'] = $userid;

$db = getUserDB();
$user = $db->prepare("select * from user u where u.id = :USERID");
$user->bindValue(':USERID', $_SESSION['id']);
if ($user->execute()) {
	$row = $user->fetch();
	$result["id"] = $row["Id"];
	$result["userName"] = $row["UserName"];
	$result["password"] = "";
	$result["admin"] = false;
	$result["visible"] = ($row["Visible"] == 1) ? true : false;
	$result["approved"] = ($row["Approved"] == 1) ? true : false;
}

echo json_encode($result);
?>
