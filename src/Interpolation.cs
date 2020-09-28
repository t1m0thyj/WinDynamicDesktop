using System;

namespace WinDynamicDesktop
{
    public enum InterpolationMethod
    {
        None,
        Linear,
        Quad,
        Cubic,
        Quart,
        Quint,
        Sine,
        Circle,
        Exponential
    }

    public static class Interpolation
    {
        public static float Calculate(float value, InterpolationMethod method)
        {
            if (value < 0)
            {
                return 0;
            }
            else if (value > 1)
            {
                return 1;
            }

            switch (method)
            {
                default:
                case InterpolationMethod.None:
                    return 0;

                case InterpolationMethod.Linear:
                    return value;

                case InterpolationMethod.Quad:
                    return InOut(value, Quad);

                case InterpolationMethod.Cubic:
                    return InOut(value, Cubic);

                case InterpolationMethod.Quart:
                    return InOut(value, Quart);

                case InterpolationMethod.Quint:
                    return InOut(value, Quint);

                case InterpolationMethod.Sine:
                    return InOut(value, Sine);

                case InterpolationMethod.Circle:
                    return InOut(value, Circle);

                case InterpolationMethod.Exponential:
                    return InOut(value, Exponential);
            }
        }

        private static float InOut(float value, Func<float,float> func)
        {
            if (value >= 0.5f)
            {
                return (1 - func((1 - value) * 2)) / 2 + 0.5f;

            }
            return func(value * 2) / 2;
        }

        private static float Quad(float value) => (float)Math.Pow(value, 2);
        private static float Cubic(float value) => (float)Math.Pow(value, 3);
        private static float Quart(float value) => (float)Math.Pow(value, 4);
        private static float Quint(float value) => (float)Math.Pow(value, 5);
        private static float Exponential(float value) => ((float)Math.Exp(2 * value) - 1) / ((float)Math.Exp(2) - 1);
        private static float Sine(float value) => 1 - (float)Math.Sin(Math.PI / 2 * (1 - value));
        private static float Circle(float value) => 1 - (float)Math.Sqrt(1.0 - value * value);
    }
}
