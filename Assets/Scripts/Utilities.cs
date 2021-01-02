using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Utilities
{
    namespace ColorManipulation
    {
        public static class ColorManipulation
        {
            public static Color InvertColor(Color color)
            {
                Color newColor = new Color(1 - color.r, 1 - color.g, 1 - color.b, 1);
                return newColor;
            }
            public static Color mixColors(Color a, Color b)
            {
                Color newColor = (a + b) / 2;

                return newColor;
            }
        }

    }
    namespace Math
    {
        class Math
        {
            public static float Average(float a, float b)
            {
                return (float)(a + b) / 2;
            }
            public static int Average(int a, int b)
            {
                return (a + b) / 2;
            }
            public static Vector2 Average (Vector2 a, Vector2 b)
            {
                return (a + b) / 2;
            }
        }
    }
}
