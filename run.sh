#!/bin/bash

cd 3DRender

# Restore dependencies
dotnet Restore

# Build project
dotnet build --configuration Release

cd ..

directory="3DRender\\bin\\Release\\net8.0"

if [ -d "$directory" ]; then
    echo "Directory $directory found"
    cd 3DRender\\bin\\Release\\net8.0
    # Run project
    dotnet exec 3DRender.dll
else
    echo "Unable to find directory $directory"
fi
