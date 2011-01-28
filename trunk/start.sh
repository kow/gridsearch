
ID=$1

cd /home/robin/gridspidergit/git/gridsearch/trunk

if [ $# -ne 1 ]; then
	echo "No index passed, finding the next free one"
	for XX in {1..10}
	do
		if [ ! -f lock.$XX ]; then
		echo "$XX is free"
		ID=$XX;
		break;
		fi
	done
fi

if [[ $ID -lt 0 ]] && [[ $ID -gt 5 ]]; then
	ECHO "Invalid spider index"
	exit -1
fi


if [ -f lock.$ID ]; then
	echo "Lock exists for ID $ID"
	exit -1
fi

echo "Starting Spider # $ID"

touch lock.$ID

ulimit -v 150000
/usr/local/bin/mono /home/robin/gridspidergit/git/gridsearch/trunk/bin/GridSpider.exe --user spiderer --password spider --host 127.0.0.1 --database gridspider > log.$ID &
PID=$!

echo "$PID" > lock.$ID

wait $PID

rm lock.$ID

