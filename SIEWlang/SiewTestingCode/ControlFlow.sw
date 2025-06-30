var a = 0;
var temp;

for (var b = 1; a < 10000; b = temp + b) {
  printl a;
  temp = a;
  a = b;
}