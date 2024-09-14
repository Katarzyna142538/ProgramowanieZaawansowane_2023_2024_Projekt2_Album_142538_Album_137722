using System;

namespace University.Common;

public static class Validators
{
    private static int CalculateControlSum(string input, int[] weights, int offset = 0)
    {
        int controlSum = 0;
        for (int i = 0; i < input.Length - 1; i++)
        {
            controlSum += weights[i + offset] * int.Parse(input[i].ToString());
        }
        return controlSum;
    }
}
