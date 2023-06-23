type tEvent = (eId: int, client: machine);

event eEventA: tEvent;
event eEventB: tEvent;
event eEventC: tEvent;
event eNextEvent;

machine ClientA
{
    var server: Server;
    var index: int;

    start state Init {
        entry (serv: Server){
            server = serv;
            goto SendRequests;
        }
    }

    state SendRequests {
        entry {
            var e: tEvent;
            if (index < 2) {
                if ($$) {
                    e = (eId = index, client = this);
                    send server, eEventA, e;
                    index = index + 1;
                }
                send this, eNextEvent;
            }
        }
        on eNextEvent goto SendRequests;
    }
}

machine ClientB
{
    var server: Server;
    var index: int;

    start state Init {
        entry (serv: Server){
            server = serv;
            goto SendRequests;
        }
    }

    state SendRequests {
        entry {
            var e: tEvent;
            if (index < 2) {
                if ($$) {
                    e = (eId = index, client = this);
                    send server, eEventB, e;
                    index = index + 1;
                }
                send this, eNextEvent;
            }
        }
        on eNextEvent goto SendRequests;
    }
}

machine ClientC
{
    var server: Server;
    var index: int;

    start state Init {
        entry (serv: Server){
            server = serv;
            goto SendRequests;
        }
    }

    state SendRequests {
        entry {
            var e: tEvent;
            if (index < 2) {
                if ($$) {
                    e = (eId = index, client = this);
                    send server, eEventC, e;
                    index = index + 1;
                }
                send this, eNextEvent;
            }
        }
        on eNextEvent goto SendRequests;
    }
}



machine Server
{
    start state ReceiveEvents {
        entry {
            new ClientA(this);
        }
        on eEventA do (req: tEvent) {
            print "eEventA processed";
        }
        on eEventB do (req: tEvent) {
            print "eEventB processed";
        }
        on eEventC do (req: tEvent) {
            print "eEventC processed";
        }
    }
}

// Run the same operation


module TestModule =  { ClientA, ClientB, ClientC, Server };
