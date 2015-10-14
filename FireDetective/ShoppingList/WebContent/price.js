function gotPrice(item, callback, data) {

	var obj = eval(data);
	item.price = obj.price_info;
	callback();
	
}

function calculatePrice(item, callback) {

	var xmlhttp = new XMLHttpRequest();	
	xmlhttp.open("GET", "getprice.jsp" + "?itemname=" + item.name);
	xmlhttp.onreadystatechange = function() {
		if (xmlhttp.readyState == 4 && xmlhttp.status == 200) {
			gotPrice(item, callback, xmlhttp.responseText);
		}
	}
	xmlhttp.send(null);
}


