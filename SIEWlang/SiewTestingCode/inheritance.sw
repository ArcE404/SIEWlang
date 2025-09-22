class SiewParent 
{
	loud(say)
	{
		printl say + "!";
	}

	louder(say)
	{
		printl say + "!!!";
	}
}

class SiewChild < SiewParent
{
	init(internalSay)
	{
		this.internal = internalSay;
	}

	sayIt(say)
	{
		super.loud(say);

		printl this.internal;
	}
}

var child = SiewChild("It even handles these things behind the scenes...");

child.sayIt("Siew is alive");

printl "I'll say it again...";

child.louder("Alive and working");