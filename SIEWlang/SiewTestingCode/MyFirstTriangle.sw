var global = "hola";
{
	fn happy()
	{
		printl global;
	}

	var global = "como";
	printl global;

	happy();
}