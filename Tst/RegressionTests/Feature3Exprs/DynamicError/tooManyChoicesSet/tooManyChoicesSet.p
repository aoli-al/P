machine Main {
	start state S {
		entry {
		    var x: set[int];
		    var y: int;
		    var i: int;

            i = 0;
            while (i < 10000) {
                x += (i);
                i = i + 1;
            }
            
		    y = choose(x); // OK
		    print format("y is {0}", y);
		    
            x += (i);
		    y = choose(x); // error
		    print format("y is {0}", y);
		}
	}
}
