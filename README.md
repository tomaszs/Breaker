# Breaker
This is a Visual Studio Express 2013 application i have called Breaker.  It is a simple solution of a problem of changes of the schema of API. Changes of schema (format) of endpoints take time to diagnose in complex applications. With this application you  can easily save schema of endpoints, and check them for any changes in any time.

# Commands:

```
Breaker config.txt
Breaker config.txt save
```

First command checks API for changes. Second saves actual schema of API endpoints for later comparision.

# Structure of generated files:

```
logs
|
--- log-2019-08-05-12_11.txt
snapshot
|
--- endpoint1url.txt
--- endpoint2url.txt
--- schema-current
    |
    --- endpoint1.txt
--- schema-old
    |
    --- endpoint1.txt
```

Logs are saved into logs folder with current time. If 'save' parameter is provided, schema is saved into files, one for each endpoint to snapshot folder. Current schema is saved into 'schema-current' folder. Old schema is saved into 'schema-old' folder.

Since schemas for endpoints are saved into two folders next to each other, it is easy to compare them with file comparision tool, for example WinMerge. Just select "schema-old" and "schema-current" and compare them to see nice information about changes in endpoints.

# config.txt

## first line - authorization phrase

First line should contain authorisation phrase that will be passed in header of each request

## second line - url to strip

URLs are long. When you compare schema-old and schema-current, they don't show entrirely in WinMerge, or other file comparision tools. To make it easier to compare endpoints you provide in second line of a config file, part of the URL that should be stripped. For example if you have endpoints:

```
http://somewebsite.com/datacenter/api/new/version2.0/appnew/getcars/secondcar
http://somewebsite.com/datacenter/api/new/version2.0/appnew/getcars/bestcar
```

Second line should contain "http://somewebsite.com/datacenter/api/new/version2.0/appnew/". That way files in schema-old and schema-current will have these names that are easy to read:

```
getcars_secondcar.txt
getcars_bestcar.txt
```

# next lines - endpoint urls

Following lines should be URLS of endpoints to check. Currently only GET requests are supported.
