#!/bin/bash
curl -Ss -k https://localhost:7071/olab/api/v3/maps -H "Authorization: $TOKEN" | jq
