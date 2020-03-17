#!/bin/bash

# cd to the directory of the script
# https://stackoverflow.com/a/16349776
cd "${0%/*}"

PYTHON_DIR=../Server/proto
CSHARP_DIR=../DesktopFrontend/DesktopFrontend/Proto

# remove old protofiles
rm -rf $PYTHON_DIR $CSHARP_DIR

mkdir -p $PYTHON_DIR $CSHARP_DIR

protoc --csharp_out=$CSHARP_DIR --python_out=$PYTHON_DIR *.proto
