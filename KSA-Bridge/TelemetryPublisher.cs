using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using KSA;

namespace KSABridge;

/// <summary>
/// Telemetry publisher - publishes vehicle state to MQTT
/// </summary>
public class TelemetryPublisher
{
    private readonly Publisher _publisher;
    private readonly BridgeConfig _config;
    private readonly BridgeState _state;
    private readonly HashSet<string> _vehicleKeys = new();
    private readonly HashSet<string> _orbitKeys = new();
    private readonly HashSet<string> _resourceKeys = new();
    private readonly HashSet<string> _performanceKeys = new();
    private readonly HashSet<string> _situationKeys = new();
    private readonly HashSet<string> _attitudeKeys = new();
    private readonly HashSet<string> _navigationKeys = new();
    private readonly HashSet<string> _stateVectorKeys = new();
    private readonly HashSet<string> _dynamicsKeys = new();
    private readonly HashSet<string> _atmosphereKeys = new();
    private readonly HashSet<string> _maneuverKeys = new();
    private readonly HashSet<string> _encounterKeys = new();
    private readonly HashSet<string> _parentBodyKeys = new();
    private int _parentBodyDumpCount = 0;

    public TelemetryPublisher(Publisher publisher, BridgeConfig config, BridgeState state)
    {
        Console.WriteLine("[KSA-Bridge] TelemetryPublisher constructor called");
        _publisher = publisher;
        _config = config;
        _state = state;
        Console.WriteLine("[KSA-Bridge] TelemetryPublisher initialized successfully");
    }

    public async Task Update()
    {
        try
        {
            var vehicle = GetActiveVehicle();
            if (vehicle == null)
            {
                return;
            }

            await PublishVehicleData(vehicle);
            await PublishOrbitData(vehicle);
            await PublishResourceData(vehicle);
            await PublishPerformanceData(vehicle);
            await PublishSituationData(vehicle);
            await PublishAttitudeData(vehicle);
            await PublishNavigationData(vehicle);
            await PublishStateVectorData(vehicle);
            await PublishDynamicsData(vehicle);
            await PublishAtmosphereData(vehicle);
            await PublishManeuverData(vehicle);
            await PublishEncounterData(vehicle);
            await PublishParentBodyData(vehicle);

            // Periodic dump of parent body to discover rotation fields (first 10 cycles)
            if (_parentBodyDumpCount < 10)
            {
                await DumpParentBody(vehicle);
                _parentBodyDumpCount++;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[KSA-Bridge] Error in Update: {ex.Message}");
        }
    }

    private Vehicle? GetActiveVehicle()
    {
        try
        {
            var vehicle = Program.ControlledVehicle;
            return vehicle;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[KSA-Bridge] Error getting active vehicle: {ex.Message}");
            return null;
        }
    }

    private enum OrbitType
    {
        Elliptical,
        Suborbital,
        Hyperbolic
    }

    // Helper: extract (x, y, z) from KSA vector types (int3, double3, float3) via reflection
    private (double x, double y, double z) ExtractVector(object vec)
    {
        var type = vec.GetType();
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        if (fields.Length >= 3)
        {
            double x = Convert.ToDouble(fields[0].GetValue(vec));
            double y = Convert.ToDouble(fields[1].GetValue(vec));
            double z = Convert.ToDouble(fields[2].GetValue(vec));
            return (x, y, z);
        }
        // Fallback: parse ToString() format "<x, y, z>"
        var s = vec.ToString()?.Trim('<', '>') ?? "0,0,0";
        var parts = s.Split(',');
        if (parts.Length >= 3)
        {
            return (double.Parse(parts[0].Trim()), double.Parse(parts[1].Trim()), double.Parse(parts[2].Trim()));
        }
        return (0, 0, 0);
    }

    private OrbitType GetOrbitType(Vehicle vehicle)
    {
        double e = vehicle.Orbit.Eccentricity;
        double periapsisElevation = vehicle.Orbit.Periapsis - vehicle.Orbit.Parent.MeanRadius;

        if (e >= 1.0)
            return OrbitType.Hyperbolic;
        if (periapsisElevation < 0)
            return OrbitType.Suborbital;
        return OrbitType.Elliptical;
    }

    private async Task PublishVehicleData(Vehicle vehicle)
    {
        try
        {
            var vehicleData = new
            {
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                vehicleName = vehicle.Id,
                parentBody = vehicle.Parent.Id,
                situation = vehicle.Situation.ToString(),
                vehicleRegion = vehicle.VehicleRegion.ToString(),
                speed = vehicle.OrbitalSpeed,
                totalMass = vehicle.TotalMass,
                propellantMass = vehicle.PropellantMass,
                referenceFrame = vehicle.NavBallData.Frame.ToString()
            };

            var topic = $"{_config.TopicPrefix}/telemetry/vehicle";
            TrackKeys(_vehicleKeys, topic, vehicleData);
            var payload = JsonSerializer.Serialize(vehicleData);
            await _publisher.PublishAsync(topic, payload);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[KSA-Bridge] Error publishing vehicle data: {ex.Message}");
        }
    }

    private async Task PublishOrbitData(Vehicle vehicle)
    {
        try
        {
            OrbitType orbitType = GetOrbitType(vehicle);

            double period = vehicle.Orbit.Period;
            double apoapsisElevation = vehicle.Orbit.Apoapsis - vehicle.Orbit.Parent.MeanRadius;
            double periapsisElevation = vehicle.Orbit.Periapsis - vehicle.Orbit.Parent.MeanRadius;
            double eccentricity = vehicle.Orbit.Eccentricity;

            double timeSincePeriapsisSeconds = vehicle.TimeSincePeriapsis.Days * 86400.0;

            double timeToPer;
            if (timeSincePeriapsisSeconds < 0)
                timeToPer = Math.Abs(timeSincePeriapsisSeconds);
            else
                timeToPer = period - timeSincePeriapsisSeconds;

            double timeToApo = timeToPer + (period / 2.0);
            if (timeToApo > period)
                timeToApo -= period;

            object orbit;

            switch (orbitType)
            {
                case OrbitType.Hyperbolic:
                    orbit = new
                    {
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        orbitType = "hyperbolic",
                        parentBody = vehicle.Orbit.Parent.Id,
                        eccentricity,
                        inclination = vehicle.Orbit.Inclination,
                        longitudeOfAscendingNode = vehicle.Orbit.LongitudeOfAscendingNode,
                        argumentOfPeriapsis = vehicle.Orbit.ArgumentOfPeriapsis,
                        semiMajorAxis = vehicle.Orbit.SemiMajorAxis,
                        periapsis = vehicle.Orbit.Periapsis,
                        periapsisElevation,
                        parentRadius = vehicle.Orbit.Parent.MeanRadius,
                        parentMass = vehicle.Orbit.ParentMass
                    };
                    break;

                case OrbitType.Suborbital:
                    orbit = new
                    {
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        orbitType = "suborbital",
                        parentBody = vehicle.Orbit.Parent.Id,
                        apoapsis = vehicle.Orbit.Apoapsis,
                        apoapsisElevation,
                        parentRadius = vehicle.Orbit.Parent.MeanRadius,
                        parentMass = vehicle.Orbit.ParentMass,
                        period,
                        eccentricity,
                        inclination = vehicle.Orbit.Inclination,
                        longitudeOfAscendingNode = vehicle.Orbit.LongitudeOfAscendingNode,
                        argumentOfPeriapsis = vehicle.Orbit.ArgumentOfPeriapsis,
                        semiMajorAxis = vehicle.Orbit.SemiMajorAxis,
                        semiMinorAxis = vehicle.Orbit.SemiMinorAxis,
                        timeToApoapsis = timeToApo,
                        timeToImpact = timeToPer
                    };
                    break;

                case OrbitType.Elliptical:
                default:
                    orbit = new
                    {
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        orbitType = "elliptical",
                        parentBody = vehicle.Orbit.Parent.Id,
                        apoapsis = vehicle.Orbit.Apoapsis,
                        periapsis = vehicle.Orbit.Periapsis,
                        apoapsisElevation,
                        periapsisElevation,
                        parentRadius = vehicle.Orbit.Parent.MeanRadius,
                        parentMass = vehicle.Orbit.ParentMass,
                        period,
                        eccentricity,
                        inclination = vehicle.Orbit.Inclination,
                        longitudeOfAscendingNode = vehicle.Orbit.LongitudeOfAscendingNode,
                        argumentOfPeriapsis = vehicle.Orbit.ArgumentOfPeriapsis,
                        semiMajorAxis = vehicle.Orbit.SemiMajorAxis,
                        semiMinorAxis = vehicle.Orbit.SemiMinorAxis,
                        timeToApoapsis = timeToApo,
                        timeToPeriapsis = timeToPer
                    };
                    break;
            }

            var topic = $"{_config.TopicPrefix}/telemetry/orbit";
            TrackKeys(_orbitKeys, topic, orbit);
            var payload = JsonSerializer.Serialize(orbit);
            await _publisher.PublishAsync(topic, payload);

            _state.LastOrbitData = new OrbitTelemetry
            {
                Apoapsis = vehicle.Orbit.Apoapsis,
                Periapsis = vehicle.Orbit.Periapsis,
                Period = vehicle.Orbit.Period
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[KSA-Bridge] Error publishing orbit data: {ex.Message}");
        }
    }

    private async Task PublishResourceData(Vehicle vehicle)
    {
        try
        {
            var resourceData = new
            {
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                propellantMass = vehicle.PropellantMass,
                totalMass = vehicle.TotalMass,
                dryMass = vehicle.TotalMass - vehicle.PropellantMass,
                massRatio = vehicle.PropellantMass / vehicle.TotalMass
            };

            var topic = $"{_config.TopicPrefix}/telemetry/resources";
            TrackKeys(_resourceKeys, topic, resourceData);
            var payload = JsonSerializer.Serialize(resourceData);
            await _publisher.PublishAsync(topic, payload);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[KSA-Bridge] Error publishing resource data: {ex.Message}");
        }
    }

    private async Task PublishPerformanceData(Vehicle vehicle)
    {
        try
        {
            ref readonly var nav = ref vehicle.NavBallData;
            var performance = new
            {
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                deltaV = nav.DeltaVInVacuum,
                twr = nav.ThrustWeightRatio,
                inertMass = vehicle.InertMass
            };

            var topic = $"{_config.TopicPrefix}/telemetry/performance";
            TrackKeys(_performanceKeys, topic, performance);
            var payload = JsonSerializer.Serialize(performance);
            await _publisher.PublishAsync(topic, payload);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[KSA-Bridge] Error publishing performance data: {ex.Message}");
        }
    }

    private async Task PublishSituationData(Vehicle vehicle)
    {
        try
        {
            var situation = vehicle.LastKinematicStates.Situation;
            var status = new
            {
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                situation = situation.ToString(),
                isLanded = situation.HasTerrainContact(),
                isSplashed = situation.HasOceanContact(),
                isFlying = !situation.HasTerrainContact() && !situation.HasOceanContact()
            };

            var topic = $"{_config.TopicPrefix}/telemetry/situation";
            TrackKeys(_situationKeys, topic, status);
            var payload = JsonSerializer.Serialize(status);
            await _publisher.PublishAsync(topic, payload);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[KSA-Bridge] Error publishing situation data: {ex.Message}");
        }
    }

    private async Task PublishAttitudeData(Vehicle vehicle)
    {
        try
        {
            ref readonly var nav = ref vehicle.NavBallData;
            var angles = ExtractVector(nav.AttitudeAngles);
            var rates = ExtractVector(nav.AttitudeRates);

            var attitude = new
            {
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                heading = angles.x,
                pitch = angles.y,
                roll = angles.z,
                rollRate = rates.x,
                pitchRate = rates.y,
                yawRate = rates.z,
                frame = nav.Frame.ToString()
            };

            var topic = $"{_config.TopicPrefix}/telemetry/attitude";
            TrackKeys(_attitudeKeys, topic, attitude);
            var payload = JsonSerializer.Serialize(attitude);
            await _publisher.PublishAsync(topic, payload);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[KSA-Bridge] Error publishing attitude data: {ex.Message}");
        }
    }

    private async Task PublishNavigationData(Vehicle vehicle)
    {
        try
        {
            ref readonly var nav = ref vehicle.NavBallData;

            var navigation = new
            {
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                altitude = nav.Altitude,
                altitudeKm = nav.Altitude / 1000.0,
                speed = nav.Speed,
                orbitalSpeed = vehicle.OrbitalSpeed,
                programAltitudeKm = Program.CurrentAltitudeKm
            };

            var topic = $"{_config.TopicPrefix}/telemetry/navigation";
            TrackKeys(_navigationKeys, topic, navigation);
            var payload = JsonSerializer.Serialize(navigation);
            await _publisher.PublishAsync(topic, payload);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[KSA-Bridge] Error publishing navigation data: {ex.Message}");
        }
    }

    private async Task PublishStateVectorData(Vehicle vehicle)
    {
        try
        {
            var sv = vehicle.Orbit.StateVectors;
            var svType = sv.GetType();
            var fields = svType.GetFields(BindingFlags.Public | BindingFlags.Instance);

            var pos = (x: 0.0, y: 0.0, z: 0.0);
            var vel = (x: 0.0, y: 0.0, z: 0.0);
            var acc = (x: 0.0, y: 0.0, z: 0.0);
            var jrk = (x: 0.0, y: 0.0, z: 0.0);

            foreach (var field in fields)
            {
                var val = field.GetValue(sv);
                if (val == null || field.FieldType.Name != "double3") continue;
                var vec = ExtractVector(val);
                switch (field.Name)
                {
                    case "PositionCci": pos = vec; break;
                    case "VelocityCci": vel = vec; break;
                    case "AccelerationCci": acc = vec; break;
                    case "JerkCci": jrk = vec; break;
                }
            }

            var stateVectors = new
            {
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                positionX = pos.x,
                positionY = pos.y,
                positionZ = pos.z,
                velocityX = vel.x,
                velocityY = vel.y,
                velocityZ = vel.z,
                accelerationX = acc.x,
                accelerationY = acc.y,
                accelerationZ = acc.z,
                jerkX = jrk.x,
                jerkY = jrk.y,
                jerkZ = jrk.z
            };

            var topic = $"{_config.TopicPrefix}/telemetry/state_vectors";
            TrackKeys(_stateVectorKeys, topic, stateVectors);
            var payload = JsonSerializer.Serialize(stateVectors);
            await _publisher.PublishAsync(topic, payload);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[KSA-Bridge] Error publishing state vector data: {ex.Message}");
        }
    }

    private async Task PublishDynamicsData(Vehicle vehicle)
    {
        try
        {
            var bodyRates = ExtractVector(vehicle.BodyRates);
            var accBody = ExtractVector(vehicle.AccelerationBody);
            var angAccBody = ExtractVector(vehicle.AngularAccelerationBody);
            ref readonly var km = ref vehicle.KinematicMeasurements;

            var dynamics = new
            {
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                bodyRateX = bodyRates.x,
                bodyRateY = bodyRates.y,
                bodyRateZ = bodyRates.z,
                accelBodyX = accBody.x,
                accelBodyY = accBody.y,
                accelBodyZ = accBody.z,
                angAccelX = angAccBody.x,
                angAccelY = angAccBody.y,
                angAccelZ = angAccBody.z,
                propellantMassFlowRate = km.PropellantMassFlowRate
            };

            var topic = $"{_config.TopicPrefix}/telemetry/dynamics";
            TrackKeys(_dynamicsKeys, topic, dynamics);
            var payload = JsonSerializer.Serialize(dynamics);
            await _publisher.PublishAsync(topic, payload);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[KSA-Bridge] Error publishing dynamics data: {ex.Message}");
        }
    }

    private async Task PublishAtmosphereData(Vehicle vehicle)
    {
        try
        {
            var ks = vehicle.LastKinematicStates;

            var atmosphere = new
            {
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                atmosphericDensity = ks.AtmosphericDensity,
                atmosphericPressure = ks.AtmosphericPressure,
                oceanDensity = ks.OceanDensity,
                terrainRadius = ks.TerrainRadius,
                oceanRadius = ks.OceanRadius,
                totalSurfaceArea = ks.TotalSurfaceArea,
                totalVolume = ks.TotalVolume,
                draft = ks.Draft
            };

            var topic = $"{_config.TopicPrefix}/telemetry/atmosphere";
            TrackKeys(_atmosphereKeys, topic, atmosphere);
            var payload = JsonSerializer.Serialize(atmosphere);
            await _publisher.PublishAsync(topic, payload);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[KSA-Bridge] Error publishing atmosphere data: {ex.Message}");
        }
    }

    private async Task PublishManeuverData(Vehicle vehicle)
    {
        try
        {
            var burnPlan = vehicle.BurnPlan;
            var flightPlan = vehicle.FlightPlan;

            var maneuver = new
            {
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                burnCount = burnPlan.BurnCount,
                hasActiveBurns = burnPlan.HasActiveBurns,
                burnGizmoActive = burnPlan.BurnGizmoActive,
                flightPlanComplete = flightPlan.IsComplete
            };

            var topic = $"{_config.TopicPrefix}/telemetry/maneuver";
            TrackKeys(_maneuverKeys, topic, maneuver);
            var payload = JsonSerializer.Serialize(maneuver);
            await _publisher.PublishAsync(topic, payload);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[KSA-Bridge] Error publishing maneuver data: {ex.Message}");
        }
    }

    private async Task PublishEncounterData(Vehicle vehicle)
    {
        try
        {
            var patch = vehicle.Patch;

            var encounter = new
            {
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                hasEncounter = patch.HasEncounter,
                hasCloseApproach = patch.HasCloseApproach,
                closestApproachDistance = patch.HasCloseApproach ? patch.ClosestApproachDistance : -1.0,
                primaryBody = patch.PrimaryBody.Id
            };

            var topic = $"{_config.TopicPrefix}/telemetry/encounter";
            TrackKeys(_encounterKeys, topic, encounter);
            var payload = JsonSerializer.Serialize(encounter);
            await _publisher.PublishAsync(topic, payload);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[KSA-Bridge] Error publishing encounter data: {ex.Message}");
        }
    }

    private async Task PublishParentBodyData(Vehicle vehicle)
    {
        try
        {
            var parent = vehicle.Orbit.Parent;

            // Phase 1: Only known-safe properties + angular velocity via interface method
            var parentBodyData = new Dictionary<string, object>
            {
                ["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                ["parentId"] = parent.Id,
                ["meanRadius"] = parent.MeanRadius,
                ["mass"] = parent.Mass,
                ["sphereOfInfluence"] = parent.SphereOfInfluence
            };

            // Try GetAngularVelocity() - declared on IParentBody
            try
            {
                parentBodyData["angularVelocity"] = parent.GetAngularVelocity();
            }
            catch (Exception ex)
            {
                parentBodyData["angularVelocityError"] = ex.Message;
            }

            // Try GetCcf2Cci() - declared on IParentBody, returns doubleQuat
            try
            {
                var q = parent.GetCcf2Cci();
                var s = q.ToString() ?? "";
                parentBodyData["ccf2CciRaw"] = s;
                // Parse "<x, y, z, w>"
                var clean = s.Trim('<', '>', ' ');
                var parts = clean.Split(',');
                if (parts.Length >= 4)
                {
                    parentBodyData["rotationQuatX"] = double.Parse(parts[0].Trim());
                    parentBodyData["rotationQuatY"] = double.Parse(parts[1].Trim());
                    parentBodyData["rotationQuatZ"] = double.Parse(parts[2].Trim());
                    parentBodyData["rotationQuatW"] = double.Parse(parts[3].Trim());
                }
            }
            catch (Exception ex)
            {
                parentBodyData["ccf2CciError"] = ex.Message;
            }

            // Try GetAxialTilt() via reflection (on Celestial, not IParentBody)
            try
            {
                var method = parent.GetType().GetMethod("GetAxialTilt");
                if (method != null)
                {
                    var result = method.Invoke(parent, null);
                    if (result != null) parentBodyData["axialTilt"] = Convert.ToDouble(result);
                }
            }
            catch (Exception ex)
            {
                parentBodyData["axialTiltError"] = ex.Message;
            }

            var topic = $"{_config.TopicPrefix}/telemetry/parent_body";
            TrackKeys(_parentBodyKeys, topic, parentBodyData);
            var payload = JsonSerializer.Serialize(parentBodyData);
            await _publisher.PublishAsync(topic, payload);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[KSA-Bridge] Error publishing parent body data: {ex.Message}");
            // Publish error to MQTT for remote diagnosis
            try
            {
                var errPayload = JsonSerializer.Serialize(new { error = ex.Message, type = ex.GetType().Name, stack = ex.StackTrace?.Substring(0, Math.Min(ex.StackTrace?.Length ?? 0, 500)) });
                await _publisher.PublishAsync($"{_config.TopicPrefix}/debug/parent_body_error", errPayload);
            }
            catch { }
        }
    }

    private async Task DumpParentBody(Vehicle vehicle)
    {
        try
        {
            var parent = vehicle.Orbit.Parent;
            var parentType = parent.GetType();
            Console.WriteLine($"\n=== PARENT BODY DEEP DUMP ===");
            Console.WriteLine($"Type: {parentType.FullName}");

            var lines = new List<string>();
            lines.Add($"parentType: {parentType.FullName}");

            // Dump all interfaces
            var interfaces = parentType.GetInterfaces();
            foreach (var iface in interfaces)
            {
                lines.Add($"interface: {iface.Name}");
            }

            // Dump all properties (including inherited)
            var props = parentType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in props)
            {
                try
                {
                    var val = prop.GetValue(parent);
                    var valStr = val?.ToString() ?? "null";
                    if (valStr.Length > 200) valStr = valStr.Substring(0, 200) + "...";
                    lines.Add($"prop {prop.Name} ({prop.PropertyType.Name}): {valStr}");
                    Console.WriteLine($"  prop {prop.Name} ({prop.PropertyType.Name}): {valStr}");
                }
                catch (Exception ex)
                {
                    lines.Add($"prop {prop.Name} ({prop.PropertyType.Name}): [Error: {ex.Message}]");
                    Console.WriteLine($"  prop {prop.Name} ({prop.PropertyType.Name}): [Error: {ex.Message}]");
                }
            }

            // Dump all fields (including inherited)
            var fields = parentType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                try
                {
                    var val = field.GetValue(parent);
                    var valStr = val?.ToString() ?? "null";
                    if (valStr.Length > 200) valStr = valStr.Substring(0, 200) + "...";
                    lines.Add($"field {field.Name} ({field.FieldType.Name}): {valStr}");
                    Console.WriteLine($"  field {field.Name} ({field.FieldType.Name}): {valStr}");
                }
                catch (Exception ex)
                {
                    lines.Add($"field {field.Name} ({field.FieldType.Name}): [Error: {ex.Message}]");
                    Console.WriteLine($"  field {field.Name} ({field.FieldType.Name}): [Error: {ex.Message}]");
                }
            }

            // Also dump methods (names only) to see if there are rotation-related methods
            var methods = parentType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var method in methods)
            {
                if (method.DeclaringType == typeof(object)) continue;
                var paramStr = string.Join(", ", method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
                lines.Add($"method {method.Name}({paramStr}) -> {method.ReturnType.Name}");
            }

            Console.WriteLine($"=== END PARENT BODY DUMP ===\n");

            // Publish to MQTT for easy capture
            var payload = JsonSerializer.Serialize(new { dump = lines });
            await _publisher.PublishAsync($"{_config.TopicPrefix}/debug/parent_body", payload);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[KSA-Bridge] Error dumping parent body: {ex.Message}");
            Console.WriteLine($"  Stack: {ex.StackTrace}");
        }
    }

    private void TrackKeys(HashSet<string> keySet, string topic, object data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data);
            var doc = JsonDocument.Parse(json);
            var keys = doc.RootElement.EnumerateObject().Select(p => p.Name).ToList();

            var newKeys = keys.Where(k => !keySet.Contains(k)).ToList();
            foreach (var key in newKeys)
            {
                keySet.Add(key);
            }

            if (newKeys.Any())
            {
                Console.WriteLine($"[KSA-Bridge] New keys on {topic}: {string.Join(", ", newKeys)}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[KSA-Bridge] Error tracking keys: {ex.Message}");
        }
    }
}