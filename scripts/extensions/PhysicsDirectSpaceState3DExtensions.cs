using System.Collections.Generic;
using Godot;
using Godot.Collections;

public static class PhysicsDirectSpaceState3DExtensions {
    public static RayIntersection ProjectRay(this PhysicsDirectSpaceState3D state, PhysicsRayQueryParameters3D query) {
        Dictionary result = state.IntersectRay(query);
        if (result.IsEmpty()) {
            return null;
        } else {
            return new RayIntersection(result);
        }
    }

    public static List<RayIntersection> ProjectRayAll(this PhysicsDirectSpaceState3D state, PhysicsRayQueryParameters3D query, int maxResults = 100) {
        var savedExclusions = query.Exclude;
        var currentExclusions = new Array<Rid>(savedExclusions);
        query.Exclude = currentExclusions;
        var results = new List<RayIntersection>();

        Dictionary result;
        while (maxResults > 0 && (result = state.IntersectRay(query)).NotEmpty()) {
            var intersection = new RayIntersection(result);

            results.Add(intersection);

            currentExclusions.Add(intersection.RID);
            query.Exclude = currentExclusions;

            --maxResults;
        }

        /*
        Logger.Debug($"Found {results.Count} results");
        foreach (var intersection in results) {
            Logger.Debug("   " + intersection);
        }
        */

        query.Exclude = savedExclusions;
        return results;
    }
}