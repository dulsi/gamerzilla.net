<?php
header("Access-Control-Allow-Origin: *");
require_once(dirname(__FILE__) . "/../common.php");
if (!isAuthorized()) {
	http_response_code(401);
	echo "401 Unauthorized";
	die();
}
$db = getUserDB();
$username = $_REQUEST["username"];
if (!$_SESSION['admin']) {
	$id = $_SESSION['id'];
	$user = $db->prepare("select username from user u where u.id = :ID");
	$user->bindValue(':ID', $id);
	if (!$user->execute()) {
		http_response_code(401);
		echo "401 Unauthorized";
		die();
	}
	$row = $user->fetch();
	if ($username != $row['username']) {
		http_response_code(401);
		echo "401 Unauthorized";
		die();
	}
}
$visible = $_REQUEST['val'];

$user = $db->prepare("select * from user u where u.username = :NAME");
$user->bindValue(':NAME', $username);
if ($user->execute() && $user->fetch()) {
	$userUpd = $db->prepare("update user set visible=:VISIBLE where username = :NAME");
	$userUpd->bindValue(':NAME', $username);
	$userUpd->bindValue(':VISIBLE', $visible);
	$userUpd->execute();
	echo json_encode(true);
}
else {
	http_response_code(400);
	echo "400 Bad Request";
	die();
}
?>
