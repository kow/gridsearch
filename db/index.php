<?php

$username="";
$password="";
$database="spider";
$type=$_GET["type"];
$search=$_GET["query"];

?>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
<title>Metaverse search</title>
<link rel="stylesheet" type="text/css" href="style.css" />
</head>

<body>

<center>
<?

mysql_connect('localhost',$username,$password);
mysql_select_db($database) or die( "Unable to select database");

$query="SELECT COUNT(*) as objects from Object";
$result=mysql_query($query) or die(mysql_error());
$row = mysql_fetch_assoc($result);
echo "Objects ".($row['objects']);

$query="SELECT COUNT(*) as objects from Region";
$result=mysql_query($query) or die(mysql_error());
$row = mysql_fetch_assoc($result);
echo " -- Regions ".($row['objects']);

$query="SELECT COUNT(*) as objects from Agent";
$result=mysql_query($query) or die(mysql_error());
$row = mysql_fetch_assoc($result);
echo " -- Agents ".($row['objects']);

$query="SELECT COUNT(*) as objects from Parcel";
$result=mysql_query($query) or die(mysql_error());
$row = mysql_fetch_assoc($result);
echo " -- Parcels ".($row['objects']);

?>

</center>

<center><image src="logo.jpg" /></center>

<form action="/" method="GET">
<center>
<p><label> Metaverse search</label></p>
<p> <input type="text" name="query" value="<?echo "$search";?>" size="40"></p>
<p> Objects <input type="radio" name="type" value="objects" <?if($type=="objects") echo "checked";?>> Parcels <input type="radio" name="type" value="parcel" <?if($type=="parcel") echo "checked";?>> 
<p class="submit"><input type="Submit" value="Metaverse Search"></p>
</center>
</form>

<?


function microtime_float()
{
    list($usec, $sec) = explode(" ", microtime());
    return ((float)$usec + (float)$sec);
}

$time_start = microtime_float();


if($search!="")
{

$search=mysql_real_escape_string($search);


if($type=="objects")
{
	$query = "Select A1.Name as CreatorName, A2.Name as OwnerName, Object.Creator,Object.Owner,Object.Name as ObjectName,Object.Description from Object LEFT JOIN Agent as A1 ON (A1.AgentID=Object.Creator) LEFT JOIN Agent as A2 ON (A2.AgentID=Object.Owner) where match(Object.Name,Object.Description) against('".$search."')";
	$result=mysql_query($query) or die(mysql_error());

	$time_end = microtime_float();
	$time = $time_end - $time_start;

	print "Returned ".mysql_num_rows($result)." results in $time seconds";

	while($row = mysql_fetch_assoc($result))
	{
		echo "<div class=\"result\">";
		echo "<a class=\"restitle\" href=\"\"> ".$row['ObjectName']."</a><br>";	
		echo $row['Description']."<br>";
		echo "<b>Owner</b> ".($row['OwnerName'])."<br>";
		echo "<b>Creator</b> ".$row['CreatorName']."<br>";
		echo "</div>";
	

	}

}

if($type=="parcel")
{
	$query = "Select Name,Description,match(Name,Description) against('".$search."') as Score from Parcel where match(Name,Description) against('".$search."') ";
	$result=mysql_query($query) or die(mysql_error());

	$time_end = microtime_float();
	$time = $time_end - $time_start;

	print "Returned ".mysql_num_rows($result)." results in $time seconds";

	while($row = mysql_fetch_assoc($result))
	{
		echo "<div class=\"result\">";
		echo "<a class=\"restitle\" href=\"\"> ".$row['Name']."</a> (revelance ".$row["Score"].")<br>";	
		echo $row['Description']."<br>";
		
		
		echo "</div>";
	

	}
	
}

//$query = "Select Object.Creator,Object.Owner,Object.Name as ObjectName,Object.Description from Object where match(Object.Name,Object.Description) against('".$search."');";




mysql_close();
}


echo "</body></html>";




?>
