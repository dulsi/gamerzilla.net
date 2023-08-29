<?php
header("Access-Control-Allow-Origin: *");
require_once(dirname(__FILE__) . "/../common.php");
session_start();
$_SESSION = array();

echo "OK";
?>
