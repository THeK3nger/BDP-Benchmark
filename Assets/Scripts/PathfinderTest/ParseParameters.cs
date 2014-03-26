using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using System.Collections;

public class ParseParameters {

    public static Dictionary<string, float> ParseFile(TextAsset text)
    {
        if (text == null)
        {
            throw new ArgumentNullException("text");
        }

        var stringText = text.text;
        var lines = stringText.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
        return lines.Select(line => line.Split('=')).ToDictionary(optParams => optParams[0], optParams => float.Parse(optParams[1]));
    }
}
