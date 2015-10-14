//---- UI ----------------------------------------------------------------------------

addCurrentItem = function() {
	var item = createItem( document.getElementById("newItemTextBox").value );
	addItem( item );
	calculatePrice( item, function() { updateItem(item); updateTotal(); })
}

readyForNewItem = function() {
	document.getElementById("newItemTextBox").value = "";
	document.getElementById("newItemTextBox").focus();
}

newItemForm_submit = function() {
	addCurrentItem();
	readyForNewItem();
}

clear_click = function() {
	deleteAllItems();
}
