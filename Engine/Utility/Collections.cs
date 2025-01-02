using System.Text;

namespace Pure.Engine.Utility;

public static class Collections
{
    /// <summary>
    /// Randomly shuffles the elements in the given collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to shuffle.</param>
    public static void Shuffle<T>(this IList<T> collection)
    {
        var rand = new Random();
        for (var i = collection.Count - 1; i > 0; i--)
        {
            var j = rand.Next(i + 1);
            (collection[j], collection[i]) = (collection[i], collection[j]);
        }
    }
    /// <summary>
    /// Shifts the elements of the given collection by the specified offset.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to be shifted.</param>
    /// <param name="offset">The number of positions by which to shift the elements. Positive values 
    /// shift elements to the right, negative values shift elements to the left.</param>
    /// <remarks>
    /// If the offset is greater than the size of the collection, the method will wrap 
    /// around the collection.
    /// </remarks>
    public static void Shift<T>(this IList<T> collection, int offset)
    {
        if (offset == default)
            return;
        if (offset < 0)
        {
            offset = Math.Abs(offset);
            for (var j = 0; j < offset; j++)
            {
                var temp = new T[collection.Count];
                for (var i = 0; i < collection.Count - 1; i++)
                    temp[i] = collection[i + 1];
                temp[^1] = collection[0];
                for (var i = 0; i < temp.Length; i++)
                    collection[i] = temp[i];
            }

            return;
        }

        for (var j = 0; j < offset; j++)
        {
            var tmp = new T[collection.Count];
            for (var i = 1; i < collection.Count; i++)
                tmp[i] = collection[i - 1];
            tmp[0] = collection[tmp.Length - 1];
            for (var i = 0; i < tmp.Length; i++)
                collection[i] = tmp[i];
        }
    }
    public static void Shift<T>(this IList<T> collection, int offset, params int[]? affectedIndexes)
    {
        if (affectedIndexes == null || affectedIndexes.Length == 0 || offset == 0)
            return;
        if (collection is List<T> list)
        {
            var results = new List<int>();
            var indexList = affectedIndexes.ToList();
            var prevTargetIndex = -1;
            var max = list.Count - 1;
            indexList.Sort();
            if (offset > 0)
                indexList.Reverse();
            foreach (var currIndex in indexList)
            {
                if (currIndex < 0 || currIndex >= list.Count)
                    continue;
                var item = list[currIndex];
                var targetIndex = Math.Clamp(currIndex + offset, 0, max);

                // prevent items order change
                if (currIndex > 0 &&
                    currIndex < max &&
                    indexList.Contains(currIndex + (offset > 0 ? 1 : -1)))
                    continue;

                // prevent overshooting of multiple items which would change the order
                var isOvershooting = (targetIndex == 0 && prevTargetIndex == 0) ||
                                     (targetIndex == max && prevTargetIndex == max) ||
                                     results.Contains(targetIndex);
                var i = indexList.IndexOf(currIndex);
                var result = isOvershooting ? offset < 0 ? i : max - i : targetIndex;
                list.RemoveAt(currIndex);
                list.Insert(result, item);
                prevTargetIndex = targetIndex;
                results.Add(result);
            }

            return;
        }

        // if not a list then convert it
        var tempList = collection.ToList();
        Shift(tempList, offset, affectedIndexes);
        for (var i = 0; i < tempList.Count; i++)
            collection[i] = tempList[i];
    }
    public static void Shift<T>(this IList<T> collection, int offset, params T[]? affectedItems)
    {
        if (affectedItems == null || affectedItems.Length == 0 || offset == 0)
            return;
        var affectedIndexes = new int[affectedItems.Length];
        for (var i = 0; i < affectedItems.Length; i++)
            affectedIndexes[i] = collection.IndexOf(affectedItems[i]);
        Shift(collection, offset, affectedIndexes);
    }
    /// <summary>
    /// Reverses the order of the elements in a collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to reverse.</param>
    public static void Reverse<T>(this IList<T> collection)
    {
        var left = 0;
        var right = collection.Count - 1;
        for (var i = 0; i < collection.Count / 2; i++)
        {
            (collection[right], collection[left]) = (collection[left], collection[right]);
            left++;
            right--;
        }
    }
    /// <typeparam name="T">
    /// The type of the elements in the matrix.</typeparam>
    /// <param name="matrix">The two-dimensional array to rotate.</param>
    /// <param name="direction">The direction and number of 90-degree turns to rotate the array. 
    /// Positive values rotate clockwise, negative values rotate counterclockwise.</param>
    /// <returns>A new two-dimensional array that is rotated clockwise or counterclockwise.</returns>
    public static T[,] Rotate<T>(this T[,] matrix, int direction)
    {
        var dir = Math.Abs(direction).Wrap(4);
        if (dir == 0)
            return matrix;

        var (m, n) = (matrix.GetLength(0), matrix.GetLength(1));
        var rotated = new T[n, m];

        if (direction > 0)
        {
            for (var i = 0; i < n; i++)
                for (var j = 0; j < m; j++)
                    rotated[i, j] = matrix[m - j - 1, i];
            direction--;
            return Rotate(rotated, direction);
        }

        for (var i = 0; i < n; i++)
            for (var j = 0; j < m; j++)
                rotated[i, j] = matrix[j, n - i - 1];

        direction++;
        return Rotate(rotated, direction);
    }
    public static void Flip<T>(this T[,] matrix, (bool horizontally, bool vertically) flip)
    {
        var rows = matrix.GetLength(0);
        var cols = matrix.GetLength(1);

        if (flip.horizontally)
            for (var i = 0; i < rows; i++)
                for (var j = 0; j < cols / 2; j++)
                    (matrix[i, cols - j - 1], matrix[i, j]) = (matrix[i, j], matrix[i, cols - j - 1]);

        if (flip.vertically == false)
            return;

        for (var i = 0; i < rows / 2; i++)
            for (var j = 0; j < cols; j++)
                (matrix[rows - i - 1, j], matrix[i, j]) = (matrix[i, j], matrix[rows - i - 1, j]);
    }
    public static T[] Flatten<T>(this T[,] matrix)
    {
        var rows = matrix.GetLength(0);
        var cols = matrix.GetLength(1);
        var result = new T[rows * cols];
        for (var i = 0; i < rows; i++)
            for (var j = 0; j < cols; j++)
                result[i * cols + j] = matrix[i, j];
        return result;
    }
    public static T[] Flatten<T>(this T[][] matrix)
    {
        var rows = matrix.GetLength(0);
        var cols = matrix.GetLength(1);
        var result = new T[rows * cols];
        for (var i = 0; i < rows; i++)
            for (var j = 0; j < cols; j++)
                result[i * cols + j] = matrix[i][j];
        return result;
    }

    /// <summary>
    /// Computes the intersection between the elements of two collections.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collections.</typeparam>
    /// <param name="collection">The first collection.</param>
    /// <param name="targetCollection">The second collection.</param>
    /// <returns>An array containing the common elements of both collections.</returns>
    public static T[] Intersect<T>(this IList<T> collection, IList<T> targetCollection)
    {
        var set1 = new HashSet<T>(collection);
        var set2 = new HashSet<T>(targetCollection);
        set1.IntersectWith(set2);
        return set1.ToArray();
    }
    /// <summary>
    /// Returns a subsequence of elements from a collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to take elements from.</param>
    /// <param name="start">The index of the first element to take.</param>
    /// <param name="end">The index of the last element to take.</param>
    /// <returns>An array containing the elements between the start and end indices, inclusive.</returns>
    public static T[] Take<T>(this IList<T> collection, int start, int end)
    {
        start = start.Wrap(collection.Count);
        end = end.Wrap(collection.Count);
        if (start > end)
            (start, end) = (end, start);

        var length = end - start;
        var result = new T[length];
        Array.Copy(collection.ToArray(), start, result, 0, length);
        return result;
    }
    public static T? ChooseOne<T>(this IList<T> collection, float seed = float.NaN)
    {
        return collection.Count == 0 ? default : collection[(0, collection.Count - 1).Random(seed)];
    }
    public static T? ChooseOneFrom<T>(this T choice, float seed = float.NaN, params T[]? choices)
    {
        var list = choices == null ? [] : choices.ToList();
        list.Add(choice);
        return ChooseOne(list, seed);
    }
    /// <summary>
    /// Calculates the average number out of a collection of numbers and returns it.
    /// </summary>
    /// <param name="collection">The collection of numbers to calculate the average of.</param>
    /// <returns>The average of the numbers in the collection.</returns>
    public static float Average(this IList<float> collection)
    {
        var sum = 0f;
        foreach (var n in collection)
            sum += n;
        return sum / collection.Count;
    }
    public static float Average(this IList<int> collection)
    {
        var sum = 0f;
        foreach (var n in collection)
            sum += n;
        return sum / collection.Count;
    }

    /// <summary>
    /// Iterates over a collection, calls <see cref="object.ToString"/>
    /// on each element and adds the returned string to the result, alongside
    /// a separator. The result is then returned.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection of elements to iterate over.</param>
    /// <param name="separator">The separator string to use between elements.</param>
    /// <returns>A string that represents the collection as a sequence of elements separated 
    /// by the specified separator string.</returns>
    public static string ToString<T>(this IList<T> collection, string separator)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < collection.Count; i++)
        {
            var sep = i != 0 ? separator : string.Empty;
            sb.Append(sep).Append(collection[i]);
        }

        return sb.ToString();
    }
    public static string ToString<T>(this T[,] matrix, (string horizontal, string vertical) separator)
    {
        var (m, n) = (matrix.GetLength(0), matrix.GetLength(1));
        var result = new StringBuilder();
        for (var i = 0; i < m; i++)
        {
            for (var j = 0; j < n; j++)
            {
                result.Append(matrix[i, j]);
                if (j < n - 1)
                    result.Append(separator.horizontal);
            }

            result.Append(separator.vertical);
        }

        result.Remove(result.Length - 1, 1);
        return result.ToString();
    }

    /// <typeparam name="T">
    /// The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to check for duplicates.</param>
    /// <returns>True if the collection contains at least one duplicate element, 
    /// false otherwise.</returns>
    public static bool HasDuplicates<T>(this IList<T> collection)
    {
        var set = new HashSet<T>();
        foreach (var t in collection)
            if (set.Add(t) == false)
                return true;
        return false;
    }

    public static T Animate<T>(this IList<T> collection, float speed, bool repeat = true)
    {
        var hash = collection.GetHashCode();
        var duration = collection.Count / speed;
        animations.TryAdd(hash, (0f, 1f));

        if (duration <= animations[hash].time)
        {
            if (repeat == false)
            {
                animations[hash] = (duration, 1f);
                return collection[^1];
            }

            animations[hash] = (0f, 1f);
        }

        var unit = animations[hash].time / duration;
        var index = (int)Math.Min(unit * collection.Count, collection.Count - 1);

        animations[hash] = (animations[hash].time + Time.Delta, 1f);
        return collection[index];
    }

    public static bool IsOneOf<T>(this T value, params T[] values)
    {
        return value.IsAnyOf(values);
    }
    public static bool IsAnyOf<T>(this T value, params T[] values)
    {
        for (var i = 0; i < values?.Length; i++)
            if (EqualityComparer<T>.Default.Equals(value, values[i]))
                return true;

        return false;
    }
    public static bool IsNoneOf<T>(this T value, params T[] values)
    {
        return value.IsAnyOf(values) == false;
    }
    public static bool IsAllOf<T>(this T value, params T[] values)
    {
        for (var i = 0; i < values?.Length; i++)
            if (EqualityComparer<T>.Default.Equals(value, values[i]) == false)
                return false;

        return true;
    }

#region Backend
    private static readonly Dictionary<int, (float time, float removeAfter)> animations = [];

    internal static void TryRemoveAnimations()
    {
        var keys = animations.Keys;
        foreach (var key in keys)
        {
            var value = animations[key];
            animations[key] = (value.time, value.removeAfter - Time.Delta);

            if (animations[key].removeAfter <= 0f)
                animations.Remove(key);
        }
    }
#endregion
}