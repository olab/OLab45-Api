#!/bin/bash

if [ $# -eq 0 ]
  then
    echo "No arguments supplied: debug/release"
    exit
fi

if [ $1 = "debug" ]; then
	export ASPNETCORE_ENVIRONMENT=Development
fi

pushd ./WebApiService/bin/$1/net7.0
./OLabWebApi  --urls http://localhost:7071
popd
