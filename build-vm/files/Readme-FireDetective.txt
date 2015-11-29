This file contains instructions on how to use the FireDetective plugin for Mozilla Firefox. Some of these steps have been adapted from the FireDetective Readme file, which can be found at C:\FireDetective\Readme.txt


1. Start the GlassFish Java EE server using on the "Start default server" icon on the desktop. It is possible that the server is running already.

2. Launch Mozilla Firefox (if not already running) using the desktop icon.

3. Navigate to about:blank (this has been set as the home page) and close all other tabs/instances (important!!).

4. Launch FireDetectiveAnalyzer.exe using the icon on the desktop.

5. In the top-right corner of the screen, it should say “Firefox connected” and “Server connected”. In Firefox, the toolbar should say “Trace consumer is recording”.

6. In Firefox, navigate to the web application (http://localhost:8080/ShoppingList/) and use it. You can enter items into the shopping list using the interface given.

7. At any point, you can now switch to FireDetective and look “under the hood” of the web application. The analysis is real-time, so you can switch back and forth.

8. You can also use FireDetective to analyze other web applications. For analyzing client side code, no additional configuration is needed: just visit the web application in Firefox. For analyzinf server-side code, follow the steps given in the last section of the FireDetective Readme (C:\FireDetective\Readme.txt).


*Note 1: If FireDetectiveAnalyzer shows "Server not connected", or if the shopping list application does not show up at (http://localhost:8080/ShoppingList/), restart the Glassfish server using the "Stop default server" and "Start default server" icons on the desktop.

*Note 2: You can host your own applications on Glassfish by deploying the application to the server using the Eclipse IDE (found at C:\eclipse\eclipse\eclipse.exe). The Eclipse installation includes the Glassfish 2.1 server adapter.

*Note 3: A more detailed demo video of the tool in action can be found at: https://www.youtube.com/watch?v=Trp82FNBeEU.