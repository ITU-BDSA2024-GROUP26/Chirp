#!/bin/bash 

echo Execute any possible migrations:
./bundle --connection "Data Source=$DATABASE_PATH"

echo Starting App 
dotnet Web.dll
