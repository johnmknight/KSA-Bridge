#!/bin/bash
# -----------------------------------------------------------------------
# publish-samples.sh
# Publishes sample telemetry to the MQTT broker for console testing.
# Requires: mosquitto_pub (part of the mosquitto-clients package)
# Usage: ./publish-samples.sh [broker_host] [broker_port]
# -----------------------------------------------------------------------

HOST="${1:-127.0.0.1}"
PORT="${2:-1884}"
DIR="$(cd "$(dirname "$0")" && pwd)"

echo "Publishing sample telemetry to $HOST:$PORT ..."
echo ""

# Publish all static telemetry topics
mosquitto_pub -h "$HOST" -p "$PORT" -t ksa/telemetry/vehicle    -f "$DIR/vehicle.json"
echo "  -> ksa/telemetry/vehicle"
mosquitto_pub -h "$HOST" -p "$PORT" -t ksa/telemetry/orbit      -f "$DIR/orbit.json"
echo "  -> ksa/telemetry/orbit"
mosquitto_pub -h "$HOST" -p "$PORT" -t ksa/telemetry/velocity   -f "$DIR/velocity.json"
echo "  -> ksa/telemetry/velocity"
mosquitto_pub -h "$HOST" -p "$PORT" -t ksa/telemetry/altitude   -f "$DIR/altitude.json"
echo "  -> ksa/telemetry/altitude"
mosquitto_pub -h "$HOST" -p "$PORT" -t ksa/telemetry/attitude   -f "$DIR/attitude.json"
echo "  -> ksa/telemetry/attitude"
mosquitto_pub -h "$HOST" -p "$PORT" -t ksa/telemetry/resources  -f "$DIR/resources.json"
echo "  -> ksa/telemetry/resources"
mosquitto_pub -h "$HOST" -p "$PORT" -t ksa/telemetry/maneuver   -f "$DIR/maneuver.json"
echo "  -> ksa/telemetry/maneuver"
mosquitto_pub -h "$HOST" -p "$PORT" -t ksa/telemetry/mission-time -f "$DIR/mission-time.json"
echo "  -> ksa/telemetry/mission-time"

echo ""
echo "All topics published. Console should now show data."
echo "Sending mission time updates every second (Ctrl+C to stop)..."
echo ""

# Keep the console alive by incrementing mission time each second.
# Start from the value in the sample file.
MET=5765
while true; do
    mosquitto_pub -h "$HOST" -p "$PORT" -t ksa/telemetry/mission-time -m "{\"value\": $MET}"
    MET=$((MET + 1))
    sleep 1
done
