event eEventA;
event eEventB;
event eEventC;
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
            if (index < 2) {
                if ($$) {
                    send server, eEventA;
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
            if (index < 2) {
                if ($$) {
                    send server, eEventB;
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
            if (index < 2) {
                if ($$) {
                    send server, eEventC;
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
            new ClientB(this);
            new ClientC(this);
        }
        on eEventA do {
            print "eEventA processed";
        }
        on eEventB do {
            print "eEventB processed";
        }
        on eEventC do {
            print "eEventC processed";
        }
    }
}



module TestModule =  { ClientA, ClientB, ClientC, Server };
