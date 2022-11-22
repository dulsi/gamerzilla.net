<?php
header("Access-Control-Allow-Origin: *");
require_once(dirname(__FILE__) . "/../common.php");
if (!isAuthorized()) {
	http_response_code(401);
	echo "401 Unauthorized";
	die();
}
if (!$_SESSION['admin']) {
	http_response_code(401);
	echo "401 Unauthorized";
	die();
}

$db = getUserDB();
$user = $db->prepare("select * from user u where u.username = :NAME");
$user->bindValue(':NAME', $username);
if ($user->execute() && $user->fetch()) {
	$userUpd = $db->prepare("update user set approved=1 where username = :NAME");
	$userUpd->bindValue(':NAME', $username);
	$userAdd->execute();
}
else {
	http_response_code(400);
	echo "400 Bad Request";
	die();
}
?>
