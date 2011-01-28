<?php
include 'include.php';

?>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
<title>Metaverse search</title>
<link rel="stylesheet" type="text/css" href="style.css" />
</head>
<body>
<?php 
if(isset ($_GET["objectID"]))
{
	//$id=htmlspecialchars($_GET["objectID"]);
	//rawurldecode($_GET["objectID"]);
	$id =mysql_real_escape_string(urldecode($_GET["objectID"]));
}
echo "ID is $id<p>";
$query="Select * from Object where LocalID LIKE '$id'";
		
$result=mysql_query($query) or die(mysql_error());
		
$row = mysql_fetch_assoc($result);
echo "Name is $row[Name]<p>";

echo '<table>';
foreach ($row as $key => $value) 
{
	echo "<tr><td>$key</td><td>$value</td></tr>";
}
echo '</table>';



echo 'Page Completed in '.getElapsedTime($time_start).' seconds.';
?>

</body>
</html>






