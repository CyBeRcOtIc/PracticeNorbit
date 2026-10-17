var items = new List<OrderItem>
{
    new OrderItem("Laptop", 1000m, 1),
    new OrderItem("Mouse", 25m, 2),
    new OrderItem("Keyboard", 50m, 3),
    new OrderItem("Headphones", 80m, 1)
};

var processor = new OrderProcessor();

EventHandler<OrderProcessedEventArgs> handler = (sender, e) =>
    Console.WriteLine($"Order {e.OrderId} done, total = {e.Total}, items = {e.ProcessedItemsCount}");

processor.OrderProcessed += handler;

int minQuantity = 2;
Predicate<OrderItem> shouldProcess = item => item.Quantity >= minQuantity;
Func<decimal, decimal> transformPrice = static price => price * 0.9m;
ItemTotalCalculator calculator = (item, price) => price * item.Quantity;

int logCount = 0;
Action<string> logger = Console.WriteLine;
logger += message => logCount++;

Console.WriteLine("--- first order ---");
decimal total = processor.Process(1, items, shouldProcess, transformPrice, calculator, logger);
Console.WriteLine("Total: " + total);
Console.WriteLine("Log messages: " + logCount);

processor.OrderProcessed -= handler;

Console.WriteLine("--- second order (unsubscribed) ---");
total = processor.Process(2, items, shouldProcess, transformPrice, calculator, logger);
Console.WriteLine("Total: " + total);

Console.WriteLine("--- wrong data ---");
var wrongItems = new List<OrderItem> { new OrderItem("Phone", -100m, 1) };

try
{
    processor.Process(3, wrongItems, shouldProcess, transformPrice, calculator, logger);
}
catch (ArgumentException ex)
{
    Console.WriteLine("Error: " + ex.Message);
}