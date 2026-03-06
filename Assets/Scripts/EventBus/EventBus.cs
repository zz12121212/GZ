using System;
using System.Collections.Generic;
using UnityEngine;

public enum EventType { 
    TimeRewindStart,// 回溯
    TimeRewindEnd,
    TimeSlowStart,// 减速
    TimeSlowEnd,
    TimeFastForwardStart,// 快进
    TimeFastForwardEnd,

}
public class EventBus 
{
    private static Dictionary<EventType, Action> eventDictionary = new Dictionary<EventType, Action>();

    public static void registerEvent(EventType type, Action action) {
        if (!eventDictionary.ContainsKey(type)) {
            eventDictionary[type] = null;
        }
        eventDictionary[type] += action;
    }

    public static void disRegisterEvent(EventType type, Action action)
    {
        if (eventDictionary.ContainsKey(type))
        {
            eventDictionary[type] -= action;
        }
    }

    public static void publish(EventType type)
    {
        if (eventDictionary.ContainsKey(type)) {
            eventDictionary[type]?.Invoke();
        }
    }
}
