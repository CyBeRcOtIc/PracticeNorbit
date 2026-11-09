using StoreAnalytics.Data;
using StoreAnalytics.Services;

var categories = TestDataFactory.GetCategories();
var suppliers = TestDataFactory.GetSuppliers();
var products = TestDataFactory.GetProducts();
var orders = TestDataFactory.GetOrders();

var report = StoreAnalyticsService.BuildReport(
    categories,
    suppliers,
    products,
    orders,
    minPrice: null,
    maxPrice: null,
    categoryId: null,
    searchText: null,
    pageNumber: 1,
    pageSize: 10);

Console.WriteLine("=== Общие показатели ===");
Console.WriteLine($"Всего товаров: {report.TotalProducts}");
Console.WriteLine($"Активных: {report.ActiveProducts}");
Console.WriteLine($"Нет в наличии: {report.OutOfStockProducts}");
Console.WriteLine($"Стоимость запасов: {report.TotalStockValue}");
Console.WriteLine($"Средняя цена активных: {report.AverageActiveProductPrice}");

Console.WriteLine();
Console.WriteLine("=== Каталог (страница 1) ===");
foreach (var item in report.CatalogPage)
{
    Console.WriteLine(
        $"{item.Id,3} {item.Name,-25} {item.CategoryName,-16} {item.SupplierName,-15} {item.Price,8} {item.PriceWithTax,10} {item.StockStatus}");
}

Console.WriteLine();
Console.WriteLine("=== Каталог: категория 1, поиск \"смарт\" ===");

var filteredReport = StoreAnalyticsService.BuildReport(
    categories,
    suppliers,
    products,
    orders,
    minPrice: null,
    maxPrice: null,
    categoryId: 1,
    searchText: "смарт",
    pageNumber: 1,
    pageSize: 10);

foreach (var item in filteredReport.CatalogPage)
{
    Console.WriteLine(
        $"{item.Id,3} {item.Name,-25} {item.CategoryName,-16} {item.SupplierName,-15} {item.Price,8} {item.PriceWithTax,10} {item.StockStatus}");
}

Console.WriteLine();
Console.WriteLine("=== Категории ===");
foreach (var category in report.Categories)
{
    Console.WriteLine(
        $"{category.CategoryName}: товаров {category.ProductCount}, активных {category.ActiveProductCount}, " +
        $"в наличии {category.InStockProductCount}, остаток {category.TotalStockQuantity}, " +
        $"стоимость {category.TotalStockValue}, цена avg/min/max: " +
        $"{category.AveragePrice?.ToString() ?? "null"}/{category.MinPrice?.ToString() ?? "null"}/{category.MaxPrice?.ToString() ?? "null"}");
}

Console.WriteLine();
Console.WriteLine("=== Группировка по категории и ценовому диапазону ===");
foreach (var group in StoreAnalyticsService.GroupByCategoryAndPriceRange(products))
{
    var names = string.Join(", ", group.Select(product => product.Name));
    Console.WriteLine($"Категория {group.Key.CategoryId}, от {group.Key.LowerBound}: {names}");
}

Console.WriteLine();
Console.WriteLine("=== Топ-5 продаж ===");
foreach (var sale in report.TopSellingProducts)
{
    Console.WriteLine(
        $"{sale.ProductName}: штук {sale.TotalQuantity}, заказов {sale.OrdersCount}, выручка {sale.Revenue}");
}

Console.WriteLine();
Console.WriteLine("=== Отложенное выполнение ===");
var numbers = new List<int> { 1, 2, 3 };

// Запрос только описан, он ещё не выполняется.
var deferredQuery = numbers.Where(number => number > 1);

// Снимок выполняется сразу и больше не меняется.
var snapshot = numbers.Where(number => number > 1).ToList();

numbers.Add(10);

Console.WriteLine($"Отложенный запрос: {string.Join(", ", deferredQuery)}");
Console.WriteLine($"Снимок: {string.Join(", ", snapshot)}");

// AsQueryable() над List<T> не создаёт SQL и не переносит работу в базу:
// запрос остаётся в памяти, просто получает тип IQueryable<T>.
// Назначение: IQueryable<T> нужен LINQ-провайдерам (например, EF Core),
// которые разбирают запрос и переводят его в SQL.
// Ограничение: для списка в памяти это только обёртка, выигрыша нет.
IQueryable<int> queryable = numbers.AsQueryable().Where(number => number > 1);
Console.WriteLine($"AsQueryable: {string.Join(", ", queryable)}");

// AsEnumerable() переключает обработку на LINQ to Objects: всё, что
// идёт после него, выполняется в памяти приложения, а не провайдером.
// Назначение: применить к данным то, что провайдер не умеет переводить в SQL.
// Ограничение: обратно к провайдеру запрос не вернуть, а фильтры после
// AsEnumerable() работают с уже загруженными в память данными.
var inMemory = queryable.AsEnumerable().Select(number => number * 2);
Console.WriteLine($"AsEnumerable: {string.Join(", ", inMemory)}");
