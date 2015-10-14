<%@ page language="java" contentType="text/html; charset=ISO-8859-1" pageEncoding="ISO-8859-1"%>


<html>
<head>
<title>The Shopping List Application!</title>


<link href="styles.css" rel="stylesheet" type="text/css" media="screen"></link>

<script src="price.js" type="text/javascript"></script>

<script src="item.js" type="text/javascript"></script>

<script src="ui.js" type="text/javascript"></script>


<script>

function getTime() {
	var result = "";
	var currentTime = new Date()
	var hours = currentTime.getHours()
	var am = hours < 12; 
	hours = ((hours + 11) % 12) + 1;
	var minutes = currentTime.getMinutes()
	if (minutes < 10) minutes = "0" + minutes
	return hours + ":" + minutes + " " + (am ? "AM" : "PM");
}

updateTime = function() {
	document.getElementById("timenow").innerHTML = getTime();
}

</script>

</head>

<body>

<div class="mainframe">

<span id="timenow">&nbsp;</span>

<span id="hideclock">
<a href="#" onclick="javascript: clearInterval(timeUpdater); getElementById('timenow').innerHTML = ''; getElementById('hideclock').innerHTML = ''; return false;">
<small>Hide clock</small>
</a>
</span>

<div class="backdrop">

<h1>My shopping list!</h1>

<form id="newItemForm" onsubmit="javascript:newItemForm_submit(); return false;">

	<input id="newItemTextBox" type="text" />
	
	<input value="Add item to list!" type="submit" />
	
	<input value="Clear" type="button" onclick="clear_click();" />	
	
</form>

<hr>

<div id="items">
</div>

<hr>

<div class="name-cell">Taxes:</div>
<div class="price-cell"><span>$ </span><span id="tax_price">0</span></div>
<br>


<hr>

<div class="name-cell">Total amount:</div>
<div class="price-cell"><span>$ </span><span id="total_price">0</span></div>
<br>

</div>




<div id="status"></div>
</div>

</body>

<!--

End of body

 -->

<script>
document.addEventListener("DOMContentLoaded", function() { updateTime(); }, true);
timeUpdater = setInterval("updateTime();", 10000);
</script>

</html>
