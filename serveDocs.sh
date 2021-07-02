#!/bin/sh

export VSINSTALLDIR="C:\Program Files (x86)\Microsoft Visual Studio\2017\Community"
export VisualStudioVersion="15.0"

appdir=$(cd $(dirname $0); pwd)
docdir="${appdir}/docs"

cd "${docdir}"
docfx docfx.json --serve
