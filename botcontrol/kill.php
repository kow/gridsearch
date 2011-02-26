<?

session_start();
if(!session_is_registered(myusername)){
header("location:main_login.php");
return;
}


$id = $_GET["pid"];
$index = $_GET["index"];
system("kill $id");
system("rm /usr/local/bin/searchbot/lock.$index");
system("rm /usr/local/bin/searchbot/log.$index");



?>

<html>
<head>
<script>
location.href="index.php";
</script>
</head>
<body>
</body>
</html>

