#!/usr/bin/perl


use XML::Simple;
use LWP::Simple;
use Data::Dumper;
use DBI;
use DBD::mysql;
use Math::BigInt;

$host = "localhost";
$port = "3306";
$database = "gridspider";
$tablename = "Region";
$user = "spiderer";
$password = "spider";

$dsn = "dbi:mysql:$database:$host:$port";
$connect = DBI->connect($dsn, $user, $password) or die "Unable to connect: $DBI::errstr\n";

my $first = true;

#open (OUT, ">region_coords.txt");

my $marker;
my $done = 0;

while( $done == 0 && ( $XML->{'IsTruncated'} == true || $first == true ))
{
	$first = false;
	$XML = getdata($marker);
        $asr = $XML->{'Contents'};
        $as = @$asr;
	$marker = $XML->{'Contents'}[$as-1]->{'Key'};
	print ("End is ".$marker."\n");

	my $query = "";
	foreach(@$asr)
	{
		$this = $_;
		$key = $this->{'Key'};
		
		if( $key =~ m/map-[2-9]*-[0-9]*-[0-9]*-objects.jpg/ )
		{
			$done = 1;
			last;
			
		}

		$X =  $this->{'Key'};
		$Y =  $this->{'Key'};

		$X =~ s/map-1-([0-9]*)-([0-9]*)-objects.jpg/\1/;
	 	$Y =~ s/map-1-([0-9]*)-([0-9]*)-objects.jpg/\2/;
		
		$XX = Math::BigInt->new($X*256);
		$XX->blsft(32);
		$YY = Math::BigInt->new($Y*256);		
		$XX->badd($YY);
 
		$query = "INSERT into Region (Grid,Handle,LastVerified) Values ('4','".$XX->bstr()."','Now()') ON DUPLICATE KEY UPDATE LastVerified=Now();\n";
 		#print $query;
		$query_handle = $connect->prepare($query);
        	$query_handle->execute() or print "Query failed \n$query\n";


	}
	#exit 0;
	#print $query;
	#$query_handle = $connect->prepare($query);
        #$query_handle->execute();
}


# Delete stale regions;


print "Removing objects from dead regions\n";
$SQL = "DELETE from Object where Grid=4 and Region IN (Select Handle from Region where Grid=4 and LastVerified<DATE_SUB(NOW(),INTERVAL 7 DAY));";
$query_handle = $connect->prepare($SQL);
$query_handle->execute();

print "Removing parcels from dead regions\n";
$SQL = "DELETE from Parcel where Grid=4 and Region IN (Select Handle from Region where Grid=4 and LastVerified<DATE_SUB(NOW(),INTERVAL 7 DAY));";
$query_handle = $connect->prepare($SQL);
$query_handle->execute();

print "Removing dead regions\n";
$SQL = "DELETE from Region where grid=4 and LastVerified<DATE_SUB(NOW(),INTERVAL 7 DAY);";
$query_handle = $connect->prepare($SQL);
$query_handle->execute();



sub getdata($marker)
{
	my $data;

	my $document = get("http://map.secondlife.com/?marker=$marker");
	my $XML = XML::Simple->new()->XMLin($document);
	return $XML;

}
