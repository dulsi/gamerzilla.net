<?php
function getDB() {
	return 	new PDO("sqlite:" . dirname(__FILE__) . "/db/Trophy.db");
}

function getUserDB() {
	return 	new PDO("sqlite:" . dirname(__FILE__) . "/db/User.db");
}

function isAuthorized() {
	session_start();
	if (isset($_SESSION['id']))
		return true;
	else
		return false;
}

function findUser($username) {
	// Check if admin user
	$userDb = getUserDB();
	$user = $userDb->prepare("select id,visible,approved from user u where u.username = :NAME");
	$user->bindValue(':NAME', $username);
	if ($user->execute()) {
		if ($row = $user->fetch()) {
			if ($row["Visible"] == 1 && $row["Approved"] == 1) {
				return $row["Id"];
			}
		}
	}
	return 0;
}

?>
