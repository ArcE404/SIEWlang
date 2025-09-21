class Cake {

  init(flavor)
  {
    this.flavor = flavor;
    return;
  }

  taste() {
    var adjective = "delicious";
    print "The " + this.flavor + " cake is " + adjective;
  }
}

var cake = Cake("German chocolate");
//var cakeInstance = cake("some");
cake.taste(); // Prints "The German chocolate cake is delicious!".