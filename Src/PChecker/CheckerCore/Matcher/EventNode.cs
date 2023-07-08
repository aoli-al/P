using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PChecker.Actors.Events;

namespace PChecker.Matcher;

public class EventNode : BaseNode
{
    public string EventName;
    public Dictionary<string, string> Constraints = new();

    public EventNode(string name, Dictionary<string, string> constraints)
    {
        EventName = name;
        Constraints = constraints;
    }

    public static EventNode Epsilon = new EventNode("", new());

    public bool Match(Event e)
    {
        if (this == Epsilon)
        {
            return false;
        }

        if (e.GetType().Name != EventName)
        {
            return false;
        }

        var payload = GetEventWithPayload(e);

        foreach (var keyValuePair in Constraints)
        {
            if (!payload.ContainsKey(keyValuePair.Key))
            {
                return false;
            }

            if (!Match(keyValuePair.Value, payload[keyValuePair.Key]))
            {
                return false;
            }
        }

        return true;
    }

    public bool Match(string pattern, string value)
    {
        if (pattern == "*")
        {
            return true;
        }

        return pattern == value;
    }

    private Dictionary<string, string> GetEventWithPayload(Event e)
    {
        var method = e.GetType().GetMethod("get_Payload");
        var payload = method.Invoke(e, new object[] { });

        if (payload == null)
        {
            return new();
        }

        var fieldNames = payload.GetType().GetField("fieldNames");
        var names = new List<string>();
        foreach (var item in (IEnumerable) fieldNames.GetValue(payload))
        {
            names.Add((string) item);
        }
        var fieldValues = payload.GetType().GetField("fieldValues");
        var values = new List<string>();
        foreach (var item in (IEnumerable) fieldValues.GetValue(payload))
        {
            if (item != null)
            {
                values.Add(item.ToString());
            }
            else
            {
                values.Add("null");
            }
        }

        Dictionary<string, string> result = new();

        for (var i = 0; i < names.Count; i++)
        {
            result[names[i]] = values[i];
        }

        return result;
    }

    public override string ToString()
    {
        return EventName + "{" + string.Join(",", Constraints.Select(it => $"{it.Key}: {it.Value}")) + "}";
    }
}