<?php
header("Access-Control-Allow-Origin: *");
require_once(dirname(__FILE__) . "/../common.php");

if (!$registrationOptions['Allow']) {
	http_response_code(400);
	echo "400 Bad Request";
	die();
}
$data = json_decode(file_get_contents('php://input'), true);
if ($data["username"] == "") {
	http_response_code(400);
	echo "400 Bad Request";
	die();
}
header('Content-Type: application/json; charset=utf-8');

$db = getUserDB();
$user = $db->prepare("select * from user u where u.username = :NAME");
$user->bindValue(':NAME', $data["username"]);
if ($user->execute() && $user->fetch()) {
	http_response_code(400);
	echo "400 Bad Request";
	die();
}
else {
	$userAdd = $db->prepare("insert into user(username, password, admin, visible, approved) values (:NAME, :PASSWORD, 0, 0, :APPROVED)");
	$userAdd->bindValue(':NAME', $data["username"]);
	$userAdd->bindValue(':PASSWORD', $data["password"]);
	$userAdd->bindValue(':APPROVED', ($registrationOptions['RequireApproval'] ? 0 : 1));
	$userAdd->execute();
}
$user = $db->prepare("select * from user u where u.id = :NAME");
$user->bindValue(':NAME', $data["username"]);
if ($user->execute()) {
	$row = $user->fetch();
	$result["id"] = $row["Id"];
	$result["userName"] = $row["UserName"];
	$result["password"] = "";
	$result["admin"] = ($row['Admin'] == 1) ? true : false;
	$result["visible"] = ($row["Visible"] == 1) ? true : false;
	$result["approved"] = ($row["Approved"] == 1) ? true : false;
}

echo json_encode($result);
?>
