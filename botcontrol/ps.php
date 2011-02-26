<?
session_start();
if(!session_is_registered(myusername)){
header("location:main_login.php");
return;
}


$pid = $_GET["pid"];

// logtail.php
$cmd = "ps u -p $pid";
exec("$cmd 2>&1", $output);
foreach($output as $outputline) {
echo ("$outputline\n");
}
?>

