#!/usr/bin/env bash

dotnet publish Alibi -o $1
dotnet publish Alibi.Plugins.Fun -o $1/Plugins
dotnet publish Alibi.Plugins.Webhook -o $1/Plugins
dotnet publish Alibi.Plugins.Cerberus -o $1/Plugins