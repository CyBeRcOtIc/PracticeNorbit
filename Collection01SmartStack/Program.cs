using System;
using System.Collections.Generic;

// 1. Default constructor
SmartStack<int> stack = new SmartStack<int>();
Console.WriteLine($"Count: {stack.Count}");
Console.WriteLine($"Capacity: {stack.Capacity}");

// 2. Constructor with specified capacity
SmartStack<int> stack2 = new SmartStack<int>(10);
Console.WriteLine($"Capacity of stack2: {stack2.Capacity}");

// 3. Push — add elements to the stack
stack.Push(10);
stack.Push(20);
stack.Push(30);
Console.WriteLine($"Count after Push: {stack.Count}");

// 4. Peek — get the top element without removing it
Console.WriteLine($"Peek: {stack.Peek()}");

// 5. Contains — check whether an element exists
Console.WriteLine($"Contains 20: {stack.Contains(20)}");
Console.WriteLine($"Contains 99: {stack.Contains(99)}");

// 6. Indexer — access elements by depth
Console.WriteLine($"stack[0]: {stack[0]}");
Console.WriteLine($"stack[1]: {stack[1]}");
Console.WriteLine($"stack[2]: {stack[2]}");

// 7. Enumerate the stack from top to bottom
Console.WriteLine("Stack elements:");
foreach (int item in stack)
{
    Console.WriteLine(item);
}

// 8. PushRange — add multiple elements to the stack
stack.PushRange(new int[] { 40, 50 });

Console.WriteLine("After PushRange:");
foreach (int item in stack)
{
    Console.WriteLine(item);
}

// 9. Pop — remove and return the top element
Console.WriteLine($"Pop: {stack.Pop()}");
Console.WriteLine($"Count after Pop: {stack.Count}");

// 10. Constructor from an existing collection
SmartStack<int> stack3 = new SmartStack<int>(new int[] { 1, 2, 3 });

Console.WriteLine("Stack created from an array:");
foreach (int item in stack3)
{
    Console.WriteLine(item);
}

// 11. Test automatic capacity expansion
SmartStack<int> smallStack = new SmartStack<int>(2);
smallStack.Push(100);
smallStack.Push(200);
smallStack.Push(300);

Console.WriteLine($"Count: {smallStack.Count}");
Console.WriteLine($"Capacity after expansion: {smallStack.Capacity}");

// 12. Remove all elements from the stack
while (stack.Count > 0)
{
    Console.WriteLine($"Pop: {stack.Pop()}");
}

// 13. Test exceptions when the stack is empty
try
{
    stack.Pop();
}
catch (InvalidOperationException)
{
    Console.WriteLine("Pop: the stack is empty!");
}

try
{
    stack.Peek();
}
catch (InvalidOperationException)
{
    Console.WriteLine("Peek: the stack is empty!");
}

// 14. Test an invalid index
try
{
    Console.WriteLine(stack[0]);
}
catch (ArgumentOutOfRangeException)
{
    Console.WriteLine("Index is out of range!");
}