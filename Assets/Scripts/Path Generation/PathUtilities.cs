using System;

public static class PathUtilities
{
    public static bool Turns(Tuple<int, int> first, Tuple<int, int> second, Tuple<int, int> third)
    {
        return
            (first.Item1 == second.Item1 && first.Item1 != third.Item1) ||
            (first.Item2 == second.Item2 && first.Item2 != third.Item2);
    }
}
