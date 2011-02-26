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

$id = $_GET["id"];

?>

<script type="text/javascript" src="js/ajax.js"> </script>
<script type="text/javascript" src="js/logtail.js"> </script>
<link type="text/css" rel="stylesheet" href="style.css" media="all">
</head>
<body onLoad="setID(<? echo "$id" ?>); getLog('start');">


<div id="header"><h1>Search bot command and control</div>

<div id="menu">
<b>Main Menu</b>
<a href="index.php">Main menu</a>
<a href="logout.php">Logout</a>


</div>




<div id="wrapper">


<div id="log" style="border:solid 1px #dddddd; margin-left:25px; font-size:9px;
padding-left:5px; padding-right:10px; padding-top:10px; padding-bottom:20px;
margin-top:10px; margin-bottom:10px; width:90%; text-align:left; height:80%; overflow:auto;" >
This is the Log Viewer. To begin viewing the log live in this window, click Start Viewer. To stop the window refreshes, click Pause Viewer.
</div>
</div>
</body>
</html>


