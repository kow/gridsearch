<?

session_start();
if(!session_is_registered(myusername)){
header("location:main_login.php");
return;
}

?>
<html>
<head>

<?
exec("/usr/local/bin/searchbot/start.sh > /dev/null &");
?>

<script>
location.href="index.php";
</script>
</head>
<body>
location.href="index.php";
</body>
</html>
