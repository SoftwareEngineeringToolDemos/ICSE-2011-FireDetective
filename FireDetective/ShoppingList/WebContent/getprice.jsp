<%@ page contentType="application/json; charset=UTF-8" %>
<%@page import="com.thechiselgroup.shoppinglist.PriceCalculator"%>

<%= "({ price_info: " + new PriceCalculator().getPrice(request.getParameter("itemname")) + " })"%>