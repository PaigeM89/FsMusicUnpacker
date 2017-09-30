#!/bin/bash

.paket/paket.exe restore

./packages/FAKE/tools/Fake.exe build.fsx $1
