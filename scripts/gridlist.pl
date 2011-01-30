#!/usr/bin/perl


use XML::Simple;
use LWP::Simple;
use Data::Dumper;

my $first = true;

open (OUT, ">region_coords.txt");

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
		
		print OUT "\t$X\t$Y\n";

	}	
}

close OUT;

print "\n\n";

sub getdata($marker)
{
	my $data;

	my $document = get("http://map.secondlife.com/?marker=$marker");
	my $XML = XML::Simple->new()->XMLin($document);
	return $XML;

}
