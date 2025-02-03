#!/bin/bash
curl -Ss -k --json "{\"Username\":\"$1\",\"Password\":\"$2\"}"  https://localhost:7071/olab/api/v3/auth/login > login.json
cat login.json | jq
export TOKEN="Bearer `cat login.json | jq '.data.authInfo.token'`"
export TOKEN=`echo $TOKEN | tr -d \"`

