<?php
header("Access-Control-Allow-Origin: *");
require_once(dirname(__FILE__) . "/../../common.php");
if (!isAuthorized()) {
	http_response_code(401);
	echo "401 Unauthorized";
	die();
}
header('Content-Type: application/json; charset=utf-8');
?>
