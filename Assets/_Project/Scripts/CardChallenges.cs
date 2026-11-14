using System.Collections.Generic;
using UnityEngine;

namespace Hatdon.CardStacker
{
    internal static class CardChallenges
    {
        internal sealed class Layout
        {
            internal readonly string Name;
            internal readonly string Hint;
            internal readonly List<Pose> Cards = new List<Pose>();

            internal Layout(string name, string hint) { Name = name; Hint = hint; }

            internal float Height { get; private set; }
            internal int Floors { get; private set; } = 1;

            internal Layout Tent(float x, float z, float degrees = 28f, float heading = 0f, float floor = 0)
            {
                float radians = degrees * Mathf.Deg2Rad;
                float foot = (.088f * Mathf.Sin(radians) + CardPlacementSession.CardThickness * Mathf.Cos(radians)) / 2f;
                float height = (.088f * Mathf.Cos(radians) + CardPlacementSession.CardThickness * Mathf.Sin(radians)) / 2f + .000005f;
                Quaternion turn = Quaternion.Euler(0, heading, 0);
                foreach (int side in new[] { -1, 1 })
                {
                    Vector3 position = new Vector3(x, floor, z) + turn * new Vector3(side * foot, height, 0);
                    Quaternion rotation = turn * Quaternion.AngleAxis(side * degrees, Vector3.forward)
                        * Quaternion.Euler(0, -side * 90, 0);
                    Cards.Add(new Pose(position, rotation));
                }
                Height = Mathf.Max(Height,floor + 2f * height);
                return this;
            }

            internal Layout Tower(float x, float z, float heading, params int[] rows)
            {
                const float spacing = .087f;
                const float lean = 22f;
                float angle = lean * Mathf.Deg2Rad;
                float rise = .088f * Mathf.Cos(angle) + CardPlacementSession.CardThickness * Mathf.Sin(angle);
                float floor = 0;
                float edge = -(rows[0] - 1) * spacing / 2f;
                Quaternion turn = Quaternion.Euler(0,heading,0);
                for (int row = 0; row < rows.Length; row++)
                {
                    for (int pair = 0; pair < rows[row]; pair++)
                    {
                        Vector3 offset = turn * new Vector3(edge + pair * spacing,0,0);
                        Tent(x + offset.x,z + offset.z,lean,heading,floor);
                    }
                    if (row == rows.Length - 1) break;
                    for (int bridge = 0; bridge < rows[row + 1]; bridge++)
                    {
                        Vector3 offset = turn * new Vector3(edge + (bridge + .5f) * spacing,0,0);
                        Flat(x + offset.x,z + offset.z,heading + 90,floor + rise + .000035f + (bridge % 2)*.00032f);
                    }
                    floor += rise + 2f*CardPlacementSession.CardThickness + .00006f;
                    edge += spacing / 2f;
                }
                Floors = Mathf.Max(Floors,rows.Length);
                return this;
            }

            internal Layout Flat(float x, float z, float heading = 0f, float height = 0f)
            {
                Cards.Add(new Pose(new Vector3(x, height + CardPlacementSession.CardThickness / 2f, z),
                    Quaternion.Euler(0, heading, 0) * Quaternion.Euler(-90, 0, 0)));
                return this;
            }
        }

        private const float RoofHeight = .080f;

        internal static readonly Layout[] Levels =
        {
            new Layout("First Fold", "Lean two cards together to form the base.").Tent(0,0),
            new Layout("Bridge Base", "Lay one flat card across two leaning pairs.").Tent(-.043f,0).Tent(.043f,0).Flat(0,0,90,RoofHeight),
            new Layout("Second Floor", "Build a leaning pair on top of the bridge.").Tower(0,0,0,2,1),
            new Layout("Side Stair", "Start with three pairs. Build the upper floor at one end.").Tower(0,0,0,3,1),
            new Layout("Crossed Towers", "Build two separate towers facing different directions.")
                .Tower(-.097f,0,25,2,1).Tower(.097f,0,-25,2,1),
            new Layout("Three Floors", "Build a pyramid with three pairs at the base.").Tower(0,0,0,3,2,1),
            new Layout("Corner Peak", "Use a wide base and build the third floor off centre.").Tower(0,0,-25,4,2,1),
            new Layout("Twin Summits", "Build two three-floor pyramids in parallel.").Tower(0,-.063f,0,3,2,1).Tower(0,.063f,0,3,2,1),
            new Layout("Four Floors", "Work upward from four pairs to a single peak.").Tower(0,0,0,4,3,2,1),
            new Layout("Grand Terrace", "Build a four-floor pyramid and a lower tower in front.").Tower(0,0,0,4,3,2,1)
                .Tower(.074f,-.13f,0,2,1)
        };

        internal static readonly Layout[] Classic =
        {
            new Layout("First Fold", "Lean two cards together to make a freestanding triangle.").Tent(0, 0),
            new Layout("Twin Peaks", "Build two separate triangles side by side.").Tent(-.075f, 0).Tent(.075f, 0),
            new Layout("The Turn", "Build two triangles facing different directions.").Tent(-.075f, 0, 26, -30).Tent(.075f, 0, 30, 40),
            new Layout("The Bridge", "Balance a flat card across the tops. Release it on its own.").Tent(-.043f, 0).Tent(.043f, 0).Flat(0, 0, 90, RoofHeight),
            new Layout("The Gate", "Build a triangle between two flat marker cards.").Tent(0, .035f, 30).Flat(-.083f, -.035f).Flat(.083f, -.035f),
            new Layout("Three Peaks", "Build three triangles in a row.").Tent(-.12f, 0, 26).Tent(0, 0, 28).Tent(.12f, 0, 30),
            new Layout("The Courtyard", "Build four triangles around an open centre.").Tent(-.094f, -.068f, 28, 20).Tent(.094f, -.068f, 28, -20)
                .Tent(-.094f, .068f, 28, -20).Tent(.094f, .068f, 28, 20),
            new Layout("Covered Walk", "Bridge both pairs of triangles in two parallel rows.").Tent(-.043f, -.067f).Tent(.043f, -.067f).Flat(0, -.067f, 90, RoofHeight)
                .Tent(-.043f, .067f).Tent(.043f, .067f).Flat(0, .067f, 90, RoofHeight),
            new Layout("The Switchback", "Follow the zigzag with five alternating triangles.").Tent(-.16f, -.055f, 28, 20).Tent(-.08f, .055f, 28, -20)
                .Tent(0, -.055f, 28, 20).Tent(.08f, .055f, 28, -20).Tent(.16f, -.055f, 28, 20),
            new Layout("Grand Pavilion", "Build three bridges. Starting at the back helps.").Tent(-.043f, .117f).Tent(.043f, .117f).Flat(0, .117f, 90, RoofHeight)
                .Tent(-.043f, 0).Tent(.043f, 0).Flat(0, 0, 90, RoofHeight).Tent(-.043f, -.117f).Tent(.043f, -.117f).Flat(0, -.117f, 90, RoofHeight)
        };

        internal static bool Matches(Layout layout, List<Rigidbody> candidates, int[] matches, bool[] visited)
        {
            if (candidates.Count < layout.Cards.Count) return false;
            for (int i = 0; i < candidates.Count; i++) matches[i] = -1;
            for (int i = 0; i < layout.Cards.Count; i++)
            {
                System.Array.Clear(visited, 0, candidates.Count);
                if (!Assign(i, layout, candidates, matches, visited)) return false;
            }
            return true;
        }

        private static bool Assign(int targetIndex, Layout layout, List<Rigidbody> cards, int[] matches, bool[] visited)
        {
            Pose target = layout.Cards[targetIndex];
            for (int i = 0; i < cards.Count; i++)
            {
                var card = cards[i];
                if (visited[i] || (card.position - target.position).sqrMagnitude > .012f * .012f) continue;
                // Treat opposite faces and either end of a playing card as equivalent.
                float alignment = Mathf.Cos(13f * Mathf.Deg2Rad);
                if (Mathf.Abs(Vector3.Dot(card.rotation * Vector3.up, target.rotation * Vector3.up)) < alignment ||
                    Mathf.Abs(Vector3.Dot(card.rotation * Vector3.forward, target.rotation * Vector3.forward)) < alignment) continue;
                visited[i] = true;
                if (matches[i] >= 0 && !Assign(matches[i], layout, cards, matches, visited)) continue;
                matches[i] = targetIndex;
                return true;
            }
            return false;
        }
    }
}
