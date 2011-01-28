<?php

$time_start = microtime_float();
$username="spiderer";
$password="spider";
$database="gridspider";
mysql_connect('127.0.0.1',$username,$password);
mysql_select_db($database) or die( "Unable to select database");

include('sphinxapi.php');

 $cl = new SphinxClient();
 $cl->SetServer( "localhost", 3312 );
 $cl->SetMatchMode( SPH_MATCH_ANY  );


//functions

function microtime_float()
{
    list($usec, $sec) = explode(" ", microtime());
    return ((float)$usec + (float)$sec);
}
function displayObjects($ids, $emphasis)
{
	$query  = " SELECT *
FROM `ObjectView` WHERE OLocalID IN (".implode(",", $ids).")
LIMIT 0 , 10";
	$result=mysql_query($query) or die(mysql_error());
	while($row = mysql_fetch_assoc($result))
		{
			displayObjectPart($row,$emphasis);
		}
}
function displayObject($id,$emphasis)
{
	$query  = " SELECT *
FROM `ObjectView` WHERE OLocalID = $id
LIMIT 0 , 1";

	$result=mysql_query($query) or die(mysql_error());
	$row = mysql_fetch_assoc($result);
	displayObjectPart($row,$emphasis);

		
}
function displayObjectPart($row,$emphasis)
{
	echo "<div class=\"result\">";
			echo "<a class=\"restitle\" href=objectInfo.php?objectID="
			     .urlencode($row['OLocalID']).">"
			     .addEmphasis($row[ObjectName],$emphasis)."</a> ";
			/* sale types
			 *  Not = 0,
        		Original = 1,
        		Copy = 2,
        		Contents = 3,
			 */
			echo "<span class=\"saleInfo\">";
			if($row['SaleType']==1)
				echo "Origonal For Sale: $$row[SalePrice]L";
			else if($row['SaleType']==2)
				echo "Copy For Sale: $$row[SalePrice]L";
			else if($row['SaleType']==3)
				echo "Contents For Sale: $$row[SalePrice]L";
			else 
				echo "Not for Sale";
			if($row['SaleType']!=0)
				echo " : Perms: ".parsePerms($row["Perms"]);	
			echo "</span><br>";
				
			echo "<span class=\"description\">"
			    .addEmphasis($row['Description'],$emphasis)."</span><br>";
			echo "<b>Owner</b> ".($row['OwnerName'])."<br>";
			echo "<b>Creator</b> ".$row['CreatorName']."<br>";
			echo getLocationURL($row['GridLoginURI'],$row['RegionName'],$row['Location'])."<br>";
			echo "</div><p>";
}
function parseLocation($location)
{
	$locationArray = locationToArray($location);
	return "$locationArray[0]/$locationArray[1]/$locationArray[2]";
}
function parsePerms($permsMask)
{
	if($permsMask==0x7FFFFFFF)return "Full Permissions";
	if($permsMask==0)return "No Permissions";	
	$pA = permsIntToArray($permsMask);
	$toReturn = "";
	foreach ($pA as $PName => $on)
		if($on)$toReturn.="$PName ";
	return "(".substr($toReturn, 0,-1).")";	
}
function locationToArray($location)
{
	// pos = (int)kvp.Value.Position.X + 
	//((int)kvp.Value.Position.Y * 255) + 
	//((int)kvp.Value.Position.Z * 65535);
   $binaryL = decbin($location);
   //101001101101100110101
   //000000000000011111111 is 255
   $xValue = $location & 255;
   $location = $location >> 8;
   $yValue = $location & 255;   
   $location = $location >> 8;
   $zValue = $location;
   return array($xValue,$yValue,$zValue);
}
function permsIntToArray($permsMask)
{
	$mod = (1<<14);
	$result = $permsMask & $mod;
	$yes = (bool)($result==1);
	//echo "Perms was $permsMask and mod was $mod and result of and was $result and yes is $yes";
	 //Transfer = 1 << 13, Modify = 1 << 14, Copy = 1 << 15, Move = 1 << 19, Damage = 1 << 20
	 return array(
	 	"Transfer"=>(($permsMask& (1 << 13))!=0),
	 	"Modify"=>(($permsMask& (1 << 14))!=0),
	 	"Copy"=>(($permsMask& (1 << 15))!=0),
	 	"Move"=>(($permsMask& (1 << 19))!=0),
	 	"Damage"=>(($permsMask& (1 << 20))!=0)
	 );
}
function getPermsNumber($copy,$mod,$trans,$move,$damage)
{
	$toReturn = 0;
	//for($inc=0;$inc<32;$inc++)$notReq[]=(1 << $inc);
	if($copy)$toReturn|=(1 << 15);else $notReq[]=(1 << 15);	
	if($mod)$toReturn|=(1 << 14);else $notReq[]=(1 << 14);	
	if($trans)$toReturn|=(1 << 13);else $notReq[]=(1 << 13);	
	if($move)$toReturn|=(1 << 19);else $notReq[]=(1 << 19);	
	if($damage)$toReturn|=(1 << 20);else $notReq[]=(1 << 20);
	if($copy==TRUE&&$mod==TRUE&&$trans==TRUE&&$move==TRUE&&$damage==TRUE)$notReq[]=0x7FFFFFFF;
	
	$returns[] = $toReturn;
	//build list of falses
	foreach($notReq as $i => $notNeeded)
	{
		foreach($returns as $k => $inner)
		{
			$newReturns[]=$inner|$notNeeded;
		}
		
		$returns = array_merge($returns,$newReturns);
		unset($newReturns);
	}
	return implode(",", $returns);
}

function getElapsedTime($time_start)
{
	$time_end = microtime_float();
	return $time_end - $time_start;
}
function getLocationURL($gridURI,$region,$location)
{
	$region = rawurlencode($region);
	if (strpos($gridURI,'lindenlab.com'))
	{
		$url= "secondlife://$region/".parseLocation($location);
		//linden grid , use second life link style
	}else
		$url= "opensim://$gridName/$region/$location";
		
	return "<a class=\"slurl\" href=$url>$url</a>";
}
function addEmphasis($link, $search)
{
	$tok = strtok($search, " ");
	while ($tok !== false)
	{
		$off=0;
		while($off!==false)
		{
			$off=stripos($link,$tok,$off);
			if($off===FAlSE)break;
			$link = substr_replace($link,"<em>", $off, 0);			
			$link = substr_replace($link,"</em>", $off+4+strlen($tok), 0);
			$off+=8+strlen($tok);
		}
		$tok = strtok(" ");
	}
	return $link;
}


/*
source objects
{
    # data source type. mandatory, no default value
	# known types are mysql, pgsql, mssql, xmlpipe, xmlpipe2, odbc
	# $username="root";
	# $password="16BE0DAA66";
	# $database="spider";
	# mysql_connect('192.168.200.130',$username,$password);
	type			= mysql

	#####################################################################
	## SQL settings (for 'mysql' and 'pgsql' types)
	#####################################################################

	# some straightforward parameters for SQL source types
	sql_host		= 192.168.200.130
	sql_user		= root
	sql_pass		= 16BE0DAA66
	sql_db			= spider
	sql_port		= 3306	# optional, default is 3306                

    # indexer query
    # document_id MUST be the very first field
    # document_id MUST be positive (non-zero, non-negative)
    # document_id MUST fit into 32 bits
    # document_id MUST be unique

    sql_query                       = \
            SELECT \
                   OLocalID, CreatorName ,	OwnerName ,	GridName ,	RegionName , \
                   ObjectName ,	SalePrice ,	SaleType , Perms ,Description \
            FROM \
                    ObjectView;

    sql_attr_uint                = OwnerName
    sql_attr_uint                = GridName
    sql_attr_uint                = RegionName
    sql_attr_uint                = SalePrice
    sql_attr_uint                = SaleType
    sql_attr_uint                = Perms


    # document info query
    # ONLY used by search utility to display document information
    # MUST be able to fetch document info by its id, therefore
    # MUST contain '$id' macro 
    #

    sql_query_info          = SELECT * FROM Object WHERE LocalID=$id
}

index objects
{
    source                  = objects
    path                    = /var/data/sphinx/objects
    min_word_len            = 3
    min_prefix_len          = 0
    min_infix_len           = 3
}

searchd
{
	port				= 3312
	log					= /var/log/searchd/searchd.log
	query_log			= /var/log/searchd/query.log
	pid_file			= /var/log/searchd/searchd.pid
}
 sudo /usr/local/bin/indexer --config /usr/local/etc/sphinx.conf --allsource objects


 */
?>
