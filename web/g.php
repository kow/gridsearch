<?php
include 'include.php';
$search="";
$start=0;
           
if(isset ($_GET["start"]))
{
	$start=htmlspecialchars($_GET["start"]);
}
if(isset ($_GET["type"]))
{
	$type=htmlspecialchars($_GET["type"]);
}
if(isset($_GET["query"]))
{
	$search=htmlspecialchars($_GET["query"]);
}
?>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en" lang="en">
<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
<title>Metaverse search</title>
<link rel="stylesheet" type="text/css" href="style.css" />
<script type="text/javascript">
function showAdvanced()
{ 
	var obj= document.getElementById("advancedSearch");
	obj.style.visibility='visible';
}
function toggleIt()
{
	var targetElement = document.getElementById("advancedSearch");
    targetElement.style.display = (targetElement.style.display != "block") ? "block" : "none";
    
    document.getElementById("showCheckBox").checked=(targetElement.style.display=="block");
       
    var linkish = document.getElementById("showContainer");
    linkish.innerHTML=(targetElement.style.display == "none") ? "Show Advanced Settings:" : "Show Advanced Settings:";
    
}
function loaded()
{
	//document.getElementById("advancedSearch").style.display=
		<?php if($_GET['advancedShown']=="on") 
		echo 'toggleIt();';?>
	//document.getElementById("showCheckBox").onClick=toggleIt();

}
</script>

</head>

<body onload="loaded()">

<center>
<?php 

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

<center></center>

<form action="g.php" method="get">
<center>
<p><label> Metaverse search</label></p>
<p> <input type="text" name="query" value="<?php echo "$search";?>" size="40"/></p>
<p> Objects <input name="type" type="radio" value="objects" <?php if($type=="objects"||$type=="") echo "checked";?>/>
Parcels <input type="radio" name="type" value="parcel" <?php if($type=="parcel") echo "checked";?>/>
</p>
<p></p>
<?php /*query=linden&
type=objects&
searchName=on&
searchDescription=on
&searchCreator=on
&searchOwner=on
&priceLow=0&
priceHigh=9000&priceUnits=1&primLow=0&primHigh=9000*/?>
<a href="javascript:toggleIt()" id="showContainer">Show Advanced Settings:</a>
 <input name="advancedShown" type="checkbox" id="showCheckBox" onclick="javascript:toggleIt()"
	 <?php if($_GET['advancedShown']=="on") echo "checked";?>/>
<div class="advancedSearch" id="advancedSearch"><table><tr>
  <td>Advanced Settings</td></tr></table>
Search In: 
<table>
<tr><td>Name: <input name="searchName" type="checkbox" 
	 <?php if($_GET['searchName']=="on"||$search=="") echo "checked";?>/></td>
<td>Description: <input name="searchDescription" type="checkbox" 
	<?php if($_GET['searchDescription']=="on"||$search=="") echo "checked";?> /></td>
<!--  :s
	<td>Creator: <input name="searchCreator" type="checkbox" 
		<?php if($_GET['searchCreator']=="on") echo "checked";?>/></td>
	<td>Owner: <input name="searchOwner" type="checkbox"
		<?php if($_GET['searchOwner']=="on") echo "checked";?>/></td>
-->
</tr></table>
<p>
  <input type="checkbox" name="searchPrice" id="searchPrice"
  	<?php if($_GET['searchPrice']=="on") echo "checked";?> />
  <label for="searchPrice">Where Price is between:</label>
  <input name="priceLow" type="text" id="priceLow" value=
  	<?php echo ($_GET['priceLow']>0)?$_GET['priceLow']:0;?> size="4" />
  and 
  <input name="priceHigh" type="text" id="priceHigh" value=
		<?php echo ($_GET['priceHigh']>0)?$_GET['priceHigh']:9000;?> size="6" />&nbsp;&nbsp;
  <select name="priceUnits" id="priceUnits">
    <option value="1" <?php if($_GET['priceUnits']==1) echo 'selected="selected"'?> disabled="disabled">USD</option>
    <option value="2" <?php if($_GET['priceUnits']==2||$search=="") echo 'selected="selected"'?>>Linden</option>
</select>
  .
</p>
  <input type="checkbox" name="searchPerms" id="searchPerms"
		<?php if($_GET['searchPerms']=="on") echo "checked";?> />
  Where Perms Have At Least 
  <input type="checkbox" name="permCopy" id="permCopy" 
  <?php if($_GET['permCopy']=="on"||$search=="") echo "checked";?> />Copy   
  <input type="checkbox" name="permModify" id="permModify"
  <?php if($_GET['permModify']=="on") echo "checked";?> />Modify  
  <input type="checkbox" name="permTransfer" id="permTransfer" 
  <?php if($_GET['permTransfer']=="on") echo "checked";?> />Transfer 
  <input type="checkbox" name="permMove" id="permMove" 
  <?php if($_GET['permMove']=="on") echo "checked";?> />Move 
  <input type="checkbox" name="permDamage" id="permDamage" 
  <?php if($_GET['permDamage']=="on") echo "checked";?> />Damage 

<p>
  <input type="checkbox" name="searchPrims" id="searchPrims" 
	<?php if($_GET['searchPrims']=="on") echo "checked";?> />
  With Prim Count Between 
  <input name="primLow" type="text" id="primLow" value=
	<?php echo ($_GET['primLow']>0)?$_GET['primLow']:0;?> size="5" /> 
  and 
  <input name="primHigh" type="text" id="primHigh" value=
	<?php echo ($_GET['primHigh']>0)?$_GET['primHigh']:9000;?> size="6" />
</p>
</div>
<p class="submit"><input type="submit" value="Metaverse Search"></input></p>
</center>
</form>


  <?php




if($search!="")
{

	$search=mysql_real_escape_string(stripslashes (str_replace ("&quot;", "\"", ($search))));
	
	$numResults=0;
	$numViewed=10;
	$page=($start/$numViewed)+1;
	
	if($type=="objects")
	{
		//quick count for generating how many pages we are dealing with
		$query="SELECT COUNT(*) as numberResults from Object where match(Object.Name,Object.Description) against('".$search."')";
		$result=mysql_query($query) or die(mysql_error());
		$row = mysql_fetch_assoc($result);
		$numResults=$row['numberResults'];
		mysql_free_result($result);	
		$match = "";	
		if($_GET['searchName']=="on")$match.=',Object.Name';
		if($_GET['searchDescription']=="on")$match.=',Object.Description';	
		// :s	
		//if($_GET['searchCreator']=="on")$match.=',A1.Name';			
		//if($_GET['searchOwner']=="on")$match.=',A2.Name';
		$match = substr($match,1);		
		$priceAddition = "";
		if($_GET['searchPrice']=="on") $priceAddition=" AND Object.SalePrice BETWEEN $_GET[priceLow] AND $_GET[priceHigh] ";
		$permsAdditon = "";
		if($_GET['searchPerms']=="on") $permsAdditon=" AND Object.Perms IN (".
		getPermsNumber($_GET['permCopy']=="on",$_GET['permModify']=="on",$_GET['permTransfer']=="on",$_GET['permMove']=="on",$_GET['permDamage']=="on").") ";
		$primsAdditon ="";
		if($_GET['searchPrims']=="on") $primsAdditon=" AND Object.Prims BETWEEN $_GET[primLow] AND $_GET[primHigh] ";
		
			$query="Select A1.Name as CreatorName, A2.Name as OwnerName,
			 A3.name as GridName, A4.Name as RegionName,A3.LoginURI as GridLoginURI,
 Object.Creator,Object.Owner,Object.Name as ObjectName, SalePrice,SaleType,Object.Perms, Object.ID as ObjectID,Object.LocalID as OLocalID,
Object.Description, Object.Location from Object 
LEFT JOIN Agent as A1 ON  (A1.AgentID=Object.Creator)
LEFT JOIN Agent as A2 ON (A2.AgentID=Object.Owner)
LEFT JOIN Grid as A3 ON (A3.PKey=Object.Grid)
LEFT JOIN Region as A4 ON (A4.Handle=Object.Region)
 where match($match) against('$search' IN BOOLEAN MODE)".$priceAddition.$permsAdditon.$primsAdditon
			." LIMIT ".$start.", 10";
		/*stripslashes (str_replace ("&quot;", "\"", ($_POST['keywords'])*/
			//IN NATURAL LANGUAGE MODE with QUERY EXPANSION
	
		//echo "<p class=\"query\">$query</p>";
		//$result=mysql_unbuffered_query($query) or die(mysql_error());
		
		//$cl->SetFilter( 'model', array( 3 ) );
		$cl->SetLimits($start,10);
		$cl->SetArrayResult( true );
		$result = $cl->Query( "$search", 'objects' );
		if ( $result === false )
		{
			echo "Query failed: " . $cl->GetLastError() . ".\n";
		}else 
		{	
			if ( $cl->GetLastWarning() ) 
			{
				echo "WARNING: " . $cl->GetLastWarning() . " ";
			}
			if ( ! empty($result["matches"]) ) 
			{
				$ids = array();
				foreach ( $result["matches"] as $doc => $docinfo )
				{
	                // echo "<p>$doc\n";
	                foreach ($docinfo as $key => $pair)
	                {
	                	//if(is_array($pair))
	                	//foreach ($pair as $type=>$value)
	                	//{
	                	//	echo "$type is $value\n";
	                	//}else echo "$key is $pair";
	                }
	                //echo "guna get $docinfo[id]";
	                //displayObject($docinfo['id'],$search);
	                $ids[]=$docinfo['id'];
	            }
	            displayObjects($ids, $search);
			}
		}
      echo "Result is \n"+implode("\n", $result);
          
          //print_r( $result );
      
			
		$time = getElapsedTime($time_start);
		if($numResults>$numViewed)
		{
			print "Showing Page ".$page." of ".$numResults." results in $time seconds<p>";
		}else 
		{
			print "Showing ".$numResults." results in $time seconds<p>";
		}
	
	}
	
	if($type=="parcel")
	{
		//im not really focusing on parcels right now, ill update it later
		$query = "Select Name,Description,match(Name,Description) against('".$search."') "
		."as Score from Parcel where match(Name,Description) against('".$search."')";
		$result=mysql_query($query);
		$numResults=mysql_num_rows($result);
		mysql_free_result($result);
		
		$query = "Select Name,Description,match(Name,Description) against('".$search."') "
		."as Score from Parcel where match(Name,Description) against('".$search."') "
		." LIMIT ".$start.", 10";
		$result=mysql_query($query) or die(mysql_error());
	
		$time_end = microtime_float();
		$time = $time_end - $time_start;
	
		if($numResults>$numViewed)
		{
			print "Showing Page ".$page." of ".$numResults." results in $time seconds";
		}else 
		{
			print "Showing ".$numResults." results in $time seconds";
		}
		while($row = mysql_fetch_assoc($result))
		{
			echo "<div class=\"result\">";
			echo "<a class=\"restitle\" href=\"\"> ".$row['Name']."</a> (revelance ".$row["Score"].")<br>";	
			echo $row['Description']."<br>";
			
			
			echo "</div><p>";
		
	
		}
		
		mysql_free_result($result);
	}
	
	//echo "num results : $numResults and num viewed $numViewed";
	if($numResults>$numViewed)
	{
		//show pages table at bottom
		echo '<p><center><table><tr><td>Page:</td>';
		$dispStart = $page-10;
		if($dispStart<1)$dispStart=1;
		$totalPages = ($numResults/$numViewed)+1;
		$dispEnd=$totalPages;
		if($dispEnd>$page+10)$dispEnd=$page+10;
		$search=urlencode($search);
		for($i=$dispStart;$i<$dispEnd;$i++)
		{
			if($i==$page)
				echo "<td>$i</td>";
			else 
			{
				echo "<td><a href=?query=$search&type=$type&start=".(($i-1)*$numViewed).">$i</a></td>";
			}
		}
		echo '</tr></table></center></p>';
		
	}
	
//$query = "Select Object.Creator,Object.Owner,Object.Name as ObjectName,Object.Description from Object where match(Object.Name,Object.Description) against('".$search."');";




//mysql_close();
}


echo "</body></html>";

?>
</body>
</html>
				
	
