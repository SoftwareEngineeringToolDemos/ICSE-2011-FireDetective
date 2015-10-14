package com.thechiselgroup.firedetective;

import java.io.IOException;
import javax.servlet.Filter;
import javax.servlet.FilterChain;
import javax.servlet.FilterConfig;
import javax.servlet.ServletException;
import javax.servlet.ServletRequest;
import javax.servlet.ServletResponse;
import javax.servlet.http.HttpServletRequest;

/**
 * Main trace filter. Please add it to your web.xml to enable tracing for your web application.
 */

public class TraceFilter implements Filter {
    
    public TraceFilter() {
    }
    
    /**
     *
     * @param request The servlet request we are processing
     * @param result The servlet response we are creating
     * @param chain The filter chain we are processing
     *
     * @exception IOException if an input/output error occurs
     * @exception ServletException if a servlet error occurs
     */
    public void doFilter(ServletRequest request, ServletResponse response, FilterChain chain) throws IOException, ServletException {

    	HttpServletRequest httpRequest = (HttpServletRequest)request;
    	String requestIdStr = httpRequest.getHeader("X-Fire-Detective-Request-Id");
    	
    	// These variable are used by the JVMTI-agent -- do not remove!
    	@SuppressWarnings("unused")
		int fireDetectiveRequestId = -1;
    	String fireDetectiveJspFile; 
    	try {
    		if (requestIdStr != null && !requestIdStr.equals(""))
    			fireDetectiveRequestId = Integer.parseInt(requestIdStr);
    	}
    	catch (NumberFormatException formatEx) {
    	}    	
    	String pathInfo = httpRequest.getPathInfo();
    	String servletPath = httpRequest.getServletPath();
    	fireDetectiveJspFile = 
    		(pathInfo != null ? pathInfo : "") + (servletPath != null ? servletPath : "");
    	if (!fireDetectiveJspFile.endsWith(".jsp"))
    		fireDetectiveJspFile = "";
    	fireDetectiveJspFile = fireDetectiveJspFile.substring(fireDetectiveJspFile.lastIndexOf("/") + 1);
    	
    	// Mark start!
    	try { throw new StartTraceException(); } catch (StartTraceException ex) { }
    	
    	// Execute filter
    	Throwable problem = null;
    	try {
    		chain.doFilter(request, response);
    	}
    	catch (Exception ex) {
    		problem = ex;
    	}

    	// Mark end!
    	try { throw new EndTraceException(); } catch (EndTraceException ex) { }
    	
    	// Rethrow possible exception
        if (problem != null) {
            if (problem instanceof ServletException) throw (ServletException)problem;
            if (problem instanceof IOException) throw (IOException)problem;
            // Can't happen...!
        }
    }
    
    
    /**
     * Destroy method for this filter
     *
     */
    public void destroy() {
    }
    
    
    /**
     * Init method for this filter
     *
     */
    public void init(FilterConfig filterConfig) {
    }
    
}
