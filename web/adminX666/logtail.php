<?

$id = $_GET["id"];

// logtail.php
$cmd = "tail -50 /home/robin/gridspidergit/git/gridsearch/trunk/log.$id";
exec("$cmd 2>&1", $output);
foreach($output as $outputline) {
echo ("$outputline\n");
}
?>

