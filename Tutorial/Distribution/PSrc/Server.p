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
            e = (eId = index, client = this);
            send server, eEventA, e;
            send server, eEventA, e;

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
            e = (eId = index, client = this);
            send server, eEventB, e;
            send server, eEventB, e;
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
            e = (eId = index, client = this);
            send server, eEventC, e;
            send server, eEventC, e;
        }
        on eNextEvent goto SendRequests;
    }
}



machine Server
{
    start state ReceiveEvents {
        entry {
            new ClientA(this);
            new ClientB(this);
            new ClientC(this);
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
