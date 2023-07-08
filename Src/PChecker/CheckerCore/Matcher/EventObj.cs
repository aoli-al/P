using PChecker.Actors;
using PChecker.Actors.Events;

namespace PChecker.Matcher;

public class EventObj
{
    public Event Event;
    public ActorId Sender;
    public ActorId Receiver;
    public int Index;

    public EventObj(Event e, ActorId sender, ActorId receiver, int index)
    {
        Event = e;
        Sender = sender;
        Receiver = receiver;
        Index = index;
    }
}