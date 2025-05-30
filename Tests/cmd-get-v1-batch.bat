@echo off & setlocal EnableDelayedExpansion

cls

if "%1"=="" ( 
    set %counter=10000
) else (
    set %counter=%1
)

for /l %%i in (1,1,%counter%) do (
    call:rand 1 1000
    set /a productId = !RAND_NUM!

    call curl --connect-timeout 10 --location --request GET "http://localhost:5167/api/product/!productId!" --header "Content-Type: application/json;""

    echo 'Calling API GET with counter:%%i productId:!productId!'
)

goto:EOF

:rand
SET /A RAND_NUM=%RANDOM% * (%2 - %1 + 1) / 32768 + %1
goto:EOF
