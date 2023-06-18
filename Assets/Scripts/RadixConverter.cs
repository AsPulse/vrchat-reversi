
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class RadixConverter : UdonSharpBehaviour
{
    private readonly string BaseLetters = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public int[] Add(int[] left, int[] right, int bases)
    {
        bool swap = left.Length < right.Length;
        int[] longer = Clone(swap ? right: left);
        int[] shorter = swap ? left : right;
        int adding = 0;
        int i = 0;
        while(adding > 0 || i < longer.Length)
        {
            if(longer.Length <= i)
            {
                int[] result = new int[longer.Length + 1];
                longer.CopyTo(result, 0);
                result[result.Length - 1] = 0;
                longer = result;
            }
            adding += longer[i];
            if (i < shorter.Length) adding += shorter[i];
            longer[i] = adding % bases;
            adding -= longer[i];
            adding /= bases;

            i++;
        }
        return longer;
    }

    private int[] Multiple(int[] left, int right, int leftBase)
    {
        int[] answer = Clone(left);
        for (int i = 0; i < answer.Length; i++)
        {
            answer[i] = answer[i] * right;
        }
        return Add(new int[] { 0 }, answer, leftBase);

    }

    private int[] Clone(int[] target)
    {
        int[] result = new int[target.Length];
        target.CopyTo(result, 0);
        return result;
    }

    public string Encode(int[] data) {
        string result = "[";
        foreach(int num in data)
        {
            result += num.ToString();
            result += ",";
        }
        result += "]";
        return result;
    }

    public string Convert(string str, int baseFrom, int baseTo)
    {
        DateTime start = DateTime.Now;
        
        char[] fromChars = str.ToCharArray();
        int fromLength = fromChars.Length;
        int[] resultChars = new int[fromLength];
        for (int i = 0; i < fromLength; i++)
        {
            resultChars[i] = BaseLetters.IndexOf(fromChars[fromLength - i - 1]);
        }

        int[] answer = new int[] { 0 };

        int[] targetBase = new int[]{ 1 };
        for (int i = 0; i < resultChars.Length; i++)
        {
            if ( i > 0 )
            {
                targetBase = Multiple(targetBase, baseFrom, baseTo);
            }
            if (resultChars[i] == 0) continue;
            int[] thisBase = Multiple(
                targetBase,
                resultChars[i],
                baseTo
            );
            answer = Add(answer, thisBase, baseTo);
        }

        string result = "";
        for(int i = answer.Length - 1; i >= 0; i--)
        {
            result += BaseLetters[answer[i]];
        }
        DateTime end = DateTime.Now;


        Debug.LogFormat("[{4}ms]: {0}({1}) is {2}({3})", str, baseFrom, result, baseTo, (end - start).TotalMilliseconds);
        return result;
    }
}
