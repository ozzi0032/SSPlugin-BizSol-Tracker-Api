# SSPlugin-BizSol-Tracker-Api
Smart Store Plugin named "BizSol Tracker"

Login through Plugin
Plugin overrides the default Login and performs Login through the custom controller which accepts three string parameters [nam,pass,returnUrl]

nam -> UsernameOrEmail
pass -> Password
returnUrl -> URL/Route to the action if the loign is failed due to invalid credentials

Login through Plugin Api
In this module one can login into Smart Store through the Plugin Web Api
Api accepts two string parameters [username,password]
If the user exists Api will perform login and return the Logged User data


