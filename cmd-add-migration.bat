 @echo off

if "%1"=="" ( 
    echo Parameter Migration name is not provided
    exit /b 1 
)

cls

dotnet build && dotnet ef migrations add %1 --project AlzaShopApi\AlzaShopApi.csproj