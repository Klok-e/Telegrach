#!/bin/bash

# cd to the directory of the script
# https://stackoverflow.com/a/16349776
cd "${0%/*}"

PYTHON_DIR=../Server/
CSHARP_DIR=../DesktopFrontend/DesktopFrontend/Proto

echo "compiling protobuffers"

# remove old protofiles
rm -rf $PYTHON_DIR/proto $CSHARP_DIR

mkdir -p $PYTHON_DIR $CSHARP_DIR

# compile csharp
protoc --csharp_out=$CSHARP_DIR proto/*.proto

# compile python
# 'proto' folder is the name of the package
# it's created automatically by protobuf
protoc --python_out=$PYTHON_DIR proto/*.proto

echo "protobuffers compilation finished"
