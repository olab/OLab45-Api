#!/bin/bash
service olab46api stop
dotnet clean OLab4WebApi.sln
dotnet build -c Release OLab4WebApi.sln
service olab46api start
service olab46api status
