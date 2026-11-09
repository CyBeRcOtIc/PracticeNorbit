using StoreAnalytics.Models;

namespace StoreAnalytics.Data;

public static class TestDataFactory
{
    public static IReadOnlyList<Category> GetCategories()
    {
        return new List<Category>
        {
            new Category(1, "Электроника"),
            new Category(2, "Периферия"),
            new Category(3, "Аксессуары"),
            new Category(4, "Бытовая техника"),
            // Empty category: no products, checks grouping and zero aggregates.
            new Category(5, "Книги")
        };
    }

    public static IReadOnlyList<Supplier> GetSuppliers()
    {
        return new List<Supplier>
        {
            new Supplier(1, "ТехноПоставка", 4.5m),
            new Supplier(2, "ДжиЭмСи Трейд", 3.8m),
            new Supplier(3, "Мир Гаджетов", 4.9m),
            new Supplier(4, "Электрон-Опт", 2.7m),
            new Supplier(5, "Домашний Уют", 4.1m),
            // Supplier without products.
            new Supplier(6, "Северный Импорт", 1.0m)
        };
    }

    public static IReadOnlyList<Product> GetProducts()
    {
        return new List<Product>
        {
            // Similar to "Мышь": checks case-insensitive substring search ("мыш").
            new Product(1, "Мышка беспроводная", 2, 1, 1500m, 25, true, new DateTime(2025, 11, 1)),
            new Product(2, "Мышь игровая", 2, 1, 3200m, 12, true, new DateTime(2025, 11, 5)),
            // Same price (4500) as the headphones in one category: checks ThenBy.
            new Product(3, "Клавиатура механическая", 2, 2, 4500m, 8, true, new DateTime(2025, 11, 10)),
            // Same price (1500) as the mouse in one category: checks ThenBy.
            new Product(4, "Клавиатура мембранная", 2, 2, 1500m, 30, true, new DateTime(2025, 11, 12)),
            new Product(5, "Наушники проводные", 2, 3, 4500m, 15, true, new DateTime(2025, 11, 15)),
            // Zero stock: fails the in-stock filter.
            new Product(6, "Веб-камера", 2, 3, 2500m, 0, true, new DateTime(2025, 11, 20)),

            // More expensive than 100_000: checks FirstOrDefault.
            new Product(7, "Ноутбук игровой", 1, 4, 150000m, 3, true, new DateTime(2025, 12, 1)),
            new Product(8, "Смартфон", 1, 4, 45000m, 10, true, new DateTime(2025, 12, 3)),
            new Product(9, "Планшет", 1, 4, 30000m, 6, true, new DateTime(2025, 12, 5)),
            new Product(10, "Телевизор", 1, 5, 60000m, 4, true, new DateTime(2025, 12, 8)),
            // No supplier (SupplierId = null).
            new Product(11, "Смарт-часы", 1, null, 12000m, 9, true, new DateTime(2025, 12, 10)),
            // Inactive product with stock: fails the IsActive filter.
            new Product(12, "Старый монитор", 1, 5, 7000m, 5, false, new DateTime(2024, 6, 1)),

            new Product(13, "Чехол для смартфона", 3, 3, 800m, 40, true, new DateTime(2025, 12, 12)),
            // Same price (500) for three products in one category: checks ThenBy.
            new Product(14, "Кабель USB-C", 3, 1, 500m, 100, true, new DateTime(2025, 12, 14)),
            new Product(15, "Держатель для телефона", 3, 1, 500m, 50, true, new DateTime(2025, 12, 15)),
            new Product(16, "Зарядное устройство", 3, 2, 1500m, 20, true, new DateTime(2025, 12, 16)),
            // No supplier, inactive, zero stock and the same price (500) as the cable: several edge cases at once.
            new Product(17, "Защитное стекло", 3, null, 500m, 0, false, new DateTime(2024, 9, 1)),

            new Product(18, "Пылесос", 4, 5, 12000m, 7, true, new DateTime(2026, 1, 3)),
            new Product(19, "Чайник", 4, 2, 2500m, 14, true, new DateTime(2026, 1, 5)),
            new Product(20, "Микроволновая печь", 4, 4, 8000m, 5, true, new DateTime(2026, 1, 7)),
            // Same price (2500) as the kettle in one category: checks ThenBy.
            new Product(21, "Фен", 4, 2, 2500m, 11, true, new DateTime(2026, 1, 9)),
            // Unsold product: its Id is not in any order.
            new Product(22, "Робот-пылесос", 4, 5, 25000m, 2, true, new DateTime(2026, 1, 10))
        };
    }

    public static IReadOnlyList<Order> GetOrders()
    {
        return new List<Order>
        {
            // The mouse (1) and the cable (14) appear in several orders; the mouse's unit price is lower than its current price.
            new Order(1, 101, new DateTime(2026, 1, 15), new List<OrderItem>
            {
                new OrderItem(1, 2, 1450m),
                new OrderItem(14, 3, 500m),
                new OrderItem(13, 1, 750m)
            }),
            // The laptop was sold at a price above the current one (the price changed over time).
            new Order(2, 102, new DateTime(2026, 1, 18), new List<OrderItem>
            {
                new OrderItem(7, 1, 152000m),
                new OrderItem(1, 1, 1500m)
            }),
            new Order(3, 103, new DateTime(2026, 1, 20), new List<OrderItem>
            {
                new OrderItem(8, 1, 45000m),
                new OrderItem(13, 2, 800m),
                new OrderItem(16, 1, 1400m)
            }),
            new Order(4, 101, new DateTime(2026, 2, 2), new List<OrderItem>
            {
                new OrderItem(3, 1, 4500m),
                new OrderItem(4, 1, 1500m),
                new OrderItem(5, 2, 4300m)
            }),
            new Order(5, 104, new DateTime(2026, 2, 10), new List<OrderItem>
            {
                new OrderItem(10, 1, 62000m),
                new OrderItem(19, 1, 2500m)
            }),
            // The mouse (1) appears for the third time; the order contains several different products.
            new Order(6, 105, new DateTime(2026, 2, 14), new List<OrderItem>
            {
                new OrderItem(2, 2, 3200m),
                new OrderItem(1, 1, 1500m),
                new OrderItem(15, 2, 500m)
            }),
            // Includes a product that is now out of stock (the webcam, stock 0).
            new Order(7, 102, new DateTime(2026, 3, 1), new List<OrderItem>
            {
                new OrderItem(9, 1, 30000m),
                new OrderItem(11, 1, 12000m),
                new OrderItem(6, 1, 2500m)
            }),
            // Includes a product that has since become inactive (the old monitor).
            new Order(8, 106, new DateTime(2026, 3, 5), new List<OrderItem>
            {
                new OrderItem(18, 1, 12500m),
                new OrderItem(21, 2, 2500m),
                new OrderItem(20, 1, 8000m),
                new OrderItem(12, 1, 7000m)
            }),
            // Includes an inactive product with zero stock (the screen protector).
            new Order(9, 103, new DateTime(2026, 3, 12), new List<OrderItem>
            {
                new OrderItem(14, 5, 500m),
                new OrderItem(16, 2, 1500m),
                new OrderItem(17, 3, 500m)
            })
        };
    }
}
