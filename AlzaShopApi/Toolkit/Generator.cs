using AlzaShopApi.Models.Database;
using AlzaShopApi.Views;

namespace AlzaShopApi.Toolkit;

/// <summary>
/// Provides methods for generating random product-related data.
/// Functionalities include creating random product names, product views, and product entities tailored to include various dynamically assigned properties.
/// </summary>
public static class Generator
{
    /// <summary>
    /// An array of predefined Lorem Ipsum sentences used for generating placeholder text.
    /// </summary>
    /// <remarks>
    /// This collection contains commonly used Lorem Ipsum sentences. These sentences are utilized
    /// in various methods, such as generating descriptions or placeholder content for products.
    /// Examples of these sentences include well-known phrases from the traditional Lorem Ipsum text.
    /// </remarks>
    private static readonly string[] LoremIpsumSentences =
    [
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
        "Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
        "Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.",
        "Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.",
        "Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."
    ];

    /// <summary>
    /// A collection of descriptive adjectives utilized in the creation of random product names.
    /// </summary>
    /// <remarks>
    /// This array contains a variety of predefined adjectives designed to add appealing
    /// characteristics to randomly generated product names. These adjectives
    /// serve as the prefix in the format "Adjective Noun", helping to make product names more engaging.
    /// </remarks>
    private static readonly string[] Adjectives = ["Innovative", "Sleek", "Durable", "Compact", "Versatile", "Premium"];

    /// <summary>
    /// Represents a collection of predefined nouns used for generating random product names.
    /// </summary>
    /// <remarks>
    /// This collection contains a set of predefined noun strings that act as the base or subject
    /// in generated product names. These nouns are combined with adjectives to create unique
    /// and engaging product titles for use in various contexts.
    /// </remarks>
    private static readonly string[] Nouns = ["Widget", "Gadget", "Appliance", "Device", "Tool", "Instrument"];

    /// <summary>
    /// Generates a random product name composed of an adjective and a noun.
    /// </summary>
    /// <returns>
    /// A string representing a randomly generated product name in the format "Adjective Noun".
    /// </returns>
    private static string GenerateRandomProductName()
    {
        return
            $"{Adjectives[Czlovek.Randomness.Utils.Next(Adjectives.Length)]} {Nouns[Czlovek.Randomness.Utils.Next(Nouns.Length)]}";
    }

    /// <summary>
    /// Generates a random product URL based on a given product name.
    /// </summary>
    /// <param name="productName">The name of the product used to generate the URL.</param>
    /// <returns>
    /// A string representing the formatted URL of the product, in lowercase with spaces replaced by hyphens.
    /// </returns>
    private static string GenerateRandomProductUrl(string productName)
    {
        return $"{Constants.Product.Server}/{productName.Replace(" ", "-").ToLower()}";
    }

    /// <summary>
    /// Generates a random product price within a predefined range.
    /// </summary>
    /// <returns>
    /// A decimal value representing the random price of a product. The value is within the range of 10 to 10,000.
    /// </returns>
    private static decimal GenerateRandomProductPrice()
    {
        return Czlovek.Randomness.Utils.Range(10, 10000);
    }

    /// <summary>
    /// Generates a random stock quantity for a product within a predefined range.
    /// </summary>
    /// <returns>
    /// An integer representing a randomly generated stock quantity within the specified range.
    /// </returns>
    private static int GenerateRandomProductStock()
    {
        return Czlovek.Randomness.Utils.Range(10, 100);
    }

    /// <summary>
    /// Generates a random product view containing randomly assigned properties such as name, URL, price, stock quantity, and description.
    /// </summary>
    /// <returns>
    /// A <see cref="CreateProductView"/> object with randomly generated product details.
    /// </returns>
    public static CreateProductView GenerateRandomProductView()
    {
        var productName = GenerateRandomProductName();

        return new CreateProductView
        {
            Name = productName,
            Url = GenerateRandomProductUrl(productName),
            StockQuantity = GenerateRandomProductStock(),
            Description = GenerateRandomDescription(productName),
            Price = GenerateRandomProductPrice()
        };
    }

    /// <summary>
    /// Generates a random product with various randomized attributes, including name, URL, price, stock quantity, and description.
    /// </summary>
    /// <returns>
    /// An instance of the <see cref="Product"/> class, populated with random data.
    /// </returns>
    public static Product GenerateRandomProduct()
    {
        var productName = GenerateRandomProductName();

        return new Product
        {
            Name = productName,
            Url = GenerateRandomProductUrl(productName),
            StockQuantity = GenerateRandomProductStock(),
            Description = GenerateRandomDescription(productName),
            Price = GenerateRandomProductPrice(),
        };
    }

    /// <summary>
    /// Generates a random description for a product based on the given product name.
    /// </summary>
    /// <param name="productName">
    /// The name of the product for which the description is to be generated.
    /// </param>
    /// <returns>
    /// A string representing a randomly generated product description that incorporates the product name.
    /// </returns>
    private static string GenerateRandomDescription(string productName)
    {
        return $"{productName} product is perfect for your needs. Experience the best quality and performance with this amazing product. {GenerateLoremIpsum(1)}";
    }

    /// <summary>
    /// Generates a string containing one or more Lorem Ipsum sentences chosen randomly.
    /// </summary>
    /// <param name="count">
    /// The number of Lorem Ipsum sentences to generate.
    /// </param>
    /// <returns>
    /// A string containing the specified number of randomly selected Lorem Ipsum sentences.
    /// </returns>
    private static string GenerateLoremIpsum(int count)
    {
        var result = new List<string>();

        for (var i = 0; i < count; i++)
        {
            result.Add(LoremIpsumSentences[Czlovek.Randomness.Utils.Next(LoremIpsumSentences.Length)]);
        }

        return string.Join(' ', result);
    }
}