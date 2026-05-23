#!/bin/sh
rm -rf bin obj
dotnet build
dotnet run --no-build
