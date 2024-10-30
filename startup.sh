#!/bin/bash 

echo Execute any possible migrations:
./bundle --connection {$DATABASE_PATH}

echo Starting App 
dotnet dotnet Chirp.Web.dll