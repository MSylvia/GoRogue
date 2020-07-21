﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using GoRogue.MapViews;
using SadRogue.Primitives;

namespace GoRogue.Debugger
{
    /// <summary>
    /// The "Interpreter" is a class designed to sit between GoRogue
    /// and System.Console. It prints output directly to the console,
    /// allowing debugging directly in a terminal.
    /// </summary>
    public static class Interpreter
    {
        private static bool _exit; // Used to decide whether or not to exit the program
        private static bool _dirty = true; // Whether or not to redraw the map

        // The routine that we're running in this test.  Null override because we initialize in Init
        private static IRoutine _routine = null!;

        // Viewport of routine's map.  Null override because we initialize in Init
        private static RoutineViewport _mapView = null!;

        #region Setup
        /// <summary>
        /// Get the Routine, Interpreter, and Map ready for action.
        /// </summary>
        public static void Init()
        {
            // Pick routine, set up its map, and create its map views.
            _routine = PickRoutine();
            _routine.GenerateMap();

            // Set up map's views and select first one automatically
            _routine.CreateViews();
            if (_routine.Views.Count == 0)
                throw new Exception("Selected map defines 0 views.");

            // Set up viewport, defaulting to first item in views list
            _mapView = new RoutineViewport(_routine, Console.WindowWidth - 1, Console.WindowHeight - 1);

            Console.WriteLine("Initialized...");
        }

        /// <summary>
        /// Picks a routine out of the possible candidates
        /// </summary>
        /// <returns>The Routine we're running in this test</returns>
        private static IRoutine PickRoutine()
        {
            Console.WriteLine("Pick your routine using numbers");
            List<IRoutine> routines = GetRoutines();
            int i = 0;
            foreach (var routine in routines)
            {
                Console.WriteLine(i + ") " + routine.Name);
                i++;
            }
            var key = Console.ReadKey().Key;
            try
            {
                string number = key.ToString().Replace("D", "").Replace("NumPad", "");
                i = int.Parse(number);
                return routines[i];
            }
            catch (Exception)
            {
                Console.WriteLine("Could not understand input; please use numbers");
                return PickRoutine();
            }
        }

        /// <summary>
        /// Gets all of the classes that implement IRoutine
        /// </summary>
        /// <returns>A list of all routines available</returns>
        private static List<IRoutine> GetRoutines()
        {
            List<IRoutine> objects = new List<IRoutine>();
            var types = Assembly.GetAssembly(typeof(IRoutine))?.GetTypes() ?? Array.Empty<Type>();
            types = types.Where(t => t.GetInterface(nameof(IRoutine)) != null).ToArray();
            foreach (Type type in types)
            {
                var instance = Activator.CreateInstance(type) as IRoutine ??
                               throw new Exception("Failed to create instance of routine.");
                objects.Add(instance);
            }
            objects.Sort();
            return objects;
        }
        #endregion

        #region Run

        /// <summary>
        /// Start listening for keypresses
        /// </summary>
        public static void Run()
        {
            while (!_exit)
            {
                if (_dirty)
                    DrawMap();

                InterpretKeyPress();
            }
        }

        #endregion

        #region UI
        private static void InterpretKeyPress()
        {
            Direction moveViewportDir = Direction.None;
            ConsoleKey key = Console.ReadKey().Key;
            switch (key)
            {
                case ConsoleKey.Escape:
                    _exit = true;
                    break;
                case ConsoleKey.Spacebar:
                    _routine?.ElapseTimeUnit();
                    _dirty = true;
                    break;
                case ConsoleKey.UpArrow:
                    moveViewportDir = Direction.Up;
                    break;
                case ConsoleKey.DownArrow:
                    moveViewportDir = Direction.Down;
                    break;
                case ConsoleKey.LeftArrow:
                    moveViewportDir = Direction.Left;
                    break;
                case ConsoleKey.RightArrow:
                    moveViewportDir = Direction.Right;
                    break;
                case ConsoleKey.OemPlus:
                    _mapView.NextView();
                    _dirty = true;
                    break;
                case ConsoleKey.OemMinus:
                    _mapView.PreviousView();
                    _dirty = true;
                    break;
            }

            if (moveViewportDir != Direction.None)
                _dirty = _mapView.CenterViewOn(_mapView.CurrentViewport.ViewArea.Center + moveViewportDir);
        }

        private static void DrawMap()
        {
            // Calculate available console space.  Make sure to subtract one to ensure we actually fit
            // text instead of going 1 over with newline.
            int width = Console.WindowWidth - 1;
            int height = Console.WindowHeight - 1;

            // Resize viewport as needed to match console size
            _mapView.ResizeViewport(width, height);

            // Draw viewport, ensuring to allow no space between characters
            Console.WriteLine(_mapView.CurrentViewport.ExtendToString(elementSeparator: ""));

            // Reset dirty flag because we just drew
            _dirty = false;
        }
        #endregion
    }
}
