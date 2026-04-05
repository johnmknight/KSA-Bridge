#!/bin/bash

# KSA-Bridge Setup Script for Linux/macOS
# This script verifies prerequisites, builds the mod, and deploys it to KSA

set -e  # Exit on error

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

clear
echo ""
echo "========================================"
echo "  KSA-Bridge Setup"
echo "========================================"
echo ""

# Step 1: Check .NET SDK
echo "[1/5] Checking .NET 10.0 SDK..."
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}ERROR: .NET SDK not found!${NC}"
    echo ""
    echo "Download from: https://dotnet.microsoft.com/download/dotnet"
    echo "Choose: \".NET 10.0 SDK\""
    echo ""
    echo "Or install via package manager:"
    echo "  Ubuntu/Debian: sudo apt install dotnet-sdk-10.0"
    echo "  Fedora/RHEL:   sudo dnf install dotnet-sdk-10.0"
    echo "  Arch:          sudo pacman -S dotnet-sdk-bin"
    echo "  macOS:         brew install dotnet"
    echo ""
    echo "After installing, restart this script."
    exit 1
fi
DOTNET_VERSION=$(dotnet --version)
echo -e "  ${GREEN}✓${NC} Found .NET $DOTNET_VERSION"
echo ""

# Step 2: Check Mosquitto
echo "[2/5] Checking Mosquitto MQTT Broker..."
if ! command -v mosquitto &> /dev/null; then
    echo -e "${RED}ERROR: Mosquitto not found!${NC}"
    echo ""
    echo "Install via package manager:"
    echo "  Ubuntu/Debian: sudo apt install mosquitto mosquitto-clients"
    echo "  Fedora/RHEL:   sudo dnf install mosquitto"
    echo "  Arch:          sudo pacman -S mosquitto"
    echo "  macOS:         brew install mosquitto"
    echo ""
    echo "After installing, restart this script."
    exit 1
fi
echo -e "  ${GREEN}✓${NC} Mosquitto found"
echo ""

# Step 3: Check if Mosquitto is running, start if not
echo "[3/5] Checking if Mosquitto is running..."
if pgrep -x "mosquitto" > /dev/null; then
    echo -e "  ${GREEN}✓${NC} Mosquitto is running"
else
    echo "  ! Mosquitto not running, starting it..."
    mosquitto -c config/mosquitto.conf &
    sleep 2
    if pgrep -x "mosquitto" > /dev/null; then
        echo -e "  ${GREEN}✓${NC} Mosquitto started successfully"
    else
        echo -e "${RED}ERROR: Could not start Mosquitto!${NC}"
        echo ""
        echo "Try running manually:"
        echo "  mosquitto -c config/mosquitto.conf"
        exit 1
    fi
fi
echo ""

# Step 4: Build the mod
echo "[4/5] Building KSA-Bridge mod..."
cd KSA-Bridge
if ! dotnet build --configuration Release; then
    echo -e "${RED}ERROR: Build failed!${NC}"
    echo ""
    echo "Check errors above. Common causes:"
    echo "  - Missing .NET dependencies"
    echo "  - NuGet package download failed (network issue?)"
    exit 1
fi
cd ..
echo -e "  ${GREEN}✓${NC} Build complete"
echo ""

# Step 5: Deploy to KSA mods directory
echo "[5/5] Deploying mod to KSA..."

# Determine KSA mods path based on OS and XDG standards
if [[ "$OSTYPE" == "darwin"* ]]; then
    # macOS
    MODS_PATH="$HOME/Library/Application Support/Kitten Space Agency/mods/KSA-Bridge"
else
    # Linux - use XDG_DATA_HOME or default
    MODS_PATH="${XDG_DATA_HOME:-$HOME/.local/share}/Kitten Space Agency/mods/KSA-Bridge"
fi

# Create directory if it doesn't exist
if ! mkdir -p "$MODS_PATH"; then
    echo -e "${RED}ERROR: Could not create mods directory!${NC}"
    echo "  Path: $MODS_PATH"
    echo ""
    echo "Check permissions or edit this script to set MODS_PATH manually."
    exit 1
fi

# Deploy files
cp -r "KSA-Bridge/bin/Release/net10.0/"* "$MODS_PATH/"
cp "KSA-Bridge/mod.toml" "$MODS_PATH/mod.toml"
cp "KSA-Bridge/ksa-bridge.toml" "$MODS_PATH/ksa-bridge.toml"

echo -e "  ${GREEN}✓${NC} Mod deployed to: $MODS_PATH"
echo ""

# Success
echo "========================================"
echo -e "  ${GREEN}✓ Setup Complete!${NC}"
echo "========================================"
echo ""
echo "Next steps:"
echo ""
echo "1. Launch KSA (from its installation directory)"
echo ""
echo "2. In a new terminal, start the web console:"
echo "   cd examples"
echo "   python3 -m http.server 8088"
echo ""
echo "3. Open in your browser:"
echo "   http://localhost:8088/hard-scifi/hardscifi-fdo-console.html"
echo ""
echo "4. Fly a mission in KSA to see telemetry!"
echo ""
echo "See SETUP.md for detailed troubleshooting."
echo ""
