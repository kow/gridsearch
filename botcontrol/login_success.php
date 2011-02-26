<?
session_start();
if(!session_is_registered(myusername)){
header("location:main_login.php");
}
else
{
header("location:index.php");
}
?>

<html>
<body>
Login Successful
</body>
</html>

