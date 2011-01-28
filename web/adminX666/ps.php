<?

$pid = $_GET["pid"];

// logtail.php
$cmd = "ps u -p $pid";
exec("$cmd 2>&1", $output);
foreach($output as $outputline) {
echo ("$outputline\n");
}
?>

