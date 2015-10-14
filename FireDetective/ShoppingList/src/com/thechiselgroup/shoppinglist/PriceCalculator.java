package com.thechiselgroup.shoppinglist;

public class PriceCalculator {	
	public int getPrice(String itemName) {
		try {
			Thread.sleep(4000);
		} catch (InterruptedException e) {
		}
		return Math.abs(itemName.hashCode() % 5) + 1; // Returns a random price from 1 - 5.	
	}
}
