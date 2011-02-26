
<?
/*
session_start();
if(!session_is_registered(myusername)){
header("location:main_login.php");
return;
}
*/


$id = $_GET["id"];

// logtail.php
$cmd = "tail -50 /usr/local/bin/searchbot/log.$id";
exec("$cmd 2>&1", $output);
foreach($output as $outputline) {
echo ("$outputline\n");
}
?>

