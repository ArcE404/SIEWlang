# SIEW (Sometimes It Even Works) — C# Interpreter Version

This is the first version of **SIEW**, a simple interpreted programming language written in **C#**.  
It represents an early academic prototype in the development of the SIEW language project.

## Overview

This version of SIEW is a **direct C# port of the Jlox interpreter** from the book *[Crafting Interpreters](https://craftinginterpreters.com/)* by Robert Nystrom.  
It was built as an **educational exercise** to understand the structure and behavior of interpreters, including lexical analysis, parsing, AST evaluation, and runtime environments.

## Features

- Tree-walk interpreter written in C#
- Dynamically typed variables
- Basic expressions and control flow (`if`, `while`, `for`)
- Functions and local scopes
- Simple REPL for interactive execution

## Limitations

This version is intentionally minimal:
- No static typing
- No arrays, records, or modules
- No standard library or system-level access
- Not intended for production use

## Future Vision

SIEW is expected to evolve into a **statically typed language** with its own **virtual machine and garbage collector**.  
A major long-term goal is to include a **built-in graphical standard library**, implemented directly in the language itself,  
to make **rapid graphical prototyping** simple and accessible — using **OpenGL** as the rendering backend.

## Purpose

This implementation serves as a **learning foundation** for the upcoming **C-based version** of SIEW, which will evolve toward:
- A bytecode VM
- A garbage collector
- Static type checking
- A graphical standard library for OpenGL-based prototyping

## License

MIT License © 2025 Augusto
