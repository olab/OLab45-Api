#!/bin/bash
set -x
service olab46api.$1 stop

pushd ..
find . -type d -name bin -ls -exec rm -Rf {} \; > /dev/null
find . -type d -name obj -ls -exec rm -Rf {} \; > /dev/null

dirprefix=`date '+%Y%m%d-%H%M%S'`
cp -r Common Common$dirprefix
cp -r Api Api$dirprefix
popd

pushd ../Common
git pull
popd

git pull

pushd WebApiService
if [ ! -L "bin" ]; then
	ln -s /opt/olab46/$1/api bin
fi
popd

dotnet clean WebApp.sln
dotnet build -c $1 WebApp.sln

service olab46api.$1 start
service olab46api.$1 status
