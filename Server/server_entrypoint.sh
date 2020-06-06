#!/bin/bash

# cd to the directory of the script
# https://stackoverflow.com/a/16349776
cd "${0%/*}"

# if not ran before, setup database
if [ ! -f "$1" ]; then
    echo "creating schema"
    python create_schema.py > /dev/null || exit 1
    echo "schema created"
    echo "blah blah" > "$1"
fi

echo "starting server"
python start_server.py
