# ChickenCS
 ChickenCS is a [Chicken](https://esolangs.org/wiki/Chicken) interpreter in C#.
## Usage
This is the help message of the interpreter.
```shell
ChickenCS: Chicken interpreter
ChickenCS [filename] [-m]
filename    The filename to interpret, will read from standard input if not given
-m          Interpret using MiniChicken instead of standard Chicken
Note: When inputting program or input from stdin, end your input with '@'
Tips: set environment variable CHICKEN_DEBUG to display debug info
```

That is: input `code@input@` when reading code from stdin and `input@` when reading from file, since Chicken is a non-interactive language, even if you program doesn't need input, you still have to enter input.

Debug mode can be enabled by setting the environment variable `CHICKEN_DEBUG`.

There are some examples in the `examples` folder, written in both standard Chicken and MiniChicken (except the quine, which is only written in standard Chicken because it won't work in MiniChicken). There is also a `translate.py` file in the `examples` folder, it is a translator in Python to translate between Chicken and MiniChicken.
## How it works
ChickenCS simulates the dynamic typing in Chicken by using a class containing the string and type of an object. 

ChickenCS supports [MiniChicken](https://esolangs.org/wiki/Chicken#MiniChicken), a representation of Chicken using numbers instead of chickens. Note that different from normal MiniChicken, the `1` opcode pushes `chicken` even in MiniChicken mode to make it more useful.
