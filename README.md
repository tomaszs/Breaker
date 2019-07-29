# Breaker
This is a Visual Studio Express 2013 application i have called Breaker.  It is a simple solution of a problem of changes of the schema of API. Changes of schema (format) of endpoints take time to diagnose in complex applications. With this application you  can easily save schema of endpoints, and check them for any changes in any time.

How to use the application:

'Breaker config.txt'

Calls Breaker and executes it with configuration in a config.txt file. If there is a difference between previously saved schema of endpoint and a new endpoint it is displayed in the console. Also it is saved into a log file.

Breaker config.txt save

Saves the new schema into schema description files

Configuration file:

First line should contain authorisation phrase that will be passed in header of each request
Following lines should be URLS of endpoints to check
