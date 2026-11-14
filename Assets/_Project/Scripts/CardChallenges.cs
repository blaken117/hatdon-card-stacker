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

            internal Layout Tent(float x, float z, float degrees = 28f, float heading = 0f)
            {
                float radians = degrees * Mathf.Deg2Rad;
                float foot = (.088f * Mathf.Sin(radians) + CardPlacementSession.CardThickness * Mathf.Cos(radians)) / 2f;
                float height = (.088f * Mathf.Cos(radians) + CardPlacementSession.CardThickness * Mathf.Sin(radians)) / 2f + .000005f;
                Quaternion turn = Quaternion.Euler(0, heading, 0);
                foreach (int side in new[] { -1, 1 })
                {
                    Vector3 position = new Vector3(x, 0, z) + turn * new Vector3(side * foot, height, 0);
                    Quaternion rotation = turn * Quaternion.AngleAxis(side * degrees, Vector3.forward)
                        * Quaternion.Euler(0, -side * 90, 0);
                    Cards.Add(new Pose(position, rotation));
                }
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
