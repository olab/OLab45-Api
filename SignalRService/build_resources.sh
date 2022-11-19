#!/bin/bash

if [ $# -eq 0 ]
  then
    echo "No arguments supplied: resourceGroup resourcePrefix"
    exit -1
fi

set -x

resourceGroup=$1
resourcePrefix=$2
signalrName=${resourcePrefix}signalr
region=eastus
azureStorageName=${resourcePrefix}signalraz
dataStorageName=${resourcePrefix}signalr
functionAppName=${resourcePrefix}signalr

# Create a resource group.
az group create --name $resourceGroup --location $region

az signalr create -n $signalrName -g $resourceGroup --service-mode Serverless --sku Standard_S1
# Get connection string for later use.
connectionString=$(az signalr key list -n $signalrName -g $resourceGroup --query primaryConnectionString -o tsv)

# Create an Azure storage account in the resource group.
az storage account create \
--name $azureStorageName \
--location $region \
--resource-group $resourceGroup \
--sku Standard_LRS

# Create an Azure storage account in the resource group.
az storage account create \
  --name $dataStorageName \
  --location $region \
  --resource-group $resourceGroup \
  --sku Standard_LRS

# Create a serverless function app in the resource group.
az functionapp create \
  --name $functionAppName \
  --storage-account $azureStorageName \
  --consumption-plan-location $region \
  --resource-group $resourceGroup \
  --functions-version 4

sleep 30
# If prompted function app version, use --force
func azure functionapp publish $functionAppName

connectionString=$(az signalr key list -n $signalrName -g $resourceGroup --query primaryConnectionString -o tsv)
az functionapp config appsettings set \
  --resource-group $resourceGroup \
  --name $functionAppName \
  --setting AzureSignalRConnectionString=$connectionString

appKey=$(az functionapp keys list --name $functionAppName --resource-group $resourceGroup --query systemKeys -o tsv)
az signalr upstream update \
  -n $signalrName \
  -g $resourceGroup \
  --template url-template="https://${functionAppName}.azurewebsites.net/runtime/webhooks/signalr?code=${appKey}"
