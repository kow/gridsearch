<?php


// select * from Grid,Region,Logins where Region.LockID=Logins.LockID AND Logins.PID=4732 AND Grid.PKey=Logins.Grid;
//+------+------+----------------------------------------------------+----------------------+------+------+-----------------+---------------------+------+-------+-----------------+--------+----------+------+------+------+-------+---------+-------------+----------+---------------------+------+
//| PKey | name | LoginURI                                           | Description          | new  | Grid | Handle          | LastScrape          | ID   | Owner | Name            | Status | LockID   | pkey | PKey | grid | First | Last    | Password    | LockID   | LastScrape          | PID  |
//+------+------+----------------------------------------------------+----------------------+------+------+-----------------+---------------------+------+-------+-----------------+--------+----------+------+------+------+-------+---------+-------------+----------+---------------------+------+
//|    4 | Agni | https://login.agni.lindenlab.com/cgi-bin/login.cgi | Secondlife Main Grid |    0 |    4 | 614627000197120 | 2011-01-26 07:12:29 | NULL | NULL  | Minas Sidhevair |      0 | 25392893 |    0 |    6 |    4 | Dr    | Steamer | 123pleiabot | 25392893 | 2011-01-26 07:11:26 | 4732 |
//+------+------+----------------------------------------------------+----------------------+------+------+-----------------+---------------------+------+-------+-----------------+--------+----------+------+------+------+-------+---------+-------------+----------+---------------------+------+


include '../include.php';


$dir = "/home/robin/gridspidergit/git/gridsearch/trunk";
$x=0;
$files = array();
$PIDs = array();
$IDs = array();
$INFOs = array();
$CPU = array();
$MEM = array();
$REGION = array();
$GRID = array();
$NAME = array();

if (is_dir($dir)) {
    if ($dh = opendir($dir)) {
        while (($file = readdir($dh)) !== false) {
            //echo "filename: $file : filetype: " . filetype($dir . $file) . "\n";
            if(preg_match("/lock\.([0-9]).*/",$file,$matches))
	    {
	        $files[$x] = $file;
		$IDs[$x] = trim($matches[1]);
		$handle = fopen("$dir/".$file,"r");
		$pid = trim(fgets($handle));
		$PIDs[$x] = $pid;
		fclose($handle);
		$data = `ps uh -p $pid`;
	        $INFOs[$x] = $data;
		preg_match("/([a-zA-Z0-9-]*)[ ]*([0-9]*)[ ]*([0-9\.]*)[ ]*([0-9\.]*).*/",$data,$matches);
				
		$CPU[$x] = $matches[3];
		$MEM[$x] = $matches[4];

		$query = "select * from Grid,Region,Logins where Region.LockID=Logins.LockID AND Logins.PID=$pid AND Grid.PKey=Logins.Grid;";
		$result=mysql_query($query) or die(mysql_error());
		$row = mysql_fetch_assoc($result);
		//echo "Objects ".($row['objects']);
		$REGION[$x] = $row['Name'];
		$NAME[$x] = $row["First"] . " ".$row["Last"];
		$GRID[$x] = $row["name"];

		$x++;
            }
	}
        closedir($dh);
    }
}


?>


<html>
<head>

<script>

function showlog(id)
{
	location.href="log.php?id="+id;
}

function kill(id)
{
	location.href="kill.php?pid="+id;
}

function start()
{
	location.href="start.php";
}

</script>

</head>

<body>

<input type="button" value="Start new bot" name="start" onClick="start()" />

<table border="1">
<tr>
<td> Bot Number </td>
<td> PID </td>
<td> CPU </td>
<td> MEM </td>
<td> Grid </td>
<td> Avatar </td>
<td> Region </td>
<td> Log File </td>
<td> Command </td>
<tr>
<?
foreach ($files as $i => $file)
{
        $pid = $PIDs[$i];
        $id = $IDs[$i];
	$cpux = $CPU[$i];
        $memx = $MEM[$i];
	$regionx = $REGION[$i];
	$gridx = $GRID[$i];
	$namex = $NAME[$i];
	//print("$i is $file PID $pid ID $id \n");	
	print("<tr>");
	print("<td>$id</td><td>$pid</td><td>$cpux</td><td>$memx</td><td>$gridx</td><td>$namex</td><td>$regionx</td><td><input type=\"button\" value=\"View\" name=\"View\" onClick=\"showlog($id)\"></input></td><td><input type=\"button\" value=\"kill\" onClick=\"kill($pid);\" /></td>\n");
	print("</tr>");

}

?>

</tr>


</table>

</body>
</html>