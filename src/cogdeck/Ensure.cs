// Copyright (c) Microsoft. All rights reserved.

namespace cogdeck
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Helper methods for argument validation.
    /// </summary>
    internal static class Ensure
    {
        private const string MissingValueName = "not provided";

        /// <summary>
        /// Ensures that the given value is not null.
        /// </summary>
        /// <param name="value">Value to ensure is not null or empty.</param>
        /// <param name="name">The name of the argument being checked.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null or empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string NotNullOrEmpty(
            [NotNull] string? value,
            [CallerArgumentExpression(nameof(value))] string name = MissingValueName)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(name);
            }
            return value;
        }

        /// <summary>
        /// Ensures that the given value is not null.
        /// </summary>
        /// <param name="value">Value to ensure is not null.</param>
        /// <param name="name">The name of the argument being checked.</param>
        /// <param name="message">Delegate for creating the failure message if the value is null.
        ///     It is recommended to use string.Format(...) over string interpolation (e.g.,$"{var}") to delay message construction until the exception is about to be thrown.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T NotNull<T>(
            [NotNull] T? value,
            [CallerArgumentExpression(nameof(value))] string name = MissingValueName,
            Func<string?>? message = null)
        {
            if (value == null)
            {
                throw new ArgumentNullException(name, message?.Invoke());
            }
            return (T)value;
        }

        /// <summary>
        /// Ensures that a given condition is true.
        /// </summary>
        /// <param name="condition">Condition to ensure is true.</param>
        /// <param name="message">The failure message if the condition is false.</param>
        /// <param name="name">The name of the condition being checked.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="condition"/> is false.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsTrue(
            bool condition,
            string? message = null,
            [CallerArgumentExpression(nameof(condition))] string name = MissingValueName)
        {
            if (!condition)
            {
                throw new ArgumentException(name, message);
            }
        }
    }
}