﻿using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Geb.Image.Formats
{
    /// <summary>
    /// Represents a dense matrix with arbitrary elements.
    /// Components that are adjacent in a column of the matrix are adjacent in the storage array.
    /// The components are said to be stored in column major order.
    /// </summary>
    /// <typeparam name="T">The type of elements in the matrix.</typeparam>
    public readonly struct DenseMatrix<T> : IEquatable<DenseMatrix<T>>
        where T : struct, IEquatable<T>
    {
        /// <summary>
        /// The 1D representation of the dense matrix.
        /// </summary>
        public readonly T[] Data;

        /// <summary>
        /// Gets the number of columns in the dense matrix.
        /// </summary>
        public readonly int Columns;

        /// <summary>
        /// Gets the number of rows in the dense matrix.
        /// </summary>
        public readonly int Rows;

        /// <summary>
        /// Gets the number of items in the array.
        /// </summary>
        public readonly int Count;

        /// <summary>
        /// Initializes a new instance of the <see cref=" DenseMatrix{T}" /> struct.
        /// </summary>
        /// <param name="length">The length of each side in the matrix.</param>
        public DenseMatrix(int length)
            : this(length, length)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref=" DenseMatrix{T}" /> struct.
        /// </summary>
        /// <param name="columns">The number of columns.</param>
        /// <param name="rows">The number of rows.</param>
        public DenseMatrix(int columns, int rows)
        {
            Guard.MustBeGreaterThan(columns, 0, nameof(columns));
            Guard.MustBeGreaterThan(rows, 0, nameof(rows));

            this.Rows = rows;
            this.Columns = columns;
            this.Count = columns * rows;
            this.Data = new T[this.Columns * this.Rows];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref=" DenseMatrix{T}"/> struct.
        /// </summary>
        /// <param name="data">The 2D array to provide access to.</param>
        public DenseMatrix(T[,] data)
        {
            Guard.NotNull(data, nameof(data));
            int rows = data.GetLength(0);
            int columns = data.GetLength(1);

            Guard.MustBeGreaterThan(rows, 0, nameof(this.Rows));
            Guard.MustBeGreaterThan(columns, 0, nameof(this.Columns));

            this.Rows = rows;
            this.Columns = columns;
            this.Count = this.Columns * this.Rows;
            this.Data = new T[this.Columns * this.Rows];

            for (int y = 0; y < this.Rows; y++)
            {
                for (int x = 0; x < this.Columns; x++)
                {
                    ref T value = ref this[y, x];
                    value = data[y, x];
                }
            }
        }

        /// <summary>
        /// Gets or sets the item at the specified position.
        /// </summary>
        /// <param name="row">The row-coordinate of the item. Must be greater than or equal to zero and less than the height of the array.</param>
        /// <param name="column">The column-coordinate of the item. Must be greater than or equal to zero and less than the width of the array.</param>
        /// <returns>The <see typeparam="T"/> at the specified position.</returns>
        public ref T this[int row, int column]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                this.CheckCoordinates(row, column);
                return ref this.Data[(row * this.Columns) + column];
            }
        }

        /// <summary>
        /// Performs an implicit conversion from a <see cref="T:T[,]" /> to a <see cref=" DenseMatrix{T}" />.
        /// </summary>
        /// <param name="data">The source array.</param>
        /// <returns>
        /// The <see cref="DenseMatrix{T}"/> representation on the source data.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator DenseMatrix<T>(T[,] data) => new DenseMatrix<T>(data);

        /// <summary>
        /// Performs an implicit conversion from a <see cref="DenseMatrix{T}"/> to a <see cref="T:T[,]" />.
        /// </summary>
        /// <param name="data">The source array.</param>
        /// <returns>
        /// The <see cref="T:T[,]"/> representation on the source data.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable SA1008 // Opening parenthesis should be spaced correctly
        public static implicit operator T[,] (DenseMatrix<T> data)
#pragma warning restore SA1008 // Opening parenthesis should be spaced correctly
        {
            var result = new T[data.Rows, data.Columns];

            for (int y = 0; y < data.Rows; y++)
            {
                for (int x = 0; x < data.Columns; x++)
                {
                    ref T value = ref result[y, x];
                    value = data[y, x];
                }
            }

            return result;
        }

        /// <summary>
        /// Fills the matrix with the given value
        /// </summary>
        /// <param name="value">The value to fill each item with</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fill(T value)
        {
            for (int i = 0; i < this.Data.Length; i++)
            {
                this.Data[i] = value;
            }
        }

        /// <summary>
        /// Clears the matrix setting each value to the default value for the element type
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => Array.Clear(this.Data, 0, this.Data.Length);

        /// <summary>
        /// Checks the coordinates to ensure they are within bounds.
        /// </summary>
        /// <param name="row">The y-coordinate of the item. Must be greater than zero and smaller than the height of the matrix.</param>
        /// <param name="column">The x-coordinate of the item. Must be greater than zero and smaller than the width of the matrix.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the coordinates are not within the bounds of the array.
        /// </exception>
        [Conditional("DEBUG")]
        private void CheckCoordinates(int row, int column)
        {
            if (row < 0 || row >= this.Rows)
            {
                throw new ArgumentOutOfRangeException(nameof(row), row, $"{row} is outwith the matrix bounds.");
            }

            if (column < 0 || column >= this.Columns)
            {
                throw new ArgumentOutOfRangeException(nameof(column), column, $"{column} is outwith the matrix bounds.");
            }
        }

        /// <inheritdoc/>
        public bool Equals(DenseMatrix<T> other)
        {
            if (this.Columns != other.Columns)
            {
                return false;
            }

            if (this.Rows != other.Rows)
            {
                return false;
            }

            for (int i = 0; i < this.Data.Length; i++)
            {
                if (!this.Data[i].Equals(other.Data[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is DenseMatrix<T> other && this.Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => this.Data.GetHashCode();
    }
}
