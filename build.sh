#!/bin/bash

if [ $# != 2 ]; then
	echo "Usage: $0 <project_name> <snk_path>"
	exit 1
fi

project_name=$1
snk_path=$2

currentDir=$(pwd)

navigateAndBuild() {
	cd $1 # Navigate to the project directory
	dotnet clean -c Release
	dotnet build -c Release -p:SignAssembly="$snk_path"
	cd $currentDir # Navigate back to the root directory
}

if [ $project_name == "main" ]; then
	navigateAndBuild src/Sharpify/
elif [ $project_name == "data" ]; then
	navigateAndBuild src/Sharpify.Data/
elif [ $project_name == "cli" ]; then
	navigateAndBuild src/Sharpify.CommandLineInterface/
fi