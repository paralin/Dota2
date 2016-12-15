#!/bin/bash

find . -name '*.cs' -exec sed -i 's/global::System.Serializable, //g' {} \;
