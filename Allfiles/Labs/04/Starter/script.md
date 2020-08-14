# Log in
    az login

    az account set -s "c20a23db-b8be-4b9e-8f54-5b78edddba81"

    $rg=(az group list --query [0].name)

    az configure --defaults location=westeurope group=$rg

# Create Azure SQL Server
    $server="sql-istvan"

    $user="testuser"

    $pw="mySol!dPassw0rd"

    az sql server create -n $server -u $user -p $pw 

# Configure Blob Storage    
## Create containers    
    $storName="storageistvan"   

    az storage account create -n  $storName --sku Standard_LRS

    $storConn=(az storage account show-connection-string -n $storName --query connectionString)

    az storage container create -n images --public-access blob --connection-string $storConn

    az storage container create -n databases --connection-string $storConn

## Upload blobs
### Upload images in batch
    az storage blob upload-batch -d images -s "C:\Users\vitos\Desktop\Work\Azure Course\AZ-204-DevelopingSolutionsforMicrosoftAzure\Allfiles\Labs\04\Starter\Images" --connection-string $storConn
### Upload db backup
    az storage blob upload -c databases -f "C:\Users\vitos\Desktop\Work\Azure Course\AZ-204-DevelopingSolutionsforMicrosoftAzure\Allfiles\Labs\04\Starter\AdventureWorks.bacpac" -n backup --connection-string $storConn

# Configure client IP to access server
## add firewall exception for "65.52.129.125"
Manually add default client IP under Firewall and Virtual networks

# Create Azure SQL Database from backup file stored in blob storage
    $db="sqldbistvan"

    az sql db create -g $rg -s $server -n $db -e Basic

    $storKey=(az storage account keys list -n $storName --query "[0].value")

    $storageUri="https://{0}.blob.core.windows.net/databases/backup" -f $storName

    az sql db import -s $server -n $db -g $rg -p $pw -u $user --storage-key $storKey --storage-key-type StorageAccessKey --storage-uri $storageUri

    --if import fails, add client ip from error message

# Update app settings
## Blob URL
    $blobURL = (az storage account show -n $storName --query primaryEndpoints.blob)

    $blobURL -replace "(?<=.net)/", "/images"

    $sqlConnString = (az sql db show-connection-string -s $server -n $db -c ado.net)

    $sqlConnString -replace "<username>", $user -replace "<password>", $pw

# run the webapp
    dotnet run

# Create CosmosDB resource:
    $cosmosdb="cosmosistvan"

    az cosmosdb create --name $cosmosdb

    az cosmosdb sql database create -a $cosmosdb -n "Retail"

    az cosmosdb sql container create -a $cosmosdb -d "Retail" -n "Online" -p "/category"

## Fetch the connection string to save as app setting 
    az cosmosdb list-connection-strings -n $cosmosdb 

# Use migration tool
## A JSON file (data.json) was prepared by running query.sql in the portal

## Add database name to Cosmos connection string like this:
    "AccountEndpoint=https://cosmosistvan.documents.azure.com:443/;AccountKey=********==;Database=Retail;"

# Read data from CosmosDB
## Go to context project
    cd ..\AdventureWorks.Context\

## Add cosmos package
    dotnet add package Microsoft.Azure.Cosmos --version 3.4.1
