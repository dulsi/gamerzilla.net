<?php
header("Access-Control-Allow-Origin: *");
require_once(dirname(__FILE__) . "/../common.php");
header('Content-Type: application/json; charset=utf-8');

echo json_encode($registrationOptions['Allow']);
?>
