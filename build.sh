#!/bin/bash
service olab45api stop
dotnet clean
dotnet build -c Release
service olab45api start
service olab45api status