using StoreAnalytics.Models;

namespace StoreAnalytics.Services;

public static class StoreAnalyticsService
{
    public static StoreAnalyticsReport BuildReport(
        IEnumerable<Category> categories,
        IEnumerable<Supplier> suppliers,
        IEnumerable<Product> products,
        IEnumerable<Order> orders,
        decimal? minPrice,
        decimal? maxPrice,
        int? categoryId,
        string? searchText,
        int pageNumber,
        int pageSize)
    {
        BasicValidation(
            categories,
            suppliers,
            products,
            orders,
            minPrice,
            maxPrice,
            pageNumber,
            pageSize
        );

        IEnumerable<Product> filteredProducts = products;

        filteredProducts = filteredProducts.Where(product => product.IsActive);

        if (minPrice.HasValue)
        {
            filteredProducts = filteredProducts.Where(product => product.Price >= minPrice);
        }

        if (maxPrice.HasValue)
        {
            filteredProducts = filteredProducts.Where(product => product.Price <= maxPrice);
        }

        if (categoryId.HasValue)
        {
            // Single() ищет категорию по уникальному Id. Если категории нет
            // или их несколько, выбросит InvalidOperationException.
            // Результат для categoryId = 1: "Электроника".
            Category selectedCategory = categories.Single(category => category.Id == categoryId.Value);

            filteredProducts = filteredProducts.Where(product => product.CategoryId == selectedCategory.Id);
        }

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            filteredProducts = filteredProducts.Where(product => product.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase));
        }

        filteredProducts = filteredProducts.Where(product => product.StockQuantity > 0);

        var catalogItems =
            from product in filteredProducts
            join category in categories
                on product.CategoryId equals category.Id into categoryGroup
            from category in categoryGroup.DefaultIfEmpty()
            join supplier in suppliers
                on product.SupplierId equals (int?)supplier.Id into supplierGroup
            from supplier in supplierGroup.DefaultIfEmpty()
            select new ProductCatalogItem(
                product.Id,
                product.Name,
                category?.Name ?? "Неизвестная категория",
                supplier?.Name ?? "Не указан",
                product.Price,
                decimal.Round(product.Price * 1.2m, 2, MidpointRounding.AwayFromZero),
                GetStockStatus(product.StockQuantity),
                product.IsActive);

        // OrderBy обязателен перед Skip/Take: без сортировки порядок
        // элементов не гарантирован, и страницы могли бы пересекаться.
        // ThenBy(Id) нужен, потому что цена и название могут совпасть,
        // а Id уникален и делает порядок одинаковым.
        // ToList() только в конце: ранний ToList() загрузил бы в память
        // все товары до сортировки и пагинации.
        List<ProductCatalogItem> catalogPage = catalogItems
            .OrderBy(item => item.Price)
            .ThenBy(item => item.Name)
            .ThenBy(item => item.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        List<CategoryStockReport> categoryReports = categories
            .GroupJoin(
                products,
                category => category.Id,
                product => product.CategoryId,
                (category, categoryProducts) => new CategoryStockReport(
                    category.Name,
                    categoryProducts.Count(),
                    categoryProducts.Count(product => product.IsActive),
                    categoryProducts.Count(product => product.StockQuantity > 0),
                    categoryProducts.Sum(product => product.StockQuantity),
                    categoryProducts.Sum(product => product.Price * product.StockQuantity),
                    categoryProducts.Select(product => (decimal?)product.Price).Average(),
                    categoryProducts.Select(product => (decimal?)product.Price).Min(),
                    categoryProducts.Select(product => (decimal?)product.Price).Max()))
            .OrderBy(report => report.CategoryName)
            .ToList();

        // SelectMany нужен вместо Select: Select вернул бы по одному списку
        // строк на каждый заказ (список списков), а SelectMany "разворачивает"
        // их в одну плоскую последовательность строк заказов.
        var orderLines = orders.SelectMany(
            order => order.Items,
            (order, item) => new
            {
                OrderId = order.Id,
                OrderDate = order.CreatedAt,
                ProductId = item.ProductId,
                item.Quantity,
                item.UnitPrice,
                LineTotal = item.Quantity * item.UnitPrice
            });

        // Обычный Join оставляет только пары, найденные с обеих сторон,
        // поэтому непроданный товар (Id 22) в рейтинг не попадёт.
        // Distinct() нужен, потому что один товар может быть в заказе
        // несколькими строками, а считать нужно уникальные заказы.
        List<ProductSalesReport> topSellingProducts = orderLines
            .Join(
                products,
                line => line.ProductId,
                product => product.Id,
                (line, product) => new { line, product })
            .GroupBy(pair => new { pair.product.Id, pair.product.Name })
            .Select(group => new ProductSalesReport(
                group.Key.Id,
                group.Key.Name,
                group.Sum(pair => pair.line.Quantity),
                group.Select(pair => pair.line.OrderId).Distinct().Count(),
                group.Sum(pair => pair.line.LineTotal)))
            .OrderByDescending(report => report.Revenue)
            .ThenByDescending(report => report.TotalQuantity)
            .ThenBy(report => report.ProductName)
            .ThenBy(report => report.ProductId)
            .Take(5)
            .ToList();

        // Any() проверяет, есть ли хотя бы один товар с остатком 0.
        // Результат на тестовых данных: true (товары с Id 6 и 17).
        bool hasOutOfStock = products.Any(product => product.StockQuantity == 0);

        // All() проверяет, что условие выполняется для каждого товара: цена >= 0.
        // Результат на тестовых данных: true (иначе BasicValidation выбросила бы исключение раньше).
        bool allPricesNonNegative = products.All(product => product.Price >= 0);

        // FirstOrDefault() ищет первый товар дороже 100_000 или возвращает null, если такого нет.
        // Результат на тестовых данных: товар с Id 7, "Ноутбук игровой", цена 150000.
        Product? expensiveProduct = products.FirstOrDefault(product => product.Price > 100_000m);

        int outOfStockProducts = hasOutOfStock
            ? products.Count(product => product.StockQuantity == 0)
            : 0;

        return new StoreAnalyticsReport(
            products.Count(),
            products.Count(product => product.IsActive),
            outOfStockProducts,
            products.Sum(product => product.Price * product.StockQuantity),
            products
                .Where(product => product.IsActive)
                .Select(product => (decimal?)product.Price)
                .Average(),
            catalogPage,
            categoryReports,
            topSellingProducts);
    }

    public static IEnumerable<IGrouping<(int CategoryId, decimal LowerBound), Product>> GroupByCategoryAndPriceRange(
    IEnumerable<Product> products)
    {
        ArgumentNullException.ThrowIfNull(products);

        return products
            .GroupBy(product => (
                product.CategoryId,
                LowerBound: decimal.Floor(product.Price / 500m) * 500m))
            .OrderBy(group => group.Key.CategoryId)
            .ThenBy(group => group.Key.LowerBound);
    }

    private static string GetStockStatus(int stockQuantity)
    {
        if (stockQuantity == 0)
        {
            return "Нет в наличии";
        }

        if (stockQuantity <= 5)
        {
            return "Мало осталось";
        }

        return "В наличии";
    }

    private static void BasicValidation(
        IEnumerable<Category> categories,
        IEnumerable<Supplier> suppliers,
        IEnumerable<Product> products,
        IEnumerable<Order> orders,
        decimal? minPrice,
        decimal? maxPrice,
        int pageNumber,
        int pageSize
    )
    {
        ArgumentNullException.ThrowIfNull(categories);
        ArgumentNullException.ThrowIfNull(suppliers);
        ArgumentNullException.ThrowIfNull(products);
        ArgumentNullException.ThrowIfNull(orders);

        if (pageNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber));
        }

        if (pageSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        if (minPrice > maxPrice)
        {
            throw new ArgumentException("The minimum price must not be greater than the maximum price.");
        }

        foreach (var item in products)
        {
            if (item.Price < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(products), "The price of the product cannot be negative.");
            }

            if (item.StockQuantity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(products), "The stock quantity of the product cannot be negative.");
            }
        }

        foreach (var order in orders)
        {
            foreach (var item in order.Items)
            {
                if (item.Quantity <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(orders), "The quantity of the ordered item must be greater than 0.");
                }

                if (item.UnitPrice < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(orders), "The price per unit of the product must not be negative.");
                }
            }
        }
    }
}
