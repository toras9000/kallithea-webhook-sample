#!/bin/bash

# python bin
PYTHON_BIN=python3

# packages path
PYTHON_PACKAGES=$(su-exec kallithea:kallithea $PYTHON_BIN -m site --user-site)

# kallithea installation directory
KALLITEHA_INSTALL_DIR=$PYTHON_PACKAGES/kallithea

# overwrite files
if [ -n "$KALLITHEA_EXTERNAL_DB" ] && [ -d "$KALLITHEA_OVERRIDE_DIR/kallithea" ]; then
    echo "Copy override files..."
    cp -v -RT "$KALLITHEA_OVERRIDE_DIR/kallithea"  "$KALLITEHA_INSTALL_DIR"
fi

# patch files
if [ -n "$KALLITHEA_PATCH_DIR" ] && [ -d "$KALLITHEA_PATCH_DIR" ]; then
    echo "Apply patches..."
    git -C "$KALLITEHA_INSTALL_DIR" apply --reject --whitespace=fix -p2 $KALLITHEA_PATCH_DIR/*
fi

# Copy extensions template
if [ -f "$KALLITHEA_EXTENSIONS_TEMPLATE" ] && [ ! -e "kallithea/config/extensions.py" ]; then
    cp "$KALLITHEA_EXTENSIONS_TEMPLATE" "kallithea/config/extensions.py"
    if [ -n "$KALLITHEA_WEBHOOK_ALLOW_ADDRS" ]; then
        allow_list=($KALLITHEA_WEBHOOK_ALLOW_ADDRS)
        allow_list=$(printf "'%s', " "${allow_list[@]}")
        sed -ri "s|^\\s*webhook_allow_list\\s*=\\s*\\('http://dummy.placeholder.example'\\)\\s*\$|webhook_allow_list = \\(${allow_list}\\)|1" "/kallithea/config/extensions.py"
    fi

fi

# normal startup
exec bash /kallithea/startup.sh

