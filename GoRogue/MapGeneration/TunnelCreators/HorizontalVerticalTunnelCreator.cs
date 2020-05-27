﻿using System;
using System.Collections.Generic;
using GoRogue.MapViews;
using SadRogue.Primitives;
using Troschuetz.Random;

namespace GoRogue.MapGeneration.TunnelCreators
{
    /// <summary>
    /// Implements a tunnel creation algorithm that creates a tunnel that performs all needed
    /// vertical movement before horizontal movement, or vice versa (depending on rng).
    /// </summary>
    public class HorizontalVerticalTunnelCreator : ITunnelCreator
    {
        private readonly IGenerator _rng;

        /// <summary>
        /// Creates a new tunnel creator.
        /// </summary>
        /// <param name="rng">RNG to use for movement selection.</param>
        public HorizontalVerticalTunnelCreator(IGenerator? rng = null)
        {
            _rng = rng ?? Random.GlobalRandom.DefaultRNG;
        }

        /// <inheritdoc/>
        public Area CreateTunnel(ISettableMapView<bool> map, Point tunnelStart, Point tunnelEnd)
        {
            var tunnel = new Area();

            if (_rng.NextBoolean())
            {
                tunnel.Add(createHTunnel(map, tunnelStart.X, tunnelEnd.X, tunnelStart.Y));
                tunnel.Add(createVTunnel(map, tunnelStart.Y, tunnelEnd.Y, tunnelEnd.X));
            }
            else
            {
                tunnel.Add(createVTunnel(map, tunnelStart.Y, tunnelEnd.Y, tunnelStart.X));
                tunnel.Add(createHTunnel(map, tunnelStart.X, tunnelEnd.X, tunnelEnd.Y));
            }

            return tunnel;
        }

        /// <inheritdoc/>
        public Area CreateTunnel(ISettableMapView<bool> map, int startX, int startY, int endX, int endY) => CreateTunnel(map, new Point(startX, startY), new Point(endX, endY));
        private static IEnumerable<Point> createHTunnel(ISettableMapView<bool> map, int xStart, int xEnd, int yPos)
        {
            for (int x = Math.Min(xStart, xEnd); x <= Math.Max(xStart, xEnd); ++x)
            {
                map[x, yPos] = true;
                yield return new Point(x, yPos);
            }
        }

        private static IEnumerable<Point> createVTunnel(ISettableMapView<bool> map, int yStart, int yEnd, int xPos)
        {
            for (int y = Math.Min(yStart, yEnd); y <= Math.Max(yStart, yEnd); ++y)
            {
                map[xPos, y] = true;
                yield return new Point(xPos, y);
            }
        }

    }
}