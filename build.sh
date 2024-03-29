#!/bin/bash
set -x
cd ../Common
find . -type d -name bin -ls -exec rm -Rf {} \; > /dev/null
find . -type d -name obj -ls -exec rm -Rf {} \; > /dev/null
git pull
cd ../Api
git pull
service olab46api.$1 stop
find . -type d -name bin -ls -exec rm -Rf {} \;
find . -type d -name obj -ls -exec rm -Rf {} \;
cd WebApiService
if [ ! -L "bin" ]; then
	ln -s /opt/olab46/$1/api bin
fi
cd ..
dotnet clean WebApp.sln
dotnet build -c $1 WebApp.sln
service olab46api.$1 start
service olab46api.$1 status
