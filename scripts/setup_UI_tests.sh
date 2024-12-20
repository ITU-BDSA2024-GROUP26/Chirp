#!/bin/bash 


dotnet build tests/Web.UITest 


dotnet build src/Web/ -c Debug 

dotnet publish src/Web/ -c Debug -o tests/Web.UITest/bin/Debug/net8.0