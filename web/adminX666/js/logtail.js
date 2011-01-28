/* logtail.js
an ajax log file tailer / viewer
copyright 2007 john minnihan.

http://freepository.com

Released under these terms
1. This script, associated functions and HTML code ("the code") may be used by you ("the recipient") for any purpose.
2. This code may be modified in any way deemed useful by the recipient.
3. This code may be used in derivative works of any kind, anywhere, by the recipient.
4. Your use of the code indicates your acceptance of these terms.
5. This notice must be kept intact with any use of the code to provide attribution.
*/

var id;

function setID(idx)
{
	id = idx;
}

function getLog(timer) {

var url = "http://search.pleiades.ca/adminX666/logtail.php?id="+id;

request.open("GET", url, true);
request.onreadystatechange = updatePage;
request.send(null);
startTail(timer);
}

function startTail(timer,idx) {
if (timer == "stop") {
stopTail();
} else {
t= setTimeout("getLog()",4000);


}
}

function stopTail() {
clearTimeout(t);
var pause = "The log viewer has been paused. To begin viewing logs again, click the Start Viewer button.";
logDiv = document.getElementById("log");
var newNode=document.createTextNode(pause);
logDiv.replaceChild(newNode,logDiv.childNodes[0]);
}

function updatePage() {
if (request.readyState == 4) {
if (request.status == 200) {
var currentLogValue = request.responseText.split("\n");
eval(currentLogValue);
logDiv = document.getElementById("log");
var logLine = ' ';
for (i=0; i < currentLogValue.length - 1; i++) {
logLine += currentLogValue[i] + '<br/>\n';
}
logDiv.innerHTML=logLine;
} else
alert("Error! Request status is " + request.status);
}
}
