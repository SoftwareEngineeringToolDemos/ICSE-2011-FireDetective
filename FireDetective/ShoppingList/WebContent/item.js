// ---- Items -------------------------------------------------------------------------

var nextItemId = 0;
var allItems = [];

createItem = function(name) {
	return { id: nextItemId++, name: name };
}

getItemInnerDiv = function(item) {
	return "<div class='name-cell'>" + item.name + " <a href='#' onclick='javascript:deleteItem(" + item.id + ");'>(x)</a></div>" +
		"<div class='price-cell'>" + (item.price ? item.price : "Calculating price...") + "</div>" +
		"<br>";
}

createItemElement = function(item) {
	var element = document.createElement("div");
	element.setAttribute("id", "item_" + item.id);
	element.innerHTML = getItemInnerDiv(item);
	return element;
}

addItem = function(item) {
	var e = createItemElement( item );
	document.getElementById("items").appendChild( e );
	allItems[item.id] = item;
}

updateItem = function(item) {
	document.getElementById("item_" + item.id).innerHTML = getItemInnerDiv( item );
	allItems[item.id] = item;
}

deleteItem = function(item_id) {
	var item = allItems[item_id];
	item.deleted = true;
	document.getElementById("items").removeChild(document.getElementById("item_" + item.id));
	updateTotal();
}

deleteAllItems = function() {
	for (var i = 0; i < allItems.length; i++)
		allItems[i].deleted = true;	
	document.getElementById("items").innerHTML = "";
	updateTotal();
}

// ---- Calculating taxes + total amount ----------------------------------------------

function roundMoney(money) {
	return Math.round(money * 100) / 100;	
}

getSum = function() {
	var sum = 0;
	for (var i = 0; i < allItems.length; i++)
		if (allItems[i].price && !allItems[i].deleted)
			sum += allItems[i].price;
	return sum;
}

function updateTotal() {
	var sum = getSum();
	var tax = roundMoney(sum * 0.1);
	var total = roundMoney(sum + tax);
	document.getElementById("tax_price").innerHTML = tax.toString();
	document.getElementById("total_price").innerHTML = total.toString();
}

