using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using System.Collections;

public class ParseParameters {

    public static Dictionary<string, float> ParseFile(TextAsset text)
    {
        Debug.Log("Parsing " + text.name);
        if (text == null)
        {
            throw new ArgumentNullException("text");
        }

        var stringText = text.text;
        Debug.Log("Parsing " + stringText);
        var lines = stringText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
        var lines2 = lines.Take(lines.Length - 2);
        return lines2.Select(line => line.Split('=')).ToDictionary(optParams => optParams[0], optParams => float.Parse(optParams[1]));
    }
}
